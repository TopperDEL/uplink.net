using System;
using System.Collections.Generic;
using System.Text;
using uplink.Contracts;

namespace uplink
{
    public class BucketService : IBucketService
    {
        public void CloseBucket(BucketRef bucketRef)
        {
            throw new NotImplementedException();
        }

        public BucketInfo CreateBucket(Project project, string bucketName, BucketConfig bucketConfig)
        {
            //SWIG.storj_uplink.create_bucket(project.)
            throw new NotImplementedException();
        }

        public void DeleteBucket(Project project, string bucketName)
        {
            throw new NotImplementedException();
        }

        public BucketInfo GetBucketInfo(Project project, string bucketName)
        {
            throw new NotImplementedException();
        }

        public BucketList ListBuckets(Project project, BucketListOptions bucketListOptions)
        {
            throw new NotImplementedException();
        }

        public BucketRef OpenBucket(Project project, string bucketName, EncryptionAccess encryptionAccess)
        {
            throw new NotImplementedException();
        }
    }
}
