#if NETSTANDARD
using System.Runtime.Loader;
#endif

namespace com.espertech.esper.common.client.assembly
{
    public interface LoadContextResolver
    {
#if NETSTANDARD
        AssemblyLoadContext GetLoadContext(CompilationContext context);
 #endif
    }
}