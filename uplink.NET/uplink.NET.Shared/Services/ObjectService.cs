using System;
using System.Collections.Generic;
using System.Text;
using uplink.NET.Exceptions;
using uplink.NET.Interfaces;
using uplink.NET.Models;

namespace uplink.NET.Services
{
    public class ObjectService : IObjectService
    {
        public UploadOperation UploadObject(BucketRef bucket, string targetPath, UploadOptions uploadOptions, byte[] bytesToUpload, bool immediateStart = true)
        {
            string error;

            var uploaderRef = SWIG.storj_uplink.upload(bucket._bucketRef, targetPath, uploadOptions.ToSWIG(), out error);

            UploadOperation upload = new UploadOperation(bytesToUpload, uploaderRef);
            if(immediateStart)
                upload.StartUploadAsync();

            return upload;
        }

        public DownloadOperation DownloadObject(BucketRef bucket, string targetPath, bool immediateStart = true)
        {
            string error;

            var objectRef = SWIG.storj_uplink.open_object(bucket._bucketRef, targetPath, out error);
            var objectMeta = SWIG.storj_uplink.get_object_meta(objectRef, out error);
            var downloaderRef = SWIG.storj_uplink.download(bucket._bucketRef, targetPath, out error);

            DownloadOperation download = new DownloadOperation(downloaderRef, objectMeta.size);
            if (immediateStart)
                download.StartDownloadAsync();

            return download;
        }

        public ObjectList ListObjects(BucketRef bucket, ListOptions listOptions)
        {
            string error;

            var objectList = SWIG.storj_uplink.list_objects(bucket._bucketRef, listOptions.ToSWIG(), out error);

            if (!string.IsNullOrEmpty(error))
                throw new ObjectListException(error);

            return ObjectList.FromSWIG(objectList);
        }
    }
}
