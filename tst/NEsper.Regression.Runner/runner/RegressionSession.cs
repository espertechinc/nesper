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

#if NETCOREAPP3_0_OR_GREATER
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
        public RegressionSession(Configuration configuration, bool useDefaultRuntime)
        {
            Id = Guid.NewGuid();
            // independent providers allows us to have interoperable runtimes that do not
            // cross each other at the session-level... instead, they are isolated runtimes
            // under the given runtime provier
            RuntimeProvider = new EPRuntimeProvider();
            Configuration = configuration;
            UseDefaultRuntime = useDefaultRuntime;
            CleanDirectory(configuration);
        }

        public Guid Id { get; set; }

        public IContainer Container => Configuration.Container;
        
        public EPRuntimeProvider RuntimeProvider { get; }

        public Configuration Configuration { get; }
        
        public bool UseDefaultRuntime { get; }
        
        public EPRuntime Runtime { get; set; }

        public void Reset()
        {
            Runtime = null;
        }

        private void CleanDirectory(Configuration config)
        {
            if (!string.IsNullOrWhiteSpace(config.Compiler.Logging.AuditDirectory)) {
                if (Directory.Exists(config.Compiler.Logging.AuditDirectory)) {
                    foreach (var subDirectory in Directory.GetDirectories(config.Compiler.Logging.AuditDirectory)) {
                        var subDirectoryName = Path.GetFileName(subDirectory);
                        //if (subDirectoryName.StartsWith("generation")) {
                            Directory.Delete(subDirectory, true);
                        //}
                    }
                }
                else {
                    Directory.CreateDirectory(config.Compiler.Logging.AuditDirectory);
                }
            }
        }
        
        public void Dispose()
        {
            Runtime?.Destroy();
            Runtime = null;
        }
        
#if NETCOREAPP3_0_OR_GREATER
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