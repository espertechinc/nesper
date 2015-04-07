///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.epl.declexpr;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.dot;
using com.espertech.esper.epl.expression.subquery;

namespace com.espertech.esper.epl.expression.visitor
{
    /// <summary>
    /// Visitor that collects <seealso cref="ExprSubselectNode"/> instances.
    /// </summary>
    public class ExprNodeSubselectDeclaredDotVisitor : ExprNodeVisitor
    {
        /// <summary>Ctor. </summary>
        public ExprNodeSubselectDeclaredDotVisitor()
        {
            Subselects = new List<ExprSubselectNode>();
            ChainedExpressionsDot = new List<ExprDotNode>();
            DeclaredExpressions = new List<ExprDeclaredNode>();
        }

        public void Reset()
        {
            Subselects.Clear();
            ChainedExpressionsDot.Clear();
            DeclaredExpressions.Clear();
        }

        /// <summary>Returns a list of lookup expression nodes. </summary>
        /// <value>lookup nodes</value>
        public IList<ExprSubselectNode> Subselects { get; private set; }

        public IList<ExprDotNode> ChainedExpressionsDot { get; private set; }

        public IList<ExprDeclaredNode> DeclaredExpressions { get; private set; }

        public bool IsVisit(ExprNode exprNode)
        {
            return true;
        }

        public void Visit(ExprNode exprNode)
        {

            if (exprNode is ExprDotNode)
            {
                ChainedExpressionsDot.Add((ExprDotNode)exprNode);
            }

            if (exprNode is ExprDeclaredNode)
            {
                DeclaredExpressions.Add((ExprDeclaredNode)exprNode);
            }

            if (!(exprNode is ExprSubselectNode))
            {
                return;
            }

            var subselectNode = (ExprSubselectNode)exprNode;
            Subselects.Add(subselectNode);
        }
    }
}
