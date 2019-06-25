package main

// #include "uplink_definitions.h"
import "C"

import "unsafe"

//export free_string
// free_string
func free_string(stringptr **C.char) {
	C.free(unsafe.Pointer(stringptr))
}