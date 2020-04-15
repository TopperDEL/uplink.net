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
        /// <param name="bucket">The Bucket to upload to</param>
        /// <param name="targetPath">The path/name of the object within the bucket</param>
        /// <param name="uploadOptions">Uploadoptions to control the store-operation</param>
        /// <param name="bytesToUpload">The binary-data to upload</param>
        /// <param name="immediateStart">Starts the upload immediately (default) or defer's it to you via the returned UploadOperation.</param>
        /// <returns>An UploadOperation containing the info about the current state of the upload</returns>
        Task<UploadOperation> UploadObjectAsync(Bucket bucket, string targetPath, UploadOptions uploadOptions, byte[] bytesToUpload, bool immediateStart = true);
        /// <summary>
        /// Uploads an object to the given bucket and the given Target-Path.
        /// </summary>
        /// <param name="bucket">The Bucket to upload to</param>
        /// <param name="targetPath">The path/name of the object within the bucket</param>
        /// <param name="uploadOptions">Uploadoptions to control the store-operation</param>
        /// <param name="bytesToUpload">The binary-data to upload</param>
        /// <param name="customMetadata">Adds custom metadata.</param>
        /// <param name="immediateStart">Starts the upload immediately (default) or defer's it to you via the returned UploadOperation.</param>
        /// <returns>An UploadOperation containing the info about the current state of the upload</returns>
        Task<UploadOperation> UploadObjectAsync(Bucket bucket, string targetPath, UploadOptions uploadOptions, byte[] bytesToUpload, CustomMetadata customMetadata, bool immediateStart = true);
        /// <summary>
        /// Lists all objects within a bucket
        /// </summary>
        /// <param name="bucket">The Bucket to list entries from</param>
        /// <param name="listOptions">Options for the listing</param>
        /// <returns>The list of found objects within the bucket and with the given ListOptions or throws an ObjectListException</returns>
        Task<ObjectList> ListObjectsAsync(Bucket bucket, ListObjectsOptions listObjectsOptions);
        /// <summary>
        /// Gets the specific object
        /// </summary>
        /// <param name="bucket">The Bucket where the object resides in</param>
        /// <param name="targetPath">The path/name of the object within the bucket</param>
        /// <returns>The object an ObjectNotFoundException</returns>
        Task<uplink.NET.Models.Object> GetObjectAsync(Bucket bucket, string targetPath);
        /// <summary>
        /// Downloads an object from the given bucket and the given Target-Path.
        /// </summary>
        /// <param name="bucket">The Bucket to download from</param>
        /// <param name="targetPath">The path/name of the object within the bucket</param>
        /// <param name="downloadOptions">The options for this download</param>
        /// <param name="immediateStart">Starts the download immediately (default) or defer's it to you via the returned DownloadOperation.</param>
        /// <returns>A DownloadOperation containing the info about the current state of the download or throws ObjectNotFoundException</returns>
        Task<DownloadOperation> DownloadObjectAsync(Bucket bucket, string targetPath, DownloadOptions downloadOptions, bool immediateStart = true);
        /// <summary>
        /// Deletes the mentioned object
        /// </summary>
        /// <param name="bucket">The Bucket where the object resides in</param>
        /// <param name="targetPath">The path/name of the object within the bucket</param>
        Task DeleteObjectAsync(Bucket bucket, string targetPath);
    }
}
