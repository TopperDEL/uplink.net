using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    /// <summary>
    /// Meta-information about an object
    /// </summary>
    public class ObjectMeta
    {
        /// <summary>
        /// The name of the bucket where the object resides in
        /// </summary>
        public string Bucket { get; set; }
        /// <summary>
        /// The path to this object within the bucket
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// true if the object is a prefix
        /// </summary>
        public bool IsPrefix { get; set; }
        /// <summary>
        /// The content type of the object
        /// </summary>
        public string ContentType { get; set; }
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
        /// <summary>
        /// The size of the object
        /// </summary>
        public ulong Size { get; set; }
        /// <summary>
        /// The checksum-bytes of the object
        /// </summary>
        public byte[] ChecksumBytes { get; set; }
        /// <summary>
        /// The checksum-length of the object
        /// </summary>
        public ulong ChecksumLength { get; set; }

        internal static ObjectMeta FromSWIG(SWIG.ObjectMeta original)
        {
            ObjectMeta ret = new ObjectMeta();
            ret.Bucket = original.bucket;
            ret.Path = original.path;
            ret.IsPrefix = original.is_prefix;
            ret.ContentType = original.content_type;
            ret.Created = DateTimeOffset.FromUnixTimeSeconds(original.created).ToLocalTime().DateTime;
            ret.Modified = DateTimeOffset.FromUnixTimeSeconds(original.modified).ToLocalTime().DateTime;
            ret.Expires = DateTimeOffset.FromUnixTimeSeconds(original.expires).ToLocalTime().DateTime;
            ret.Size = original.size;
            //Todo: ret.ChecksumBytes = 
            ret.ChecksumLength = original.checksum_length;

            SWIG.storj_uplink.free_object_meta(original);

            return ret;
        }
    }
}
