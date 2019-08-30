using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Exceptions
{
    public class ObjectNotFoundException : Exception
    {
        public string TargetPath { get; private set; }
        public ObjectNotFoundException(string targetPath, string error): base(error)
        {
            TargetPath = targetPath;
        }
    }
}
