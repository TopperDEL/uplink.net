//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 4.0.0
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------

namespace uplink.SWIG {

public class ObjectInfo : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal ObjectInfo(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(ObjectInfo obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~ObjectInfo() {
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
          storj_uplinkPINVOKE.delete_ObjectInfo(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public uint version {
    set {
      storj_uplinkPINVOKE.ObjectInfo_version_set(swigCPtr, value);
    } 
    get {
      uint ret = storj_uplinkPINVOKE.ObjectInfo_version_get(swigCPtr);
      return ret;
    } 
  }

  public BucketInfo bucket {
    set {
      storj_uplinkPINVOKE.ObjectInfo_bucket_set(swigCPtr, BucketInfo.getCPtr(value));
    } 
    get {
      global::System.IntPtr cPtr = storj_uplinkPINVOKE.ObjectInfo_bucket_get(swigCPtr);
      BucketInfo ret = (cPtr == global::System.IntPtr.Zero) ? null : new BucketInfo(cPtr, false);
      return ret;
    } 
  }

  public string path {
    set {
      storj_uplinkPINVOKE.ObjectInfo_path_set(swigCPtr, value);
    } 
    get {
      string ret = storj_uplinkPINVOKE.ObjectInfo_path_get(swigCPtr);
      return ret;
    } 
  }

  public bool is_prefix {
    set {
      storj_uplinkPINVOKE.ObjectInfo_is_prefix_set(swigCPtr, value);
    } 
    get {
      bool ret = storj_uplinkPINVOKE.ObjectInfo_is_prefix_get(swigCPtr);
      return ret;
    } 
  }

  public string content_type {
    set {
      storj_uplinkPINVOKE.ObjectInfo_content_type_set(swigCPtr, value);
    } 
    get {
      string ret = storj_uplinkPINVOKE.ObjectInfo_content_type_get(swigCPtr);
      return ret;
    } 
  }

  public long size {
    set {
      storj_uplinkPINVOKE.ObjectInfo_size_set(swigCPtr, value);
    } 
    get {
      long ret = storj_uplinkPINVOKE.ObjectInfo_size_get(swigCPtr);
      return ret;
    } 
  }

  public long created {
    set {
      storj_uplinkPINVOKE.ObjectInfo_created_set(swigCPtr, value);
    } 
    get {
      long ret = storj_uplinkPINVOKE.ObjectInfo_created_get(swigCPtr);
      return ret;
    } 
  }

  public long modified {
    set {
      storj_uplinkPINVOKE.ObjectInfo_modified_set(swigCPtr, value);
    } 
    get {
      long ret = storj_uplinkPINVOKE.ObjectInfo_modified_get(swigCPtr);
      return ret;
    } 
  }

  public long expires {
    set {
      storj_uplinkPINVOKE.ObjectInfo_expires_set(swigCPtr, value);
    } 
    get {
      long ret = storj_uplinkPINVOKE.ObjectInfo_expires_get(swigCPtr);
      return ret;
    } 
  }

  public ObjectInfo() : this(storj_uplinkPINVOKE.new_ObjectInfo(), true) {
  }

}

}
