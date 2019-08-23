﻿using System;
using System.Collections.Generic;
using System.Text;
using uplink.NET.Models;

namespace uplink.NET.Interfaces
{
    public interface IBucketService
    {
        BucketInfo CreateBucket(Project project, string bucketName, BucketConfig bucketConfig);
        BucketInfo GetBucketInfo(Project project, string bucketName);
        BucketRef OpenBucket(Project project, string bucketName, EncryptionAccess encryptionAccess);
        BucketList ListBuckets(Project project, BucketListOptions bucketListOptions);
        void DeleteBucket(Project project, string bucketName);
        void CloseBucket(BucketRef bucketRef);
    }
}
