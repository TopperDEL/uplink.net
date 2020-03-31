package main

// #include "uplink_definitions.h"
import "C"

import  (
	libuplink "storj.io/storj/lib/uplink"
)

var restrictions []libuplink.EncryptionRestriction;

//export prepare_restrictions
// prepare_restrictions creates a temporary EncryptionRestrictions-Array to be filled by append_restriction and used by restrict_scope2
func prepare_restrictions(restrictionsLen C.size_t) {
	irestrictionsLen := int(restrictionsLen)
	
	restrictions = make([]libuplink.EncryptionRestriction, 0, irestrictionsLen)
}

//export append_restriction
// append_restriction appends one EncryptionRestriction by providing the contents directly
func append_restriction(bucket *C.char, path_prefix *C.char){
	restriction := libuplink.EncryptionRestriction{
				Bucket:     C.GoString(bucket),
				PathPrefix: C.GoString(path_prefix),
			}
	
	restrictions = append(restrictions, restriction)
}