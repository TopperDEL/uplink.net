using System;
using System.Collections.Generic;
using System.Text;
using uplink.NET.Models;

namespace uplink.NET.Interfaces
{
    public interface IObjectService
    {
        void UploadObject(BucketRef bucket, string targetPath, UploadOptions uploadOptions);
    }
}
