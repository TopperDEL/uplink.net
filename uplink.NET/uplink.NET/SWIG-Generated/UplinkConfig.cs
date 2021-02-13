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

internal class UplinkConfig : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal UplinkConfig(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(UplinkConfig obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~UplinkConfig() {
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
          storj_uplinkPINVOKE.delete_UplinkConfig(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public string user_agent {
    set {
      storj_uplinkPINVOKE.UplinkConfig_user_agent_set(swigCPtr, value);
    } 
    get {
      string ret = storj_uplinkPINVOKE.UplinkConfig_user_agent_get(swigCPtr);
      return ret;
    } 
  }

  public int dial_timeout_milliseconds {
    set {
      storj_uplinkPINVOKE.UplinkConfig_dial_timeout_milliseconds_set(swigCPtr, value);
    } 
    get {
      int ret = storj_uplinkPINVOKE.UplinkConfig_dial_timeout_milliseconds_get(swigCPtr);
      return ret;
    } 
  }

  public string temp_directory {
    set {
      storj_uplinkPINVOKE.UplinkConfig_temp_directory_set(swigCPtr, value);
    } 
    get {
      string ret = storj_uplinkPINVOKE.UplinkConfig_temp_directory_get(swigCPtr);
      return ret;
    } 
  }

  public UplinkConfig() : this(storj_uplinkPINVOKE.new_UplinkConfig(), true) {
  }

}

}
