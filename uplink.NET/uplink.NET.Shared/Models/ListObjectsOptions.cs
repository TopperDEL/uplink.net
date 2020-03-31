using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    /// <summary>
    /// The options to list objects within a bucket
    /// </summary>
    public class ListObjectsOptions
    {
        /// <summary>
        /// The prefix
        /// </summary>
        public string Prefix { get; set; }
        /// <summary>
        /// The cursor
        /// </summary>
        public string Cursor { get; set; }
        /// <summary>
        /// Listing should be recursive
        /// </summary>
        public bool Recursive { get; set; }
        /// <summary>
        /// Listing should be recursive
        /// </summary>
        public bool System { get; set; }
        /// <summary>
        /// Listing should be recursive
        /// </summary>
        public bool Custom { get; set; }

        internal SWIG.ListObjectsOptions ToSWIG()
        {
            SWIG.ListObjectsOptions ret = new SWIG.ListObjectsOptions();
            ret.prefix = Prefix;
            ret.cursor = Cursor;
            ret.recursive = Recursive;
            ret.system = System;
            ret.custom = Custom;

            return ret;
        }
    }
}
