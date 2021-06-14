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
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.epl.expression.subquery;

namespace com.espertech.esper.common.@internal.epl.expression.visitor
{
    /// <summary>
    ///     Visitor that collects subqueries, declared-expression and chained-dot.
    /// </summary>
    public class ExprNodeSubselectDeclaredDotVisitor : ExprNodeVisitor
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        public ExprNodeSubselectDeclaredDotVisitor()
        {
            Subselects = new List<ExprSubselectNode>(4);
            ChainedExpressionsDot = new List<ExprDotNode>(4);
            DeclaredExpressions = new List<ExprDeclaredNode>(1);
        }
        
        public bool IsWalkDeclExprParam => true;

        /// <summary>
        ///     Returns a list of lookup expression nodes.
        /// </summary>
        /// <returns>lookup nodes</returns>
        public IList<ExprSubselectNode> Subselects { get; }

        public IList<ExprDotNode> ChainedExpressionsDot { get; }

        public IList<ExprDeclaredNode> DeclaredExpressions { get; }

        public bool IsVisit(ExprNode exprNode)
        {
            return true;
        }

        public void Visit(ExprNode exprNode)
        {
            if (exprNode is ExprDotNode) {
                ChainedExpressionsDot.Add((ExprDotNode) exprNode);
            }

            if (exprNode is ExprDeclaredNode) {
                DeclaredExpressions.Add((ExprDeclaredNode) exprNode);
            }

            if (exprNode is ExprSubselectNode) {
                var subselectNode = (ExprSubselectNode) exprNode;
                Subselects.Add(subselectNode);
            }
        }

        public void Reset()
        {
            Subselects.Clear();
            ChainedExpressionsDot.Clear();
            DeclaredExpressions.Clear();
        }
    }
} // end of namespace