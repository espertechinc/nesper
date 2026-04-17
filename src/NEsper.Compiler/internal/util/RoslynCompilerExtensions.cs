using com.espertech.esper.common.client.artifact;
using com.espertech.esper.compiler.client;

namespace com.espertech.esper.compiler.@internal.util
{
    public static class RoslynCompilerExtensions
    {
        /// <summary>
        /// Creates a RoslynCompiler from explicit dependencies.
        /// </summary>
        /// <param name="metadataReferenceResolver"></param>
        /// <param name="coreAssemblyProvider"></param>
        /// <returns></returns>
        public static RoslynCompiler RoslynCompiler(
            MetadataReferenceResolver metadataReferenceResolver,
            CoreAssemblyProvider coreAssemblyProvider)
        {
            return new RoslynCompiler(metadataReferenceResolver, coreAssemblyProvider);
        }
    }
}