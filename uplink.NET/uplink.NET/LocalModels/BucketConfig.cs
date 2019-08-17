using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.Net.LocalModels
{
    public class BucketConfig:uplink.Net.Contracts.Models.BucketConfig
    {
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
