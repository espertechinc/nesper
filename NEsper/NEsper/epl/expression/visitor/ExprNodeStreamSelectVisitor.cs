///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.enummethod.dot;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.dot;

namespace com.espertech.esper.epl.expression.visitor
{
    /// <summary>
    /// Visitor that collects event property identifier information under expression nodes.
    /// The visitor can be configued to not visit aggregation nodes thus ignoring
    /// properties under aggregation nodes such as sum, avg, min/max etc.
    /// </summary>
    public class ExprNodeStreamSelectVisitor : ExprNodeVisitor {
        private readonly bool isVisitAggregateNodes;
        private bool hasStreamSelect;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="visitAggregateNodes">
        /// true to indicate that the visitor should visit aggregate nodes, or false
        /// if the visitor ignores aggregate nodes
        /// </param>
        public ExprNodeStreamSelectVisitor(bool visitAggregateNodes) {
            this.isVisitAggregateNodes = visitAggregateNodes;
        }
    
        public bool IsVisit(ExprNode exprNode) {
            if (exprNode is ExprLambdaGoesNode) {
                return false;
            }
    
            if (isVisitAggregateNodes) {
                return true;
            }
    
            return !(exprNode is ExprAggregateNode);
        }
    
        public bool HasStreamSelect() {
            return hasStreamSelect;
        }
    
        public void Visit(ExprNode exprNode) {
            if (exprNode is ExprStreamUnderlyingNode) {
                hasStreamSelect = true;
            }
            if (exprNode is ExprDotNode) {
                ExprDotNode streamRef = (ExprDotNode) exprNode;
                if (streamRef.StreamReferencedIfAny != null) {
                    hasStreamSelect = true;
                }
            }
        }
    }
} // end of namespace
