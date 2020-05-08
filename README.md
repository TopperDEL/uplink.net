# uplink.NET
[![Storj.io](https://storj.io/img/storj-badge.svg)](https://storj.io)

**A .Net/C#-wrapper for Storj (v3)**

This library enables you to connect to the [storj](https://storj.io)-network to upload and retrieve files to the distributed and secure cloud-storage. It is based on "uplink-c", the C-bindings provided from Storj-labs to connect to the storj-network.

The library is quite new and may contain bugs or missing documentation. Use at your own risk!

If you want to help out, check the open issues, create some or open a PR.

## Use

Search for "[uplink.NET](https://www.nuget.org/packages/uplink.NET)" on Nuget and install the latest version into your project.
See the [wiki](https://github.com/TopperDEL/uplink.net/wiki) for details how to use the library.

You may also try the included sample-app for UWP and Android. There you should find additional details on how to use the library.

## Build (Windows and Android)

**Prerequesits**

Building everything by yourself is possible on Windows. You'll need the following tools:

* Git (to get the uplinkc-repository)
* SWIG (to generate the C#-proxy)
* Go (to build uplink-c)
* [MSYS2](https://www.msys2.org/) (used by Go to compile a Windows-DLL) => see [here](https://github.com/orlp/dev-on-windows/wiki/Installing-GCC--&-MSYS2) for installation instructions
* Visual Studio 2017 or higher to compile the uplink.NET-library
* Android NDK (to build the Android-Release) => install Xamarin with your Visual Studio, start VS, go to Extensions => Android => SDA Manager, Choose Tools and install NDK. Check that your environment variables include "$ANDROID_HOME" and that the path contains no spaces. If it got installed in "C:\Program files (x86)" (default) change the path in the variable to "C:\PROGRA~2\...". Otherwise you'll get an "exec: could not find file"-error during build.

**Noob-Disclaimer**

The process described here is the way I was successfull so far. It might not be the simplest way or you might be successfull using different compilers, versions, systems and whatever. I'm open to any adjustments, enhancements or proposials. Just let me know!

**Building-steps**

First you need to pull the latest version of this repository. Open a command prompt, choose a suitable working directory and enter:
```
git clone https://github.com/topperdel/uplink.net.git
```

If you want you can adjust the uplinkc-Version to use (the github-tag) by setting the STORJ_VERSION-Parameter within build.bat to the one to use.

Then do
```
cd \uplink.net
build
```

This will start the build-process. During the build it will clone the uplinkc-repository into the above chosen working directory.

Once finished it will open the folder "Build-results" within the explorer and it should contain a storj_uplink.dll (Windows x64), a storj_uplink-x86.dll, a "cs-Files"-Folder and an "Android"-folder (containing the so-files for android with the correct ABI-lib-path).

The files already got copied to the correct locations for the Visual Studio solution.

Build the solution.

Feel good.

## Build (Linux) with WSL

**Prerequesits**

Building the linux .so-file on Windows is possible with Windows Subsystem for Linux (WSL). Currently you need four files from the build-process above for Windows and Android.

First of all make sure that git and go are installed on WSL. Starting with a fresh Debian, this would basically be the list of commands (according to [these instructions](https://sal.as/post/install-golan-on-wsl/)):

```
sudo apt-get update
sudo apt-get upgrade
sudo apt-get install wget
wget https://dl.google.com/go/go1.14.2.linux-amd64.tar.gz (you might find a more current version, but the golang-go-package is NOT working)
sudo tar -xvf go1.14.2.linux-amd64.tar.gz (adjust the version to the one downloaded in the step before)
sudo mv go /usr/local
sudo nano ~/.bashrc
```

Scroll down and add these to your .bashrc profile:
```
export GOROOT=/usr/local/go
export GOPATH=$HOME/go
export PATH=$GOPATH/bin:$GOROOT/bin:$PATH
```

Save and close the file with "Ctrl + o" and "Ctrl + x". Then update the current session:
```
source ~/.bashrc
```

Then install git:
```
sudo apt-get install git
```

Clone and build the uplink-c-repo:
```
git clone --branch v1.0.2 https://github.com/storj/uplink-c.git
cd uplink-c
go build
```

If there are not errors, you can build the linux .so-file like this:
```
go build -buildmode c-shared -o storj_uplink.so
```

But be sure to copy the following files to the uplink-c-folder. You may just copy and paste them from the uplink-c-folder on Windows created above into that folder. Calling explorer on the current WSL-folder is as easy as this:
```
explorer.exe .
```

Copy those files and build the shared-libray:
* storj_uplink_second_wrap.c
* storj_uplink.h
* custom_metadata_helper.go
* restrict_scope_helper.go

Copy the generated storj_uplink.so to the runtimes/linux-x64/native-folder under uplink.Net.

## Build (MacOS)

For this task you need a Mac. The procedure is basically the same:
* install go
* install git
* clone the uplink-c-repo
* add the 4 files mentioned above
* build using the following command:
```
go build -buildmode c-shared -o storj_uplink.dylib
```

Place the resulting file in the runtimes/osx-x64/native-folder.

## Testing

To run the test within the VS-solution you have to set the VALID_API_KEY within TestConstants.cs of the test-project to a valid API-key. If you use a different satellite, change that address, too. Storj provides a local test-net you can spin up very quickly that would be possible to use, too.
