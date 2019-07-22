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
    ///     Visitor that collects event property identifier information under expression nodes.
    /// </summary>
    [Serializable]
    public class ExprNodeIdentifierCollectVisitorWContainer : ExprNodeVisitorWithParent
    {
        /// <summary>Ctor. </summary>
        public ExprNodeIdentifierCollectVisitorWContainer()
        {
            ExprProperties = new List<Pair<ExprNode, ExprIdentNode>>(2);
        }

        /// <summary>
        ///     Returns list of event property stream numbers and names that uniquely identify which property is from whcih stream,
        ///     and the name of each.
        /// </summary>
        /// <value>list of event property statement-unique INFO</value>
        public IList<Pair<ExprNode, ExprIdentNode>> ExprProperties { get; }

        public bool IsVisit(ExprNode exprNode)
        {
            return true;
        }

        public void Visit(
            ExprNode exprNode,
            ExprNode containerExprNode)
        {
            var identNode = exprNode as ExprIdentNode;
            if (identNode != null) {
                ExprProperties.Add(new Pair<ExprNode, ExprIdentNode>(containerExprNode, identNode));
            }
        }
    }
}