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
        /// Prefix allows to filter objects by a key prefix. If not empty, it must end with slash.
        /// </summary>
        public string Prefix { get; set; }
        /// <summary>
        /// Cursor sets the starting position of the iterator. The first item listed will be the one after the cursor.
        /// </summary>
        public string Cursor { get; set; }
        /// <summary>
        /// Recursive iterates the objects without collapsing prefixes.
        /// </summary>
        public bool Recursive { get; set; }
        /// <summary>
        /// System includes SystemMetadata in the results.
        /// </summary>
        public bool System { get; set; }
        /// <summary>
        /// Custom includes CustomMetadata in the results.
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
