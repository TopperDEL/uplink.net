
%{
#include <stdio.h>
#include <stdarg.h>

// Einfaches Logging für die SWIG-Schnittstelle
void swig_log(const char *format, ...) {
    va_list args;
    va_start(args, format);
    vfprintf(stderr, format, args);
    fprintf(stderr, "\n");
    fflush(stderr);
    va_end(args);
}
%}

%inline %{
void swig_trace(const char* function_name) {
    fprintf(stderr, "SWIG TRACE: %s\n", function_name);
    fflush(stderr);
}
%}

%exception {
    try {
        swig_trace("$function");
        $action;
    } catch (const std::exception &e) {
        fprintf(stderr, "SWIG EXCEPTION: %s\n", e.what());
        fflush(stderr);
        throw;
    }
}

%module storj_uplink
		
%{
	/* Includes the header in the wrapper code */
	#include "uplink_definitions.h"
%}
		 
/* Parse the header file to generate wrappers */
%include "uplink_definitions.h"