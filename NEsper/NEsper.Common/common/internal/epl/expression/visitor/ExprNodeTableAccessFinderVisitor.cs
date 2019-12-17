///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.table;

namespace com.espertech.esper.common.@internal.epl.expression.visitor
{
    public class ExprNodeTableAccessFinderVisitor : ExprNodeVisitor
    {
        public bool HasTableAccess { get; private set; }

        public bool IsVisit(ExprNode exprNode)
        {
            return !HasTableAccess;
        }

        public void Visit(ExprNode exprNode)
        {
            if (exprNode is ExprTableAccessNode) {
                HasTableAccess = true;
            }
        }
    }
}