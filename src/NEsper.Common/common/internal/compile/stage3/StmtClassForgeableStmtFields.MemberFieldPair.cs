using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;

namespace com.espertech.esper.common.@internal.compile.stage3
{
    public partial class StmtClassForgeableStmtFields
    {
        private class MemberFieldPair
        {
            public MemberFieldPair(
                CodegenTypedParam member,
                CodegenField field)
            {
                Member = member;
                Field = field;
            }

            public CodegenTypedParam Member { get; }

            public CodegenField Field { get; }
        }
    }
}