using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    /// <summary>
    /// The handle for an opened bucket. The bucket has to be closed with this handle after use.
    /// </summary>
    public class Bucket
    {
        internal SWIG.Bucket _bucketRef;

        public string Name
        {
            get
            {
                return _bucketRef.name;
            }
        }
        
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

        internal static Bucket FromSWIG(SWIG.Bucket original)
        {
            Bucket ret = new Bucket();
            ret._bucketRef = original;

            return ret;
        }
    }
}
