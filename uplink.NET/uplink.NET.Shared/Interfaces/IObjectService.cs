using System;
using System.Collections.Generic;
using System.Text;
using uplink.NET.Models;

namespace uplink.NET.Interfaces
{
    public interface IObjectService
    {
        UploadOperation UploadObject(BucketRef bucket, string targetPath, UploadOptions uploadOptions, byte[] bytesToUpload, bool immediateStart = true);
        DownloadOperation DownloadObject(BucketRef bucket, string targetPath, bool immediateStart = true);
    }
}
