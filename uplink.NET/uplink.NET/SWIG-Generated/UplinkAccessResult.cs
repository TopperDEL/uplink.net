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

internal class UplinkAccessResult : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal UplinkAccessResult(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(UplinkAccessResult obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~UplinkAccessResult() {
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
          storj_uplinkPINVOKE.delete_UplinkAccessResult(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public UplinkAccess access {
    set {
      storj_uplinkPINVOKE.UplinkAccessResult_access_set(swigCPtr, UplinkAccess.getCPtr(value));
    } 
    get {
      global::System.IntPtr cPtr = storj_uplinkPINVOKE.UplinkAccessResult_access_get(swigCPtr);
      UplinkAccess ret = (cPtr == global::System.IntPtr.Zero) ? null : new UplinkAccess(cPtr, false);
      return ret;
    } 
  }

  public UplinkError error {
    set {
      storj_uplinkPINVOKE.UplinkAccessResult_error_set(swigCPtr, UplinkError.getCPtr(value));
    } 
    get {
      global::System.IntPtr cPtr = storj_uplinkPINVOKE.UplinkAccessResult_error_get(swigCPtr);
      UplinkError ret = (cPtr == global::System.IntPtr.Zero) ? null : new UplinkError(cPtr, false);
      return ret;
    } 
  }

  public UplinkAccessResult() : this(storj_uplinkPINVOKE.new_UplinkAccessResult(), true) {
  }

}

}
