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
        public string UploadId { get; set; }
        public int TotalBytes { get; set; }
        public int BytesCompleted { get; set; }
        public uint CurrentPartNumber { get; set; }
        public bool Failed { get; set; }
        public string FailedMessage { get; set; }
    }
}
