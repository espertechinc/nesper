using System;

#if NETSTANDARD
using System.Runtime.Loader;
#endif

using com.espertech.esper.compat;
using com.espertech.esper.container;

namespace com.espertech.esper.common.@internal.util
{
    public static class AssemblyLoadContextExtensions
    {
#if NETSTANDARD
        public static AssemblyLoadContext LoadContext(this IContainer container)
        {
            container.CheckContainer();
            return container.Has<AssemblyLoadContext>()
                ? container.Resolve<AssemblyLoadContext>()
                : AssemblyLoadContext.Default;
        }

        public static IDisposable EnterContextualReflection(this IContainer container)
        {
            return LoadContext(container).EnterContextualReflection();
        }
#else
        public static IDisposable EnterContextualReflection(this IContainer container)
        {
            return new VoidDisposable();
        }
#endif
    }
}