///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.expression.visitor
{
    public class ExprNodeStreamUseCollectVisitor : ExprNodeVisitor
    {
        public IList<ExprStreamRefNode> Referenced { get; } = new List<ExprStreamRefNode>();

        public bool IsWalkDeclExprParam => true;

        public bool IsVisit(ExprNode exprNode)
        {
            return true;
        }

        public void Visit(ExprNode exprNode)
        {
            if (!(exprNode is ExprStreamRefNode)) {
                return;
            }

            Referenced.Add((ExprStreamRefNode) exprNode);
        }
    }
}