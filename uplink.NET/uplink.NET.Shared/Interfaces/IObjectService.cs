using System;
using System.Collections.Generic;
using System.Text;
using uplink.NET.Models;

namespace uplink.NET.Interfaces
{
    public interface IObjectService
    {
        UploadOperation UploadObject(BucketRef bucket, string targetPath, UploadOptions uploadOptions, byte[] bytesToUpload, bool immediateStart = true);
        ObjectList ListObjects(BucketRef bucket, ListOptions listOptions);
        ObjectMeta GetObjectMeta(BucketRef bucket, string targetPath);
        DownloadOperation DownloadObject(BucketRef bucket, string targetPath, bool immediateStart = true);
    }
}
