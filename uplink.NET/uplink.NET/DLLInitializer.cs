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
        private const int RTLD_NOW = 0x002;
        private const int RTLD_GLOBAL = 0x100;

        [System.Runtime.InteropServices.DllImport("libdl.so.2", EntryPoint = "dlopen")]
        static extern System.IntPtr dlopen_linux(string filename, int flags);

        [System.Runtime.InteropServices.DllImport("libdl.so.2", EntryPoint = "dlerror")]
        static extern System.IntPtr dlerror_linux();

        // macOS library loading using libdl (different path)
        [System.Runtime.InteropServices.DllImport("libdl.dylib", EntryPoint = "dlopen")]
        static extern System.IntPtr dlopen_macos(string filename, int flags);

        static DLLInitializer()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (System.Environment.Is64BitProcess)
                {
                    // Load 64-bit dll
                    var handle = LoadLibraryEx(@"x64/storj_uplink.dll", System.IntPtr.Zero, LoadLibraryFlags.None);
                    if (handle != System.IntPtr.Zero)
                    {
                        //Ignore it - there is a possible fallback
                    }
                }
                else
                {
                    // Load 32-bit dll
                    var handle = LoadLibraryEx(@"x86/storj_uplink.dll", System.IntPtr.Zero, LoadLibraryFlags.None);
                    if (handle != System.IntPtr.Zero)
                    {
                        //Ignore it - there is a possible fallback
                    }
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
            try
            {
                var handle = dlopen_linux(path, RTLD_NOW | RTLD_GLOBAL);
                // Even if handle is null, don't throw - let the P/Invoke use default search paths
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
                var handle = dlopen_macos(path, RTLD_NOW | RTLD_GLOBAL);
                // Even if handle is null, don't throw - let the P/Invoke use default search paths
            }
            catch
            {
                // Silently ignore - the library might be found through default search paths
            }
        }

        public static void Init()
        {
            //Method is just to explicitly call the static constructor
        }
    }
}
