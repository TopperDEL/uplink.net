using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    /// <summary>
    /// Holds information about an object within a bucket
    /// </summary>
    public class Object
    {
        /// <summary>
        /// The key
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// true if it is a prefix
        /// </summary>
        public bool IsPrefix { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public SystemMetadata SystemMetadata { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public CustomMetadata CustomMetadata { get; set; }

        internal static Object FromSWIG(SWIG.UplinkObject original, bool disposeObjectInfo = true)
        {
            Object ret = new Object();
            ret.Key = original.key;
            ret.IsPrefix = original.is_prefix;
            ret.SystemMetadata = SystemMetadata.FromSWIG(original.system);
            ret.CustomMetadata = CustomMetadata.FromSWIG(original);

            if (disposeObjectInfo)
                original.Dispose();

            return ret;
        }
    }
}
