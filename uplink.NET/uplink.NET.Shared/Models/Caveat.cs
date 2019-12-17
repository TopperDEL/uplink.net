using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    /// <summary>
    /// A caveat describes the actions, that are possible within a scope/restricted APIkey
    /// </summary>
    public class Caveat
    {
        /// <summary>
        /// Disallow reading of content
        /// </summary>
        public bool DisallowReads { get; set; }

        /// <summary>
        /// Disallow writing of content
        /// </summary>
        public bool DisallowWrites { get; set; }

        /// <summary>
        /// Disallow listing of content
        /// </summary>
        public bool DisallowLists { get; set; }

        /// <summary>
        /// Disallow deletion of content
        /// </summary>
        public bool DisallowDeletes { get; set; }

        internal SWIG.Caveat ToSWIG()
        {
            SWIG.Caveat ret = new SWIG.Caveat();
            ret.disallow_reads = DisallowReads;
            ret.disallow_writes = DisallowWrites;
            ret.disallow_lists = DisallowLists;
            ret.disallow_deletes = DisallowDeletes;

            return ret;
        }
    }
}
