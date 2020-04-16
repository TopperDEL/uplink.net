using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    /// <summary>
    /// The handle for a bucket.
    /// </summary>
    public class Bucket: IDisposable
    {
        internal SWIG.Bucket _bucketRef;
        internal SWIG.BucketResult _bucketResultRef;

        /// <summary>
        /// The name of the bucket
        /// </summary>
        public string Name
        {
            get
            {
                return _bucketRef.name;
            }
        }
        
        /// <summary>
        /// The DateTime the bucket was created
        /// </summary>
        public DateTime Created
        {
            get
            {
                return DateTimeOffset.FromUnixTimeSeconds(_bucketRef.created).ToLocalTime().DateTime; ;
            }
        }

        private Bucket()
        {

        }

        internal static Bucket FromSWIG(SWIG.Bucket original, SWIG.BucketResult bucketResult = null)
        {
            Bucket ret = new Bucket();
            ret._bucketRef = original;
            ret._bucketResultRef = bucketResult;

            return ret;
        }

        public void Dispose()
        {
            if(_bucketRef != null)
            {
                SWIG.storj_uplink.free_bucket(_bucketRef);
                _bucketRef.Dispose();
                _bucketRef = null;
            }
            if (_bucketResultRef != null)
            {
                SWIG.storj_uplink.free_bucket_result(_bucketResultRef);
                _bucketResultRef.Dispose();
                _bucketResultRef = null;
            }
        }
    }
}
