using System;
using System.Collections.Generic;
using System.Text;
using uplink.NET.Interfaces;
using uplink.NET.Models;

namespace uplink.NET.Services
{
    public class ObjectService : IObjectService
    {
        public void UploadObject(BucketRef bucket, string targetPath, UploadOptions uploadOptions)
        {
            string error;

            var uploadRef = SWIG.storj_uplink.upload(bucket._bucketRef, targetPath, uploadOptions._handle, out error);
            byte[] stringBytes = Encoding.UTF8.GetBytes("Mein erster upload");
            uint length = (uint)stringBytes.Length;
            var returnedLength = SWIG.storj_uplink.upload_write(uploadRef, stringBytes, length, out error);
            SWIG.storj_uplink.upload_commit(uploadRef, out error);

            var downloadRef = SWIG.storj_uplink.download(bucket._bucketRef, targetPath, out error);
            byte[] readBytes = new byte[length];
            SWIG.storj_uplink.download_read(downloadRef, readBytes, length, out error);
            SWIG.storj_uplink.download_close(downloadRef, out error);
            string resultString = Encoding.UTF8.GetString(readBytes);
        }
    }
}
