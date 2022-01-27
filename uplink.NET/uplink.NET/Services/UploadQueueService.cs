using SQLite;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using uplink.NET.Interfaces;
using uplink.NET.Models;
using System.Linq;

namespace uplink.NET.Services
{
    public class UploadQueueService : IUploadQueueService, IDisposable
    {
        SQLiteAsyncConnection _connection;
        CancellationTokenSource _source;
        Task _uploadTask;
        readonly string _databasePath;

        public event UploadQueueChangedEventHandler UploadQueueChangedEvent;

        public UploadQueueService()
        {
            _databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "uplinkNET.db");
        }

        public async Task ClearAllPendingUploadsAsync()
        {
            await InitAsync().ConfigureAwait(false);

            await _connection.DropTableAsync<UploadQueueEntry>().ConfigureAwait(false);
            await _connection.DropTableAsync<UploadQueueEntryData>().ConfigureAwait(false);

            await _connection.CloseAsync().ConfigureAwait(false);
            _connection = null;
        }

        public UploadQueueService(string databasePath)
        {
            _databasePath = databasePath;
        }

        public bool UploadInProgress
        {
            get
            {
                return _uploadTask != null && (_uploadTask.Status == TaskStatus.Running || _uploadTask.Status == TaskStatus.WaitingToRun || _uploadTask.Status == TaskStatus.WaitingForActivation);
            }
        }

        private async Task InitAsync()
        {
            if (_connection == null)
            {
                _connection = new SQLiteAsyncConnection(_databasePath);

                await _connection.CreateTableAsync<UploadQueueEntry>().ConfigureAwait(false);
                await _connection.CreateTableAsync<UploadQueueEntryData>().ConfigureAwait(false);
            }
        }

        public async Task AddObjectToUploadQueueAsync(string bucketName, string key, string accessGrant, byte[] objectData, string identifier)
        {
            await AddObjectToUploadQueueAsync(bucketName, key, accessGrant, new MemoryStream(objectData), identifier).ConfigureAwait(false);
        }

        public async Task AddObjectToUploadQueueAsync(string bucketName, string key, string accessGrant, Stream stream, string identifier)
        {
            await InitAsync().ConfigureAwait(false);

            var entry = new UploadQueueEntry();
            entry.AccessGrant = accessGrant;
            entry.Identifier = identifier;
            entry.BucketName = bucketName;
            entry.Key = key;
            entry.TotalBytes = (int)stream.Length;
            entry.BytesCompleted = 0;

            await _connection.InsertAsync(entry).ConfigureAwait(false);

            var entryData = new UploadQueueEntryData();
            entryData.UploadQueueEntryId = entry.Id;
            entryData.Bytes = new byte[stream.Length];
            stream.Read(entryData.Bytes, 0, (int)stream.Length);

            await _connection.InsertAsync(entryData).ConfigureAwait(false);

            ProcessQueueInBackground();

            UploadQueueChangedEvent?.Invoke(QueueChangeType.EntryAdded, entry);
        }

        public async Task CancelUploadAsync(string key)
        {
            await InitAsync().ConfigureAwait(false);

            var entry = await _connection.Table<UploadQueueEntry>().Where(e => e.Key == key).FirstOrDefaultAsync().ConfigureAwait(false);
            if (entry != null)
            {
                await RemoveEntry(entry).ConfigureAwait(false);
            }
        }

        public async Task RetryAsync(string key)
        {
            await InitAsync().ConfigureAwait(false);

            var entry = await _connection.Table<UploadQueueEntry>().Where(e => e.Key == key).FirstOrDefaultAsync().ConfigureAwait(false);
            if (entry != null)
            {
                try
                {
                    var access = new Access(entry.AccessGrant);
                    var multipartUploadService = new MultipartUploadService(access);
                    await multipartUploadService.AbortUploadAsync(entry.BucketName, entry.Key, entry.UploadId).ConfigureAwait(false);
                }
                catch
                {
                    //Ignore errors - we restart that upload anyways
                }

                entry.BytesCompleted = 0;
                entry.Failed = false;
                entry.FailedMessage = string.Empty;
                entry.CurrentPartNumber = 0;
                entry.UploadId = string.Empty;
                await _connection.UpdateAsync(entry).ConfigureAwait(false);

                UploadQueueChangedEvent?.Invoke(QueueChangeType.EntryUpdated, entry);
            }
        }

        public async Task<List<UploadQueueEntry>> GetAwaitingUploadsAsync()
        {
            await InitAsync().ConfigureAwait(false);

            return await _connection.Table<UploadQueueEntry>().ToListAsync().ConfigureAwait(false);
        }

        public void ProcessQueueInBackground()
        {
            if (_uploadTask == null || _uploadTask.Status == TaskStatus.RanToCompletion)
            {
                _source = new CancellationTokenSource();
                var uploadCancelToken = _source.Token;

                _uploadTask = Task.Run(() => DoUploadAsync(uploadCancelToken), uploadCancelToken);
            }
        }

        public void StopQueueInBackground()
        {
            if (_source != null && _source.Token.CanBeCanceled)
            {
                _source.Cancel();
            }
        }

        private async Task DoUploadAsync(CancellationToken token)
        {
            await InitAsync().ConfigureAwait(false);

            try
            {
                while (!token.IsCancellationRequested)
                {
                    var toUpload = await _connection.Table<UploadQueueEntry>().Where(e => !e.Failed).FirstOrDefaultAsync().ConfigureAwait(false);
                    if (toUpload != null)
                    {
                        try
                        {
                            var access = new Access(toUpload.AccessGrant);
                            var multipartUploadService = new MultipartUploadService(access);

                            //If the upload has not UploadId, begin it
                            if (!token.IsCancellationRequested && string.IsNullOrEmpty(toUpload.UploadId))
                            {
                                var uploadInfo = await multipartUploadService.BeginUploadAsync(toUpload.BucketName, toUpload.Key, new UploadOptions()).ConfigureAwait(false);
                                toUpload.UploadId = uploadInfo.UploadId;

                                //Save the UploadId
                                await _connection.UpdateAsync(toUpload).ConfigureAwait(false);
                            }

                            if (!token.IsCancellationRequested)
                            {
                                var toUploadData = await _connection.Table<UploadQueueEntryData>().Where(d => d.UploadQueueEntryId == toUpload.Id).FirstOrDefaultAsync().ConfigureAwait(false);
                                if (toUploadData != null)
                                {
                                    while (!token.IsCancellationRequested && toUpload.BytesCompleted != toUpload.TotalBytes)
                                    {
                                        //Now upload batches of 5 MiB (5242880 bytes)
                                        var bytesToUpload = toUploadData.Bytes.Skip(toUpload.BytesCompleted).Take(5242880).ToArray();
                                        var upload = await multipartUploadService.UploadPartAsync(toUpload.BucketName, toUpload.Key, toUpload.UploadId, toUpload.CurrentPartNumber, bytesToUpload).ConfigureAwait(false);

                                        //Refresh the uploaded bytes counter and define the next part number
                                        toUpload.BytesCompleted += (int)upload.BytesWritten;
                                        toUpload.CurrentPartNumber++;

                                        //Save the current state
                                        await _connection.UpdateAsync(toUpload).ConfigureAwait(false);

                                        UploadQueueChangedEvent?.Invoke(QueueChangeType.EntryUpdated, toUpload);
                                    }
                                }
                            }

                            //If all bytes are uploaded, commit the upload.
                            if (!token.IsCancellationRequested && toUpload.BytesCompleted == toUpload.TotalBytes)
                            {
                                var commitResult = await multipartUploadService.CommitUploadAsync(toUpload.BucketName, toUpload.Key, toUpload.UploadId, new CommitUploadOptions()).ConfigureAwait(false);
                                if (string.IsNullOrEmpty(commitResult.Error))
                                {
                                    await RemoveEntry(toUpload).ConfigureAwait(false);
                                }
                                else
                                {
                                    toUpload.Failed = true;
                                    toUpload.FailedMessage = commitResult.Error;

                                    //Save the current state
                                    await _connection.UpdateAsync(toUpload).ConfigureAwait(false);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            toUpload.Failed = true;
                            toUpload.FailedMessage = ex.Message;
                            toUpload.UploadId = String.Empty; //Otherwise the upload-queue might get stuck if the upload cannot be succeeded, as on the next run it would hit the same error again

                            //Save the current state
                            await _connection.UpdateAsync(toUpload).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        return;
                    }
                }
            }
            catch
            {
                //That's ok, simply quit. The next run should fix it.
            }
        }

        public async Task<int> GetOpenUploadCountAsync()
        {
            await InitAsync().ConfigureAwait(false);

            return await _connection.Table<UploadQueueEntry>().CountAsync().ConfigureAwait(false);
        }

        private async Task RemoveEntry(UploadQueueEntry entry)
        {
            await _connection.Table<UploadQueueEntry>().DeleteAsync(e => e.Id == entry.Id).ConfigureAwait(false);
            await _connection.Table<UploadQueueEntryData>().DeleteAsync(e => e.UploadQueueEntryId == entry.Id).ConfigureAwait(false);

            UploadQueueChangedEvent?.Invoke(QueueChangeType.EntryRemoved, entry);
        }

        public void Dispose()
        {
            if(_source!= null)
            {
                _source.Dispose();
                _source = null;
            }
        }
    }
}
