using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    public class ObjectMeta
    {
        public string Bucket { get; set; }
        public string Path { get; set; }
        public bool IsPrefix { get; set; }
        public string ContentType { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public DateTime Expires { get; set; }
        public ulong Size { get; set; }
        public byte[] ChecksumBytes { get; set; }
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

            return ret;
        }
    }
}
