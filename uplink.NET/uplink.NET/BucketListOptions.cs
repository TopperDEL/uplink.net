using System;
using System.Collections.Generic;
using System.Text;

namespace uplink
{
    public class BucketListOptions
    {
        public string Cursor { get; set; }
        public int Direction { get; set; }
        public int Limit { get; set; }
    }
}
