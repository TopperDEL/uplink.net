using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using uplink.NET.Models;

namespace uplink.NET.Interfaces
{
    public interface IMultipartUploadService
    {
        Task<UploadInfo> BeginUploadAsync(string bucketName, string objectKey, UploadOptions uploadOptions);
        Task<CommitUploadResult> CommitUploadAsync(string bucketName, string objectKey, string uploadId, CommitUploadOptions commitUploadOptions);
        Task AbortUploadAsync(string bucketName, string objectKey, string uploadId);
        Task<PartUploadResult> UploadPartAsync(string bucketName, string objectKey, string uploadId, uint partNumber, byte[] partBytes);
        Task UploadPartSetETagAsync(PartUpload partUpload, string eTag);
        Task<PartResult> GetPartUploadInfoAsync(PartUpload partUpload);
        Task<UploadsList> ListUploadsAsync(string bucketName, ListUploadOptions listUploadOptions);
        Task<UploadPartsList> ListUploadPartsAsync(string bucketName, string objectKey, string uploadId, ListUploadPartsOptions listUploadPartOptions);
    }
}
