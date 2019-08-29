using System;
using System.Collections.Generic;
using System.Text;
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
    }
}
