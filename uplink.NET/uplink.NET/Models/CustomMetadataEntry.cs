using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    public class CustomMetadataEntry
    {
        /// <summary>
        /// The key of the CustomMetadatEntry.
        /// When choosing a custom key for your application start it with a prefix "app:key",
        /// as an example application named "Image Board" might use a key "image-board:title".
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// The value for the key.
        /// </summary>
        public string Value { get; set; }
    }
}
