using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    public class UploadInfo
    {
        /// <summary>
        /// The internal ID of this upload
        /// </summary>
        public string UploadId { get; set; }

        /// <summary>
        /// The key
        /// </summary>
        public string Key { get; set; }
        
        /// <summary>
        /// true if it is a prefix
        /// </summary>
        public bool IsPrefix { get; set; }

        /// <summary>
        /// The system metadata
        /// </summary>
        public SystemMetadata SystemMetadata { get; set; }

        /// <summary>
        /// The custom metadata
        /// </summary>
        public CustomMetadata CustomMetadata { get; set; }

        internal static UploadInfo FromSWIG(SWIG.UplinkUploadInfo original)
        {
            UploadInfo ret = new UploadInfo();
            ret.Key = original.key;
            ret.IsPrefix = original.is_prefix;
            ret.SystemMetadata = SystemMetadata.FromSWIG(original.system);
            //ret.CustomMetadata = CustomMetadata.FromSWIG(original.custom); //ToDo: support custom metadata here
            
            return ret;
        }
    }
}
