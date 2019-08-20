using System;
using System.Collections.Generic;
using System.Text;
using uplink.Net.Contracts.Models;

namespace uplink.Net.Contracts.Interfaces
{
    public interface IBucketService
    {
        IBucketInfo CreateBucket(IProject project, string bucketName, IBucketConfig bucketConfig);
        IBucketInfo GetBucketInfo(IProject project, string bucketName);
        IBucketRef OpenBucket(IProject project, string bucketName, IEncryptionAccess encryptionAccess);
        IBucketList ListBuckets(IProject project, IBucketListOptions bucketListOptions);
        void DeleteBucket(IProject project, string bucketName);
        void CloseBucket(IBucketRef bucketRef);
    }
}
