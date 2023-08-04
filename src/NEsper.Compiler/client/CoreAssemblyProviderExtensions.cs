using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;

using com.espertech.esper.container;

namespace com.espertech.esper.compiler.client
{
    public static class CoreAssemblyProviderExtensions
    {
        /// <summary>
        /// Provides the assemblies should be provided to the compiler as part of the compilation and linking process.
        /// </summary>
        public static IEnumerable<Assembly> GetCoreAssemblies()
        {
#if NETCOREAPP3_0_OR_GREATER
            return AssemblyLoadContext.Default.Assemblies;
#else
            return AppDomain.CurrentDomain.GetAssemblies();
#endif
        }

        /// <summary>
        /// Retrieves an instance of the CoreAssemblyProvider.  If none has been registered, this method
        /// will return the default resolver.
        /// </summary>
        /// <param name="container">the IoC container</param>
        /// <returns></returns>
        public static CoreAssemblyProvider CoreAssemblyProvider(this IContainer container)
        {
            container.CheckContainer();

            lock (container) {
                if (container.DoesNotHave<CoreAssemblyProvider>()) {
                    return GetCoreAssemblies;
                }
            }

            return container.Resolve<CoreAssemblyProvider>();
        }
    }
}