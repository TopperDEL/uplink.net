@echo off
SET STORJ_VERSION=v0.30.2
cd ..
IF NOT EXIST "storj\" (
echo *** Cloning storj
git clone --branch %STORJ_VERSION% https://github.com/storj/storj.git
) else (
echo *** Folder "storj" already there - using it.
)

set /p DUMMY=You may alter the storj-library now. Otherwise or if you're ready: hit ENTER to continue...

echo *** Copying necessary files
copy .\uplink.net\SWIG\*.i .\storj\lib\uplinkc\ /Y
copy .\uplink.net\SWIG\*.exe .\storj\lib\uplinkc\ /Y
copy .\uplink.net\GO\*.go .\storj\lib\uplinkc\ /Y

cd .\storj\lib\uplinkc\

echo *** Replacing the version-variable within the DLL
fart "storj_uplink_second.i" "STORJVERSION" "%STORJ_VERSION%"

echo *** Running SWIG to generate a c-file to be add to the dll
swig -csharp -namespace uplink.SWIG storj_uplink_first.i

echo *** Generating Windows-x64-DLL for the first time - this produces a necessary h-file
set CC=gcc
set CXX=g++
set GOARCH=amd64
set CGO_ENABLED=1
go build -o storj_uplink.dll -buildmode c-shared

echo *** Deleting generated cs-files
del *.cs

echo *** Deleting generated c-files
del *.c

echo *** Deleting generated dll-files
del *.dll

echo *** Removing some types that lead to errors with SWIG
findstr /V "GoComplex" storj_uplink.h > storj_uplink2.h
echo *** Removing unnecessary static check
findstr /V "pointer_matching_GoInt" storj_uplink2.h > storj_uplink3.h
del storj_uplink.h
del storj_uplink2.h
ren storj_uplink3.h storj_uplink.h

echo *** Running SWIG again with the second i-file. It includes more typemaps.
swig -csharp -namespace uplink.SWIG storj_uplink_second.i

echo *** Generating Windows-x86-DLL - this is the final-x86-DLL for Windows
set CC=i686-w64-mingw32-gcc
set CXX=i686-w64-mingw32-g++
set GOARCH=386
set CGO_ENABLED=1
set CGO_CFLAGS=-g -O2 -Wl,--kill-at
set CGO_CXXFLAGS=-g -O2 -Wl,--kill-at
set CGO_FFLAGS=-g -O2 -Wl,--kill-at
set CGO_LDFLAGS=-g -O2 -Wl,--kill-at
go build -ldflags="-s -w" -o storj_uplink-x86.dll -buildmode c-shared
echo *** Generating Windows-x64-DLL - this is the final-x64-DLL for Windows
set CC=gcc
set CXX=g++
set GOARCH=amd64
set CGO_ENABLED=1
set CGO_CFLAGS=-g -O2
set CGO_CXXFLAGS=-g -O2
set CGO_FFLAGS=-g -O2
set CGO_LDFLAGS=-g -O2
go build -ldflags="-s -w" -o storj_uplink.dll -buildmode c-shared

echo *** Replacing ref-modifier from free_string-method
fart "storj_uplink.cs" "storj_uplinkPINVOKE.free_string(ref tmpp0);" "storj_uplinkPINVOKE.free_string(tmpp0);"
fart "storj_uplinkPINVOKE.cs" "public static extern void free_string(ref global::System.IntPtr jarg1);" "public static extern void free_string(global::System.IntPtr jarg1);"

echo *** Create result-folder
cd ..\..\..
mkdir Build-Results

echo *** Copy Windows-DLLs
copy .\storj\lib\uplinkc\*.dll .\Build-Results\ /Y
copy .\storj\lib\uplinkc\storj_uplink-x86.dll .\uplink.net\uplink.NET\uplink.NET\x86\storj_uplink.dll /Y
copy .\storj\lib\uplinkc\storj_uplink.dll .\uplink.net\uplink.NET\uplink.NET\x64\storj_uplink.dll /Y

echo *** Copy cs-files
cd Build-Results
mkdir cs-Files
cd..
copy .\storj\lib\uplinkc\*.cs .\Build-Results\cs-Files /Y
del .\uplink.net\uplink.NET\uplink.NET.Shared\SWIG-Generated\*.cs
copy .\storj\lib\uplinkc\*.cs .\uplink.net\uplink.NET\uplink.NET.Shared\SWIG-Generated\ /Y

echo *** Generating Android-DLLs
echo *** Go and get a coffee...
cd .\storj\lib\uplinkc

echo *** Removing (hopefully) unnecessary reference to Shwlapi.h
findstr /V "hlwapi" storj_uplink_second_wrap.c > storj_uplink_second_wrap2.c
del storj_uplink_second_wrap.c
ren storj_uplink_second_wrap2.c storj_uplink_second_wrap.c

echo *** Removing 64bit-checks - otherwise the android 32-bit-so-files would not get generated
findstr /V "check_for_64_bit" storj_uplink.h > storj_uplink.h2
del storj_uplink.h
ren storj_uplink.h2 storj_uplink.h


set TOOLCHAIN=%ANDROID_HOME%\ndk-bundle\toolchains\llvm\prebuilt\windows-x86_64\bin

set GOOS=android
set CGO_ENABLED=1

set GOARCH=arm
set CC=%TOOLCHAIN%\armv7a-linux-androideabi16-clang
set CXX=%TOOLCHAIN%\armv7a-linux-androideabi16-clang++
set GOARM=7
echo *** Target: armeabi-v7a
go build -ldflags="-s -w" -tags linux -buildmode c-shared -o ..\..\..\Build-Results/Android/armeabi-v7a/libstorj_uplink.so storj.io/storj/lib/uplinkc
copy ..\..\..\Build-Results\Android\armeabi-v7a\libstorj_uplink.so ..\..\..\uplink.net\uplink.NET\uplink.NET.Droid\libs\armeabi-v7a\ /Y

set GOARM=

set GOARCH=arm64
set CC=%TOOLCHAIN%\aarch64-linux-android21-clang
set CXX=%TOOLCHAIN%\aarch64-linux-android21-clang++
echo *** Target: arm64-v8a
go build -ldflags="-s -w" -tags linux -buildmode c-shared -o ..\..\..\Build-Results/Android/arm64-v8a/libstorj_uplink.so storj.io/storj/lib/uplinkc
copy ..\..\..\Build-Results\Android\arm64-v8a\libstorj_uplink.so ..\..\..\uplink.net\uplink.NET\uplink.NET.Droid\libs\arm64-v8a\ /Y

set GOARCH=386
set CC=%TOOLCHAIN%\i686-linux-android16-clang
set CXX=%TOOLCHAIN%\i686-linux-android16-clang++
echo *** Target: x86
go build -ldflags="-s -w" -tags linux -buildmode c-shared -o ..\..\..\Build-Results/Android/x86/libstorj_uplink.so storj.io/storj/lib/uplinkc
copy ..\..\..\Build-Results\Android\x86\libstorj_uplink.so ..\..\..\uplink.net\uplink.NET\uplink.NET.Droid\libs\x86\ /Y

set GOARCH=amd64
set CC=%TOOLCHAIN%\x86_64-linux-android21-clang
set CXX=%TOOLCHAIN%\x86_64-linux-android21-clang++
echo *** Target: x86_64
go build -ldflags="-s -w" -tags linux -buildmode c-shared -o ..\..\..\Build-Results/Android/x86_64/libstorj_uplink.so storj.io/storj/lib/uplinkc
copy ..\..\..\Build-Results\Android\x86_64\libstorj_uplink.so ..\..\..\uplink.net\uplink.NET\uplink.NET.Droid\libs\x86_64\ /Y

cd ..\..\..
%SystemRoot%\explorer.exe .\Build-Results\

echo ***********************************************
echo *** Done! Find the results in .\Build-Results\ *** 
echo *** IMPORTANT: Close this command prompt if    ***
echo *** you want to build again. Otherwise check   ***
echo *** your environment variables as this script  ***
echo *** changed them.                              *** 
echo ***********************************************