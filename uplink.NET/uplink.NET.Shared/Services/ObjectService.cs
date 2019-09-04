using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using uplink.NET.Exceptions;
using uplink.NET.Interfaces;
using uplink.NET.Models;

namespace uplink.NET.Services
{
    public class ObjectService : IObjectService
    {
        public async Task<UploadOperation> UploadObjectAsync(BucketRef bucket, string targetPath, UploadOptions uploadOptions, byte[] bytesToUpload, bool immediateStart = true)
        {
            string error = string.Empty;
            var uploadOptionsSWIG = uploadOptions.ToSWIG();

            var uploaderRef = await Task.Run<SWIG.UploaderRef>(() => SWIG.storj_uplink.upload(bucket._bucketRef, targetPath, uploadOptionsSWIG, out error));

            SWIG.storj_uplink.free_upload_opts(uploadOptionsSWIG);

            UploadOperation upload = new UploadOperation(bytesToUpload, uploaderRef);
            if(immediateStart)
                upload.StartUploadAsync(); //Don't await it, otherwise it would "block" UploadObjectAsync

            return upload;
        }

        public async Task<DownloadOperation> DownloadObjectAsync(BucketRef bucket, string targetPath, bool immediateStart = true)
        {
            string error = string.Empty;

            var objectRef = await Task.Run<SWIG.ObjectRef>(() => SWIG.storj_uplink.open_object(bucket._bucketRef, targetPath, out error));
            if (!string.IsNullOrEmpty(error))
                throw new ObjectNotFoundException(targetPath, error);

            var objectMeta = await Task.Run<SWIG.ObjectMeta>(() => SWIG.storj_uplink.get_object_meta(objectRef, out error));
            if (!string.IsNullOrEmpty(error))
                throw new ObjectNotFoundException(targetPath, error);

            await Task.Run(() => SWIG.storj_uplink.close_object(objectRef, out error));
            if (!string.IsNullOrEmpty(error))
                throw new ObjectNotFoundException(targetPath, error);

            var downloaderRef = SWIG.storj_uplink.download(bucket._bucketRef, targetPath, out error);

            DownloadOperation download = new DownloadOperation(downloaderRef, objectMeta.size);
            if (immediateStart)
                download.StartDownloadAsync(); //Don't await it, otherwise it would "block" DownloadObjectAsync

            return download;
        }

        public async Task<ObjectList> ListObjectsAsync(BucketRef bucket, ListOptions listOptions)
        {
            string error = string.Empty;

            var objectList = await Task.Run<SWIG.ObjectList>(() => SWIG.storj_uplink.list_objects(bucket._bucketRef, listOptions.ToSWIG(), out error));

            if (!string.IsNullOrEmpty(error))
                throw new ObjectListException(error);

            return ObjectList.FromSWIG(objectList);
        }

        public async Task<ObjectMeta> GetObjectMetaAsync(BucketRef bucket, string targetPath)
        {
            string error = string.Empty;

            var objectRef = await Task.Run<SWIG.ObjectRef>(() => SWIG.storj_uplink.open_object(bucket._bucketRef, targetPath, out error));
            if (!string.IsNullOrEmpty(error))
                throw new ObjectNotFoundException(targetPath, error);

            var objectMeta = await Task.Run<SWIG.ObjectMeta>(() => SWIG.storj_uplink.get_object_meta(objectRef, out error));
            if (!string.IsNullOrEmpty(error))
                throw new ObjectNotFoundException(targetPath, error);

            await Task.Run(() => SWIG.storj_uplink.close_object(objectRef, out error));
            if (!string.IsNullOrEmpty(error))
                throw new ObjectNotFoundException(targetPath, error);

            return ObjectMeta.FromSWIG(objectMeta);
        }

        public async Task DeleteObjectAsync(BucketRef bucket, string targetPath)
        {
            string error = string.Empty;

            await Task.Run(() => SWIG.storj_uplink.delete_object(bucket._bucketRef, targetPath, out error));
            if (!string.IsNullOrEmpty(error))
                throw new ObjectNotFoundException(targetPath, error);
        }
    }
}
