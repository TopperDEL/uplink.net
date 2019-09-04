using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    /// <summary>
    /// Options for listing buckets
    /// </summary>
    public class BucketListOptions
    {
        /// <summary>
        /// The cursors
        /// </summary>
        public string Cursor { get; set; }
        /// <summary>
        /// The direction
        /// </summary>
        public ListDirection Direction { get; set; }
        /// <summary>
        /// Limit for the amount of search result
        /// </summary>
        public long Limit { get; set; }

        //ToDo: finish Mapping
        internal SWIG.BucketListOptions ToSWIG()
        {
            SWIG.BucketListOptions ret = new SWIG.BucketListOptions();
            ret.cursor = Cursor;
            //Finish mapping once PR is merged ret.direction = ListDirectionHelper.ToSWIG(Direction);
            ret.limit = Limit;

            return ret;
        }
    }
}
