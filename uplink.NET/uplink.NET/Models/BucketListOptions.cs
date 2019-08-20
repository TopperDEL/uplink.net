using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    public class BucketListOptions : uplink.NET.Contracts.Models.IBucketListOptions
    {
        public string Cursor { get; set; }
        public int Direction { get; set; }
        public int Limit { get; set; }

        //ToDo: finish Mapping
        internal SWIG.BucketListOptions ToSWIG()
        {
            SWIG.BucketListOptions swig = new SWIG.BucketListOptions();
            //swig.encryption_parameters = EncryptionParameters.ToSWIG();
            //swig.path_cipher = PathCipher;
            //swig.redundancy_scheme = RedundancyScheme.ToSWIG();
            return swig;
        }
    }
}
