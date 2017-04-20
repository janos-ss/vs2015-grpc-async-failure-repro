//------------------------------------------------------------------------------
// <copyright file="VSPackage1.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Routeguide;
using Grpc.Core;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell.Interop;

namespace VSIXProject1
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(VSPackage1.PackageGuidString)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class VSPackage1 : Package
    {
        /// <summary>
        /// VSPackage1 GUID string.
        /// </summary>
        public const string PackageGuidString = "cb79fa1b-ea24-435e-b841-21d1981c6438";

        /// <summary>
        /// Initializes a new instance of the <see cref="VSPackage1"/> class.
        /// </summary>
        public VSPackage1()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            runRepro();
        }

        private void runRepro()
        {
            var channel = new Channel("127.0.0.1:50052", ChannelCredentials.Insecure);
            var client = new RouteGuideClient(new RouteGuide.RouteGuideClient(channel));

            // Looking for a valid feature
            client.GetFeature(409146138, -746188906);

            // Feature missing.
            //client.GetFeature(0, 0);

            // Looking for features between 40, -75 and 42, -73.
            client.ListFeatures(400000000, -750000000, 420000000, -730000000).Wait();

            // Record a few randomly selected points from the features file.
            //client.RecordRoute(RouteGuideUtil.ParseFeatures(RouteGuideUtil.DefaultFeaturesFile), 10).Wait();

            // Send and receive some notes.
            //client.RouteChat().Wait();

            channel.ShutdownAsync().Wait();
        }

        /// <summary>
        /// Sample client code that makes gRPC calls to the server.
        /// </summary>
        public class RouteGuideClient
        {
            readonly RouteGuide.RouteGuideClient client;

            public RouteGuideClient(RouteGuide.RouteGuideClient client)
            {
                this.client = client;
            }

            /// <summary>
            /// Blocking unary call example.  Calls GetFeature and prints the response.
            /// </summary>
            public void GetFeature(int lat, int lon)
            {
                try
                {
                    Log("*** GetFeature: lat={0} lon={1}", lat, lon);

                    Point request = new Point { Latitude = lat, Longitude = lon };

                    Feature feature = client.GetFeature(request);
                    if (feature.Exists())
                    {
                        Log("Found feature called \"{0}\" at {1}, {2}",
                            feature.Name, feature.Location.GetLatitude(), feature.Location.GetLongitude());
                    }
                    else
                    {
                        Log("Found no feature at {0}, {1}",
                            feature.Location.GetLatitude(), feature.Location.GetLongitude());
                    }
                }
                catch (RpcException e)
                {
                    Log("RPC failed " + e);
                    throw;
                }
            }

            /// <summary>
            /// Server-streaming example. Calls listFeatures with a rectangle of interest. Prints each response feature as it arrives.
            /// </summary>
            public async System.Threading.Tasks.Task ListFeatures(int lowLat, int lowLon, int hiLat, int hiLon)
            {
                try
                {
                    Log("*** ListFeatures: lowLat={0} lowLon={1} hiLat={2} hiLon={3}", lowLat, lowLon, hiLat,
                        hiLon);

                    Rectangle request = new Rectangle
                    {
                        Lo = new Point { Latitude = lowLat, Longitude = lowLon },
                        Hi = new Point { Latitude = hiLat, Longitude = hiLon }
                    };

                    using (var call = client.ListFeatures(request))
                    {
                        var responseStream = call.ResponseStream;
                        StringBuilder responseLog = new StringBuilder("Result: ");

                        while (await responseStream.MoveNext())
                        {
                            Feature feature = responseStream.Current;
                            responseLog.Append(feature.ToString());
                        }
                        Log(responseLog.ToString());
                    }
                }
                catch (RpcException e)
                {
                    Log("RPC failed " + e);
                    throw;
                }
            }

            private void Log(string s, params object[] args)
            {
                Debug.WriteLine(string.Format(s, args));
            }

            private void Log(string s)
            {
                Debug.WriteLine(s);
            }
        }
        #endregion
    }
}
