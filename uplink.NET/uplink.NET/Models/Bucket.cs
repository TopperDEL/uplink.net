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
        internal SWIG.UplinkBucket _bucketRef;
        internal SWIG.UplinkBucketResult _bucketResultRef;
        internal SWIG.UplinkProject _projectRef;

        private string _name;
        /// <summary>
        /// The name of the bucket
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        private DateTime _created;
        /// <summary>
        /// The DateTime the bucket was created
        /// </summary>
        public DateTime Created
        {
            get
            {
                return _created;
            }
        }

        private Bucket()
        {

        }

        internal static Bucket FromSWIG(SWIG.UplinkBucket original, SWIG.UplinkProject projectRef, SWIG.UplinkBucketResult bucketResult = null)
        {
            Bucket ret = new Bucket();
            ret._bucketRef = original;
            ret._bucketResultRef = bucketResult;
            ret._projectRef = projectRef;
            ret._name = original.name;
            ret._created = DateTimeOffset.FromUnixTimeSeconds(original.created).ToLocalTime().DateTime;

            return ret;
        }

        public void Dispose()
        {
            if(_bucketRef != null)
            {
                SWIG.storj_uplink.uplink_free_bucket(_bucketRef);
                _bucketRef.Dispose();
                _bucketRef = null;
            }
            if (_bucketResultRef != null)
            {
                SWIG.storj_uplink.uplink_free_bucket_result(_bucketResultRef);
                _bucketResultRef.Dispose();
                _bucketResultRef = null;
            }
            //Don't dispose the _projectRef - it might still be in use!
        }
    }
}
