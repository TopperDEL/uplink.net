%module storj_uplink

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

%{
#include <shlwapi.h>
#pragma comment(lib, "Shlwapi.lib")
%}
		
%{
	/* Includes the header in the wrapper code */
	#include "uplink_definitions.h"
	#include "storj_uplink.h"
%}
		 
/* Parse the header file to generate wrappers */
%include "storj_uplink.h"
%include "uplink_definitions.h"