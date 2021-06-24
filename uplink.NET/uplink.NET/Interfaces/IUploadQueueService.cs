using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using uplink.NET.Models;

namespace uplink.NET.Interfaces
{
    public interface IUploadQueueService
    {
        bool UploadInProgress { get; }
        Task AddObjectToUploadQueue(string bucketName, string key, string accessGrant, byte[] objectData, string identifier);
        Task<List<UploadQueueEntry>> GetAwaitingUploadsAsync();
        Task CancelUploadAsync(string key);
        void ProcessQueueInBackground();
        void StopQueueInBackground();

        Task<int> GetOpenUploadCountAsync();
    }
}
