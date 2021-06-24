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
    public class UploadQueueService : IUploadQueueService
    {
        SQLiteAsyncConnection _connection;
        CancellationTokenSource _source;
        Task _uploadTask;
        readonly string _databasePath;

        public UploadQueueService()
        {
            _databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "uplinkNET.db");
        }

        public async Task ClearAllPendingUploadsAsync()
        {
            await InitAsync();

            await _connection.DropTableAsync<UploadQueueEntry>();
            await _connection.DropTableAsync<UploadQueueEntryData>();

            await _connection.CloseAsync();
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

                await _connection.CreateTableAsync<UploadQueueEntry>();
                await _connection.CreateTableAsync<UploadQueueEntryData>();
            }
        }

        public async Task AddObjectToUploadQueue(string bucketName, string key, string accessGrant, byte[] objectData, string identifier)
        {
            await InitAsync();

            var entry = new UploadQueueEntry();
            entry.AccessGrant = accessGrant;
            entry.Identifier = identifier;
            entry.BucketName = bucketName;
            entry.Key = key;
            entry.TotalBytes = objectData.Length;
            entry.BytesCompleted = 0;

            await _connection.InsertAsync(entry);

            var entryData = new UploadQueueEntryData();
            entryData.UploadQueueEntryId = entry.Id;
            entryData.Bytes = objectData;

            await _connection.InsertAsync(entryData);

            ProcessQueueInBackground();
        }

        public async Task CancelUploadAsync(string key)
        {
            await InitAsync();

            var entry = await _connection.Table<UploadQueueEntry>().Where(e => e.Key == key).FirstOrDefaultAsync();
            if (entry != null)
            {
                var id = entry.Id;
                await _connection.Table<UploadQueueEntry>().DeleteAsync(e => e.Id == id);
                await _connection.Table<UploadQueueEntryData>().DeleteAsync(e => e.UploadQueueEntryId == id);
            }
        }

        public Task<List<UploadOperation>> GetAwaitingUploadsAsync()
        {
            throw new NotImplementedException();
        }

        public void ProcessQueueInBackground()
        {
            if (_uploadTask == null)
            {
                _source = new CancellationTokenSource();
                var uploadCancelToken = _source.Token;

                _uploadTask = Task.Run(() => DoUploadAsync(uploadCancelToken), uploadCancelToken);
            }
        }

        public void StopQueueInBackground()
        {
            if(_source!= null && _source.Token.CanBeCanceled)
            {
                _source.Cancel();
            }
        }

        private async Task DoUploadAsync(CancellationToken token)
        {
            await InitAsync();

            try
            {
                while (!token.IsCancellationRequested)
                {
                    var toUpload = await _connection.Table<UploadQueueEntry>().Where(e => !e.Failed).FirstOrDefaultAsync();
                    if (toUpload != null)
                    {
                        try
                        {
                            var access = new Access(toUpload.AccessGrant);
                            var bucketService = new BucketService(access);
                            var bucket = await bucketService.GetBucketAsync(toUpload.BucketName);
                            var multipartUploadService = new MultipartUploadService(access);

                            //If the upload has not UploadId, begin it
                            if (!token.IsCancellationRequested && string.IsNullOrEmpty(toUpload.UploadId))
                            {
                                var uploadInfo = await multipartUploadService.BeginUploadAsync(toUpload.BucketName, toUpload.Key, new UploadOptions());
                                toUpload.UploadId = uploadInfo.UploadId;

                                //Save the UploadId
                                await _connection.UpdateAsync(toUpload);
                            }

                            if (!token.IsCancellationRequested)
                            {
                                var toUploadData = await _connection.Table<UploadQueueEntryData>().Where(d => d.UploadQueueEntryId == toUpload.Id).FirstOrDefaultAsync();
                                if (toUploadData != null)
                                {
                                    while (!token.IsCancellationRequested && toUpload.BytesCompleted != toUpload.TotalBytes)
                                    {
                                        //Now upload batches of 256 KiB (262144 bytes)
                                        var bytesToUpload = toUploadData.Bytes.Skip(toUpload.BytesCompleted).Take(262144).ToArray();
                                        var upload = await multipartUploadService.UploadPartAsync(toUpload.BucketName, toUpload.Key, toUpload.UploadId, toUpload.CurrentPartNumber, bytesToUpload);

                                        //Refresh the uploaded bytes counter and define the next part number
                                        toUpload.BytesCompleted += (int)upload.BytesWritten;
                                        toUpload.CurrentPartNumber++;

                                        //Save the current state
                                        await _connection.UpdateAsync(toUpload);
                                    }
                                }
                            }

                            //If all bytes are uploaded, commit the upload.
                            if (!token.IsCancellationRequested && toUpload.BytesCompleted == toUpload.TotalBytes)
                            {
                                var commitResult = await multipartUploadService.CommitUploadAsync(toUpload.BucketName, toUpload.Key, toUpload.UploadId, new CommitUploadOptions());
                                if (string.IsNullOrEmpty(commitResult.Error))
                                {
                                    await _connection.Table<UploadQueueEntry>().DeleteAsync(e => e.Id == toUpload.Id);
                                    await _connection.Table<UploadQueueEntryData>().DeleteAsync(e => e.UploadQueueEntryId == toUpload.Id);
                                }
                                else
                                {
                                    toUpload.Failed = true;
                                    toUpload.FailedMessage = commitResult.Error;

                                    //Save the current state
                                    await _connection.UpdateAsync(toUpload);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            toUpload.Failed = true;
                            toUpload.FailedMessage = ex.Message;

                            //Save the current state
                            await _connection.UpdateAsync(toUpload);
                        }
                    }
                    else
                    {
                        return;
                    }
                }
            }
            catch { } //That's ok, simply quit. The next run should fix it.
        }

        public async Task<int> GetOpenUploadCountAsync()
        {
            await InitAsync();

            return await _connection.Table<UploadQueueEntry>().CountAsync();
        }
    }
}
