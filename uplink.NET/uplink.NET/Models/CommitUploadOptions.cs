using System;
using System.Collections.Generic;
using System.Text;
using uplink.NET.Exceptions;

namespace uplink.NET.Models
{
    public class CommitUploadOptions
    {
        public CustomMetadata CustomMetadata { get; set; }

        internal void ToSWIG(SWIG.UplinkUpload upload)
        {
            if (CustomMetadata != null)
            {
                if (UploadOperation.customMetadataMutex.WaitOne(1000))
                {
                    try
                    {
                        CustomMetadata.ToSWIG(); //Appends the customMetadata in the go-layer to a global field
                        using (SWIG.UplinkError customMetadataError = SWIG.storj_uplink.upload_set_custom_metadata2(upload))
                        {
                            if (customMetadataError != null && !string.IsNullOrEmpty(customMetadataError.message))
                            {
                                throw new SetCustomMetadataFailedException(customMetadataError.message);
                            }
                        }
                    }
                    finally
                    {
                        UploadOperation.customMetadataMutex.ReleaseMutex();
                    }
                }
            }
        }
    }
}
