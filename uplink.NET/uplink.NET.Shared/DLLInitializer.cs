using System;
using System.Collections.Generic;
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

        static DLLInitializer()
        {
#if !__ANDROID__
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
#endif
        }

        public static void Init()
        {
            //Method is just to explicitly call the static constructor
        }
    }
}
