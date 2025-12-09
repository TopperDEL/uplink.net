# Investigation: Linux SEGFAULT Issues in uplink.NET

## Summary

This document details the investigation into SEGFAULT issues that occur when using uplink.NET on Linux with .NET 5+ (CoreCLR), but not on Windows or with older Mono runtimes.

## Background

The uplink.NET library uses SWIG (version 4.0.1) to generate C# bindings for a Go-based native library (`storj_uplink.dll` on Windows, `libstorj_uplink.so` on Linux). The SEGFAULTs began appearing when:

1. Users migrated from .NET Framework 4.x / Mono to .NET 5/6/7/8+
2. Running on Linux with CoreCLR instead of Mono

## Root Causes Identified

### 1. Delegate Lifetime and Garbage Collection Issues (CRITICAL)

**Problem**: In the SWIG-generated `storj_uplinkPINVOKE.cs`, delegates are created and passed to native code for callbacks:

```csharp
static ExceptionDelegate applicationDelegate = new ExceptionDelegate(SetPendingApplicationException);
static SWIGStringDelegate stringDelegate = new SWIGStringDelegate(CreateString);
```

These delegates are registered with native code via:
- `SWIGRegisterExceptionCallbacks_storj_uplink()`
- `SWIGRegisterStringCallback_storj_uplink()`

The native code stores function pointers to these delegates. However, on .NET Core/5+, the JIT compiler and GC are more aggressive than Mono. The delegates can be:
- Moved in memory during compaction
- Collected if not explicitly rooted

When native code later tries to call through these function pointers, it encounters invalid memory addresses, causing SEGFAULT.

**Solution**: Pin the delegates using `GCHandle.Alloc()` to prevent them from being collected or moved:

```csharp
static System.Runtime.InteropServices.GCHandle[] delegateHandles;

static SWIGExceptionHelper() {
    delegateHandles = new System.Runtime.InteropServices.GCHandle[] {
        System.Runtime.InteropServices.GCHandle.Alloc(applicationDelegate),
        // ... all other delegates
    };
    // Then register callbacks
}
```

### 2. Thread-Safety Issues in Exception Handling

**Problem**: The `SWIGPendingException` class has a thread-safety bug:

```csharp
[global::System.ThreadStatic]
private static global::System.Exception pendingException = null;  // Thread-static
private static int numExceptionsPending = 0;                       // NOT thread-static
private static global::System.Object exceptionsLock = null;

public static void Set(global::System.Exception e) {
    // ...
    lock(exceptionsLock) {
        numExceptionsPending++;  // Lock can be null!
    }
}
```

The `exceptionsLock` is initialized in a static constructor, but `lock(null)` throws `ArgumentNullException`. Additionally, multiple threads can race on `numExceptionsPending`.

**Solution**: Use `Interlocked` operations for thread-safe counter updates:

```csharp
private static int numExceptionsPending = 0;

public static void Set(global::System.Exception e) {
    // ...
    System.Threading.Interlocked.Increment(ref numExceptionsPending);
}

public static global::System.Exception Retrieve() {
    // ...
    System.Threading.Interlocked.Decrement(ref numExceptionsPending);
}
```

### 3. Double-Free Memory Issues

**Problem**: When native cleanup functions are called (like `uplink_close_project`, `uplink_close_download`), the SWIG wrapper objects still believe they own the memory. When the C# object is disposed or finalized, it tries to free the already-freed memory.

**Solution**: A `DisposalHelper` class was added to clear the `swigCMemOwn` flag after native cleanup:

```csharp
public static void ClearOwnership(IDisposable swigObject)
{
    var field = type.GetField("swigCMemOwn", BindingFlags.Instance | BindingFlags.NonPublic);
    if (field != null)
        field.SetValue(swigObject, false);
}
```

### 4. Native Library Loading on Linux

**Problem**: The `DLLInitializer` class only handled Windows library loading, leaving Linux to rely on default library search paths which may fail.

**Solution**: Added Linux and macOS support using `dlopen`:

```csharp
else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
    TryLoadLinuxLibrary("runtimes/linux-x64/native/libstorj_uplink.so");
    TryLoadLinuxLibrary("libstorj_uplink.so");
}
```

## Changes Made

### storj_uplinkPINVOKE.cs

1. Added `GCHandle` array to pin all callback delegates
2. Changed `SWIGPendingException` to use `Interlocked` operations instead of `lock`
3. Added `GCHandle` for string delegate

### DLLInitializer.cs

1. Added Linux library loading using `dlopen` from `libdl.so.2`
2. Added macOS library loading using `dlopen` from `libdl.dylib`

### DisposalHelper.cs (previously added)

1. Helper class to clear SWIG ownership after native cleanup functions are called

## Testing Recommendations

1. Run existing tests on Linux with .NET 8
2. Test concurrent operations (multiple uploads/downloads simultaneously)
3. Test long-running operations with GC pressure
4. Test application shutdown scenarios

## Related Issues

- Double-free SEGFAULT fix (#49)
- This investigation

## References

- [.NET P/Invoke documentation](https://docs.microsoft.com/en-us/dotnet/standard/native-interop/pinvoke)
- [GCHandle documentation](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.gchandle)
- [SWIG C# documentation](http://www.swig.org/Doc4.0/CSharp.html)
