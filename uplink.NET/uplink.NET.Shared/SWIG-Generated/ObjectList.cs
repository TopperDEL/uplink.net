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

internal class ObjectList : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal ObjectList(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(ObjectList obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~ObjectList() {
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
          storj_uplinkPINVOKE.delete_ObjectList(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public string bucket {
    set {
      storj_uplinkPINVOKE.ObjectList_bucket_set(swigCPtr, value);
    } 
    get {
      string ret = storj_uplinkPINVOKE.ObjectList_bucket_get(swigCPtr);
      return ret;
    } 
  }

  public string prefix {
    set {
      storj_uplinkPINVOKE.ObjectList_prefix_set(swigCPtr, value);
    } 
    get {
      string ret = storj_uplinkPINVOKE.ObjectList_prefix_get(swigCPtr);
      return ret;
    } 
  }

  public bool more {
    set {
      storj_uplinkPINVOKE.ObjectList_more_set(swigCPtr, value);
    } 
    get {
      bool ret = storj_uplinkPINVOKE.ObjectList_more_get(swigCPtr);
      return ret;
    } 
  }

  public ObjectInfo items {
    set {
      storj_uplinkPINVOKE.ObjectList_items_set(swigCPtr, ObjectInfo.getCPtr(value));
    } 
    get {
      global::System.IntPtr cPtr = storj_uplinkPINVOKE.ObjectList_items_get(swigCPtr);
      ObjectInfo ret = (cPtr == global::System.IntPtr.Zero) ? null : new ObjectInfo(cPtr, false);
      return ret;
    } 
  }

  public int length {
    set {
      storj_uplinkPINVOKE.ObjectList_length_set(swigCPtr, value);
    } 
    get {
      int ret = storj_uplinkPINVOKE.ObjectList_length_get(swigCPtr);
      return ret;
    } 
  }

  public ObjectList() : this(storj_uplinkPINVOKE.new_ObjectList(), true) {
  }

}

}
