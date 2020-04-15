package main

// #include "uplink_definitions.h"
import "C"

var customMetadata = map[string]string{}

//export prepare_custommetadata
// prepare_custommetadata creates a temporary SharePrefixes-Array to be filled by append_custommetadata and used by upload_set_custom_metadata2
func prepare_custommetadata() {
	customMetadata = make(map[string]string)
}

//export append_custommetadata
// append_custommetadata appends one CustomMetadata by providing the contents directly
func append_custommetadata(key *C.char, value *C.char){
	customMetadata[C.GoString(key)] = C.GoString(value)
}

//export upload_set_custom_metadata2
// upload_set_custom_metadata2 sets the customMetadata on an upload
func upload_set_custom_metadata2(upload *C.Upload) *C.Error {
	up, ok := universe.Get(upload._handle).(*Upload)
	if !ok {
		return mallocError(ErrInvalidHandle.New("upload"))
	}

	err := up.upload.SetCustomMetadata(up.scope.ctx, customMetadata)

	return mallocError(err)
}