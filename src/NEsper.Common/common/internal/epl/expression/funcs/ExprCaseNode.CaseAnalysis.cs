using System.Collections.Generic;

using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    public partial class ExprCaseNode
    {
        public class CaseAnalysis
        {
            private IList<UniformPair<ExprNode>> _whenThenNodeList;
            private ExprNode _optionalCompareExprNode;
            private ExprNode _optionalElseExprNode;

            public CaseAnalysis(
                IList<UniformPair<ExprNode>> whenThenNodeList,
                ExprNode optionalCompareExprNode,
                ExprNode optionalElseExprNode)
            {
                this._whenThenNodeList = whenThenNodeList;
                this._optionalCompareExprNode = optionalCompareExprNode;
                this._optionalElseExprNode = optionalElseExprNode;
            }

            public IList<UniformPair<ExprNode>> WhenThenNodeList => _whenThenNodeList;

            public ExprNode OptionalCompareExprNode => _optionalCompareExprNode;

            public ExprNode OptionalElseExprNode => _optionalElseExprNode;
        }
    }
}