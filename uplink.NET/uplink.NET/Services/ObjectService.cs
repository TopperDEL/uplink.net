using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using uplink.NET.Exceptions;
using uplink.NET.Interfaces;
using uplink.NET.Models;

namespace uplink.NET.Services
{
    public class ObjectService : IObjectService
    {
        static List<SWIG.UplinkListObjectsOptions> _listOptions = new List<SWIG.UplinkListObjectsOptions>(); //ToDo: Temporary until SWIG does not enforce IDisposable on UplinkListObjectsOptions
        static List<SWIG.UplinkUploadOptions> _uploadOptions = new List<SWIG.UplinkUploadOptions>();

        private readonly Access _access;

        public ObjectService(Access access)
        {
            _access = access;
        }

        public async Task<UploadOperation> UploadObjectAsync(Bucket bucket, string targetPath, UploadOptions uploadOptions, byte[] bytesToUpload)
        {
            return await UploadObjectAsync(bucket, targetPath, uploadOptions, bytesToUpload, null, true).ConfigureAwait(false);
        }

        public async Task<UploadOperation> UploadObjectAsync(Bucket bucket, string targetPath, UploadOptions uploadOptions, byte[] bytesToUpload, bool immediateStart)
        {
            return await UploadObjectAsync(bucket, targetPath, uploadOptions, bytesToUpload, null, immediateStart).ConfigureAwait(false);
        }

        public async Task<UploadOperation> UploadObjectAsync(Bucket bucket, string targetPath, UploadOptions uploadOptions, Stream stream)
        {
            return await UploadObjectAsync(bucket, targetPath, uploadOptions, stream, null, true).ConfigureAwait(false);
        }

        public async Task<UploadOperation> UploadObjectAsync(Bucket bucket, string targetPath, UploadOptions uploadOptions, Stream stream, bool immediateStart)
        {
            return await UploadObjectAsync(bucket, targetPath, uploadOptions, stream, null, immediateStart).ConfigureAwait(false);
        }

        public async Task<UploadOperation> UploadObjectAsync(Bucket bucket, string targetPath, UploadOptions uploadOptions, Stream stream, CustomMetadata customMetadata)
        {
            return await UploadObjectAsync(bucket, targetPath, uploadOptions, stream, customMetadata, true).ConfigureAwait(false);
        }

        public async Task<UploadOperation> UploadObjectAsync(Bucket bucket, string targetPath, UploadOptions uploadOptions, Stream stream, CustomMetadata customMetadata, bool immediateStart)
        {
            var uploadOptionsSWIG = uploadOptions.ToSWIG();
            _uploadOptions.Add(uploadOptionsSWIG);

            using (SWIG.UplinkUploadResult uploadResult = await Task.Run(() => SWIG.storj_uplink.uplink_upload_object(_access._project, bucket.Name, targetPath, uploadOptionsSWIG)).ConfigureAwait(false))
            {
                Console.WriteLine("UploadObjectAsync: Uploading object to bucket: {0}, targetPath: {1}", bucket.Name, targetPath);
                UploadOperation upload = new UploadOperation(stream, uploadResult, targetPath, customMetadata);
                if (immediateStart)
                    upload.StartUploadAsync(); //Don't await it, otherwise it would "block" UploadObjectAsync

                return upload;
            }
        }

        public async Task<UploadOperation> UploadObjectAsync(Bucket bucket, string targetPath, UploadOptions uploadOptions, byte[] bytesToUpload, CustomMetadata customMetadata)
        {
            return await UploadObjectAsync(bucket, targetPath, uploadOptions, bytesToUpload, customMetadata, true).ConfigureAwait(false);
        }

        public async Task<UploadOperation> UploadObjectAsync(Bucket bucket, string targetPath, UploadOptions uploadOptions, byte[] bytesToUpload, CustomMetadata customMetadata, bool immediateStart)
        {
            var uploadOptionsSWIG = uploadOptions.ToSWIG();
            _uploadOptions.Add(uploadOptionsSWIG);

            using (SWIG.UplinkUploadResult uploadResult = await Task.Run(() => SWIG.storj_uplink.uplink_upload_object(_access._project, bucket.Name, targetPath, uploadOptionsSWIG)).ConfigureAwait(false))
            {
                Console.WriteLine("UploadObjectAsync: Uploading object to bucket: {0}, targetPath: {1}", bucket.Name, targetPath);
                UploadOperation upload = new UploadOperation(bytesToUpload, uploadResult, targetPath, customMetadata);
                if (immediateStart)
                    upload.StartUploadAsync(); //Don't await it, otherwise it would "block" UploadObjectAsync

                return upload;
            }
        }

        public async Task<ChunkedUploadOperation> UploadObjectChunkedAsync(Bucket bucket, string targetPath, UploadOptions uploadOptions, CustomMetadata customMetadata)
        {
            var uploadOptionsSWIG = uploadOptions.ToSWIG();
            _uploadOptions.Add(uploadOptionsSWIG);
            using (SWIG.UplinkUploadResult uploadResult = await Task.Run(() => SWIG.storj_uplink.uplink_upload_object(_access._project, bucket.Name, targetPath, uploadOptionsSWIG)).ConfigureAwait(false))
            {
                Console.WriteLine("UploadObjectChunkedAsync: Uploading object to bucket: {0}, targetPath: {1}", bucket.Name, targetPath);
                ChunkedUploadOperation upload = new ChunkedUploadOperation(uploadResult, targetPath, customMetadata);

                return upload;
            }
        }

        public async Task<DownloadOperation> DownloadObjectAsync(Bucket bucket, string targetPath, DownloadOptions downloadOptions)
        {
            return await DownloadObjectAsync(bucket, targetPath, downloadOptions, true).ConfigureAwait(false);
        }

        public async Task<DownloadOperation> DownloadObjectAsync(Bucket bucket, string targetPath, DownloadOptions downloadOptions, bool immediateStart)
        {
            using (var downloadOptionsSWIG = downloadOptions.ToSWIG())
            {
                using (SWIG.UplinkDownloadResult downloadResult = await Task.Run(() => SWIG.storj_uplink.uplink_download_object(_access._project, bucket.Name, targetPath, downloadOptionsSWIG)).ConfigureAwait(false))
                {
                    Console.WriteLine("DownloadObjectAsync: Downloading object from bucket: {0}, targetPath: {1}", bucket.Name, targetPath);
                    if (downloadResult.error != null && !string.IsNullOrEmpty(downloadResult.error.message))
                        throw new ObjectNotFoundException(targetPath, downloadResult.error.message);

                    using (SWIG.UplinkObjectResult objectResult = await Task.Run(() => SWIG.storj_uplink.uplink_download_info(downloadResult.download)).ConfigureAwait(false))
                    {
                        if (objectResult.error != null && !string.IsNullOrEmpty(objectResult.error.message))
                            throw new ObjectNotFoundException(targetPath, objectResult.error.message);

                        DownloadOperation download = new DownloadOperation(downloadResult, objectResult.object_.system.content_length, targetPath);
                        if (immediateStart)
                            download.StartDownloadAsync(); //Don't await it, otherwise it would "block" DownloadObjectAsync

                        return download;
                    }
                }
            }
        }

        public async Task<DownloadStream> DownloadObjectAsStreamAsync(Bucket bucket, string targetPath)
        {
            Console.WriteLine("DownloadObjectAsStreamAsync: Downloading object as stream from bucket: {0}, targetPath: {1}", bucket.Name, targetPath);
            var objectToDownload = await GetObjectAsync(bucket, targetPath);
            return new DownloadStream(bucket, (int)objectToDownload.SystemMetadata.ContentLength, targetPath);
        }
        

        public async Task<ObjectList> ListObjectsAsync(Bucket bucket, ListObjectsOptions listObjectsOptions)
        {
            Console.WriteLine("ListObjectsAsync: Listing objects in bucket: {0}", bucket.Name);
            var listObjectsOptionsSWIG = listObjectsOptions.ToSWIG();
            _listOptions.Add(listObjectsOptionsSWIG);

            using (SWIG.UplinkObjectIterator objectIterator = await Task.Run(() => SWIG.storj_uplink.uplink_list_objects(_access._project, bucket.Name, listObjectsOptionsSWIG)).ConfigureAwait(false))
            {
                using (SWIG.UplinkError error = SWIG.storj_uplink.uplink_object_iterator_err(objectIterator))
                {
                    if (error != null && !string.IsNullOrEmpty(error.message))
                    {
                        throw new BucketListException(error.message);
                    }
                }

                ObjectList objectList = new ObjectList();

                while (SWIG.storj_uplink.uplink_object_iterator_next(objectIterator))
                {
                    using (var objectResult = SWIG.storj_uplink.uplink_object_iterator_item(objectIterator))
                    {
                        objectList.Items.Add(uplink.NET.Models.Object.FromSWIG(objectResult, true));
                    }
                }
                return objectList;
            }
        }

        public async Task<uplink.NET.Models.Object> GetObjectAsync(Bucket bucket, string targetPath)
        {
            Console.WriteLine("GetObjectAsync: Retrieving object from bucket: {0}, targetPath: {1}", bucket.Name, targetPath);
            using (var objectResult = await Task.Run(() => SWIG.storj_uplink.uplink_stat_object(_access._project, bucket.Name, targetPath)).ConfigureAwait(false))
            {
                if (objectResult.error != null && !string.IsNullOrEmpty(objectResult.error.message))
                    throw new ObjectNotFoundException(targetPath, objectResult.error.message);

                return uplink.NET.Models.Object.FromSWIG(objectResult.object_, true);
            }
        }

        public async Task DeleteObjectAsync(Bucket bucket, string targetPath)
        {
            Console.WriteLine("DeleteObjectAsync: Deleting object from bucket: {0}, targetPath: {1}", bucket.Name, targetPath);
            using (SWIG.UplinkObjectResult objectResult = await Task.Run(() => SWIG.storj_uplink.uplink_delete_object(_access._project, bucket.Name, targetPath)).ConfigureAwait(false))
            {
                if (objectResult.error != null && !string.IsNullOrEmpty(objectResult.error.message))
                {
                    throw new ObjectNotFoundException(targetPath, objectResult.error.message);
                }
                if (objectResult.object_ == null)
                {
                    throw new ObjectNotFoundException(targetPath);
                }
            }
        }

        public async Task MoveObjectAsync(Bucket oldBucket, string oldKey, Bucket newBucket, string newKey)
        {
            Console.WriteLine("MoveObjectAsync: Moving object from bucket: {0}, oldKey: {1} to bucket: {2}, newKey: {3}", oldBucket.Name, oldKey, newBucket.Name, newKey);
            using (var options = new SWIG.UplinkMoveObjectOptions())
            using (SWIG.UplinkError error = await Task.Run(() => SWIG.storj_uplink.uplink_move_object(_access._project, oldBucket.Name, oldKey, newBucket.Name, newKey, options)))
            {
                if (error != null && !string.IsNullOrEmpty(error.message))
                {
                    throw new ObjectNotFoundException(error.message);
                }
            }
        }

        public async Task CopyObjectAsync(Bucket oldBucket, string oldKey, Bucket newBucket, string newKey)
        {
            Console.WriteLine("CopyObjectAsync: Copying object from bucket: {0}, oldKey: {1} to bucket: {2}, newKey: {3}", oldBucket.Name, oldKey, newBucket.Name, newKey);
            using (var options = new SWIG.UplinkCopyObjectOptions())
            using (SWIG.UplinkObjectResult result = await Task.Run(() => SWIG.storj_uplink.uplink_copy_object(_access._project, oldBucket.Name, oldKey, newBucket.Name, newKey, options)))
            {
                if (result.error != null && !string.IsNullOrEmpty(result.error.message))
                {
                    throw new ObjectNotFoundException(result.error.message);
                }
            }
        }

        public async Task UpdateObjectMetadataAsync(Bucket bucket, string targetPath, CustomMetadata metadata)
        {
            Console.WriteLine("UpdateObjectMetadataAsync: Updating metadata for object in bucket: {0}, targetPath: {1}", bucket.Name, targetPath);
            await UploadOperation.customMetadataSemaphore.WaitAsync();
            try
            {
                metadata.ToSWIG(); //Appends the customMetadata in the go-layer to a global field
                using (var options = new SWIG.UplinkUploadObjectMetadataOptions())
                using (SWIG.UplinkError error = await Task.Run(() => SWIG.storj_uplink.uplink_update_object_metadata2(_access._project, bucket.Name, targetPath, options)).ConfigureAwait(false))
                {
                    if (error != null && !string.IsNullOrEmpty(error.message))
                    {
                        throw new CouldNotUpdateObjectMetadataException(error.message);
                    }
                }
            }
            finally
            {
                UploadOperation.customMetadataSemaphore.Release();
            }
        }
    }
}
