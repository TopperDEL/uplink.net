using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    public class CustomMetadata
    {
        public List<CustomMetadataEntry> Entries { get; set; }

        internal static CustomMetadata FromSWIG(SWIG.CustomMetadata original)
        {
            CustomMetadata ret = new CustomMetadata();
            ret.Entries = new List<CustomMetadataEntry>();
            //ToDo: add helper-method in SWIG-Wrapper
            //foreach(var entry in original.entries)

            return ret;
        }

        internal SWIG.CustomMetadata ToSWIG()
        {
            SWIG.CustomMetadata ret = new SWIG.CustomMetadata();
            //ret.entries
            //ret.Entries = new List<CustomMetadataEntry>();
            //ToDo: add helper-method in SWIG-Wrapper
            //foreach(var entry in original.entries)

            return ret;
        }
    }
}
