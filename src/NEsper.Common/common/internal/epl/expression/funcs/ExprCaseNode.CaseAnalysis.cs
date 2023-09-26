using System.Collections.Generic;

using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    public partial class ExprCaseNode
    {
        public class CaseAnalysis
        {
            private IList<UniformPair<ExprNode>> whenThenNodeList;
            private ExprNode optionalCompareExprNode;
            private ExprNode optionalElseExprNode;

            public CaseAnalysis(
                IList<UniformPair<ExprNode>> whenThenNodeList,
                ExprNode optionalCompareExprNode,
                ExprNode optionalElseExprNode)
            {
                this.whenThenNodeList = whenThenNodeList;
                this.optionalCompareExprNode = optionalCompareExprNode;
                this.optionalElseExprNode = optionalElseExprNode;
            }

            public IList<UniformPair<ExprNode>> WhenThenNodeList => whenThenNodeList;

            public ExprNode OptionalCompareExprNode => optionalCompareExprNode;

            public ExprNode OptionalElseExprNode => optionalElseExprNode;
        }
    }
}