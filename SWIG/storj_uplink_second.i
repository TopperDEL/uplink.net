%module storj_uplink
%pragma(csharp) moduleclassmodifiers="internal class"
%typemap(csclassmodifiers) SWIGTYPE "internal class"
%include "csharp.swg"

/*Mapping for primitive types*/
%define MAP_SPECIAL(CTYPE, CSTYPE, TYPECHECKPRECEDENCE)
%typemap(ctype) CTYPE "CTYPE"
%typemap(imtype) CTYPE "CSTYPE"
%typemap(cstype) CTYPE "CSTYPE"

%typemap(csin) CTYPE "$csinput"
%typemap(csout, excode=SWIGEXCODE) CTYPE {
    CSTYPE ret = $imcall;$excode
    return ret;
  }

%typemap(csvarin, excode=SWIGEXCODE2) CTYPE %{
    set {
      $imcall;$excode
    } %}

%typemap(csvarout, excode=SWIGEXCODE2) CTYPE %{
    get {
      CSTYPE ret = $imcall;$excode
      return ret;
    } %}

%typemap(in) CTYPE %{ $1 = ($1_ltype)$input; %}
%typemap(out) CTYPE %{ $result = (CTYPE)$1; %}
/*%typemap(typecheck) CTYPE = CSTYPE;
%typecheck(SWIG_TYPECHECK_##TYPECHECKPRECEDENCE) CTYPE *OUTPUT, CSTYPE &OUTPUT "";*/
%enddef
MAP_SPECIAL(int8_t, sbyte, int8_t)
MAP_SPECIAL(int16_t, short, int16_t)
MAP_SPECIAL(int32_t, int, int32_t)
MAP_SPECIAL(int64_t, long, int64_t)
MAP_SPECIAL(uint8_t, byte, uint8_t)
MAP_SPECIAL(uint16_t, ushort, uint16_t)
MAP_SPECIAL(uint32_t, uint, uint32_t)
MAP_SPECIAL(uint64_t, ulong, uint64_t)
MAP_SPECIAL(_Bool, bool, _Bool)

%{
	/* Includes the header in the wrapper code */
	#include "uplink_definitions.h"
	#include "storj_uplink.h"
%}
		 
/* Parse the header file to generate wrappers */
%include "storj_uplink.h"
%include "uplink_definitions.h"
extern char* get_storj_version();

%inline %{

char* get_storj_version(){
	return "STORJVERSION";
}
%}

%pragma(csharp) imclasscode=%{
  public static class StringMarshalHelper : IDisposable {
	
	 public static System.IntPtr NativeUtf8FromString(string managedString)
        {
            int len = System.Text.Encoding.UTF8.GetByteCount(managedString);
            byte[] buffer = new byte[len + 1];
            System.Text.Encoding.UTF8.GetBytes(managedString, 0, managedString.Length, buffer, 0);
            System.IntPtr nativeUtf8 = System.Runtime.InteropServices.Marshal.AllocHGlobal(buffer.Length);
            System.Runtime.InteropServices.Marshal.Copy(buffer, 0, nativeUtf8, buffer.Length);
            return nativeUtf8;
        }

	public static string StringFromNativeUtf8(System.IntPtr nativeUtf8)
        {
            int len = 0;
            while (System.Runtime.InteropServices.Marshal.ReadByte(nativeUtf8, len) != 0) ++len;
            byte[] buffer = new byte[len];
            System.Runtime.InteropServices.Marshal.Copy(nativeUtf8, buffer, 0, buffer.Length);
            return System.Text.Encoding.UTF8.GetString(buffer);
        }
  }
%}

%pragma(csharp) imclasscode=%{
  public class SWIGStringMarshal : global::System.IDisposable {
    public readonly global::System.Runtime.InteropServices.HandleRef swigCPtr;
    public SWIGStringMarshal(string str) {
      swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, global::System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(str));
    }
    public virtual void Dispose() {
      global::System.Runtime.InteropServices.Marshal.FreeHGlobal(swigCPtr.Handle);
      global::System.GC.SuppressFinalize(this);
    }
  }
%}

%typemap(imtype, out="global::System.IntPtr") char *, char[ANY], char[]   "global::System.Runtime.InteropServices.HandleRef"
%typemap(out) char *, char[ANY], char[] %{ $result = $1; %}
%typemap(csin) char *, char[ANY], char[] "new $imclassname.SWIGStringMarshal.NativeUtf8FromString($csinput)"
%typemap(csout, excode=SWIGEXCODE) char *, char[ANY], char[] {
    string ret = $imclassname.StringMarshalHelper.StringFromNativeUtf8($imcall);$excode
    return ret;
  }
%typemap(csvarin, excode=SWIGEXCODE2) char *, char[ANY], char[] %{
    set {
      $imcall;$excode
    } %}
%typemap(csvarout, excode=SWIGEXCODE2) char *, char[ANY], char[] %{
    get {
      string ret = $imclassname.StringMarshalHelper.StringFromNativeUtf8($imcall);$excode
      return ret;
    } %}