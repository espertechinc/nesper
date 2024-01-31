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
    /// Visitor interface for use with expression node trees.
    /// </summary>
    public interface ExprNodeVisitor
    {
        /// <summary> Allows visitor to indicate whether to visit a given node.
        /// Implicitly if a visitor doesn't visit a node it would also not visit any descendent child nodes of that node.
        /// </summary>
        /// <param name="exprNode">is the node in questions
        /// </param>
        /// <returns> true if the visitor wants to visit the child node (next call is visit), or false to skip child
        /// </returns>
        bool IsVisit(ExprNode exprNode);

        /// <summary> Visit the given expression node.</summary>
        /// <param name="exprNode">is the expression node to visit
        /// </param>
        void Visit(ExprNode exprNode);

        bool IsWalkDeclExprParam { get; }
    }
}