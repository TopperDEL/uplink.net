@echo off
cd ..
IF NOT EXIST "storj\" (
echo *** Cloning storj
git clone https://github.com/storj/storj.git
) else (
echo *** Folder "storj" already there - using it.
)

set /p DUMMY=You may alter the storj-library now. Otherwise or if you're ready: hit ENTER to continue...

echo *** Copying necessary files
copy .\uplink.net\SWIG\*.i .\storj\lib\uplinkc\ /Y
copy .\uplink.net\GO\*.go .\storj\lib\uplinkc\ /Y

cd .\storj\lib\uplinkc\

echo *** Running SWIG to generate a c-file to be add to the dll
swig -csharp -namespace uplink.SWIG storj_uplink_first.i

echo *** Generating Windows-x64-DLL for the first time - this produces a necessary h-file
REM Here one might alter the build settings to generate a DLL for x86, too
go build -o storj_uplink.dll -buildmode c-shared

echo *** Deleting generated cs-files
del *.cs

echo *** Deleting generated c-files
del *.c

echo *** Deleting generated dll-files
del *.dll

echo *** Removing some types that lead to errors with SWIG
findstr /V "GoComplex" storj_uplink.h > storj_uplink2.h
del storj_uplink.h
ren storj_uplink2.h storj_uplink.h

echo *** Running SWIG again with the second i-file. It includes more typemaps.
swig -csharp -namespace uplink.SWIG storj_uplink_second.i

echo *** Generating Windows-x64-DLL for the second time - this includes all SWIG-proxies and is the final-DLL for Windows
REM Here one might alter the build settings to generate a DLL for x86, too
go build -o storj_uplink.dll -buildmode c-shared

echo *** Create result-folder
cd ..\..\..
mkdir Build-Results

echo *** Copy Windows-x64-DLL
copy .\storj\lib\uplinkc\*.dll .\Build-Results\ /Y
copy .\storj\lib\uplinkc\*.dll .\uplink.net\uplink.NET\uplink.NET\ /Y

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

set TOOLCHAIN=%ANDROID_HOME%\ndk-bundle\toolchains\llvm\prebuilt\windows-x86_64\bin

set GOOS=android
set CGO_ENABLED=1

set GOARCH=arm
set CC=%TOOLCHAIN%\armv7a-linux-androideabi16-clang
set CXX=%TOOLCHAIN%\armv7a-linux-androideabi16-clang++
set GOARM=7
echo *** Target: armeabi-v7a
go build -tags linux -buildmode c-shared -o ..\..\..\Build-Results/Android/armeabi-v7a/libstorj_uplink.so storj.io/storj/lib/uplinkc
copy ..\..\..\Build-Results\Android\armeabi-v7a\libstorj_uplink.so ..\..\..\uplink.net\uplink.NET\uplink.NET.Droid\libs\armeabi-v7a\ /Y

set GOARM=

set GOARCH=arm64
set CC=%TOOLCHAIN%\aarch64-linux-android21-clang
set CXX=%TOOLCHAIN%\aarch64-linux-android21-clang++
echo *** Target: arm64-v8a
go build -tags linux -buildmode c-shared -o ..\..\..\Build-Results/Android/arm64-v8a/libstorj_uplink.so storj.io/storj/lib/uplinkc
copy ..\..\..\Build-Results\Android\arm64-v8a\libstorj_uplink.so ..\..\..\uplink.net\uplink.NET\uplink.NET.Droid\libs\arm64-v8a\ /Y

set GOARCH=386
set CC=%TOOLCHAIN%\i686-linux-android16-clang
set CXX=%TOOLCHAIN%\i686-linux-android16-clang++
echo *** Target: x86
go build -tags linux -buildmode c-shared -o ..\..\..\Build-Results/Android/x86/libstorj_uplink.so storj.io/storj/lib/uplinkc
copy ..\..\..\Build-Results\Android\x86\libstorj_uplink.so ..\..\..\uplink.net\uplink.NET\uplink.NET.Droid\libs\x86\ /Y

set GOARCH=amd64
set CC=%TOOLCHAIN%\x86_64-linux-android21-clang
set CXX=%TOOLCHAIN%\x86_64-linux-android21-clang++
echo *** Target: x86_64
go build -tags linux -buildmode c-shared -o ..\..\..\Build-Results/Android/x86_64/libstorj_uplink.so storj.io/storj/lib/uplinkc
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