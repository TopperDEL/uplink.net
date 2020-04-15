package main

// #include "uplink_definitions.h"
import "C"
import (
	"reflect"
	"unsafe"
)

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

var index = 0
var array []C.CustomMetadataEntry

//export prepare_get_custommetadata
// prepare_get_custommetadata 
func prepare_get_custommetadata(object *C.Object) {
	index = 0
	*(*reflect.SliceHeader)(unsafe.Pointer(&array)) = reflect.SliceHeader{
		Data: uintptr(unsafe.Pointer(object.custom.entries)),
		Len:  int(object.custom.count),
		Cap:  int(object.custom.count),
	}
}

//export get_next_custommetadata
func get_next_custommetadata() C.CustomMetadataEntry {
	var meta = array[index]
	index = index + 1
	
	return meta
}