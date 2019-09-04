using System;
using System.Collections.Generic;
using System.Text;

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
        public List<ObjectInfo> Items { get; set; }
        /// <summary>
        /// The amount of items within this list
        /// </summary>
        public int Length { get; set; }

        internal static ObjectList FromSWIG(SWIG.ObjectList original)
        {
            ObjectList ret = new ObjectList();
            ret.Bucket = original.bucket;
            ret.Prefix = original.prefix;
            ret.More = original.more;
            ret.Length = original.length;
            ret.Items = new List<ObjectInfo>();
            for (int i = 0; i < original.length; i++)
            {
                ret.Items.Add(ObjectInfo.FromSWIG(SWIG.storj_uplink.get_objectinfo_at(original, i), false));
            }

            //ToDo: check why it fails - SWIG.storj_uplink.free_list_objects(original);

            return ret;
        }
    }
}
