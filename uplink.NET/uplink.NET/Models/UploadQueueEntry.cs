using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    public class UploadQueueEntry
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Identifier { get; set; }
        public string AccessGrant { get; set; }
        public string BucketName { get; set; }
        public string Key { get; set; }
        public byte[] Bytes { get; set; }
    }
}
