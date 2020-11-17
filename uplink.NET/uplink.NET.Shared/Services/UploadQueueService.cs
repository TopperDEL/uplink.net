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

namespace uplink.NET.Services
{
    public class UploadQueueService : IUploadQueueService
    {
        SQLiteAsyncConnection _connection;
        CancellationToken _uploadCancelToken;
        Task _uploadTask;

        public UploadQueueService()
        {
           
        }

        public bool UploadInProgress { get
            {
                return _uploadTask != null && ( _uploadTask.Status == TaskStatus.Running || _uploadTask.Status == TaskStatus.WaitingToRun);
            }
        }

        private async Task InitAsync()
        {
            if (_connection == null)
            {
                var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "uplinkNET.db");
                _connection = new SQLiteAsyncConnection(databasePath);

                await _connection.CreateTableAsync<UploadQueueEntry>();
            }
        }

        public async Task AddObjectToUploadQueue(string bucketName, string key, string accessGrant, byte[] objectData, string identifier = null)
        {
            await InitAsync();

            var entry = new UploadQueueEntry();
            entry.AccessGrant = accessGrant;
            entry.Identifier = string.IsNullOrWhiteSpace(identifier) ? Guid.NewGuid().ToString() : identifier;
            entry.BucketName = bucketName;
            entry.Key = key;
            entry.Bytes = objectData;

            await _connection.InsertAsync(entry);
        }

        public async Task CancelUploadAsync(string key)
        {
            await InitAsync();

            await _connection.Table<UploadQueueEntry>().DeleteAsync(e => e.Key == key);
        }

        public Task<List<UploadOperation>> GetAwaitingUploadsAsync()
        {
            throw new NotImplementedException();
        }

        public void ProcessQueueInBackground()
        {
            if (_uploadTask == null || _uploadTask.Status != TaskStatus.Running)
            {
                if (_uploadTask != null)
                    _uploadTask.Dispose();

                CancellationTokenSource source = new CancellationTokenSource();
                _uploadCancelToken = source.Token;

                _uploadTask = Task.Factory.StartNew(() => DoUploadAsync(_uploadCancelToken), _uploadCancelToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
        }

        private async Task DoUploadAsync(CancellationToken token)
        {
            await InitAsync();

            try
            {
                while (!token.IsCancellationRequested)
                {
                    var toUpload = await _connection.Table<UploadQueueEntry>().FirstAsync();

                    try
                    {
                        var access = new Access(toUpload.AccessGrant);
                        var bucketService = new BucketService(access);
                        var bucket = await bucketService.GetBucketAsync(toUpload.BucketName);
                        var objectService = new ObjectService(access);
                        var upload = await objectService.UploadObjectAsync(bucket, toUpload.Key, new UploadOptions(), toUpload.Bytes, false);
                        await upload.StartUploadAsync();

                        if (upload.Completed)
                        {
                            await _connection.Table<UploadQueueEntry>().DeleteAsync(e => e.Id == toUpload.Id);
                        }
                        else
                        {
                            //ToDo: Error handling
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            catch { } //That's ok, nothing more to do.
        }
    }
}
