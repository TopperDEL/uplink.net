package main

// #include "uplink_definitions.h"
import "C"

import  (
	"fmt"

	"storj.io/common/macaroon"
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

//export restrict_scope2
// restrict_scope2 restricts a given scope with the provided caveat and the temporary encryption restrictions
func restrict_scope2(scopeRef C.ScopeRef, caveat C.Caveat, cerr **C.char) C.ScopeRef {
	scope, ok := universe.Get(scopeRef._handle).(*libuplink.Scope)
	if !ok {
		*cerr = C.CString("invalid scope")
		return C.ScopeRef{}
	}

	caveatGo := macaroon.Caveat{
		DisallowReads:   bool(caveat.disallow_reads),
		DisallowWrites:  bool(caveat.disallow_writes),
		DisallowLists:   bool(caveat.disallow_lists),
		DisallowDeletes: bool(caveat.disallow_deletes),
	}

	apiKeyRestricted, err := scope.APIKey.Restrict(caveatGo)
	if err != nil {
		*cerr = C.CString(fmt.Sprintf("%+v", err))
		return C.ScopeRef{}
	}

	apiKeyRestricted, encAccessRestricted, err := scope.EncryptionAccess.Restrict(apiKeyRestricted, restrictions...)
	if err != nil {
		*cerr = C.CString(fmt.Sprintf("%+v", err))
		return C.ScopeRef{}
	}

	scopeRestricted := &libuplink.Scope{
		SatelliteAddr:    scope.SatelliteAddr,
		APIKey:           apiKeyRestricted,
		EncryptionAccess: encAccessRestricted,
	}
	return C.ScopeRef{_handle: universe.Add(scopeRestricted)}
}