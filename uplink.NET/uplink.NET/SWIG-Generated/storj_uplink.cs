//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 4.0.1
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------

namespace uplink.SWIG {

internal class storj_uplink {
  public static UplinkAccessResult uplink_parse_access(string accessString) {
    UplinkAccessResult ret = new UplinkAccessResult(storj_uplinkPINVOKE.uplink_parse_access(new storj_uplinkPINVOKE.SWIGStringMarshal(accessString).swigCPtr), true);
    return ret;
  }

  public static UplinkAccessResult uplink_request_access_with_passphrase(string satellite_address, string api_key, string passphrase) {
    UplinkAccessResult ret = new UplinkAccessResult(storj_uplinkPINVOKE.uplink_request_access_with_passphrase(new storj_uplinkPINVOKE.SWIGStringMarshal(satellite_address).swigCPtr, new storj_uplinkPINVOKE.SWIGStringMarshal(api_key).swigCPtr, new storj_uplinkPINVOKE.SWIGStringMarshal(passphrase).swigCPtr), true);
    return ret;
  }

  public static UplinkStringResult uplink_access_satellite_address(UplinkAccess access) {
    UplinkStringResult ret = new UplinkStringResult(storj_uplinkPINVOKE.uplink_access_satellite_address(UplinkAccess.getCPtr(access)), true);
    return ret;
  }

  public static UplinkStringResult uplink_access_serialize(UplinkAccess access) {
    UplinkStringResult ret = new UplinkStringResult(storj_uplinkPINVOKE.uplink_access_serialize(UplinkAccess.getCPtr(access)), true);
    return ret;
  }

  public static UplinkAccessResult uplink_access_share(UplinkAccess access, UplinkPermission permission, UplinkSharePrefix prefixes, long prefixes_count) {
    UplinkAccessResult ret = new UplinkAccessResult(storj_uplinkPINVOKE.uplink_access_share(UplinkAccess.getCPtr(access), UplinkPermission.getCPtr(permission), UplinkSharePrefix.getCPtr(prefixes), prefixes_count), true);
    if (storj_uplinkPINVOKE.SWIGPendingException.Pending) throw storj_uplinkPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static UplinkError uplink_access_override_encryption_key(UplinkAccess access, string bucket, string prefix, UplinkEncryptionKey encryptionKey) {
    global::System.IntPtr cPtr = storj_uplinkPINVOKE.uplink_access_override_encryption_key(UplinkAccess.getCPtr(access), new storj_uplinkPINVOKE.SWIGStringMarshal(bucket).swigCPtr, new storj_uplinkPINVOKE.SWIGStringMarshal(prefix).swigCPtr, UplinkEncryptionKey.getCPtr(encryptionKey));
    UplinkError ret = (cPtr == global::System.IntPtr.Zero) ? null : new UplinkError(cPtr, false);
    return ret;
  }

  public static void uplink_free_string_result(UplinkStringResult result) {
    storj_uplinkPINVOKE.uplink_free_string_result(UplinkStringResult.getCPtr(result));
    if (storj_uplinkPINVOKE.SWIGPendingException.Pending) throw storj_uplinkPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void uplink_free_access_result(UplinkAccessResult result) {
    storj_uplinkPINVOKE.uplink_free_access_result(UplinkAccessResult.getCPtr(result));
    if (storj_uplinkPINVOKE.SWIGPendingException.Pending) throw storj_uplinkPINVOKE.SWIGPendingException.Retrieve();
  }

  public static UplinkBucketResult uplink_stat_bucket(UplinkProject project, string bucket_name) {
    UplinkBucketResult ret = new UplinkBucketResult(storj_uplinkPINVOKE.uplink_stat_bucket(UplinkProject.getCPtr(project), new storj_uplinkPINVOKE.SWIGStringMarshal(bucket_name).swigCPtr), true);
    return ret;
  }

  public static UplinkBucketResult uplink_create_bucket(UplinkProject project, string bucket_name) {
    UplinkBucketResult ret = new UplinkBucketResult(storj_uplinkPINVOKE.uplink_create_bucket(UplinkProject.getCPtr(project), new storj_uplinkPINVOKE.SWIGStringMarshal(bucket_name).swigCPtr), true);
    return ret;
  }

  public static UplinkBucketResult uplink_ensure_bucket(UplinkProject project, string bucket_name) {
    UplinkBucketResult ret = new UplinkBucketResult(storj_uplinkPINVOKE.uplink_ensure_bucket(UplinkProject.getCPtr(project), new storj_uplinkPINVOKE.SWIGStringMarshal(bucket_name).swigCPtr), true);
    return ret;
  }

  public static UplinkBucketResult uplink_delete_bucket(UplinkProject project, string bucket_name) {
    UplinkBucketResult ret = new UplinkBucketResult(storj_uplinkPINVOKE.uplink_delete_bucket(UplinkProject.getCPtr(project), new storj_uplinkPINVOKE.SWIGStringMarshal(bucket_name).swigCPtr), true);
    return ret;
  }

  public static UplinkBucketResult uplink_delete_bucket_with_objects(UplinkProject project, string bucket_name) {
    UplinkBucketResult ret = new UplinkBucketResult(storj_uplinkPINVOKE.uplink_delete_bucket_with_objects(UplinkProject.getCPtr(project), new storj_uplinkPINVOKE.SWIGStringMarshal(bucket_name).swigCPtr), true);
    return ret;
  }

  public static void uplink_free_bucket_result(UplinkBucketResult result) {
    storj_uplinkPINVOKE.uplink_free_bucket_result(UplinkBucketResult.getCPtr(result));
    if (storj_uplinkPINVOKE.SWIGPendingException.Pending) throw storj_uplinkPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void uplink_free_bucket(UplinkBucket bucket) {
    storj_uplinkPINVOKE.uplink_free_bucket(UplinkBucket.getCPtr(bucket));
  }

  public static UplinkBucketIterator uplink_list_buckets(UplinkProject project, UplinkListBucketsOptions options) {
    global::System.IntPtr cPtr = storj_uplinkPINVOKE.uplink_list_buckets(UplinkProject.getCPtr(project), UplinkListBucketsOptions.getCPtr(options));
    UplinkBucketIterator ret = (cPtr == global::System.IntPtr.Zero) ? null : new UplinkBucketIterator(cPtr, false);
    return ret;
  }

  public static bool uplink_bucket_iterator_next(UplinkBucketIterator iterator) {
    bool ret = storj_uplinkPINVOKE.uplink_bucket_iterator_next(UplinkBucketIterator.getCPtr(iterator));
    return ret;
  }

  public static UplinkError uplink_bucket_iterator_err(UplinkBucketIterator iterator) {
    global::System.IntPtr cPtr = storj_uplinkPINVOKE.uplink_bucket_iterator_err(UplinkBucketIterator.getCPtr(iterator));
    UplinkError ret = (cPtr == global::System.IntPtr.Zero) ? null : new UplinkError(cPtr, false);
    return ret;
  }

  public static UplinkBucket uplink_bucket_iterator_item(UplinkBucketIterator iterator) {
    global::System.IntPtr cPtr = storj_uplinkPINVOKE.uplink_bucket_iterator_item(UplinkBucketIterator.getCPtr(iterator));
    UplinkBucket ret = (cPtr == global::System.IntPtr.Zero) ? null : new UplinkBucket(cPtr, false);
    return ret;
  }

  public static void uplink_free_bucket_iterator(UplinkBucketIterator iterator) {
    storj_uplinkPINVOKE.uplink_free_bucket_iterator(UplinkBucketIterator.getCPtr(iterator));
  }

  public static UplinkAccessResult uplink_config_request_access_with_passphrase(UplinkConfig config, string satellite_address, string api_key, string passphrase) {
    UplinkAccessResult ret = new UplinkAccessResult(storj_uplinkPINVOKE.uplink_config_request_access_with_passphrase(UplinkConfig.getCPtr(config), new storj_uplinkPINVOKE.SWIGStringMarshal(satellite_address).swigCPtr, new storj_uplinkPINVOKE.SWIGStringMarshal(api_key).swigCPtr, new storj_uplinkPINVOKE.SWIGStringMarshal(passphrase).swigCPtr), true);
    if (storj_uplinkPINVOKE.SWIGPendingException.Pending) throw storj_uplinkPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static UplinkProjectResult uplink_config_open_project(UplinkConfig config, UplinkAccess access) {
    UplinkProjectResult ret = new UplinkProjectResult(storj_uplinkPINVOKE.uplink_config_open_project(UplinkConfig.getCPtr(config), UplinkAccess.getCPtr(access)), true);
    if (storj_uplinkPINVOKE.SWIGPendingException.Pending) throw storj_uplinkPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static UplinkObjectResult uplink_copy_object(UplinkProject project, string old_bucket_name, string old_object_key, string new_bucket_name, string new_object_key, UplinkCopyObjectOptions options) {
    UplinkObjectResult ret = new UplinkObjectResult(storj_uplinkPINVOKE.uplink_copy_object(UplinkProject.getCPtr(project), new storj_uplinkPINVOKE.SWIGStringMarshal(old_bucket_name).swigCPtr, new storj_uplinkPINVOKE.SWIGStringMarshal(old_object_key).swigCPtr, new storj_uplinkPINVOKE.SWIGStringMarshal(new_bucket_name).swigCPtr, new storj_uplinkPINVOKE.SWIGStringMarshal(new_object_key).swigCPtr, UplinkCopyObjectOptions.getCPtr(options)), true);
    return ret;
  }

  public static void prepare_custommetadata() {
    storj_uplinkPINVOKE.prepare_custommetadata();
  }

  public static void append_custommetadata(string key, string value) {
    storj_uplinkPINVOKE.append_custommetadata(new storj_uplinkPINVOKE.SWIGStringMarshal(key).swigCPtr, new storj_uplinkPINVOKE.SWIGStringMarshal(value).swigCPtr);
  }

  public static UplinkError upload_set_custom_metadata2(UplinkUpload upload) {
    global::System.IntPtr cPtr = storj_uplinkPINVOKE.upload_set_custom_metadata2(UplinkUpload.getCPtr(upload));
    UplinkError ret = (cPtr == global::System.IntPtr.Zero) ? null : new UplinkError(cPtr, false);
    return ret;
  }

  public static void prepare_get_custommetadata(UplinkObject object_) {
    storj_uplinkPINVOKE.prepare_get_custommetadata(UplinkObject.getCPtr(object_));
  }

  public static UplinkCustomMetadataEntry get_next_custommetadata() {
    UplinkCustomMetadataEntry ret = new UplinkCustomMetadataEntry(storj_uplinkPINVOKE.get_next_custommetadata(), true);
    return ret;
  }

  public static UplinkCommitUploadResult uplink_commit_upload2(UplinkProject project, string bucket_name, string object_key, string upload_id) {
    UplinkCommitUploadResult ret = new UplinkCommitUploadResult(storj_uplinkPINVOKE.uplink_commit_upload2(UplinkProject.getCPtr(project), new storj_uplinkPINVOKE.SWIGStringMarshal(bucket_name).swigCPtr, new storj_uplinkPINVOKE.SWIGStringMarshal(object_key).swigCPtr, new storj_uplinkPINVOKE.SWIGStringMarshal(upload_id).swigCPtr), true);
    return ret;
  }

  public static UplinkError uplink_update_object_metadata2(UplinkProject project, string bucket_name, string object_key, UplinkUploadObjectMetadataOptions options) {
    global::System.IntPtr cPtr = storj_uplinkPINVOKE.uplink_update_object_metadata2(UplinkProject.getCPtr(project), new storj_uplinkPINVOKE.SWIGStringMarshal(bucket_name).swigCPtr, new storj_uplinkPINVOKE.SWIGStringMarshal(object_key).swigCPtr, UplinkUploadObjectMetadataOptions.getCPtr(options));
    UplinkError ret = (cPtr == global::System.IntPtr.Zero) ? null : new UplinkError(cPtr, false);
    return ret;
  }

  public static UplinkDownloadResult uplink_download_object(UplinkProject project, string bucket_name, string object_key, UplinkDownloadOptions options) {
    UplinkDownloadResult ret = new UplinkDownloadResult(storj_uplinkPINVOKE.uplink_download_object(UplinkProject.getCPtr(project), new storj_uplinkPINVOKE.SWIGStringMarshal(bucket_name).swigCPtr, new storj_uplinkPINVOKE.SWIGStringMarshal(object_key).swigCPtr, UplinkDownloadOptions.getCPtr(options)), true);
    return ret;
  }

  public static UplinkReadResult uplink_download_read(UplinkDownload download, SWIGTYPE_p_void bytes, uint length) {
    UplinkReadResult ret = new UplinkReadResult(storj_uplinkPINVOKE.uplink_download_read(UplinkDownload.getCPtr(download), SWIGTYPE_p_void.getCPtr(bytes), length), true);
    return ret;
  }

  public static UplinkObjectResult uplink_download_info(UplinkDownload download) {
    UplinkObjectResult ret = new UplinkObjectResult(storj_uplinkPINVOKE.uplink_download_info(UplinkDownload.getCPtr(download)), true);
    return ret;
  }

  public static void uplink_free_read_result(UplinkReadResult result) {
    storj_uplinkPINVOKE.uplink_free_read_result(UplinkReadResult.getCPtr(result));
    if (storj_uplinkPINVOKE.SWIGPendingException.Pending) throw storj_uplinkPINVOKE.SWIGPendingException.Retrieve();
  }

  public static UplinkError uplink_close_download(UplinkDownload download) {
    global::System.IntPtr cPtr = storj_uplinkPINVOKE.uplink_close_download(UplinkDownload.getCPtr(download));
    UplinkError ret = (cPtr == global::System.IntPtr.Zero) ? null : new UplinkError(cPtr, false);
    return ret;
  }

  public static void uplink_free_download_result(UplinkDownloadResult result) {
    storj_uplinkPINVOKE.uplink_free_download_result(UplinkDownloadResult.getCPtr(result));
    if (storj_uplinkPINVOKE.SWIGPendingException.Pending) throw storj_uplinkPINVOKE.SWIGPendingException.Retrieve();
  }

  public static EdgeCredentialsResult edge_register_access(EdgeConfig config, UplinkAccess access, EdgeRegisterAccessOptions options) {
    EdgeCredentialsResult ret = new EdgeCredentialsResult(storj_uplinkPINVOKE.edge_register_access(EdgeConfig.getCPtr(config), UplinkAccess.getCPtr(access), EdgeRegisterAccessOptions.getCPtr(options)), true);
    if (storj_uplinkPINVOKE.SWIGPendingException.Pending) throw storj_uplinkPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static void edge_free_credentials_result(EdgeCredentialsResult result) {
    storj_uplinkPINVOKE.edge_free_credentials_result(EdgeCredentialsResult.getCPtr(result));
    if (storj_uplinkPINVOKE.SWIGPendingException.Pending) throw storj_uplinkPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void edge_free_credentials(EdgeCredentials credentials) {
    storj_uplinkPINVOKE.edge_free_credentials(EdgeCredentials.getCPtr(credentials));
  }

  public static UplinkStringResult edge_join_share_url(string baseURL, string accessKeyID, string bucket, string key, EdgeShareURLOptions options) {
    UplinkStringResult ret = new UplinkStringResult(storj_uplinkPINVOKE.edge_join_share_url(new storj_uplinkPINVOKE.SWIGStringMarshal(baseURL).swigCPtr, new storj_uplinkPINVOKE.SWIGStringMarshal(accessKeyID).swigCPtr, new storj_uplinkPINVOKE.SWIGStringMarshal(bucket).swigCPtr, new storj_uplinkPINVOKE.SWIGStringMarshal(key).swigCPtr, EdgeShareURLOptions.getCPtr(options)), true);
    return ret;
  }

  public static UplinkEncryptionKeyResult uplink_derive_encryption_key(string passphrase, SWIGTYPE_p_void salt, uint length) {
    UplinkEncryptionKeyResult ret = new UplinkEncryptionKeyResult(storj_uplinkPINVOKE.uplink_derive_encryption_key(new storj_uplinkPINVOKE.SWIGStringMarshal(passphrase).swigCPtr, SWIGTYPE_p_void.getCPtr(salt), length), true);
    return ret;
  }

  public static void uplink_free_encryption_key_result(UplinkEncryptionKeyResult result) {
    storj_uplinkPINVOKE.uplink_free_encryption_key_result(UplinkEncryptionKeyResult.getCPtr(result));
    if (storj_uplinkPINVOKE.SWIGPendingException.Pending) throw storj_uplinkPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void uplink_free_error(UplinkError err) {
    storj_uplinkPINVOKE.uplink_free_error(UplinkError.getCPtr(err));
  }

  public static byte uplink_internal_UniverseIsEmpty() {
    byte ret = storj_uplinkPINVOKE.uplink_internal_UniverseIsEmpty();
    return ret;
  }

  public static UplinkError uplink_move_object(UplinkProject project, string old_bucket_name, string old_object_key, string new_bucket_name, string new_object_key, UplinkMoveObjectOptions options) {
    global::System.IntPtr cPtr = storj_uplinkPINVOKE.uplink_move_object(UplinkProject.getCPtr(project), new storj_uplinkPINVOKE.SWIGStringMarshal(old_bucket_name).swigCPtr, new storj_uplinkPINVOKE.SWIGStringMarshal(old_object_key).swigCPtr, new storj_uplinkPINVOKE.SWIGStringMarshal(new_bucket_name).swigCPtr, new storj_uplinkPINVOKE.SWIGStringMarshal(new_object_key).swigCPtr, UplinkMoveObjectOptions.getCPtr(options));
    UplinkError ret = (cPtr == global::System.IntPtr.Zero) ? null : new UplinkError(cPtr, false);
    return ret;
  }

  public static UplinkUploadInfoResult uplink_begin_upload(UplinkProject project, string bucket_name, string object_key, UplinkUploadOptions options) {
    UplinkUploadInfoResult ret = new UplinkUploadInfoResult(storj_uplinkPINVOKE.uplink_begin_upload(UplinkProject.getCPtr(project), new storj_uplinkPINVOKE.SWIGStringMarshal(bucket_name).swigCPtr, new storj_uplinkPINVOKE.SWIGStringMarshal(object_key).swigCPtr, UplinkUploadOptions.getCPtr(options)), true);
    return ret;
  }

  public static void uplink_free_upload_info_result(UplinkUploadInfoResult result) {
    storj_uplinkPINVOKE.uplink_free_upload_info_result(UplinkUploadInfoResult.getCPtr(result));
    if (storj_uplinkPINVOKE.SWIGPendingException.Pending) throw storj_uplinkPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void uplink_free_upload_info(UplinkUploadInfo info) {
    storj_uplinkPINVOKE.uplink_free_upload_info(UplinkUploadInfo.getCPtr(info));
  }

  public static UplinkCommitUploadResult uplink_commit_upload(UplinkProject project, string bucket_name, string object_key, string upload_id, UplinkCommitUploadOptions options) {
    UplinkCommitUploadResult ret = new UplinkCommitUploadResult(storj_uplinkPINVOKE.uplink_commit_upload(UplinkProject.getCPtr(project), new storj_uplinkPINVOKE.SWIGStringMarshal(bucket_name).swigCPtr, new storj_uplinkPINVOKE.SWIGStringMarshal(object_key).swigCPtr, new storj_uplinkPINVOKE.SWIGStringMarshal(upload_id).swigCPtr, UplinkCommitUploadOptions.getCPtr(options)), true);
    return ret;
  }

  public static void uplink_free_commit_upload_result(UplinkCommitUploadResult result) {
    storj_uplinkPINVOKE.uplink_free_commit_upload_result(UplinkCommitUploadResult.getCPtr(result));
    if (storj_uplinkPINVOKE.SWIGPendingException.Pending) throw storj_uplinkPINVOKE.SWIGPendingException.Retrieve();
  }

  public static UplinkError uplink_abort_upload(UplinkProject project, string bucket_name, string object_key, string upload_id) {
    global::System.IntPtr cPtr = storj_uplinkPINVOKE.uplink_abort_upload(UplinkProject.getCPtr(project), new storj_uplinkPINVOKE.SWIGStringMarshal(bucket_name).swigCPtr, new storj_uplinkPINVOKE.SWIGStringMarshal(object_key).swigCPtr, new storj_uplinkPINVOKE.SWIGStringMarshal(upload_id).swigCPtr);
    UplinkError ret = (cPtr == global::System.IntPtr.Zero) ? null : new UplinkError(cPtr, false);
    return ret;
  }

  public static UplinkPartUploadResult uplink_upload_part(UplinkProject project, string bucket_name, string object_key, string upload_id, uint part_number) {
    UplinkPartUploadResult ret = new UplinkPartUploadResult(storj_uplinkPINVOKE.uplink_upload_part(UplinkProject.getCPtr(project), new storj_uplinkPINVOKE.SWIGStringMarshal(bucket_name).swigCPtr, new storj_uplinkPINVOKE.SWIGStringMarshal(object_key).swigCPtr, new storj_uplinkPINVOKE.SWIGStringMarshal(upload_id).swigCPtr, part_number), true);
    return ret;
  }

  public static UplinkWriteResult uplink_part_upload_write(UplinkPartUpload upload, SWIGTYPE_p_void bytes, uint length) {
    UplinkWriteResult ret = new UplinkWriteResult(storj_uplinkPINVOKE.uplink_part_upload_write(UplinkPartUpload.getCPtr(upload), SWIGTYPE_p_void.getCPtr(bytes), length), true);
    return ret;
  }

  public static UplinkError uplink_part_upload_commit(UplinkPartUpload upload) {
    global::System.IntPtr cPtr = storj_uplinkPINVOKE.uplink_part_upload_commit(UplinkPartUpload.getCPtr(upload));
    UplinkError ret = (cPtr == global::System.IntPtr.Zero) ? null : new UplinkError(cPtr, false);
    return ret;
  }

  public static UplinkError uplink_part_upload_abort(UplinkPartUpload upload) {
    global::System.IntPtr cPtr = storj_uplinkPINVOKE.uplink_part_upload_abort(UplinkPartUpload.getCPtr(upload));
    UplinkError ret = (cPtr == global::System.IntPtr.Zero) ? null : new UplinkError(cPtr, false);
    return ret;
  }

  public static UplinkError uplink_part_upload_set_etag(UplinkPartUpload upload, string etag) {
    global::System.IntPtr cPtr = storj_uplinkPINVOKE.uplink_part_upload_set_etag(UplinkPartUpload.getCPtr(upload), new storj_uplinkPINVOKE.SWIGStringMarshal(etag).swigCPtr);
    UplinkError ret = (cPtr == global::System.IntPtr.Zero) ? null : new UplinkError(cPtr, false);
    return ret;
  }

  public static UplinkPartResult uplink_part_upload_info(UplinkPartUpload upload) {
    UplinkPartResult ret = new UplinkPartResult(storj_uplinkPINVOKE.uplink_part_upload_info(UplinkPartUpload.getCPtr(upload)), true);
    return ret;
  }

  public static void uplink_free_part_result(UplinkPartResult result) {
    storj_uplinkPINVOKE.uplink_free_part_result(UplinkPartResult.getCPtr(result));
    if (storj_uplinkPINVOKE.SWIGPendingException.Pending) throw storj_uplinkPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void uplink_free_part_upload_result(UplinkPartUploadResult result) {
    storj_uplinkPINVOKE.uplink_free_part_upload_result(UplinkPartUploadResult.getCPtr(result));
    if (storj_uplinkPINVOKE.SWIGPendingException.Pending) throw storj_uplinkPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void uplink_free_part(UplinkPart part) {
    storj_uplinkPINVOKE.uplink_free_part(UplinkPart.getCPtr(part));
  }

  public static UplinkUploadIterator uplink_list_uploads(UplinkProject project, string bucket_name, UplinkListUploadsOptions options) {
    global::System.IntPtr cPtr = storj_uplinkPINVOKE.uplink_list_uploads(UplinkProject.getCPtr(project), new storj_uplinkPINVOKE.SWIGStringMarshal(bucket_name).swigCPtr, UplinkListUploadsOptions.getCPtr(options));
    UplinkUploadIterator ret = (cPtr == global::System.IntPtr.Zero) ? null : new UplinkUploadIterator(cPtr, false);
    return ret;
  }

  public static bool uplink_upload_iterator_next(UplinkUploadIterator iterator) {
    bool ret = storj_uplinkPINVOKE.uplink_upload_iterator_next(UplinkUploadIterator.getCPtr(iterator));
    return ret;
  }

  public static UplinkError uplink_upload_iterator_err(UplinkUploadIterator iterator) {
    global::System.IntPtr cPtr = storj_uplinkPINVOKE.uplink_upload_iterator_err(UplinkUploadIterator.getCPtr(iterator));
    UplinkError ret = (cPtr == global::System.IntPtr.Zero) ? null : new UplinkError(cPtr, false);
    return ret;
  }

  public static UplinkUploadInfo uplink_upload_iterator_item(UplinkUploadIterator iterator) {
    global::System.IntPtr cPtr = storj_uplinkPINVOKE.uplink_upload_iterator_item(UplinkUploadIterator.getCPtr(iterator));
    UplinkUploadInfo ret = (cPtr == global::System.IntPtr.Zero) ? null : new UplinkUploadInfo(cPtr, false);
    return ret;
  }

  public static void uplink_free_upload_iterator(UplinkUploadIterator iterator) {
    storj_uplinkPINVOKE.uplink_free_upload_iterator(UplinkUploadIterator.getCPtr(iterator));
  }

  public static UplinkPartIterator uplink_list_upload_parts(UplinkProject project, string bucket_name, string object_key, string upload_id, UplinkListUploadPartsOptions options) {
    global::System.IntPtr cPtr = storj_uplinkPINVOKE.uplink_list_upload_parts(UplinkProject.getCPtr(project), new storj_uplinkPINVOKE.SWIGStringMarshal(bucket_name).swigCPtr, new storj_uplinkPINVOKE.SWIGStringMarshal(object_key).swigCPtr, new storj_uplinkPINVOKE.SWIGStringMarshal(upload_id).swigCPtr, UplinkListUploadPartsOptions.getCPtr(options));
    UplinkPartIterator ret = (cPtr == global::System.IntPtr.Zero) ? null : new UplinkPartIterator(cPtr, false);
    return ret;
  }

  public static bool uplink_part_iterator_next(UplinkPartIterator iterator) {
    bool ret = storj_uplinkPINVOKE.uplink_part_iterator_next(UplinkPartIterator.getCPtr(iterator));
    return ret;
  }

  public static UplinkError uplink_part_iterator_err(UplinkPartIterator iterator) {
    global::System.IntPtr cPtr = storj_uplinkPINVOKE.uplink_part_iterator_err(UplinkPartIterator.getCPtr(iterator));
    UplinkError ret = (cPtr == global::System.IntPtr.Zero) ? null : new UplinkError(cPtr, false);
    return ret;
  }

  public static UplinkPart uplink_part_iterator_item(UplinkPartIterator iterator) {
    global::System.IntPtr cPtr = storj_uplinkPINVOKE.uplink_part_iterator_item(UplinkPartIterator.getCPtr(iterator));
    UplinkPart ret = (cPtr == global::System.IntPtr.Zero) ? null : new UplinkPart(cPtr, false);
    return ret;
  }

  public static void uplink_free_part_iterator(UplinkPartIterator iterator) {
    storj_uplinkPINVOKE.uplink_free_part_iterator(UplinkPartIterator.getCPtr(iterator));
  }

  public static UplinkObjectResult uplink_stat_object(UplinkProject project, string bucket_name, string object_key) {
    UplinkObjectResult ret = new UplinkObjectResult(storj_uplinkPINVOKE.uplink_stat_object(UplinkProject.getCPtr(project), new storj_uplinkPINVOKE.SWIGStringMarshal(bucket_name).swigCPtr, new storj_uplinkPINVOKE.SWIGStringMarshal(object_key).swigCPtr), true);
    return ret;
  }

  public static UplinkObjectResult uplink_delete_object(UplinkProject project, string bucket_name, string object_key) {
    UplinkObjectResult ret = new UplinkObjectResult(storj_uplinkPINVOKE.uplink_delete_object(UplinkProject.getCPtr(project), new storj_uplinkPINVOKE.SWIGStringMarshal(bucket_name).swigCPtr, new storj_uplinkPINVOKE.SWIGStringMarshal(object_key).swigCPtr), true);
    return ret;
  }

  public static void uplink_free_object_result(UplinkObjectResult obj) {
    storj_uplinkPINVOKE.uplink_free_object_result(UplinkObjectResult.getCPtr(obj));
    if (storj_uplinkPINVOKE.SWIGPendingException.Pending) throw storj_uplinkPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void uplink_free_object(UplinkObject obj) {
    storj_uplinkPINVOKE.uplink_free_object(UplinkObject.getCPtr(obj));
  }

  public static UplinkError uplink_update_object_metadata(UplinkProject project, string bucket_name, string object_key, UplinkCustomMetadata new_metadata, UplinkUploadObjectMetadataOptions options) {
    global::System.IntPtr cPtr = storj_uplinkPINVOKE.uplink_update_object_metadata(UplinkProject.getCPtr(project), new storj_uplinkPINVOKE.SWIGStringMarshal(bucket_name).swigCPtr, new storj_uplinkPINVOKE.SWIGStringMarshal(object_key).swigCPtr, UplinkCustomMetadata.getCPtr(new_metadata), UplinkUploadObjectMetadataOptions.getCPtr(options));
    UplinkError ret = (cPtr == global::System.IntPtr.Zero) ? null : new UplinkError(cPtr, false);
    if (storj_uplinkPINVOKE.SWIGPendingException.Pending) throw storj_uplinkPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static UplinkObjectIterator uplink_list_objects(UplinkProject project, string bucket_name, UplinkListObjectsOptions options) {
    global::System.IntPtr cPtr = storj_uplinkPINVOKE.uplink_list_objects(UplinkProject.getCPtr(project), new storj_uplinkPINVOKE.SWIGStringMarshal(bucket_name).swigCPtr, UplinkListObjectsOptions.getCPtr(options));
    UplinkObjectIterator ret = (cPtr == global::System.IntPtr.Zero) ? null : new UplinkObjectIterator(cPtr, false);
    return ret;
  }

  public static bool uplink_object_iterator_next(UplinkObjectIterator iterator) {
    bool ret = storj_uplinkPINVOKE.uplink_object_iterator_next(UplinkObjectIterator.getCPtr(iterator));
    return ret;
  }

  public static UplinkError uplink_object_iterator_err(UplinkObjectIterator iterator) {
    global::System.IntPtr cPtr = storj_uplinkPINVOKE.uplink_object_iterator_err(UplinkObjectIterator.getCPtr(iterator));
    UplinkError ret = (cPtr == global::System.IntPtr.Zero) ? null : new UplinkError(cPtr, false);
    return ret;
  }

  public static UplinkObject uplink_object_iterator_item(UplinkObjectIterator iterator) {
    global::System.IntPtr cPtr = storj_uplinkPINVOKE.uplink_object_iterator_item(UplinkObjectIterator.getCPtr(iterator));
    UplinkObject ret = (cPtr == global::System.IntPtr.Zero) ? null : new UplinkObject(cPtr, false);
    return ret;
  }

  public static void uplink_free_object_iterator(UplinkObjectIterator iterator) {
    storj_uplinkPINVOKE.uplink_free_object_iterator(UplinkObjectIterator.getCPtr(iterator));
  }

  public static UplinkProjectResult uplink_open_project(UplinkAccess access) {
    UplinkProjectResult ret = new UplinkProjectResult(storj_uplinkPINVOKE.uplink_open_project(UplinkAccess.getCPtr(access)), true);
    return ret;
  }

  public static UplinkError uplink_close_project(UplinkProject project) {
    global::System.IntPtr cPtr = storj_uplinkPINVOKE.uplink_close_project(UplinkProject.getCPtr(project));
    UplinkError ret = (cPtr == global::System.IntPtr.Zero) ? null : new UplinkError(cPtr, false);
    return ret;
  }

  public static UplinkError uplink_revoke_access(UplinkProject project, UplinkAccess access) {
    global::System.IntPtr cPtr = storj_uplinkPINVOKE.uplink_revoke_access(UplinkProject.getCPtr(project), UplinkAccess.getCPtr(access));
    UplinkError ret = (cPtr == global::System.IntPtr.Zero) ? null : new UplinkError(cPtr, false);
    return ret;
  }

  public static void uplink_free_project_result(UplinkProjectResult result) {
    storj_uplinkPINVOKE.uplink_free_project_result(UplinkProjectResult.getCPtr(result));
    if (storj_uplinkPINVOKE.SWIGPendingException.Pending) throw storj_uplinkPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void prepare_shareprefixes(uint shareprefixesLen) {
    storj_uplinkPINVOKE.prepare_shareprefixes(shareprefixesLen);
  }

  public static void append_shareprefix(string bucket, string prefix) {
    storj_uplinkPINVOKE.append_shareprefix(new storj_uplinkPINVOKE.SWIGStringMarshal(bucket).swigCPtr, new storj_uplinkPINVOKE.SWIGStringMarshal(prefix).swigCPtr);
  }

  public static UplinkAccessResult access_share2(UplinkAccess access, UplinkPermission permission) {
    UplinkAccessResult ret = new UplinkAccessResult(storj_uplinkPINVOKE.access_share2(UplinkAccess.getCPtr(access), UplinkPermission.getCPtr(permission)), true);
    if (storj_uplinkPINVOKE.SWIGPendingException.Pending) throw storj_uplinkPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static UplinkUploadResult uplink_upload_object(UplinkProject project, string bucket_name, string object_key, UplinkUploadOptions options) {
    UplinkUploadResult ret = new UplinkUploadResult(storj_uplinkPINVOKE.uplink_upload_object(UplinkProject.getCPtr(project), new storj_uplinkPINVOKE.SWIGStringMarshal(bucket_name).swigCPtr, new storj_uplinkPINVOKE.SWIGStringMarshal(object_key).swigCPtr, UplinkUploadOptions.getCPtr(options)), true);
    return ret;
  }

  public static UplinkWriteResult uplink_upload_write(UplinkUpload upload, SWIGTYPE_p_void bytes, uint length) {
    UplinkWriteResult ret = new UplinkWriteResult(storj_uplinkPINVOKE.uplink_upload_write(UplinkUpload.getCPtr(upload), SWIGTYPE_p_void.getCPtr(bytes), length), true);
    return ret;
  }

  public static UplinkError uplink_upload_commit(UplinkUpload upload) {
    global::System.IntPtr cPtr = storj_uplinkPINVOKE.uplink_upload_commit(UplinkUpload.getCPtr(upload));
    UplinkError ret = (cPtr == global::System.IntPtr.Zero) ? null : new UplinkError(cPtr, false);
    return ret;
  }

  public static UplinkError uplink_upload_abort(UplinkUpload upload) {
    global::System.IntPtr cPtr = storj_uplinkPINVOKE.uplink_upload_abort(UplinkUpload.getCPtr(upload));
    UplinkError ret = (cPtr == global::System.IntPtr.Zero) ? null : new UplinkError(cPtr, false);
    return ret;
  }

  public static UplinkObjectResult uplink_upload_info(UplinkUpload upload) {
    UplinkObjectResult ret = new UplinkObjectResult(storj_uplinkPINVOKE.uplink_upload_info(UplinkUpload.getCPtr(upload)), true);
    return ret;
  }

  public static UplinkError uplink_upload_set_custom_metadata(UplinkUpload upload, UplinkCustomMetadata custom) {
    global::System.IntPtr cPtr = storj_uplinkPINVOKE.uplink_upload_set_custom_metadata(UplinkUpload.getCPtr(upload), UplinkCustomMetadata.getCPtr(custom));
    UplinkError ret = (cPtr == global::System.IntPtr.Zero) ? null : new UplinkError(cPtr, false);
    if (storj_uplinkPINVOKE.SWIGPendingException.Pending) throw storj_uplinkPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static void uplink_free_write_result(UplinkWriteResult result) {
    storj_uplinkPINVOKE.uplink_free_write_result(UplinkWriteResult.getCPtr(result));
    if (storj_uplinkPINVOKE.SWIGPendingException.Pending) throw storj_uplinkPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void uplink_free_upload_result(UplinkUploadResult result) {
    storj_uplinkPINVOKE.uplink_free_upload_result(UplinkUploadResult.getCPtr(result));
    if (storj_uplinkPINVOKE.SWIGPendingException.Pending) throw storj_uplinkPINVOKE.SWIGPendingException.Retrieve();
  }

  public static string get_storj_version() {
    string ret = storj_uplinkPINVOKE.SWIGStringMarshal.StringFromNativeUtf8(storj_uplinkPINVOKE.get_storj_version());
    return ret;
  }

  public static string get_go_version() {
    string ret = storj_uplinkPINVOKE.SWIGStringMarshal.StringFromNativeUtf8(storj_uplinkPINVOKE.get_go_version());
    return ret;
  }

  public static readonly int UPLINK_ERROR_INTERNAL = storj_uplinkPINVOKE.UPLINK_ERROR_INTERNAL_get();
  public static readonly int UPLINK_ERROR_CANCELED = storj_uplinkPINVOKE.UPLINK_ERROR_CANCELED_get();
  public static readonly int UPLINK_ERROR_INVALID_HANDLE = storj_uplinkPINVOKE.UPLINK_ERROR_INVALID_HANDLE_get();
  public static readonly int UPLINK_ERROR_TOO_MANY_REQUESTS = storj_uplinkPINVOKE.UPLINK_ERROR_TOO_MANY_REQUESTS_get();
  public static readonly int UPLINK_ERROR_BANDWIDTH_LIMIT_EXCEEDED = storj_uplinkPINVOKE.UPLINK_ERROR_BANDWIDTH_LIMIT_EXCEEDED_get();
  public static readonly int UPLINK_ERROR_STORAGE_LIMIT_EXCEEDED = storj_uplinkPINVOKE.UPLINK_ERROR_STORAGE_LIMIT_EXCEEDED_get();
  public static readonly int UPLINK_ERROR_SEGMENTS_LIMIT_EXCEEDED = storj_uplinkPINVOKE.UPLINK_ERROR_SEGMENTS_LIMIT_EXCEEDED_get();
  public static readonly int UPLINK_ERROR_PERMISSION_DENIED = storj_uplinkPINVOKE.UPLINK_ERROR_PERMISSION_DENIED_get();
  public static readonly int UPLINK_ERROR_BUCKET_NAME_INVALID = storj_uplinkPINVOKE.UPLINK_ERROR_BUCKET_NAME_INVALID_get();
  public static readonly int UPLINK_ERROR_BUCKET_ALREADY_EXISTS = storj_uplinkPINVOKE.UPLINK_ERROR_BUCKET_ALREADY_EXISTS_get();
  public static readonly int UPLINK_ERROR_BUCKET_NOT_EMPTY = storj_uplinkPINVOKE.UPLINK_ERROR_BUCKET_NOT_EMPTY_get();
  public static readonly int UPLINK_ERROR_BUCKET_NOT_FOUND = storj_uplinkPINVOKE.UPLINK_ERROR_BUCKET_NOT_FOUND_get();
  public static readonly int UPLINK_ERROR_OBJECT_KEY_INVALID = storj_uplinkPINVOKE.UPLINK_ERROR_OBJECT_KEY_INVALID_get();
  public static readonly int UPLINK_ERROR_OBJECT_NOT_FOUND = storj_uplinkPINVOKE.UPLINK_ERROR_OBJECT_NOT_FOUND_get();
  public static readonly int UPLINK_ERROR_UPLOAD_DONE = storj_uplinkPINVOKE.UPLINK_ERROR_UPLOAD_DONE_get();
  public static readonly int EDGE_ERROR_AUTH_DIAL_FAILED = storj_uplinkPINVOKE.EDGE_ERROR_AUTH_DIAL_FAILED_get();
  public static readonly int EDGE_ERROR_REGISTER_ACCESS_FAILED = storj_uplinkPINVOKE.EDGE_ERROR_REGISTER_ACCESS_FAILED_get();
}

}
