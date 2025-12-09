using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace uplink.SWIG
{
    internal class DLLInitializer
    {
        enum LoadLibraryFlags : uint
        {
            None = 0,
            DONT_RESOLVE_DLL_REFERENCES = 0x00000001,
            LOAD_IGNORE_CODE_AUTHZ_LEVEL = 0x00000010,
            LOAD_LIBRARY_AS_DATAFILE = 0x00000002,
            LOAD_LIBRARY_AS_DATAFILE_EXCLUSIVE = 0x00000040,
            LOAD_LIBRARY_AS_IMAGE_RESOURCE = 0x00000020,
            LOAD_LIBRARY_SEARCH_APPLICATION_DIR = 0x00000200,
            LOAD_LIBRARY_SEARCH_DEFAULT_DIRS = 0x00001000,
            LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR = 0x00000100,
            LOAD_LIBRARY_SEARCH_SYSTEM32 = 0x00000800,
            LOAD_LIBRARY_SEARCH_USER_DIRS = 0x00000400,
            LOAD_WITH_ALTERED_SEARCH_PATH = 0x00000008
        }

        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
        static extern System.IntPtr LoadLibraryEx(string lpFileName, System.IntPtr hReservedNull, LoadLibraryFlags dwFlags);

        // Linux library loading using libdl
        // We use NativeLibrary on .NET Core/5+ as it handles cross-platform library loading better
        private const int RTLD_NOW = 0x002;
        private const int RTLD_GLOBAL = 0x100;

        // Try libdl.so.2 first (most common), then libdl.so as fallback
        // Note: On some Linux distributions (Alpine, musl-based), the library name may differ
        [System.Runtime.InteropServices.DllImport("libdl.so.2", EntryPoint = "dlopen")]
        private static extern System.IntPtr dlopen_linux2(string filename, int flags);

        [System.Runtime.InteropServices.DllImport("libdl.so", EntryPoint = "dlopen")]
        private static extern System.IntPtr dlopen_linux(string filename, int flags);

        // macOS library loading using libdl (different path)
        [System.Runtime.InteropServices.DllImport("libdl.dylib", EntryPoint = "dlopen")]
        private static extern System.IntPtr dlopen_macos(string filename, int flags);

        static DLLInitializer()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (System.Environment.Is64BitProcess)
                {
                    // Load 64-bit dll
                    LoadLibraryEx(@"x64/storj_uplink.dll", System.IntPtr.Zero, LoadLibraryFlags.None);
                }
                else
                {
                    // Load 32-bit dll
                    LoadLibraryEx(@"x86/storj_uplink.dll", System.IntPtr.Zero, LoadLibraryFlags.None);
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // On Linux, try to load the native library from the runtimes folder
                // This helps .NET Core/5+ find the correct library on Linux
                TryLoadLinuxLibrary("runtimes/linux-x64/native/libstorj_uplink.so");
                TryLoadLinuxLibrary("libstorj_uplink.so");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // On macOS, the library might be in different locations
                TryLoadMacOSLibrary("runtimes/osx-x64/native/libstorj_uplink.dylib");
                TryLoadMacOSLibrary("libstorj_uplink.dylib");
            }
        }

        private static void TryLoadLinuxLibrary(string path)
        {
            // Try libdl.so.2 first (most common on glibc-based systems like Ubuntu, Debian, RHEL)
            // Then fall back to libdl.so (for other distributions)
            try
            {
                dlopen_linux2(path, RTLD_NOW | RTLD_GLOBAL);
                return; // Success with libdl.so.2
            }
            catch (DllNotFoundException)
            {
                // libdl.so.2 not found, try libdl.so
            }
            catch
            {
                // Other error with libdl.so.2, try libdl.so
            }

            try
            {
                dlopen_linux(path, RTLD_NOW | RTLD_GLOBAL);
            }
            catch
            {
                // Silently ignore - the library might be found through default search paths
            }
        }

        private static void TryLoadMacOSLibrary(string path)
        {
            try
            {
                dlopen_macos(path, RTLD_NOW | RTLD_GLOBAL);
            }
            catch
            {
                // Silently ignore - the library might be found through default search paths
            }
        }

        public static void Init()
        {
            //Method is just to explicitly call the static constructor
            
            // Pin SWIG callback delegates to prevent GC collection on .NET Core/5+ Linux.
            // This must be called after the native library is loaded but before any P/Invoke calls.
            uplink.NET.SWIGHelpers.DelegateKeepAlive.Initialize();
        }
    }
}
