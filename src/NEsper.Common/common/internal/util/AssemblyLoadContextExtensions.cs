using System;

#if NETSTANDARD
using System.Runtime.Loader;

using com.espertech.esper.common.client.assembly;
#endif

using com.espertech.esper.compat;
using com.espertech.esper.container;

namespace com.espertech.esper.common.@internal.util
{
    public static class AssemblyLoadContextExtensions
    {
#if NETSTANDARD
        public static AssemblyLoadContext GetLoadContext(this IContainer container, CompilationContext compilationContext)
        {
            container.CheckContainer();
            if (container.Has<LoadContextResolver>()) {
                // Allows the configuration to specify a LoadContextResolver that will be provided
                // with the compilation context.  If the resolver does not provide a value, then
                // we will use the default behavior.
                var resolver = container.Resolve<LoadContextResolver>();
                var context = resolver.GetLoadContext(compilationContext);
                if (context != null) {
                    return context;
                }
            }
            
            return container.Has<AssemblyLoadContext>()
                ? container.Resolve<AssemblyLoadContext>()
                : AssemblyLoadContext.Default;
        }
        
        public static IDisposable EnterContextualReflection(this IContainer container, CompilationContext compilationContext)
        {
            return GetLoadContext(container).EnterContextualReflection();
        }
#else
        public static IDisposable EnterContextualReflection(this IContainer container)
        {
            return new VoidDisposable();
        }
#endif
    }
}