using System.Reflection;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.compile.stage1
{
    /// <summary>
    /// The response (non-exceptional) to a compilation request.
    /// </summary>
    public class CompileResponse
    {
        private readonly CompileRequest _request;
        private readonly Pair<Assembly, byte[]> _assemblyWithImage;

        public CompileRequest Request => _request;

        public Pair<Assembly, byte[]> AssemblyWithImage => _assemblyWithImage;

        public Assembly Assembly => _assemblyWithImage.First;
        
        public CompileResponse(
            CompileRequest request,
            Pair<Assembly, byte[]> assemblyWithImage)
        {
            _request = request;
            _assemblyWithImage = assemblyWithImage;
        }
    }
}