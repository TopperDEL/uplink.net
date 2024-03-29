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

internal class UplinkPart : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal UplinkPart(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(UplinkPart obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~UplinkPart() {
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
          storj_uplinkPINVOKE.delete_UplinkPart(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public uint part_number {
    set {
      storj_uplinkPINVOKE.UplinkPart_part_number_set(swigCPtr, value);
    } 
    get {
      uint ret = storj_uplinkPINVOKE.UplinkPart_part_number_get(swigCPtr);
      return ret;
    } 
  }

  public uint size {
    set {
      storj_uplinkPINVOKE.UplinkPart_size_set(swigCPtr, value);
    } 
    get {
      uint ret = storj_uplinkPINVOKE.UplinkPart_size_get(swigCPtr);
      return ret;
    } 
  }

  public long modified {
    set {
      storj_uplinkPINVOKE.UplinkPart_modified_set(swigCPtr, value);
    } 
    get {
      long ret = storj_uplinkPINVOKE.UplinkPart_modified_get(swigCPtr);
      return ret;
    } 
  }

  public string etag {
    set {
      storj_uplinkPINVOKE.UplinkPart_etag_set(swigCPtr, new storj_uplinkPINVOKE.SWIGStringMarshal(value).swigCPtr);
    } 
    get {
      string ret = storj_uplinkPINVOKE.SWIGStringMarshal.StringFromNativeUtf8(storj_uplinkPINVOKE.UplinkPart_etag_get(swigCPtr));
      return ret;
    } 
  }

  public uint etag_length {
    set {
      storj_uplinkPINVOKE.UplinkPart_etag_length_set(swigCPtr, value);
    } 
    get {
      uint ret = storj_uplinkPINVOKE.UplinkPart_etag_length_get(swigCPtr);
      return ret;
    } 
  }

  public UplinkPart() : this(storj_uplinkPINVOKE.new_UplinkPart(), true) {
  }

}

}
