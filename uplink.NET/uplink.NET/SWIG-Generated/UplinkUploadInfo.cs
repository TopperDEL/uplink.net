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

internal class UplinkUploadInfo : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal UplinkUploadInfo(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(UplinkUploadInfo obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~UplinkUploadInfo() {
    Dispose(false);
  }

  public void Dispose() {
    Dispose(true);
    global::System.GC.SuppressFinalize(this);
  }

  protected virtual void Dispose(bool disposing) {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          storj_uplinkPINVOKE.delete_UplinkUploadInfo(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public string upload_id {
    set {
      storj_uplinkPINVOKE.UplinkUploadInfo_upload_id_set(swigCPtr, value);
    } 
    get {
      string ret = storj_uplinkPINVOKE.UplinkUploadInfo_upload_id_get(swigCPtr);
      return ret;
    } 
  }

  public string key {
    set {
      storj_uplinkPINVOKE.UplinkUploadInfo_key_set(swigCPtr, value);
    } 
    get {
      string ret = storj_uplinkPINVOKE.UplinkUploadInfo_key_get(swigCPtr);
      return ret;
    } 
  }

  public bool is_prefix {
    set {
      storj_uplinkPINVOKE.UplinkUploadInfo_is_prefix_set(swigCPtr, value);
    } 
    get {
      bool ret = storj_uplinkPINVOKE.UplinkUploadInfo_is_prefix_get(swigCPtr);
      return ret;
    } 
  }

  public UplinkSystemMetadata system {
    set {
      storj_uplinkPINVOKE.UplinkUploadInfo_system_set(swigCPtr, UplinkSystemMetadata.getCPtr(value));
    } 
    get {
      global::System.IntPtr cPtr = storj_uplinkPINVOKE.UplinkUploadInfo_system_get(swigCPtr);
      UplinkSystemMetadata ret = (cPtr == global::System.IntPtr.Zero) ? null : new UplinkSystemMetadata(cPtr, false);
      return ret;
    } 
  }

  public UplinkCustomMetadata custom {
    set {
      storj_uplinkPINVOKE.UplinkUploadInfo_custom_set(swigCPtr, UplinkCustomMetadata.getCPtr(value));
    } 
    get {
      global::System.IntPtr cPtr = storj_uplinkPINVOKE.UplinkUploadInfo_custom_get(swigCPtr);
      UplinkCustomMetadata ret = (cPtr == global::System.IntPtr.Zero) ? null : new UplinkCustomMetadata(cPtr, false);
      return ret;
    } 
  }

  public UplinkUploadInfo() : this(storj_uplinkPINVOKE.new_UplinkUploadInfo(), true) {
  }

}

}
