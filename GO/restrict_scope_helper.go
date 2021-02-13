package main

// #include "uplink_definitions.h"
import "C"

import  (
	"time"

	"storj.io/uplink"
)

var shareprefixes []uplink.SharePrefix;

//export prepare_shareprefixes
// prepare_shareprefixes creates a temporary SharePrefixes-Array to be filled by append_shareprefix and used by access_share2
func prepare_shareprefixes(shareprefixesLen C.size_t) {
	ishareprefixesLen := int(shareprefixesLen)
	
	shareprefixes = make([]uplink.SharePrefix, 0, ishareprefixesLen)
}

//export append_shareprefix
// append_shareprefix appends one SharePrefix by providing the contents directly
func append_shareprefix(bucket *C.char, prefix *C.char){
	shareprefix := uplink.SharePrefix{
				Bucket:     C.GoString(bucket),
				Prefix: C.GoString(prefix),
			}
	
	shareprefixes = append(shareprefixes, shareprefix)
}

//export access_share2
// access_share2 restricts a given scope with the provided caveat and the temporary encryption shareprefixes
func access_share2(access *C.UplinkAccess, permission C.UplinkPermission) C.UplinkAccessResult { //nolint:golint
	if access == nil {
		return C.UplinkAccessResult{
			error: mallocError(ErrNull.New("access")),
		}
	}

	acc, ok := universe.Get(access._handle).(*Access)
	if !ok {
		return C.UplinkAccessResult{
			error: mallocError(ErrInvalidHandle.New("access")),
		}
	}

	perm := uplink.Permission{
		AllowDownload: bool(permission.allow_download),
		AllowUpload:   bool(permission.allow_upload),
		AllowList:     bool(permission.allow_list),
		AllowDelete:   bool(permission.allow_delete),
	}

	if permission.not_before != 0 {
		perm.NotBefore = time.Unix(int64(permission.not_before), 0)
	}
	if permission.not_after != 0 {
		perm.NotAfter = time.Unix(int64(permission.not_after), 0)
	}

	newAccess, err := acc.Share(perm, shareprefixes...)
	if err != nil {
		return C.UplinkAccessResult{
			error: mallocError(err),
		}
	}
	return C.AccessResult{
		access: (*C.UplinkAccess)(mallocHandle(universe.Add(&Access{newAccess}))),
	}
}