using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    /// <summary>
    /// The options to list objects within a bucket
    /// </summary>
    public class ListOptions
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
        /// The delimiter
        /// </summary>
        public char Delimiter { get; set; }
        /// <summary>
        /// Listing should be recursive
        /// </summary>
        public bool Recursive { get; set; }
        /// <summary>
        /// The direction to do the listing
        /// </summary>
        public ListDirection Direction { get; set; }
        /// <summary>
        /// The limit regarding the amount of search-results
        /// </summary>
        public long Limit { get; set; }

        internal SWIG.ListOptions ToSWIG()
        {
            SWIG.ListOptions ret = new SWIG.ListOptions();
            ret.prefix = Prefix;
            ret.cursor = Cursor;
            ret.delimiter = Delimiter;
            ret.recursive = Recursive;
            ret.direction = ListDirectionHelper.ToSWIG(Direction);
            ret.limit = Limit;

            return ret;
        }
    }
}
