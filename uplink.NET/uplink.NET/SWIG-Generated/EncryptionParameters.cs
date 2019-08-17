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

public class EncryptionParameters : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal EncryptionParameters(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(EncryptionParameters obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~EncryptionParameters() {
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
          storj_uplinkPINVOKE.delete_EncryptionParameters(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public CipherSuite cipher_suite {
    set {
      storj_uplinkPINVOKE.EncryptionParameters_cipher_suite_set(swigCPtr, (int)value);
    } 
    get {
      CipherSuite ret = (CipherSuite)storj_uplinkPINVOKE.EncryptionParameters_cipher_suite_get(swigCPtr);
      return ret;
    } 
  }

  public int block_size {
    set {
      storj_uplinkPINVOKE.EncryptionParameters_block_size_set(swigCPtr, value);
    } 
    get {
      int ret = storj_uplinkPINVOKE.EncryptionParameters_block_size_get(swigCPtr);
      return ret;
    } 
  }

  public EncryptionParameters() : this(storj_uplinkPINVOKE.new_EncryptionParameters(), true) {
  }

}

}
