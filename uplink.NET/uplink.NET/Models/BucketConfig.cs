using System;
using System.Collections.Generic;
using System.Text;
using uplink.Net.Contracts.Models;

namespace uplink.Net.Models
{
    public class BucketConfig:uplink.Net.Contracts.Models.IBucketConfig
    {
        public int PathCipher { get; set; }
        public Contracts.Models.IEncryptionParameters EncryptionParameters { get; set; }
        public Contracts.Models.IRedundancyScheme RedundancyScheme { get; set; }


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
