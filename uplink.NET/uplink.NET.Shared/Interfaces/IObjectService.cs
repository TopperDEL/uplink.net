using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using uplink.NET.Models;

namespace uplink.NET.Interfaces
{
    public interface IObjectService
    {
        /// <summary>
        /// Uploads an object to the given bucket and the given Target-Path.
        /// </summary>
        /// <param name="bucket">The BucketRef-Handle retrieved from BucketService.OpenBucket().</param>
        /// <param name="targetPath">The path/name of the object within the bucket</param>
        /// <param name="uploadOptions">Uploadoptions to control the store-operation</param>
        /// <param name="bytesToUpload">The binary-data to upload</param>
        /// <param name="immediateStart">Starts the upload immediately (default) or defer's it to you via the returned UploadOperation.</param>
        /// <returns>An UploadOperation containing the info about the current state of the upload</returns>
        Task<UploadOperation> UploadObjectAsync(BucketRef bucket, string targetPath, UploadOptions uploadOptions, byte[] bytesToUpload, bool immediateStart = true);
        /// <summary>
        /// Lists all objects within a bucket
        /// </summary>
        /// <param name="bucket">The BucketRef-Handle retrieved from BucketService.OpenBucket().</param>
        /// <param name="listOptions">Options for the listing</param>
        /// <returns>The list of found objects within the bucket and with the given ListOptions or throws an ObjectListException</returns>
        Task<ObjectList> ListObjectsAsync(BucketRef bucket, ListOptions listOptions);
        /// <summary>
        /// Gets the meta-data of a specific object
        /// </summary>
        /// <param name="bucket">The BucketRef-Handle retrieved from BucketService.OpenBucket().</param>
        /// <param name="targetPath">The path/name of the object within the bucket</param>
        /// <returns>The object meta-data or an ObjectNotFoundException</returns>
        Task<ObjectMeta> GetObjectMetaAsync(BucketRef bucket, string targetPath);
        /// <summary>
        /// Downloads an object from the given bucket and the given Target-Path.
        /// </summary>
        /// <param name="bucket">The BucketRef-Handle retrieved from BucketService.OpenBucket().</param>
        /// <param name="targetPath">The path/name of the object within the bucket</param>
        /// <param name="immediateStart">Starts the download immediately (default) or defer's it to you via the returned DownloadOperation.</param>
        /// <returns>A DownloadOperation containing the info about the current state of the download or throws ObjectNotFoundException</returns>
        Task<DownloadOperation> DownloadObjectAsync(BucketRef bucket, string targetPath, bool immediateStart = true);
        /// <summary>
        /// Deletes the mentioned object
        /// </summary>
        /// <param name="bucket">The BucketRef-Handle retrieved from BucketService.OpenBucket().</param>
        /// <param name="targetPath">The path/name of the object within the bucket</param>
        Task DeleteObjectAsync(BucketRef bucket, string targetPath);
    }
}
