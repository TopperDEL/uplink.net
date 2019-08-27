using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
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
