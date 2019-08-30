using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    public class ObjectInfo
    {
        public uint Version { get; set; }
        public BucketInfo Bucket { get; set; }
        public string Path { get; set; }
        public bool IsPrefix { get; set; }
        public string ContentType { get; set; }
        public long Size { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public DateTime Expires { get; set; }

        internal static ObjectInfo FromSWIG(SWIG.ObjectInfo original)
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

            return ret;
        }
    }
}
