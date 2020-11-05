using System.Collections.Generic;

using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.compile.stage2
{
    public class FilterSpecValidatedDesc
    {
        public FilterSpecValidatedDesc(
            IList<ExprNode> expressions,
            IList<StmtClassForgeableFactory> additionalForgeables)
        {
            Expressions = expressions;
            AdditionalForgeables = additionalForgeables;
        }

        public IList<ExprNode> Expressions { get; }

        public IList<StmtClassForgeableFactory> AdditionalForgeables { get; }
    }
}