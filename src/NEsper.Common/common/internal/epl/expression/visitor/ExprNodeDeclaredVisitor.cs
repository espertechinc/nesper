///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.declared.compiletime;

namespace com.espertech.esper.common.@internal.epl.expression.visitor
{
    /// <summary>
    ///     Visitor that collects <seealso cref="ExprDeclaredNode" /> instances.
    /// </summary>
    public class ExprNodeDeclaredVisitor : ExprNodeVisitor
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        public ExprNodeDeclaredVisitor()
        {
            DeclaredExpressions = new List<ExprDeclaredNode>(1);
        }


        public bool IsWalkDeclExprParam => true;

        public IList<ExprDeclaredNode> DeclaredExpressions { get; }

        public bool IsVisit(ExprNode exprNode)
        {
            return true;
        }

        public void Visit(ExprNode exprNode)
        {
            if (exprNode is ExprDeclaredNode node) {
                DeclaredExpressions.Add(node);
            }
        }

        public void Reset()
        {
            DeclaredExpressions.Clear();
        }

        public void Clear()
        {
            DeclaredExpressions.Clear();
        }
    }
}