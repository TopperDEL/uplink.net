using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    /// <summary>
    /// The list of part upload found with a search operation
    /// </summary>
    public class UploadPartsList
    {
        /// <summary>
        /// The items within this list
        /// </summary>
        public List<Part> Items { get; set; }

        public UploadPartsList()
        {
            Items = new List<Part>();
        }
    }
}
