using System.Collections.Generic;

using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.compile.stage1
{
    /// <summary>
    /// A request to compile a set of classes.
    /// </summary>
    public class CompileRequest
    {
        private readonly ICollection<CompilableClass> _classes;
        private readonly ModuleCompileTimeServices _moduleCompileTimeServices;
        private readonly Consumer<object> _compileResultConsumer;

        public ICollection<CompilableClass> Classes => _classes;

        public ModuleCompileTimeServices ModuleCompileTimeServices => _moduleCompileTimeServices;

        public Consumer<object> CompileResultConsumer => _compileResultConsumer;

        public CompileRequest(
            ICollection<CompilableClass> classes,
            ModuleCompileTimeServices moduleCompileTimeServices,
            Consumer<object> compileResultConsumer)
        {
            _classes = classes;
            _moduleCompileTimeServices = moduleCompileTimeServices;
            _compileResultConsumer = compileResultConsumer;
        }
    }
}