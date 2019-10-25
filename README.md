# uplink.NET
[![Storj.io](https://storj.io/img/storj-badge.svg)](https://storj.io)

**A .Net/C#-wrapper for Storj (v3)**

This library enables you to connect to the storj-network to upload and retrieve files to the distributed and secure cloud-storage.

The library is quite new and may contain bugs or missing documentation. Use at your own risk!

If you want to help out, check the open issues, create some or open a PR.

## Use

Search for "[uplink.NET](https://www.nuget.org/packages/uplink.NET)" on Nuget and install the latest version into your project.
See the [wiki](https://github.com/TopperDEL/uplink.net/wiki) for details how to use the library.

You may also try the included sample-app for UWP and Android. There you should find additional details on how to use the library.

## Build

**Prerequesits**

Building everything by yourself is possible on Windows. You'll need the following tools:

* Git (to get the storj-repository)
* SWIG (to generate the C#-proxy)
* Go (to build the C-Binding to storj/uplink)
* [MSYS2](https://www.msys2.org/) (used by go to compile a Windows-DLL) => see [here](https://github.com/orlp/dev-on-windows/wiki/Installing-GCC--&-MSYS2) for installation instructions
* Visual Studio 2017 or higher to compile the uplink.NET-library
* Android NDK (to build the Android-Release) => install Xamarin with your Visual Studio, start VS, go to Extensions => Android => SDA Manager, Choose Tools and install NDK. Check that your environment variables include "$ANDROID_HOME" and that the path contains no spaces. If it got installed in "C:\Program files (x86)" (default) change the path in the variable to "C:\PROGRA~2\...". Otherwise you'll get an "exec: could not find file"-error during build.

**Noob-Disclaimer**

The process described here is the way I was successfull so far. It might not be the simplest way or you might be successfull using different compilers, versions, systems and whatever. I'm open to any adjustments, enhancements or proposials. Just let me know!

**Building-steps**

First you need to pull the latest version of this repository. Open a command prompt, choose a suitable working directory and enter:
```
git clone https://github.com/topperdel/uplink.net.git
```

Then do
```
cd \uplink.net
build
```

This will start the build-process. During the build it will clone the storj-repository into the above chosen working directory.

Once finished it will open the folder "Build-results" within the explorer and it should contain a storj_uplink.dll (Windows x64), a storj_uplink-x86.dll, a "cs-Files"-Folder and an "Android"-folder (containing the so-files for android with the correct ABI-lib-path).

The files already got copied to the correct locations for the Visual Studio solution.

Unfortunately, there are still some manual tasks to do:

In the file storj_uplinkPINVOKE.cs search for "free_string" and remove the "ref" from the parameter jarg1. This should be handled by SWIG, but I don't know how.
In the file storj_uplink.cs also remove the "ref" from the call to storj_uplinkPINVOKE.free_string().

Build the solution.
