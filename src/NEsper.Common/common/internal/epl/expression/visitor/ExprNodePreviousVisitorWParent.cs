///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.prev;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.visitor
{
    /// <summary>
    /// Visitor for getting a list of "prev" functions.
    /// </summary>
    public class ExprNodePreviousVisitorWParent : ExprNodeVisitorWithParent
    {
        private IList<Pair<ExprNode, ExprPreviousNode>> _previous;

        public bool IsWalkDeclExprParam => true;

        public bool IsVisit(ExprNode exprNode)
        {
            return true;
        }

        public void Visit(
            ExprNode exprNode,
            ExprNode parentExprNode)
        {
            if (exprNode is ExprPreviousNode node) {
                if (_previous == null) {
                    _previous = new List<Pair<ExprNode, ExprPreviousNode>>();
                }

                _previous.Add(new Pair<ExprNode, ExprPreviousNode>(parentExprNode, node));
            }
        }

        /// <summary>Returns the pair of previous nodes and their parent expression. </summary>
        /// <value>nodes</value>
        public IList<Pair<ExprNode, ExprPreviousNode>> Previous => _previous;
    }
}