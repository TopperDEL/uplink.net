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
  public static AccessResult parse_access(string p0) {
    AccessResult ret = new AccessResult(storj_uplinkPINVOKE.parse_access(p0), true);
    return ret;
  }

  public static AccessResult request_access_with_passphrase(string p0, string p1, string p2) {
    AccessResult ret = new AccessResult(storj_uplinkPINVOKE.request_access_with_passphrase(p0, p1, p2), true);
    return ret;
  }

  public static StringResult access_serialize(Access p0) {
    StringResult ret = new StringResult(storj_uplinkPINVOKE.access_serialize(Access.getCPtr(p0)), true);
    return ret;
  }

  public static AccessResult access_share(Access p0, Permission p1, SharePrefix p2, long p3) {
    AccessResult ret = new AccessResult(storj_uplinkPINVOKE.access_share(Access.getCPtr(p0), Permission.getCPtr(p1), SharePrefix.getCPtr(p2), p3), true);
    if (storj_uplinkPINVOKE.SWIGPendingException.Pending) throw storj_uplinkPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static void free_string_result(StringResult p0) {
    storj_uplinkPINVOKE.free_string_result(StringResult.getCPtr(p0));
    if (storj_uplinkPINVOKE.SWIGPendingException.Pending) throw storj_uplinkPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void free_access_result(AccessResult p0) {
    storj_uplinkPINVOKE.free_access_result(AccessResult.getCPtr(p0));
    if (storj_uplinkPINVOKE.SWIGPendingException.Pending) throw storj_uplinkPINVOKE.SWIGPendingException.Retrieve();
  }

  public static BucketResult stat_bucket(Project p0, string p1) {
    BucketResult ret = new BucketResult(storj_uplinkPINVOKE.stat_bucket(Project.getCPtr(p0), p1), true);
    return ret;
  }

  public static BucketResult create_bucket(Project p0, string p1) {
    BucketResult ret = new BucketResult(storj_uplinkPINVOKE.create_bucket(Project.getCPtr(p0), p1), true);
    return ret;
  }

  public static BucketResult ensure_bucket(Project p0, string p1) {
    BucketResult ret = new BucketResult(storj_uplinkPINVOKE.ensure_bucket(Project.getCPtr(p0), p1), true);
    return ret;
  }

  public static BucketResult delete_bucket(Project p0, string p1) {
    BucketResult ret = new BucketResult(storj_uplinkPINVOKE.delete_bucket(Project.getCPtr(p0), p1), true);
    return ret;
  }

  public static void free_bucket_result(BucketResult p0) {
    storj_uplinkPINVOKE.free_bucket_result(BucketResult.getCPtr(p0));
    if (storj_uplinkPINVOKE.SWIGPendingException.Pending) throw storj_uplinkPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void free_bucket(Bucket p0) {
    storj_uplinkPINVOKE.free_bucket(Bucket.getCPtr(p0));
  }

  public static BucketIterator list_buckets(Project p0, ListBucketsOptions p1) {
    global::System.IntPtr cPtr = storj_uplinkPINVOKE.list_buckets(Project.getCPtr(p0), ListBucketsOptions.getCPtr(p1));
    BucketIterator ret = (cPtr == global::System.IntPtr.Zero) ? null : new BucketIterator(cPtr, false);
    return ret;
  }

  public static bool bucket_iterator_next(BucketIterator p0) {
    bool ret = storj_uplinkPINVOKE.bucket_iterator_next(BucketIterator.getCPtr(p0));
    return ret;
  }

  public static Error bucket_iterator_err(BucketIterator p0) {
    global::System.IntPtr cPtr = storj_uplinkPINVOKE.bucket_iterator_err(BucketIterator.getCPtr(p0));
    Error ret = (cPtr == global::System.IntPtr.Zero) ? null : new Error(cPtr, false);
    return ret;
  }

  public static Bucket bucket_iterator_item(BucketIterator p0) {
    global::System.IntPtr cPtr = storj_uplinkPINVOKE.bucket_iterator_item(BucketIterator.getCPtr(p0));
    Bucket ret = (cPtr == global::System.IntPtr.Zero) ? null : new Bucket(cPtr, false);
    return ret;
  }

  public static void free_bucket_iterator(BucketIterator p0) {
    storj_uplinkPINVOKE.free_bucket_iterator(BucketIterator.getCPtr(p0));
  }

  public static AccessResult config_request_access_with_passphrase(Config p0, string p1, string p2, string p3) {
    AccessResult ret = new AccessResult(storj_uplinkPINVOKE.config_request_access_with_passphrase(Config.getCPtr(p0), p1, p2, p3), true);
    if (storj_uplinkPINVOKE.SWIGPendingException.Pending) throw storj_uplinkPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static ProjectResult config_open_project(Config p0, Access p1) {
    ProjectResult ret = new ProjectResult(storj_uplinkPINVOKE.config_open_project(Config.getCPtr(p0), Access.getCPtr(p1)), true);
    if (storj_uplinkPINVOKE.SWIGPendingException.Pending) throw storj_uplinkPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static void prepare_custommetadata() {
    storj_uplinkPINVOKE.prepare_custommetadata();
  }

  public static void append_custommetadata(string p0, string p1) {
    storj_uplinkPINVOKE.append_custommetadata(p0, p1);
  }

  public static Error upload_set_custom_metadata2(Upload p0) {
    global::System.IntPtr cPtr = storj_uplinkPINVOKE.upload_set_custom_metadata2(Upload.getCPtr(p0));
    Error ret = (cPtr == global::System.IntPtr.Zero) ? null : new Error(cPtr, false);
    return ret;
  }

  public static DownloadResult download_object(Project p0, string p1, string p2, DownloadOptions p3) {
    DownloadResult ret = new DownloadResult(storj_uplinkPINVOKE.download_object(Project.getCPtr(p0), p1, p2, DownloadOptions.getCPtr(p3)), true);
    return ret;
  }

  public static ReadResult download_read(Download p0, SWIGTYPE_p_void p1, uint p2) {
    ReadResult ret = new ReadResult(storj_uplinkPINVOKE.download_read(Download.getCPtr(p0), SWIGTYPE_p_void.getCPtr(p1), p2), true);
    return ret;
  }

  public static ObjectResult download_info(Download p0) {
    ObjectResult ret = new ObjectResult(storj_uplinkPINVOKE.download_info(Download.getCPtr(p0)), true);
    return ret;
  }

  public static void free_read_result(ReadResult p0) {
    storj_uplinkPINVOKE.free_read_result(ReadResult.getCPtr(p0));
    if (storj_uplinkPINVOKE.SWIGPendingException.Pending) throw storj_uplinkPINVOKE.SWIGPendingException.Retrieve();
  }

  public static Error close_download(Download p0) {
    global::System.IntPtr cPtr = storj_uplinkPINVOKE.close_download(Download.getCPtr(p0));
    Error ret = (cPtr == global::System.IntPtr.Zero) ? null : new Error(cPtr, false);
    return ret;
  }

  public static void free_download_result(DownloadResult p0) {
    storj_uplinkPINVOKE.free_download_result(DownloadResult.getCPtr(p0));
    if (storj_uplinkPINVOKE.SWIGPendingException.Pending) throw storj_uplinkPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void free_error(Error p0) {
    storj_uplinkPINVOKE.free_error(Error.getCPtr(p0));
  }

  public static byte internal_UniverseIsEmpty() {
    byte ret = storj_uplinkPINVOKE.internal_UniverseIsEmpty();
    return ret;
  }

  public static ObjectResult stat_object(Project p0, string p1, string p2) {
    ObjectResult ret = new ObjectResult(storj_uplinkPINVOKE.stat_object(Project.getCPtr(p0), p1, p2), true);
    return ret;
  }

  public static ObjectResult delete_object(Project p0, string p1, string p2) {
    ObjectResult ret = new ObjectResult(storj_uplinkPINVOKE.delete_object(Project.getCPtr(p0), p1, p2), true);
    return ret;
  }

  public static void free_object_result(ObjectResult p0) {
    storj_uplinkPINVOKE.free_object_result(ObjectResult.getCPtr(p0));
    if (storj_uplinkPINVOKE.SWIGPendingException.Pending) throw storj_uplinkPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void free_object(Object p0) {
    storj_uplinkPINVOKE.free_object(Object.getCPtr(p0));
  }

  public static ObjectIterator list_objects(Project p0, string p1, ListObjectsOptions p2) {
    global::System.IntPtr cPtr = storj_uplinkPINVOKE.list_objects(Project.getCPtr(p0), p1, ListObjectsOptions.getCPtr(p2));
    ObjectIterator ret = (cPtr == global::System.IntPtr.Zero) ? null : new ObjectIterator(cPtr, false);
    return ret;
  }

  public static bool object_iterator_next(ObjectIterator p0) {
    bool ret = storj_uplinkPINVOKE.object_iterator_next(ObjectIterator.getCPtr(p0));
    return ret;
  }

  public static Error object_iterator_err(ObjectIterator p0) {
    global::System.IntPtr cPtr = storj_uplinkPINVOKE.object_iterator_err(ObjectIterator.getCPtr(p0));
    Error ret = (cPtr == global::System.IntPtr.Zero) ? null : new Error(cPtr, false);
    return ret;
  }

  public static Object object_iterator_item(ObjectIterator p0) {
    global::System.IntPtr cPtr = storj_uplinkPINVOKE.object_iterator_item(ObjectIterator.getCPtr(p0));
    Object ret = (cPtr == global::System.IntPtr.Zero) ? null : new Object(cPtr, false);
    return ret;
  }

  public static void free_object_iterator(ObjectIterator p0) {
    storj_uplinkPINVOKE.free_object_iterator(ObjectIterator.getCPtr(p0));
  }

  public static ProjectResult open_project(Access p0) {
    ProjectResult ret = new ProjectResult(storj_uplinkPINVOKE.open_project(Access.getCPtr(p0)), true);
    return ret;
  }

  public static Error close_project(Project p0) {
    global::System.IntPtr cPtr = storj_uplinkPINVOKE.close_project(Project.getCPtr(p0));
    Error ret = (cPtr == global::System.IntPtr.Zero) ? null : new Error(cPtr, false);
    return ret;
  }

  public static void free_project_result(ProjectResult p0) {
    storj_uplinkPINVOKE.free_project_result(ProjectResult.getCPtr(p0));
    if (storj_uplinkPINVOKE.SWIGPendingException.Pending) throw storj_uplinkPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void prepare_shareprefixes(uint p0) {
    storj_uplinkPINVOKE.prepare_shareprefixes(p0);
  }

  public static void append_shareprefix(string p0, string p1) {
    storj_uplinkPINVOKE.append_shareprefix(p0, p1);
  }

  public static AccessResult access_share2(Access p0, Permission p1) {
    AccessResult ret = new AccessResult(storj_uplinkPINVOKE.access_share2(Access.getCPtr(p0), Permission.getCPtr(p1)), true);
    if (storj_uplinkPINVOKE.SWIGPendingException.Pending) throw storj_uplinkPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static UploadResult upload_object(Project p0, string p1, string p2, UploadOptions p3) {
    UploadResult ret = new UploadResult(storj_uplinkPINVOKE.upload_object(Project.getCPtr(p0), p1, p2, UploadOptions.getCPtr(p3)), true);
    return ret;
  }

  public static WriteResult upload_write(Upload p0, SWIGTYPE_p_void p1, uint p2) {
    WriteResult ret = new WriteResult(storj_uplinkPINVOKE.upload_write(Upload.getCPtr(p0), SWIGTYPE_p_void.getCPtr(p1), p2), true);
    return ret;
  }

  public static Error upload_commit(Upload p0) {
    global::System.IntPtr cPtr = storj_uplinkPINVOKE.upload_commit(Upload.getCPtr(p0));
    Error ret = (cPtr == global::System.IntPtr.Zero) ? null : new Error(cPtr, false);
    return ret;
  }

  public static Error upload_abort(Upload p0) {
    global::System.IntPtr cPtr = storj_uplinkPINVOKE.upload_abort(Upload.getCPtr(p0));
    Error ret = (cPtr == global::System.IntPtr.Zero) ? null : new Error(cPtr, false);
    return ret;
  }

  public static ObjectResult upload_info(Upload p0) {
    ObjectResult ret = new ObjectResult(storj_uplinkPINVOKE.upload_info(Upload.getCPtr(p0)), true);
    return ret;
  }

  public static Error upload_set_custom_metadata(Upload p0, CustomMetadata p1) {
    global::System.IntPtr cPtr = storj_uplinkPINVOKE.upload_set_custom_metadata(Upload.getCPtr(p0), CustomMetadata.getCPtr(p1));
    Error ret = (cPtr == global::System.IntPtr.Zero) ? null : new Error(cPtr, false);
    if (storj_uplinkPINVOKE.SWIGPendingException.Pending) throw storj_uplinkPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static void free_write_result(WriteResult p0) {
    storj_uplinkPINVOKE.free_write_result(WriteResult.getCPtr(p0));
    if (storj_uplinkPINVOKE.SWIGPendingException.Pending) throw storj_uplinkPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void free_upload_result(UploadResult p0) {
    storj_uplinkPINVOKE.free_upload_result(UploadResult.getCPtr(p0));
    if (storj_uplinkPINVOKE.SWIGPendingException.Pending) throw storj_uplinkPINVOKE.SWIGPendingException.Retrieve();
  }

  public static string get_storj_version() {
    string ret = storj_uplinkPINVOKE.get_storj_version();
    return ret;
  }

  public static readonly int ERROR_INTERNAL = storj_uplinkPINVOKE.ERROR_INTERNAL_get();
  public static readonly int ERROR_CANCELED = storj_uplinkPINVOKE.ERROR_CANCELED_get();
  public static readonly int ERROR_INVALID_HANDLE = storj_uplinkPINVOKE.ERROR_INVALID_HANDLE_get();
  public static readonly int ERROR_TOO_MANY_REQUESTS = storj_uplinkPINVOKE.ERROR_TOO_MANY_REQUESTS_get();
  public static readonly int ERROR_BANDWIDTH_LIMIT_EXCEEDED = storj_uplinkPINVOKE.ERROR_BANDWIDTH_LIMIT_EXCEEDED_get();
  public static readonly int ERROR_BUCKET_NAME_INVALID = storj_uplinkPINVOKE.ERROR_BUCKET_NAME_INVALID_get();
  public static readonly int ERROR_BUCKET_ALREADY_EXISTS = storj_uplinkPINVOKE.ERROR_BUCKET_ALREADY_EXISTS_get();
  public static readonly int ERROR_BUCKET_NOT_EMPTY = storj_uplinkPINVOKE.ERROR_BUCKET_NOT_EMPTY_get();
  public static readonly int ERROR_BUCKET_NOT_FOUND = storj_uplinkPINVOKE.ERROR_BUCKET_NOT_FOUND_get();
  public static readonly int ERROR_OBJECT_KEY_INVALID = storj_uplinkPINVOKE.ERROR_OBJECT_KEY_INVALID_get();
  public static readonly int ERROR_OBJECT_NOT_FOUND = storj_uplinkPINVOKE.ERROR_OBJECT_NOT_FOUND_get();
  public static readonly int ERROR_UPLOAD_DONE = storj_uplinkPINVOKE.ERROR_UPLOAD_DONE_get();
}

}
