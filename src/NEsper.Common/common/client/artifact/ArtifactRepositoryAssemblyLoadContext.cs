using System;
using System.IO;

using com.espertech.esper.common.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.function;
using com.espertech.esper.compat.logging;

#if NETCOREAPP3_0_OR_GREATER
using System.Reflection;
using System.Runtime.Loader;
#endif

namespace com.espertech.esper.common.client.artifact
{
#if NETCOREAPP3_0_OR_GREATER
    public class ArtifactRepositoryAssemblyLoadContext : BaseArtifactRepository,
        IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private AssemblyLoadContext _assemblyLoadContext;
        private readonly TypeResolver _typeResolver;

        /// <summary>
        /// Returns the assembly load context
        /// </summary>
        public AssemblyLoadContext AssemblyLoadContext => _assemblyLoadContext;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parentTypeResolver"></param>
        /// <param name="assemblyResolver"></param>
        public ArtifactRepositoryAssemblyLoadContext(
            TypeResolver parentTypeResolver,
            AssemblyResolver assemblyResolver)
        {
            _assemblyLoadContext = CreateAssemblyLoadContext(
                this,
                assemblyResolver,
                "default-artifact-repository-assembly-load-context",
                true);
            _typeResolver = new ArtifactTypeResolver(this, parentTypeResolver);
        }

        /// <summary>
        /// Constructs an artifact repository with a given load context name.
        /// </summary>
        /// <param name="deploymentId"></param>
        /// <param name="parentTypeResolver"></param>
        /// <param name="assemblyResolver"></param>
        public ArtifactRepositoryAssemblyLoadContext(
            string deploymentId,
            TypeResolver parentTypeResolver,
            AssemblyResolver assemblyResolver)
        {
            _assemblyLoadContext = CreateAssemblyLoadContext(
                this,
                assemblyResolver,
                "artifact-repository-assembly-load-context-" + deploymentId,
                true);
            _typeResolver = new ArtifactTypeResolver(this, parentTypeResolver);
        }

        /// <summary>
        /// Performs cleanup on the artifact repository.
        /// </summary>
        public override void Dispose()
        {
#if DEGUG && TRACE
            Console.WriteLine("Dispose: {0}", _assemblyLoadContext);
#endif
            _assemblyLoadContext?.Unload();
            _assemblyLoadContext = null;
        }

        protected override Supplier<Assembly> MaterializeAssemblySupplier(byte[] image)
        {
            // When materializing an assembly, it is important that we know which AssemblyLoadContext we should be
            // using.  During deployment or rollout, there should a contextual reflection context.  However, outside
            // of these contexts, it may be possible that someone requests a class (and by proxy assembly) to be
            // materialized.

            var assemblyLoadContextReference = new System.WeakReference<AssemblyLoadContext>(_assemblyLoadContext);
            return () => {
                if (assemblyLoadContextReference.TryGetTarget(out var assemblyLoadContext)) {
                    using (assemblyLoadContext.EnterContextualReflection()) {
                        using var stream = new MemoryStream(image);
                        return assemblyLoadContext.LoadFromStream(stream);
                    }
                }

                throw new IllegalStateException("AssemblyLoadContext is no longer in scope");
            };
        }

        public static AssemblyLoadContext CreateAssemblyLoadContext(
            IArtifactRepository repository,
            AssemblyResolver assemblyResolver,
            string contextName,
            bool isCollectable)
        {
            var repositoryReference = new System.WeakReference<IArtifactRepository>(repository);
            var assemblyLoadContext = new AssemblyLoadContext(contextName, isCollectable);
            assemblyLoadContext.Resolving += (
                context,
                assemblyName) => {
                //Console.WriteLine("Resolve: {0} {1}", context, assemblyName);
                using (context.EnterContextualReflection()) {
                    if (repositoryReference.TryGetTarget(out var repositoryInstance)) {
                        var assemblyBaseName = assemblyName.Name;
                        var artifact = repositoryInstance.Resolve(assemblyBaseName);
                        var assembly = artifact?.Assembly;
                        if (assembly == null) {
                            assembly = assemblyResolver?.Invoke(assemblyName);
                        }

                        return assembly;
                    }

                    throw new IllegalStateException("repository is no longer in scope");
                }
            };

            return assemblyLoadContext;
        }

        /// <summary>
        /// Creates a classLoader.
        /// </summary>
        /// <value></value>
        public override TypeResolver TypeResolver => _typeResolver;
    }
#endif
}