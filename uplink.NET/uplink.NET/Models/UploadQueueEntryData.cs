using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    public class UploadQueueEntryData
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int UploadQueueEntryId { get; set; }
        public byte[] Bytes { get; set; }
    }
}
