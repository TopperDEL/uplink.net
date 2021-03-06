﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace uplink.NET.Models
{
    /// <summary>
    /// The list of objects found with a search operation
    /// </summary>
    public class ObjectList
    {
        /// <summary>
        /// The items within this list
        /// </summary>
        public List<Object> Items { get; set; }

        public ObjectList()
        {
            Items = new List<Object>();
        }
    }
}
