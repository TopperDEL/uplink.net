%module storj_uplink
%include "csharp.swg"

/*Wrap "out string"-parameters*/
%typemap(csin,pre="global::System.IntPtr tmp$csinput=global::System.IntPtr.Zero;",
              post="$csinput=global::System.Runtime.InteropServices.Marshal.PtrToStringAnsi(tmp$csinput);
			  if(tmp$csinput != System.IntPtr.Zero) storj_uplinkPINVOKE.free_string(tmp$csinput);") char **OUTPUT "ref tmp$csinput";
%typemap(cstype) char **OUTPUT "out string";
%typemap(imtype) char **OUTPUT "ref global::System.IntPtr"
%apply char **OUTPUT { char **p0 };
%apply char **OUTPUT { char **p1 };
%apply char **OUTPUT { char **p2 };
%apply char **OUTPUT { char **p3 };
%apply char **OUTPUT { char **p4 };

/*Wrap byte-arrays (byte[])*/
%include "arrays_csharp.i"
CSHARP_ARRAYS(uint8_t, byte)
%apply uint8_t OUTPUT[] { uint8_t *p0 }
%apply uint8_t OUTPUT[] { uint8_t *p1 }
%apply uint8_t OUTPUT[] { uint8_t *p2 }
/*%apply uint8_t OUTPUT[] { uint8_t *p3 }*/

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

%{
	/* Includes the header in the wrapper code */
	#include "uplink_definitions.h"
	#include "storj_uplink.h"
	extern BucketInfo get_bucketinfo_at(BucketList list, int index);
	extern ObjectInfo get_objectinfo_at(ObjectList list, int index);
	extern EncryptionAccessRef new_encryption_access_with_default_key2(uint8_t* bytes);
%}

CSHARP_ARRAYS(EncryptionRestriction, EncryptionRestriction)
%apply EncryptionRestriction OUTPUT[] { EncryptionRestriction **p2 }
		 
/* Parse the header file to generate wrappers */
%include "storj_uplink.h"
%include "uplink_definitions.h"
extern BucketInfo get_bucketinfo_at(BucketList list, int index);
extern ObjectInfo get_objectinfo_at(ObjectList list, int index);
extern EncryptionAccessRef new_encryption_access_with_default_key2(uint8_t* bytes);

%inline %{
BucketInfo get_bucketinfo_at(BucketList list, int index){
	return *(list.items+index);
}

extern ObjectInfo get_objectinfo_at(ObjectList list, int index){
	return *(list.items+index);
}

EncryptionAccessRef new_encryption_access_with_default_key2(uint8_t* bytes){
	return new_encryption_access_with_default_key(bytes);
}
%}
