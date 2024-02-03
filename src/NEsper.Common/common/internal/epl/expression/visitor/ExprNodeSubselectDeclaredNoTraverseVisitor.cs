///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.declared.compiletime;
using com.espertech.esper.common.@internal.epl.expression.subquery;

namespace com.espertech.esper.common.@internal.epl.expression.visitor
{
    /// <summary>
    /// Visitor that collects <seealso cref="ExprSubselectNode" /> instances only
    /// directly under alias expressions, and declared expressions, stopping at nested declared expressions.
    /// </summary>
    public class ExprNodeSubselectDeclaredNoTraverseVisitor : ExprNodeVisitorWithParent
    {
        private readonly ExprDeclaredNode _declaration;
        private readonly IList<ExprSubselectNode> _subselects;

        /// <summary>Ctor. </summary>
        /// <param name="declaration"></param>
        public ExprNodeSubselectDeclaredNoTraverseVisitor(ExprDeclaredNode declaration)
        {
            _declaration = declaration;
            _subselects = new List<ExprSubselectNode>(1);
        }

        public bool IsWalkDeclExprParam => true;

        public void Reset()
        {
            _subselects.Clear();
        }

        /// <summary>
        /// Returns a list of lookup expression nodes.
        /// </summary>
        /// <value>lookup nodes</value>
        public IList<ExprSubselectNode> Subselects => _subselects;

        public bool IsVisit(ExprNode exprNode)
        {
            return exprNode != _declaration && !(exprNode is ExprDeclaredNode);
        }

        public void Visit(
            ExprNode exprNode,
            ExprNode parentExprNode)
        {
            var subselectNode = exprNode as ExprSubselectNode;
            if (subselectNode != null) {
                _subselects.Add(subselectNode);
            }
        }
    }
}