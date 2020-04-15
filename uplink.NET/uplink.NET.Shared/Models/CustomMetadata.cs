using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace uplink.NET.Models
{
    public class CustomMetadata
    {
        private static Mutex mut = new Mutex();

        public List<CustomMetadataEntry> Entries { get; set; }

        public CustomMetadata()
        {
            Entries = new List<CustomMetadataEntry>();
        }

        internal static CustomMetadata FromSWIG(SWIG.Object obj)
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
                        var entry = SWIG.storj_uplink.get_next_custommetadata();
                        ret.Entries.Add(new CustomMetadataEntry() { Key = entry.key, Value = entry.value });
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
