using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Exceptions
{
    /// <summary>
    /// The object within a bucket could not be found
    /// </summary>
    public class ObjectNotFoundException : Exception
    {
        /// <summary>
        /// The name of the object that could not be found
        /// </summary>
        public string TargetPath { get; private set; }
        public ObjectNotFoundException(string targetPath): base()
        {
            TargetPath = targetPath;
        }

        public ObjectNotFoundException(string targetPath, string error) : base(error)
        {
            TargetPath = targetPath;
        }
    }
}
