using com.espertech.esper.container;

namespace com.espertech.esper.compiler.@internal.util
{
    public static class RoslynCompilerExtensions
    {
        /// <summary>
        /// Resolves a RoslynCompiler from the container.  Attempts to reuse the same compiler for the lifetime
        /// of the container.
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static RoslynCompiler RoslynCompiler(this IContainer container)
        {
            return container.ResolveSingleton<RoslynCompiler>(() => new RoslynCompiler(container));
        }
    }
}