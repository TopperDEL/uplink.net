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

internal class CustomMetadataEntry : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal CustomMetadataEntry(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(CustomMetadataEntry obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~CustomMetadataEntry() {
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
          storj_uplinkPINVOKE.delete_CustomMetadataEntry(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public string key {
    set {
      storj_uplinkPINVOKE.CustomMetadataEntry_key_set(swigCPtr, value);
    } 
    get {
      string ret = storj_uplinkPINVOKE.CustomMetadataEntry_key_get(swigCPtr);
      return ret;
    } 
  }

  public uint key_length {
    set {
      storj_uplinkPINVOKE.CustomMetadataEntry_key_length_set(swigCPtr, value);
    } 
    get {
      uint ret = storj_uplinkPINVOKE.CustomMetadataEntry_key_length_get(swigCPtr);
      return ret;
    } 
  }

  public string value {
    set {
      storj_uplinkPINVOKE.CustomMetadataEntry_value_set(swigCPtr, value);
    } 
    get {
      string ret = storj_uplinkPINVOKE.CustomMetadataEntry_value_get(swigCPtr);
      return ret;
    } 
  }

  public uint value_length {
    set {
      storj_uplinkPINVOKE.CustomMetadataEntry_value_length_set(swigCPtr, value);
    } 
    get {
      uint ret = storj_uplinkPINVOKE.CustomMetadataEntry_value_length_get(swigCPtr);
      return ret;
    } 
  }

  public CustomMetadataEntry() : this(storj_uplinkPINVOKE.new_CustomMetadataEntry(), true) {
  }

}

}
