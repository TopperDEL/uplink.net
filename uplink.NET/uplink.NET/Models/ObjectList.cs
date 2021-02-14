using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace uplink.NET.Models
{
    /// <summary>
    /// The list of objects found with a search operation
    /// </summary>
    public class ObjectList
    {
        /// <summary>
        /// The name of the bucket where the object resides in
        /// </summary>
        public string Bucket { get; set; }
        /// <summary>
        /// The prefix of this object
        /// </summary>
        public string Prefix { get; set; }
        /// <summary>
        /// true if there are more items available
        /// </summary>
        public bool More { get; set; }
        /// <summary>
        /// The items within this list
        /// </summary>
        public List<Object> Items { get; set; }

        public ObjectList()
        {
            Items = new List<Object>();
        }

        internal static ObjectList FromSWIG(SWIG.UplinkObjectIterator iterator)
        {
            ObjectList ret = new ObjectList();
            ret.Items = new List<Object>();
            
            while(SWIG.storj_uplink.uplink_object_iterator_next(iterator))
            {
                ret.Items.Add(Object.FromSWIG(SWIG.storj_uplink.uplink_object_iterator_item(iterator)));
            }

            return ret;
        }
    }
}
