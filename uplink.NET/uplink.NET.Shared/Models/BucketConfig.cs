using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    public class BucketConfig
    {
        public int PathCipher { get; set; }
        public EncryptionParameters EncryptionParameters { get; set; }
        public RedundancyScheme RedundancyScheme { get; set; }


        //ToDo: finish Mapping
        internal SWIG.BucketConfig ToSWIG()
        {
            SWIG.BucketConfig swig = new SWIG.BucketConfig();
            //swig.encryption_parameters = EncryptionParameters.ToSWIG();
            //swig.path_cipher = PathCipher;
            //swig.redundancy_scheme = RedundancyScheme.ToSWIG();
            return swig;
        }
    }
}
