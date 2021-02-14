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
        Access _access;

        public ObjectService(Access access)
        {
            _access = access;
        }

        public async Task<UploadOperation> UploadObjectAsync(Bucket bucket, string targetPath, UploadOptions uploadOptions, byte[] bytesToUpload, bool immediateStart = true)
        {
            return await UploadObjectAsync(bucket, targetPath, uploadOptions, bytesToUpload, null, immediateStart);
        }

        public async Task<UploadOperation> UploadObjectAsync(Bucket bucket, string targetPath, UploadOptions uploadOptions, Stream stream, bool immediateStart = true)
        {
            return await UploadObjectAsync(bucket, targetPath, uploadOptions, stream, null, immediateStart);
        }

        public async Task<UploadOperation> UploadObjectAsync(Bucket bucket, string targetPath, UploadOptions uploadOptions, Stream stream, CustomMetadata customMetadata, bool immediateStart = true)
        {
            var uploadOptionsSWIG = uploadOptions.ToSWIG();

            SWIG.UplinkUploadResult uploadResult = await Task.Run(() => SWIG.storj_uplink.uplink_upload_object(_access._project, bucket.Name, targetPath, uploadOptionsSWIG));

            UploadOperation upload = new UploadOperation(stream, uploadResult, targetPath, customMetadata);
            if (immediateStart)
                upload.StartUploadAsync(); //Don't await it, otherwise it would "block" UploadObjectAsync

            return upload;
        }

        public async Task<UploadOperation> UploadObjectAsync(Bucket bucket, string targetPath, UploadOptions uploadOptions, byte[] bytesToUpload, CustomMetadata customMetadata, bool immediateStart = true)
        {
            var uploadOptionsSWIG = uploadOptions.ToSWIG();

            SWIG.UplinkUploadResult uploadResult = await Task.Run(() => SWIG.storj_uplink.uplink_upload_object(_access._project, bucket.Name, targetPath, uploadOptionsSWIG));

            UploadOperation upload = new UploadOperation(bytesToUpload, uploadResult, targetPath, customMetadata);
            if (immediateStart)
                upload.StartUploadAsync(); //Don't await it, otherwise it would "block" UploadObjectAsync

            return upload;
        }

        public async Task<ChunkedUploadOperation> UploadObjectChunkedAsync(Bucket bucket, string targetPath, UploadOptions uploadOptions, CustomMetadata customMetadata)
        {
            var uploadOptionsSWIG = uploadOptions.ToSWIG();

            SWIG.UplinkUploadResult uploadResult = await Task.Run(() => SWIG.storj_uplink.uplink_upload_object(_access._project, bucket.Name, targetPath, uploadOptionsSWIG));

            ChunkedUploadOperation upload = new ChunkedUploadOperation(uploadResult, targetPath, customMetadata);

            return upload;
        }

        public async Task<DownloadOperation> DownloadObjectAsync(Bucket bucket, string targetPath, DownloadOptions downloadOptions, bool immediateStart = true)
        {
            var downloadOptionsSWIG = downloadOptions.ToSWIG();

            SWIG.UplinkDownloadResult downloadResult = await Task.Run(() => SWIG.storj_uplink.uplink_download_object(_access._project, bucket.Name, targetPath, downloadOptionsSWIG));

            if (downloadResult.error != null && !string.IsNullOrEmpty(downloadResult.error.message))
                throw new ObjectNotFoundException(targetPath, downloadResult.error.message);

            SWIG.UplinkObjectResult objectResult = await Task.Run(() => SWIG.storj_uplink.uplink_download_info(downloadResult.download));
            if (objectResult.error != null && !string.IsNullOrEmpty(objectResult.error.message))
                throw new ObjectNotFoundException(targetPath, objectResult.error.message);

            DownloadOperation download = new DownloadOperation(downloadResult, objectResult.object_.system.content_length, targetPath);
            if (immediateStart)
                download.StartDownloadAsync(); //Don't await it, otherwise it would "block" DownloadObjectAsync

            SWIG.storj_uplink.uplink_free_object_result(objectResult);

            return download;
        }

        public async Task<ObjectList> ListObjectsAsync(Bucket bucket, ListObjectsOptions listObjectsOptions)
        {
            SWIG.UplinkObjectIterator objectIterator = await Task.Run(() => SWIG.storj_uplink.uplink_list_objects(_access._project, bucket.Name, listObjectsOptions.ToSWIG()));

            SWIG.UplinkError error = SWIG.storj_uplink.uplink_object_iterator_err(objectIterator);
            if (error != null && !string.IsNullOrEmpty(error.message))
                throw new BucketListException(error.message);
            SWIG.storj_uplink.uplink_free_error(error);

            ObjectList objectList = new ObjectList();

            while (SWIG.storj_uplink.uplink_object_iterator_next(objectIterator))
            {
                objectList.Items.Add(uplink.NET.Models.Object.FromSWIG(SWIG.storj_uplink.uplink_object_iterator_item(objectIterator), true));
            }
            SWIG.storj_uplink.uplink_free_object_iterator(objectIterator);

            return objectList;
        }

        public async Task<uplink.NET.Models.Object> GetObjectAsync(Bucket bucket, string targetPath)
        {
            var objectResult = await Task.Run(() => SWIG.storj_uplink.uplink_stat_object(_access._project, bucket.Name, targetPath));

            if (objectResult.error != null && !string.IsNullOrEmpty(objectResult.error.message))
                throw new ObjectNotFoundException(targetPath, objectResult.error.message);

            return uplink.NET.Models.Object.FromSWIG(objectResult.object_, true);
        }

        public async Task DeleteObjectAsync(Bucket bucket, string targetPath)
        {
            SWIG.UplinkObjectResult objectResult = await Task.Run(() => SWIG.storj_uplink.uplink_delete_object(_access._project, bucket.Name, targetPath));

            if (objectResult.error != null && !string.IsNullOrEmpty(objectResult.error.message))
                throw new ObjectNotFoundException(targetPath, objectResult.error.message);

            SWIG.storj_uplink.uplink_free_object_result(objectResult);
        }
    }
}
