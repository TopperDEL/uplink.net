using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    /// <summary>
    /// The handle for an opened bucket. The bucket has to be closed with this handle after use.
    /// </summary>
    public class BucketRef
    {
        internal SWIG.BucketRef _bucketRef;

        internal static BucketRef FromSWIG(SWIG.BucketRef original)
        {
            BucketRef ret = new BucketRef();
            ret._bucketRef = original;

            return ret;
        }
    }
}
