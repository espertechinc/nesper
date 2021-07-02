///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Linq;

#if NETCORE
using System.Runtime;
using System.Runtime.Loader;
#endif

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.container;
using com.espertech.esper.runtime.client;

namespace com.espertech.esper.regressionrun.runner
{
    public class RegressionSession : IDisposable
    {
        public RegressionSession(Configuration configuration)
        {
            Configuration = configuration;
            configuration.Compiler.Logging.EnableCode = true;
            configuration.Compiler.Logging.AuditDirectory = @"E:\Logs\NEsper\NEsper.Regression.Review";

            foreach (var directory in Directory.GetDirectories(configuration.Compiler.Logging.AuditDirectory)) {
                var directoryName = Path.GetFileName(directory);
                switch (directoryName) {
                    case "bin":
                    case "obj":
                        break;
                    default:
                        Directory.Delete(directory, true);
                        break;
                }
            }

#if NETCORE
            LoadContext = new DisposableAssemblyLoadContext();
            Configuration.Container.Register<AssemblyLoadContext>(
                    LoadContext,
                    Lifespan.Singleton);
#endif
        }

        public IContainer Container => Configuration.Container;

        public Configuration Configuration { get; }

        public EPRuntime Runtime { get; set; }
        
#if NETCORE
        public DisposableAssemblyLoadContext LoadContext { get; set; }
#endif
        
        public void Dispose()
        {
            Runtime?.Destroy();
            Runtime = null;

#if NETCORE
            LoadContext?.Dispose();
            LoadContext = null;
#endif
        }
        
#if NETCORE
        public class DisposableAssemblyLoadContext : AssemblyLoadContext,
            IDisposable
        {
            /// <summary>
            /// Default constructor.
            /// </summary>
            public DisposableAssemblyLoadContext() : base(null, true)
            {
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting
            /// unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                var assemblies = base.Assemblies.ToList();
                
                Console.WriteLine($"{GetType().Name}: Unloading {assemblies.Count} assemblies.");
                foreach (var assembly in assemblies) {
                    Console.WriteLine($"{GetType().Name}: Unloading {assembly.GetName().Name}.");
                }

                Unload();

                GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                GC.Collect(3, GCCollectionMode.Optimized, true, true);
            }
        }
#endif
    }
} // end of namespace