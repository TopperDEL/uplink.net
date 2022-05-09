using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using uplink.NET.Exceptions;
using uplink.NET.Interfaces;
using uplink.NET.Models;

namespace uplink.NET.Services
{
    public class MultipartUploadService : IMultipartUploadService
    {
        static List<SWIG.UplinkListUploadPartsOptions> _listUploadPartsOptions = new List<SWIG.UplinkListUploadPartsOptions>(); //ToDo: Temporary until SWIG does not enforce IDisposable on UplinkListObjectsOptions
        static List<SWIG.UplinkListUploadsOptions> _listUploadsOptions = new List<SWIG.UplinkListUploadsOptions>(); //ToDo: Temporary until SWIG does not enforce IDisposable on UplinkListObjectsOptions
        static List<SWIG.UplinkUploadOptions> _uploadOptions = new List<SWIG.UplinkUploadOptions>();

        private readonly Access _access;

        public MultipartUploadService(Access access)
        {
            _access = access;
        }

        public async Task<UploadInfo> BeginUploadAsync(string bucketName, string objectKey, UploadOptions uploadOptions)
        {
            var uploadOptionsSWIG = uploadOptions.ToSWIG();
            _uploadOptions.Add(uploadOptionsSWIG);

            using (SWIG.UplinkUploadInfoResult uploadinfoResult = await Task.Run(() => SWIG.storj_uplink.uplink_begin_upload(_access._project, bucketName, objectKey, uploadOptionsSWIG)).ConfigureAwait(false))
            {
                if (uploadinfoResult.error != null && !string.IsNullOrEmpty(uploadinfoResult.error.message))
                    throw new MultipartUploadFailedException(objectKey, uploadinfoResult.error.message);

                return UploadInfo.FromSWIG(uploadinfoResult.info);
            }
        }

        public async Task<PartUploadResult> UploadPartAsync(string bucketName, string objectKey, string uploadId, uint partNumber, byte[] partBytes)
        {
            using (var partUploadResult = await Task.Run(() => SWIG.storj_uplink.uplink_upload_part(_access._project, bucketName, objectKey, uploadId, partNumber)).ConfigureAwait(false))
            {
                PartUploadResult result = new PartUploadResult(partUploadResult.part_upload);

                if (partUploadResult.error != null && !string.IsNullOrEmpty(partUploadResult.error.message))
                {
                    result.Error = partUploadResult.error.message;
                }
                else
                {
                    try
                    {
                        result.BytesWritten = await Task.Run(() => DoUnsafeUpload(partUploadResult.part_upload, objectKey, partBytes)).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        result.Error = ex.Message;
                    }
                }
                return result;
            }
        }

        private unsafe uint DoUnsafeUpload(SWIG.UplinkPartUpload partUpload, string objectKey, byte[] partBytes)
        {
            fixed (byte* arrayPtr = partBytes)
            {
                using (SWIG.UplinkWriteResult sentResult = SWIG.storj_uplink.uplink_part_upload_write(partUpload, new SWIG.SWIGTYPE_p_void(new IntPtr(arrayPtr), true), (uint)partBytes.Length))
                {
                    if (sentResult.error != null && !string.IsNullOrEmpty(sentResult.error.message))
                        throw new MultipartUploadFailedException(objectKey, sentResult.error.message);

                    using (var commitResult = SWIG.storj_uplink.uplink_part_upload_commit(partUpload))
                    {
                        if (commitResult != null && !string.IsNullOrEmpty(commitResult.message))
                            throw new MultipartUploadFailedException(objectKey, commitResult.message);

                        return sentResult.bytes_written;
                    }
                }
            }
        }

        public async Task UploadPartSetETagAsync(PartUpload partUpload, string eTag)
        {
            var result = await Task.Run(() => SWIG.storj_uplink.uplink_part_upload_set_etag(partUpload._partUpload, eTag)).ConfigureAwait(false);
            if (result != null && !string.IsNullOrEmpty(result.message))
                throw new SetETagFailedException(result.message);
        }

        public async Task AbortUploadAsync(string bucketName, string objectKey, string uploadId)
        {
            var result = await Task.Run(() => SWIG.storj_uplink.uplink_abort_upload(_access._project, bucketName, objectKey, uploadId)).ConfigureAwait(false);
            if (result != null && !string.IsNullOrEmpty(result.message))
                throw new SetETagFailedException(result.message);
        }

        public async Task<CommitUploadResult> CommitUploadAsync(string bucketName, string objectKey, string uploadId, CommitUploadOptions commitUploadOptions)
        {
            CommitUploadResult result = new CommitUploadResult();
            await Task.Run(() =>
            {
                UploadOperation.customMetadataSemaphore.Wait();
                try
                {
                    if (commitUploadOptions.CustomMetadata != null)
                    {
                        commitUploadOptions.CustomMetadata.ToSWIG(); //Appends the customMetadata in the go-layer to a global field
                    }

                    using (var commitUploadResult = SWIG.storj_uplink.uplink_commit_upload2(_access._project, bucketName, objectKey, uploadId))
                    {
                        if (commitUploadResult.error != null && !string.IsNullOrEmpty(commitUploadResult.error.message))
                        {
                            result.Error = commitUploadResult.error.message;
                        }
                        else
                        {
                            result.Object = Models.Object.FromSWIG(commitUploadResult.object_);
                        }
                    }
                }
                finally
                {
                    UploadOperation.customMetadataSemaphore.Release();
                }
            }).ConfigureAwait(false);
            return result;
        }

        public async Task<PartResult> GetPartUploadInfoAsync(PartUpload partUpload)
        {
            using (var uplinkPartResult = await Task.Run(() => SWIG.storj_uplink.uplink_part_upload_info(partUpload._partUpload)).ConfigureAwait(false))
            {
                var partResult = new PartResult();
                if (uplinkPartResult.error != null && !string.IsNullOrEmpty(uplinkPartResult.error.message))
                {
                    partResult.Error = uplinkPartResult.error.message;
                }
                else
                {
                    partResult.Part = Part.FromSWIG(uplinkPartResult.part);
                }

                return partResult;
            }
        }

        public async Task<UploadPartsList> ListUploadPartsAsync(string bucketName, string objectKey, string uploadId, ListUploadPartsOptions listUploadPartOptions)
        {
            var listUploadPartsOptionsSWIG = listUploadPartOptions.ToSWIG();
            _listUploadPartsOptions.Add(listUploadPartsOptionsSWIG);

            using (SWIG.UplinkPartIterator partIterator = await Task.Run(() => SWIG.storj_uplink.uplink_list_upload_parts(_access._project, bucketName, objectKey, uploadId, listUploadPartsOptionsSWIG)).ConfigureAwait(false))
            {
                using (SWIG.UplinkError error = SWIG.storj_uplink.uplink_part_iterator_err(partIterator))
                {
                    if (error != null && !string.IsNullOrEmpty(error.message))
                    {
                        throw new UploadPartsListException(error.message);
                    }
                }

                UploadPartsList uploadPartList = new UploadPartsList();

                while (SWIG.storj_uplink.uplink_part_iterator_next(partIterator))
                {
                    using (var partUploadResult = SWIG.storj_uplink.uplink_part_iterator_item(partIterator))
                    {
                        uploadPartList.Items.Add(uplink.NET.Models.Part.FromSWIG(partUploadResult));
                    }
                }
                return uploadPartList;
            }
        }

        public async Task<UploadsList> ListUploadsAsync(string bucketName, ListUploadOptions listUploadOptions)
        {
            var listUploadsOptionsSWIG = listUploadOptions.ToSWIG();
            _listUploadsOptions.Add(listUploadsOptionsSWIG);

            using (SWIG.UplinkUploadIterator uploadIterator = await Task.Run(() => SWIG.storj_uplink.uplink_list_uploads(_access._project, bucketName, listUploadsOptionsSWIG)).ConfigureAwait(false))
            {
                using (SWIG.UplinkError error = SWIG.storj_uplink.uplink_upload_iterator_err(uploadIterator))
                {
                    if (error != null && !string.IsNullOrEmpty(error.message))
                    {
                        throw new UploadsListException(error.message);
                    }
                }

                UploadsList uploadsList = new UploadsList();

                while (SWIG.storj_uplink.uplink_upload_iterator_next(uploadIterator))
                {
                    using (var uploadInfo = SWIG.storj_uplink.uplink_upload_iterator_item(uploadIterator))
                    {
                        uploadsList.Items.Add(uplink.NET.Models.UploadInfo.FromSWIG(uploadInfo));
                    }
                }
                return uploadsList;
            }
        }
    }
}
