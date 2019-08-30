%module storj_uplink
		
%{
	/* Includes the header in the wrapper code */
	#include "uplink_definitions.h"
%}
		 
/* Parse the header file to generate wrappers */
%include "uplink_definitions.h"

