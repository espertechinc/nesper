///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.collection;
using com.espertech.esper.epl.enummethod.dot;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.visitor
{
    /// <summary>
    /// Visitor that collects event property identifier information under expression nodes.
    /// The visitor can be configued to not visit aggregation nodes thus ignoring
    /// properties under aggregation nodes such as sum, avg, min/max etc.
    /// </summary>
    public class ExprNodeIdentifierVisitor : ExprNodeVisitor
    {
        private readonly IList<Pair<int, string>> _exprProperties;
        private readonly bool _isVisitAggregateNodes;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="visitAggregateNodes">
        /// true to indicate that the visitor should visit aggregate nodes, or false
        /// if the visitor ignores aggregate nodes
        /// </param>
        public ExprNodeIdentifierVisitor(bool visitAggregateNodes)
        {
            _isVisitAggregateNodes = visitAggregateNodes;
            _exprProperties = new List<Pair<int, string>>();
        }

        public bool IsVisit(ExprNode exprNode)
        {
            if (exprNode is ExprLambdaGoesNode)
            {
                return false;
            }

            if (_isVisitAggregateNodes)
            {
                return true;
            }

            return !(exprNode is ExprAggregateNode);
        }

        /// <summary>
        /// Returns list of event property stream numbers and names that uniquely identify which
        /// property is from whcih stream, and the name of each.
        /// </summary>
        /// <value>list of event property statement-unique INFO</value>
        public IList<Pair<int, string>> ExprProperties
        {
            get { return _exprProperties; }
        }

        public void Visit(ExprNode exprNode)
        {
            if (!(exprNode is ExprIdentNode))
            {
                return;
            }

            var identNode = (ExprIdentNode) exprNode;

            var streamId = identNode.StreamId;
            var propertyName = identNode.ResolvedPropertyName;

            _exprProperties.Add(new Pair<int, string>(streamId, propertyName));
        }
    }
} // end of namespace
