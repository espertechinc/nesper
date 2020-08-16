using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.@internal.compile.stage3;

namespace com.espertech.esper.common.@internal.compile.stage1
{
    /// <summary>
    /// The response (non-exceptional) to a compilation request.
    /// </summary>
    public class CompileResponse
    {
        private readonly CompileRequest _request;
        private readonly Assembly _assembly;

        public CompileRequest Request => _request;

        public Assembly Assembly => _assembly;

        public CompileResponse(
            CompileRequest request,
            Assembly assembly)
        {
            _request = request;
            _assembly = assembly;
        }
    }
}