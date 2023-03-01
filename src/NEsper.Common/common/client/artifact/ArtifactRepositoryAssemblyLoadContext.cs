using System;
using System.IO;
using System.Linq;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.compat.logging;

#if NETCORE
using System.Reflection;
using System.Runtime.Loader;
#endif

namespace com.espertech.esper.common.client.artifact
{
#if NETCORE
    public class ArtifactRepositoryAssemblyLoadContext : BaseArtifactRepository, IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private AssemblyLoadContext _assemblyLoadContext;

        /// <summary>
        /// Returns the assembly load context
        /// </summary>
        public AssemblyLoadContext AssemblyLoadContext => _assemblyLoadContext;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ArtifactRepositoryAssemblyLoadContext()
        {
            _assemblyLoadContext = CreateAssemblyLoadContext(
                this,
                "default-artifact-repository-assembly-load-context",
                true);
        }

        /// <summary>
        /// Constructs an artifact repository with a given load context name.
        /// </summary>
        /// <param name="deploymentId"></param>
        public ArtifactRepositoryAssemblyLoadContext(string deploymentId)
        {
            _assemblyLoadContext = CreateAssemblyLoadContext(
                this,
                "artifact-repository-assembly-load-context-" + deploymentId,
                true);
        }

        /// <summary>
        /// Performs cleanup on the artifact repository.
        /// </summary>
        public override void Dispose()
        {
            Console.WriteLine("Dispose: {0}", _assemblyLoadContext);
            _assemblyLoadContext?.Unload();
            _assemblyLoadContext = null;
        }

        protected override Assembly MaterializeAssembly(byte[] image)
        {
            // When materializing an assembly, it is important that we know which AssemblyLoadContext we should be
            // using.  During deployment or rollout, there should a contextual reflection context.  However, outside
            // of these contexts, it may be possible that someone requests a class (and by proxy assembly) to be
            // materialized.

            using (_assemblyLoadContext.EnterContextualReflection()) {
                using var stream = new MemoryStream(image);
                return _assemblyLoadContext.LoadFromStream(stream);
            }
        }
        
        public static AssemblyLoadContext CreateAssemblyLoadContext(
            IArtifactRepository repository,
            string contextName,
            bool isCollectable)
        {
            var assemblyLoadContext = new AssemblyLoadContext(contextName, isCollectable);
            assemblyLoadContext.Resolving += (context, assemblyName) => {
                //Console.WriteLine("Resolve: {0} {1}", context, assemblyName);
                using (context.EnterContextualReflection()) {
                    var assemblyBaseName = assemblyName.Name;
                    var artifact = repository.Resolve(assemblyBaseName);
                    var assembly = artifact?.Assembly;
                    return assembly;
                }
            };
            assemblyLoadContext.ResolvingUnmanagedDll += (assembly, assemblyName) => {
                //Console.WriteLine("ResolvingUnmanagedDll: {0}", assemblyName);
                throw new NotImplementedException();
            };
            assemblyLoadContext.Unloading += (context) => {
                //Console.WriteLine("Unloading: {0}", context);
            };
            
            return assemblyLoadContext;
        }
    }
#endif
}