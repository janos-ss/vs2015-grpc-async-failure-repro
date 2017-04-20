# Reproducer for assembly loading issue with `System.Interactive.Async`

The files are based on the Route Guide example in [gRPC Basics: C#][].

Repro steps:

1. Build project

2. Start the Route Guide service using the command line:

        cd RouteGuide
        ../RouteGuideServer/bin/Debug/RouteGuideServer.exe

3. Put breakpoints in the `runRepro` method in `VSIXProject1.VSPackage1.cs`

4. Press `F5` to start debuging **VSIXProject1**

5. In the experimental instance of Visual Studio that opens, open any solution to trigger initialization of the VSIX package

The line `client.ListFeatures(...)` in the `runRepro` method will throw an exception similar to this:

> An exception of type 'System.IO.FileNotFoundException' occurred in Grpc.Core.dll but was not handled in user code
> 
> Additional information: Could not load file or assembly 'System.Interactive.Async, Version=3.0.1000.0, Culture=neutral, PublicKeyToken=94bc3704cddfc263' or one of its dependencies. The system cannot find the file specified.


[gRPC Basics: C#]:http://www.grpc.io/docs/tutorials/basic/csharp.html
