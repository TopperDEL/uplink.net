using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    /// <summary>
    /// Holds information about an object within a bucket
    /// </summary>
    public class ObjectInfo
    {
        /// <summary>
        /// The version of the object
        /// </summary>
        public uint Version { get; set; }
        /// <summary>
        /// The info about the bucket where the object resides in
        /// </summary>
        public BucketInfo Bucket { get; set; }
        /// <summary>
        /// The path to this object within the bucket
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// true if it is a prefix
        /// </summary>
        public bool IsPrefix { get; set; }
        /// <summary>
        /// The content type of this object
        /// </summary>
        public string ContentType { get; set; }
        /// <summary>
        /// The size of this object
        /// </summary>
        public long Size { get; set; }
        /// <summary>
        /// The creation date
        /// </summary>
        public DateTime Created { get; set; }
        /// <summary>
        /// The last modification date
        /// </summary>
        public DateTime Modified { get; set; }
        /// <summary>
        /// The expiration date
        /// </summary>
        public DateTime Expires { get; set; }

        internal static ObjectInfo FromSWIG(SWIG.ObjectInfo original, bool disposeObjectInfo = true)
        {
            ObjectInfo ret = new ObjectInfo();
            ret.Version = original.version;
            ret.Bucket = BucketInfo.FromSWIG(original.bucket);
            ret.Path = original.path;
            ret.IsPrefix = original.is_prefix;
            ret.ContentType = original.content_type;
            ret.Size = original.size;
            ret.Created = DateTimeOffset.FromUnixTimeSeconds(original.created).ToLocalTime().DateTime;
            ret.Modified = DateTimeOffset.FromUnixTimeSeconds(original.modified).ToLocalTime().DateTime;
            ret.Expires = DateTimeOffset.FromUnixTimeSeconds(original.expires).ToLocalTime().DateTime;

            if (disposeObjectInfo)
                SWIG.storj_uplink.free_object_info(original);

            return ret;
        }
    }
}
