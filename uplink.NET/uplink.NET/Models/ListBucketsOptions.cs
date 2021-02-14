using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    public class ListBucketsOptions
    {
        public string Cursor { get; set; }

        internal SWIG.UplinkListBucketsOptions ToSWIG()
        {
            SWIG.UplinkListBucketsOptions ret = new SWIG.UplinkListBucketsOptions();
            ret.cursor = Cursor;

            return ret;
        }
    }
}
