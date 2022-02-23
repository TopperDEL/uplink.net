%module storj_uplink
		
%{
	/* Includes the header in the wrapper code */
	#include "uplink_definitions.h"
%}
		 
/* Parse the header file to generate wrappers */
%include "uplink_definitions.h"

%pragma(csharp) imclasscode=%{
  protected class SWIGStringHelper2 {

    public delegate string SWIGStringDelegate(string message);
    static SWIGStringDelegate stringDelegate = new SWIGStringDelegate(CreateString);

    [global::System.Runtime.InteropServices.DllImport("$dllimport", EntryPoint="SWIGRegisterStringCallback_$module")]
    public static extern void SWIGRegisterStringCallback_$module(SWIGStringDelegate stringDelegate);

    static string CreateString(string cString) {
      return cString;
    }

    static SWIGStringHelper2() {
      SWIGRegisterStringCallback_$module(stringDelegate);
    }
  }

  static protected SWIGStringHelper swigStringHelper = new SWIGStringHelper();
%}