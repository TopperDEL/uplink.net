using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    /// <summary>
    /// The list of uploads found with a search operation
    /// </summary>
    public class UploadsList
    {
        /// <summary>
        /// The items within this list
        /// </summary>
        public List<UploadInfo> Items { get; set; }

        public UploadsList()
        {
            Items = new List<UploadInfo>();
        }
    }
}
