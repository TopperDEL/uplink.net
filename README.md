# uplink.NET
[![Storj.io](https://storj.io/img/storj-badge.svg)](https://storj.io)

**A .Net/C#-wrapper for Storj (v3)**

This library enables you to connect to the [storj](https://storj.io)-network to upload and retrieve files to the distributed and secure cloud-storage.

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

If you want you can adjust the Storj-Version to use (the github-tag) by setting the STORJ_VERSION-Parameter within build.bat to the one to use.

Then do
```
cd \uplink.net
build
```

This will start the build-process. During the build it will clone the storj-repository into the above chosen working directory.

Once finished it will open the folder "Build-results" within the explorer and it should contain a storj_uplink.dll (Windows x64), a storj_uplink-x86.dll, a "cs-Files"-Folder and an "Android"-folder (containing the so-files for android with the correct ABI-lib-path).

The files already got copied to the correct locations for the Visual Studio solution.

Build the solution.

Feel good.

## Testing

To run the test within the VS-solution you have to set the VALID_API_KEY within TestConstants.cs of the test-project to a valid API-key. If you use a different satellite, change that address, too. Storj provides a local test-net you can spin up very quickly that would be possible to use, too.
