using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.Net.LocalModels
{
    public class BucketListOptions : uplink.Net.Contracts.Models.BucketListOptions
    {
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
