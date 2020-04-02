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
