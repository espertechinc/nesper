///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.expression.visitor
{
    /// <summary>
    ///     Visitor that early-exists when it finds a context partition property.
    /// </summary>
    public class ExprNodeContextPropertiesVisitor : ExprNodeVisitor
    {
        public bool IsFound { get; private set; }

        public bool IsWalkDeclExprParam => true;

        public bool IsVisit(ExprNode exprNode)
        {
            return !IsFound;
        }

        public void Visit(ExprNode exprNode)
        {
            if (!(exprNode is ExprContextPropertyNode)) {
                return;
            }

            IsFound = true;
        }
    }
}