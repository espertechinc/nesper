using com.espertech.esper.common.@internal.bytecodemodel.core;

namespace com.espertech.esper.compiler.@internal.util
{
    public partial class RoslynCompiler
    {
        public class SourceBasic : Source
        {
            public string Name { get; set; }
            public string Code { get; set; }

            public SourceBasic()
            {
            }

            public SourceBasic(
                string sourceName,
                string code)
            {
                Name = sourceName;
                Code = code;
            }
        }
    }
}