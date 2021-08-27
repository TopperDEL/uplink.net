using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace uplink.NET.Models
{
    public class CustomMetadata
    {
        private static Mutex mut = new Mutex();

        /// <summary>
        /// CustomMetadata contains custom user metadata about the object.
        /// The keys and values in custom metadata are expected to be valid UTF-8.
        /// When choosing a custom key for your application start it with a prefix "app:key",
        /// as an example application named "Image Board" might use a key "image-board:title".
        /// </summary>
        public List<CustomMetadataEntry> Entries { get; set; }

        public CustomMetadata()
        {
            Entries = new List<CustomMetadataEntry>();
        }

        internal static CustomMetadata FromSWIG(SWIG.UplinkObject obj)
        {
            CustomMetadata ret = new CustomMetadata();
            ret.Entries = new List<CustomMetadataEntry>();

            if (mut.WaitOne(1000))
            {
                try
                {
                    SWIG.storj_uplink.prepare_get_custommetadata(obj);

                    for (int i = 0; i < obj.custom.count; i++)
                    {
                        using (var entry = SWIG.storj_uplink.get_next_custommetadata())
                        {
                            ret.Entries.Add(new CustomMetadataEntry { Key = entry.key, Value = entry.value });
                        }
                    }
                }
                finally
                {
                    mut.ReleaseMutex();
                }
            }

            return ret;
        }

        internal void ToSWIG()
        {
            SWIG.storj_uplink.prepare_custommetadata();
            foreach (var entry in Entries)
                SWIG.storj_uplink.append_custommetadata(entry.Key, entry.Value);
        }
    }
}
