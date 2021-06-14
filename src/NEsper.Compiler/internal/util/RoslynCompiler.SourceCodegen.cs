using com.espertech.esper.common.@internal.bytecodemodel.core;

namespace com.espertech.esper.compiler.@internal.util
{
    public partial class RoslynCompiler
    {
        public class SourceCodegen : Source
        {
            private readonly CodegenClass _codegenClass;
            public string Name => _codegenClass.ClassName;
            public string Code => CodegenSyntaxGenerator.Compile(_codegenClass);

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="codegenClass"></param>
            public SourceCodegen(CodegenClass codegenClass)
            {
                _codegenClass = codegenClass;
            }
        }
    }
}