using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using uplink.NET.Models;

namespace uplink.NET.Interfaces
{
    public interface IObjectService
    {
        /// <summary>
        /// Uploads an object to the given bucket and the given Target-Path. Immediately starts the upload.
        /// </summary>
        /// <param name="bucket">The Bucket to upload to</param>
        /// <param name="targetPath">The path/name of the object within the bucket</param>
        /// <param name="uploadOptions">Uploadoptions to control the store-operation</param>
        /// <param name="bytesToUpload">The binary-data to upload</param>
        /// <returns>An UploadOperation containing the info about the current state of the upload</returns>
        Task<UploadOperation> UploadObjectAsync(Bucket bucket, string targetPath, UploadOptions uploadOptions, byte[] bytesToUpload);
        /// <summary>
        /// Uploads an object to the given bucket and the given Target-Path.
        /// </summary>
        /// <param name="bucket">The Bucket to upload to</param>
        /// <param name="targetPath">The path/name of the object within the bucket</param>
        /// <param name="uploadOptions">Uploadoptions to control the store-operation</param>
        /// <param name="bytesToUpload">The binary-data to upload</param>
        /// <param name="immediateStart">Starts the upload immediately or defer's it to you via the returned UploadOperation.</param>
        /// <returns>An UploadOperation containing the info about the current state of the upload</returns>
        Task<UploadOperation> UploadObjectAsync(Bucket bucket, string targetPath, UploadOptions uploadOptions, byte[] bytesToUpload, bool immediateStart);
        /// <summary>
        /// Uploads an object to the given bucket and the given Target-Path. Immediately starts the upload.
        /// </summary>
        /// <param name="bucket">The Bucket to upload to</param>
        /// <param name="targetPath">The path/name of the object within the bucket</param>
        /// <param name="uploadOptions">Uploadoptions to control the store-operation</param>
        /// <param name="bytesToUpload">The binary-data to upload</param>
        /// <param name="customMetadata">Adds custom metadata.</param>
        /// <returns>An UploadOperation containing the info about the current state of the upload</returns>
        Task<UploadOperation> UploadObjectAsync(Bucket bucket, string targetPath, UploadOptions uploadOptions, byte[] bytesToUpload, CustomMetadata customMetadata);
        /// <summary>
        /// Uploads an object to the given bucket and the given Target-Path.
        /// </summary>
        /// <param name="bucket">The Bucket to upload to</param>
        /// <param name="targetPath">The path/name of the object within the bucket</param>
        /// <param name="uploadOptions">Uploadoptions to control the store-operation</param>
        /// <param name="bytesToUpload">The binary-data to upload</param>
        /// <param name="customMetadata">Adds custom metadata.</param>
        /// <param name="immediateStart">Starts the upload immediately or defer's it to you via the returned UploadOperation.</param>
        /// <returns>An UploadOperation containing the info about the current state of the upload</returns>
        Task<UploadOperation> UploadObjectAsync(Bucket bucket, string targetPath, UploadOptions uploadOptions, byte[] bytesToUpload, CustomMetadata customMetadata, bool immediateStart);
        /// <summary>
        /// Uploads an object to the given bucket and the given Target-Path. Immediately starts the upload.
        /// </summary>
        /// <param name="bucket">The Bucket to upload to</param>
        /// <param name="targetPath">The path/name of the object within the bucket</param>
        /// <param name="uploadOptions">Uploadoptions to control the store-operation</param>
        /// <param name="stream">The binary-stream to upload</param>
        /// <returns>An UploadOperation containing the info about the current state of the upload</returns>
        Task<UploadOperation> UploadObjectAsync(Bucket bucket, string targetPath, UploadOptions uploadOptions, Stream stream);
        /// <summary>
        /// Uploads an object to the given bucket and the given Target-Path.
        /// </summary>
        /// <param name="bucket">The Bucket to upload to</param>
        /// <param name="targetPath">The path/name of the object within the bucket</param>
        /// <param name="uploadOptions">Uploadoptions to control the store-operation</param>
        /// <param name="stream">The binary-stream to upload</param>
        /// <param name="immediateStart">Starts the upload immediately or defer's it to you via the returned UploadOperation.</param>
        /// <returns>An UploadOperation containing the info about the current state of the upload</returns>
        Task<UploadOperation> UploadObjectAsync(Bucket bucket, string targetPath, UploadOptions uploadOptions, Stream stream, bool immediateStart);
        /// <summary>
        /// Uploads an object to the given bucket and the given Target-Path. Immediately starts the upload.
        /// </summary>
        /// <param name="bucket">The Bucket to upload to</param>
        /// <param name="targetPath">The path/name of the object within the bucket</param>
        /// <param name="uploadOptions">Uploadoptions to control the store-operation</param>
        /// <param name="stream">The binary-stream to upload</param>
        /// <param name="customMetadata">Adds custom metadata.</param>
        /// <returns>An UploadOperation containing the info about the current state of the upload</returns>
        Task<UploadOperation> UploadObjectAsync(Bucket bucket, string targetPath, UploadOptions uploadOptions, Stream stream, CustomMetadata customMetadata);
        /// <summary>
        /// Uploads an object to the given bucket and the given Target-Path.
        /// </summary>
        /// <param name="bucket">The Bucket to upload to</param>
        /// <param name="targetPath">The path/name of the object within the bucket</param>
        /// <param name="uploadOptions">Uploadoptions to control the store-operation</param>
        /// <param name="stream">The binary-stream to upload</param>
        /// <param name="customMetadata">Adds custom metadata.</param>
        /// <param name="immediateStart">Starts the upload immediately or defer's it to you via the returned UploadOperation.</param>
        /// <returns>An UploadOperation containing the info about the current state of the upload</returns>
        Task<UploadOperation> UploadObjectAsync(Bucket bucket, string targetPath, UploadOptions uploadOptions, Stream stream, CustomMetadata customMetadata, bool immediateStart);
        /// <summary>
        /// Uploads an object to the given bucket and the given Target-Path via chunks. The chunks can be provided as they arrive.
        /// </summary>
        /// <param name="bucket">The Bucket to upload to</param>
        /// <param name="targetPath">The path/name of the object within the bucket</param>
        /// <param name="uploadOptions">Uploadoptions to control the store-operation</param>
        /// <param name="customMetadata">Adds custom metadata.</param>
        /// <returns>A ChunkedUploadOperation that can be filled with byte-chunks by you</returns>
        Task<ChunkedUploadOperation> UploadObjectChunkedAsync(Bucket bucket, string targetPath, UploadOptions uploadOptions, CustomMetadata customMetadata);
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
        /// <returns>The object or raises an ObjectNotFoundException</returns>
        Task<uplink.NET.Models.Object> GetObjectAsync(Bucket bucket, string targetPath);
        /// <summary>
        /// Downloads an object from the given bucket and the given Target-Path. Immediately starts the download.
        /// </summary>
        /// <param name="bucket">The Bucket to download from</param>
        /// <param name="targetPath">The path/name of the object within the bucket</param>
        /// <param name="downloadOptions">The options for this download</param>
        /// <returns>A DownloadOperation containing the info about the current state of the download or throws ObjectNotFoundException</returns>
        Task<DownloadOperation> DownloadObjectAsync(Bucket bucket, string targetPath, DownloadOptions downloadOptions);
        /// <summary>
        /// Downloads an object from the given bucket and the given Target-Path.
        /// </summary>
        /// <param name="bucket">The Bucket to download from</param>
        /// <param name="targetPath">The path/name of the object within the bucket</param>
        /// <param name="downloadOptions">The options for this download</param>
        /// <param name="immediateStart">Starts the download immediately or defer's it to you via the returned DownloadOperation.</param>
        /// <returns>A DownloadOperation containing the info about the current state of the download or throws ObjectNotFoundException</returns>
        Task<DownloadOperation> DownloadObjectAsync(Bucket bucket, string targetPath, DownloadOptions downloadOptions, bool immediateStart);
        /// <summary>
        /// Deletes the mentioned object
        /// </summary>
        /// <param name="bucket">The Bucket where the object resides in</param>
        /// <param name="targetPath">The path/name of the object within the bucket</param>
        Task DeleteObjectAsync(Bucket bucket, string targetPath);
        /// <summary>
        /// Moves an object from one bucket/key to another bucket/key. The bucket may differ or be the same one.
        /// </summary>
        /// <param name="oldBucket">The old bucket (from-bucket)</param>
        /// <param name="oldKey">The old key (from-key)</param>
        /// <param name="newBucket">The new bucket (to-bucket)</param>
        /// <param name="newKey">The new key (to-key)</param>
        /// <returns>Nothing on success or raises an ObjectMoveException</returns>
        Task MoveObjectAsync(Bucket oldBucket, string oldKey, Bucket newBucket, string newKey);
        /// <summary>
        /// Updates the CustomMetadata of an object by replacing it with the one given.
        /// </summary>
        /// <param name="bucket">The Bucket</param>
        /// <param name="targetPath">The path/name of the object within the bucket</param>
        /// <param name="metadata">The new Metadata</param>
        /// <returns>Nothing on success or raises an CouldNotUpdateObjectMetadataException</returns>
        Task UpdateObjectMetadataAsync(Bucket bucket, string targetPath, CustomMetadata metadata);
    }
}
