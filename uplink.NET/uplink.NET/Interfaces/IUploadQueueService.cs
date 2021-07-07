using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using uplink.NET.Models;

namespace uplink.NET.Interfaces
{
    public enum QueueChangeType
    {
        EntryAdded,
        EntryRemoved,
        EntryUpdated
    }
    public delegate void UploadQueueChangedEventHandler(QueueChangeType queueChangeType, UploadQueueEntry entry);
    public interface IUploadQueueService
    {
        bool UploadInProgress { get; }
        Task AddObjectToUploadQueueAsync(string bucketName, string key, string accessGrant, byte[] objectData, string identifier);
        Task AddObjectToUploadQueueAsync(string bucketName, string key, string accessGrant, Stream stream, string identifier);
        Task<List<UploadQueueEntry>> GetAwaitingUploadsAsync();
        Task CancelUploadAsync(string key);
        Task RetryAsync(string key);
        void ProcessQueueInBackground();
        void StopQueueInBackground();

        Task<int> GetOpenUploadCountAsync();

        event UploadQueueChangedEventHandler UploadQueueChangedEvent;
    }
}
