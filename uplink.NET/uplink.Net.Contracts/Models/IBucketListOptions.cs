using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Contracts.Models
{
    public interface IBucketListOptions
    {
        string Cursor { get; set; }
        int Direction { get; set; }
        int Limit { get; set; }
    }
}
