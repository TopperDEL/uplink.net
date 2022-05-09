package main

// #include "uplink_definitions.h"
import "C"
import (
	"reflect"
	"unsafe"
	
	"storj.io/uplink"
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
func upload_set_custom_metadata2(upload *C.UplinkUpload) *C.UplinkError {
	up, ok := universe.Get(upload._handle).(*Upload)
	if !ok {
		return mallocError(ErrInvalidHandle.New("upload"))
	}

	err := up.upload.SetCustomMetadata(up.scope.ctx, customMetadata)

	return mallocError(err)
}

var index = 0
var array []C.UplinkCustomMetadataEntry

//export prepare_get_custommetadata
// prepare_get_custommetadata 
func prepare_get_custommetadata(object *C.UplinkObject) {
	index = 0
	*(*reflect.SliceHeader)(unsafe.Pointer(&array)) = reflect.SliceHeader{
		Data: uintptr(unsafe.Pointer(object.custom.entries)),
		Len:  int(object.custom.count),
		Cap:  int(object.custom.count),
	}
}

//export get_next_custommetadata
func get_next_custommetadata() C.UplinkCustomMetadataEntry {
	var meta = array[index]
	index = index + 1
	
	return meta
}

//export uplink_commit_upload2
// uplink_commit_upload2 commits a multipart upload to bucket and key started with uplink_begin_upload.
func uplink_commit_upload2(project *C.UplinkProject, bucket_name, object_key, upload_id *C.uplink_const_char) C.UplinkCommitUploadResult { //nolint:golint
	if project == nil {
		return C.UplinkCommitUploadResult{
			error: mallocError(ErrNull.New("project")),
		}
	}
	if bucket_name == nil {
		return C.UplinkCommitUploadResult{
			error: mallocError(ErrNull.New("bucket_name")),
		}
	}
	if object_key == nil {
		return C.UplinkCommitUploadResult{
			error: mallocError(ErrNull.New("object_key")),
		}
	}
	if upload_id == nil {
		return C.UplinkCommitUploadResult{
			error: mallocError(ErrNull.New("upload_id")),
		}
	}

	proj, ok := universe.Get(project._handle).(*Project)
	if !ok {
		return C.UplinkCommitUploadResult{
			error: mallocError(ErrInvalidHandle.New("project")),
		}
	}

	opts := &uplink.CommitUploadOptions{}
	opts.CustomMetadata = customMetadata

	object, err := proj.CommitUpload(proj.scope.ctx, C.GoString(bucket_name), C.GoString(object_key), C.GoString(upload_id), opts)
	return C.UplinkCommitUploadResult{
		error:  mallocError(err),
		object: mallocObject(object),
	}
}

//export uplink_update_object_metadata2
// uplink_update_object_metadata replaces the custom metadata for the object at the specific key with customMetadata.
// Any existing custom metadata will be deleted.
func uplink_update_object_metadata2(project *C.UplinkProject, bucket_name, object_key *C.uplink_const_char, options *C.UplinkUploadObjectMetadataOptions) *C.UplinkError { //nolint:golint
	if project == nil {
		return mallocError(ErrNull.New("project"))
	}

	if bucket_name == nil {
		return mallocError(ErrNull.New("bucket_name"))
	}

	if object_key == nil {
		return mallocError(ErrNull.New("object_key"))
	}

	proj, ok := universe.Get(project._handle).(*Project)
	if !ok {
		return mallocError(ErrInvalidHandle.New("project"))
	}

	err := proj.UpdateObjectMetadata(proj.scope.ctx, C.GoString(bucket_name), C.GoString(object_key), customMetadata, nil)
	return mallocError(err)
}