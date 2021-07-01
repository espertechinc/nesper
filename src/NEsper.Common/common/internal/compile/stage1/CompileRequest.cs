using System.Collections.Generic;

using com.espertech.esper.common.@internal.compile.stage3;

namespace com.espertech.esper.common.@internal.compile.stage1
{
    /// <summary>
    /// A request to compile a set of classes.
    /// </summary>
    public class CompileRequest
    {
        private readonly ICollection<CompilableClass> _classes;
        private readonly ModuleCompileTimeServices _moduleCompileTimeServices;

        public ICollection<CompilableClass> Classes => _classes;

        public ModuleCompileTimeServices ModuleCompileTimeServices => _moduleCompileTimeServices;

        public CompileRequest(
            ICollection<CompilableClass> classes,
            ModuleCompileTimeServices moduleCompileTimeServices)
        {
            _classes = classes;
            _moduleCompileTimeServices = moduleCompileTimeServices;
        }
    }
}