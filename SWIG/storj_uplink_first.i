%module storj_uplink
		
%{
	/* Includes the header in the wrapper code */
	#include "uplink_definitions.h"
	extern BucketInfo get_bucketinfo_at(BucketList list, int index);
%}
		 
/* Parse the header file to generate wrappers */
%include "uplink_definitions.h"
extern BucketInfo get_bucketinfo_at(BucketList list, int index);

%inline %{
extern BucketInfo get_bucketinfo_at(BucketList list, int index){
	return *(list.items+index);
}
%}
