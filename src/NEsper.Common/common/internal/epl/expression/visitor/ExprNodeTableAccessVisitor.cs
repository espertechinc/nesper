///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.table;

namespace com.espertech.esper.common.@internal.epl.expression.visitor
{
    public class ExprNodeTableAccessVisitor : ExprNodeVisitor
    {
        private readonly ISet<ExprTableAccessNode> _nodesToAddTo;

        public ExprNodeTableAccessVisitor(ISet<ExprTableAccessNode> nodesToAddTo)
        {
            _nodesToAddTo = nodesToAddTo;
        }

        public bool IsWalkDeclExprParam => true;

        public bool IsVisit(ExprNode exprNode)
        {
            return true;
        }

        public void Visit(ExprNode exprNode)
        {
            var asTableAccessNode = exprNode as ExprTableAccessNode;
            if (asTableAccessNode != null) {
                _nodesToAddTo.Add(asTableAccessNode);
            }
        }
    }
} // end of namespace