///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.epl.declexpr;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.visitor
{
    /// <summary>
    ///     Visitor that collects <seealso cref="ExprDeclaredNode" /> instances.
    /// </summary>
    public class ExprNodeDeclaredVisitor : ExprNodeVisitor
    {
        private readonly IList<ExprDeclaredNode> _declaredExpressions;

        /// <summary>
        ///     Ctor.
        /// </summary>
        public ExprNodeDeclaredVisitor()
        {
            _declaredExpressions = new List<ExprDeclaredNode>(1);
        }

        public IList<ExprDeclaredNode> DeclaredExpressions
        {
            get { return _declaredExpressions; }
        }

        public void Reset()
        {
            _declaredExpressions.Clear();
        }

        public bool IsVisit(ExprNode exprNode)
        {
            return true;
        }

        public void Visit(ExprNode exprNode)
        {
            if (exprNode is ExprDeclaredNode)
            {
                _declaredExpressions.Add((ExprDeclaredNode) exprNode);
            }
        }

        public void Clear()
        {
            _declaredExpressions.Clear();
        }
    }
}