using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    public enum ListDirection
    {
        STORJ_BEFORE = -2,
        STORJ_BACKWARD = -1,
        STORJ_FORWARD = 1,
        STORJ_AFTER = 2
    }

    internal static class ListDirectionHelper
    {
        internal static ListDirection FromSWIG(SWIG.ListDirection original)
        {
            return (ListDirection)Enum.Parse(typeof(ListDirection), original.ToString());
        }

        internal static SWIG.ListDirection ToSWIG(ListDirection original)
        {
            return (SWIG.ListDirection)Enum.Parse(typeof(SWIG.ListDirection), original.ToString());
        }
    }
}
