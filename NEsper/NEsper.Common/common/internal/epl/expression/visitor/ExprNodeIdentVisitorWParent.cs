///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.collection;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.expression.visitor
{
    /// <summary>
    /// Visitor for getting a list of identifier nodes with their parent node, which can be null if there is no parent node.
    /// </summary>
    [Serializable]
    public class ExprNodeIdentVisitorWParent : ExprNodeVisitorWithParent
    {
        private readonly IList<Pair<ExprNode, ExprIdentNode>> _identNodes = new List<Pair<ExprNode, ExprIdentNode>>();

        public bool IsVisit(ExprNode exprNode)
        {
            return true;
        }

        public void Visit(
            ExprNode exprNode,
            ExprNode parentExprNode)
        {
            if (exprNode is ExprIdentNode) {
                _identNodes.Add(new Pair<ExprNode, ExprIdentNode>(parentExprNode, (ExprIdentNode) exprNode));
            }
        }

        public IList<Pair<ExprNode, ExprIdentNode>> IdentNodes {
            get { return _identNodes; }
        }
    }
}