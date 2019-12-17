///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.collection;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.visitor
{
    /// <summary>
    ///     Visitor that collects event property identifier information under expression nodes.
    ///     The visitor can be configued to not visit aggregation nodes thus ignoring
    ///     properties under aggregation nodes such as sum, avg, min/max etc.
    /// </summary>
    public class ExprNodeIdentifierVisitor : ExprNodeVisitor
    {
        private readonly bool isVisitAggregateNodes;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="visitAggregateNodes">
        ///     true to indicate that the visitor should visit aggregate nodes, or falseif the visitor ignores aggregate nodes
        /// </param>
        public ExprNodeIdentifierVisitor(bool visitAggregateNodes)
        {
            isVisitAggregateNodes = visitAggregateNodes;
            ExprProperties = new List<Pair<int, string>>();
        }

        /// <summary>
        ///     Returns list of event property stream numbers and names that uniquely identify which
        ///     property is from whcih stream, and the name of each.
        /// </summary>
        /// <value>list of event property statement-unique info</value>
        public IList<Pair<int, string>> ExprProperties { get; }

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
            if (!(exprNode is ExprIdentNode)) {
                return;
            }

            var identNode = (ExprIdentNode) exprNode;

            var streamId = identNode.StreamId;
            var propertyName = identNode.ResolvedPropertyName;

            ExprProperties.Add(new Pair<int, string>(streamId, propertyName));
        }
    }
} // end of namespace