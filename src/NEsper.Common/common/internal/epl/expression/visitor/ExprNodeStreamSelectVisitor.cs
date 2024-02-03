///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.core;

namespace com.espertech.esper.common.@internal.epl.expression.visitor
{
    /// <summary>
    ///     Visitor that collects event property identifier information under expression nodes.
    ///     The visitor can be configued to not visit aggregation nodes thus ignoring
    ///     properties under aggregation nodes such as sum, avg, min/max etc.
    /// </summary>
    public class ExprNodeStreamSelectVisitor : ExprNodeVisitor
    {
        private readonly bool isVisitAggregateNodes;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="visitAggregateNodes">
        ///     true to indicate that the visitor should visit aggregate nodes, or falseif the visitor ignores aggregate nodes
        /// </param>
        public ExprNodeStreamSelectVisitor(bool visitAggregateNodes)
        {
            isVisitAggregateNodes = visitAggregateNodes;
        }

        public bool IsWalkDeclExprParam => true;

        public bool HasStreamSelect { get; private set; }

        public bool IsVisit(ExprNode exprNode)
        {
            if (exprNode is ExprLambdaGoesNode) {
                return false;
            }

            if (isVisitAggregateNodes) {
                return true;
            }

            return !(exprNode is ExprAggregateNode);
        }

        public void Visit(ExprNode exprNode)
        {
            if (exprNode is ExprStreamUnderlyingNode) {
                HasStreamSelect = true;
            }

            var streamRef = exprNode as ExprDotNode;
            if (streamRef?.StreamReferencedIfAny != null) {
                HasStreamSelect = true;
            }
        }
    }
} // end of namespace