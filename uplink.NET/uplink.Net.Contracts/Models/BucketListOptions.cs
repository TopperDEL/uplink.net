using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.Net.Contracts.Models
{
    public abstract class BucketListOptions
    {
        public string Cursor { get; set; }
        public int Direction { get; set; }
        public int Limit { get; set; }
    }
}
