using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    public class ListOptions
    {
        public string Prefix { get; set; }
        public string Cursor { get; set; }
        public char Delimiter { get; set; }
        public bool Recursive { get; set; }
        public ListDirection Direction { get; set; }
        public long Limit { get; set; }

        internal SWIG.ListOptions ToSWIG()
        {
            SWIG.ListOptions ret = new SWIG.ListOptions();
            ret.prefix = Prefix;
            ret.cursor = Cursor;
            ret.delimiter = Delimiter;
            ret.recursive = Recursive;
            ret.direction = ListDirectionHelper.ToSWIG(Direction);
            ret.limit = Limit;

            return ret;
        }
    }
}
