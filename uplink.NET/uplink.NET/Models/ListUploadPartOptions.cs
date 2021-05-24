using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    public class ListUploadPartsOptions
    {
        /// <summary>
        /// Cursor sets the starting position of the iterator. The first item listed will be the one after the cursor.
        /// </summary>
        public uint Cursor { get; set; }

        internal SWIG.UplinkListUploadPartsOptions ToSWIG()
        {
            SWIG.UplinkListUploadPartsOptions ret = new SWIG.UplinkListUploadPartsOptions();
            ret.cursor = Cursor;

            return ret;
        }
    }
}
