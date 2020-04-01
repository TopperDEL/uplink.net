using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    public class Permission
    {
        public bool AllowDownload { get; set; }
        public bool AllowUpload { get; set; }
        public bool AllowList { get; set; }
        public bool AllowDelete { get; set; }
        public DateTime NotBefore { get; set; }
        public DateTime NotAfter { get; set; }

        public Permission()
        {
            NotBefore = DateTime.Now;
            NotAfter = DateTime.MaxValue;
        }

        internal SWIG.Permission ToSWIG()
        {
            SWIG.Permission permission = new SWIG.Permission();
            permission.allow_download = AllowDownload;
            permission.allow_upload = AllowUpload;
            permission.allow_list = AllowList;
            permission.allow_delete = AllowDelete;
            permission.not_before = new DateTimeOffset(NotBefore).ToUnixTimeSeconds();
            permission.not_after = new DateTimeOffset(NotAfter).ToUnixTimeSeconds();

            return permission;
        }
    }
}
