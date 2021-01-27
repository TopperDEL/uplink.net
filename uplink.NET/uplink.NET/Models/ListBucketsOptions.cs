using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    public class ListBucketsOptions
    {
        public string Cursor { get; set; }

        internal SWIG.ListBucketsOptions ToSWIG()
        {
            SWIG.ListBucketsOptions ret = new SWIG.ListBucketsOptions();
            ret.cursor = Cursor;

            return ret;
        }
    }
}
