///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.expression.visitor;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public interface ExprNode : ExprNodeRenderable,
        ExprValidator
    {
        /// <summary>
        /// Returns precedence.
        /// </summary>
        /// <value>precedence</value>
        ExprPrecedenceEnum Precedence { get; }

        /// <summary>
        /// Return true if a expression node semantically equals the current node, or false if not.
        /// <para />Concrete implementations should compare the type and any additional information
        /// that impact the evaluation of a node.
        /// </summary>
        /// <param name="node">to compare to</param>
        /// <param name="ignoreStreamPrefix">when the equals-comparison can ignore prefix of event properties</param>
        /// <returns>true if semantically equal, or false if not equals</returns>
        bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix);

        /// <summary>
        /// Accept the visitor. The visitor will first visit the parent then visit all child nodes, then their child nodes.
        /// <para />The visitor can decide to skip child nodes by returning false in isVisit.
        /// </summary>
        /// <param name="visitor">to visit each node and each child node.</param>
        void Accept(ExprNodeVisitor visitor);

        /// <summary>
        /// Accept the visitor. The visitor will first visit the parent then visit all child nodes, then their child nodes.
        /// <para />The visitor can decide to skip child nodes by returning false in isVisit.
        /// </summary>
        /// <param name="visitor">to visit each node and each child node.</param>
        void Accept(ExprNodeVisitorWithParent visitor);

        /// <summary>
        /// Accept a visitor that receives both parent and child node.
        /// </summary>
        /// <param name="visitor">to apply</param>
        /// <param name="parent">node</param>
        void AcceptChildnodes(
            ExprNodeVisitorWithParent visitor,
            ExprNode parent);

        /// <summary>
        /// Adds a child node.
        /// </summary>
        /// <param name="childNode">is the child evaluation tree node to add</param>
        void AddChildNode(ExprNode childNode);

        /// <summary>
        /// Adds child nodes.
        /// </summary>
        /// <param name="childNodes">are the child evaluation tree node to add</param>
        void AddChildNodes(ICollection<ExprNode> childNodes);

        /// <summary>
        /// Returns list of child nodes.
        /// </summary>
        /// <value>list of child nodes</value>
        ExprNode[] ChildNodes { get; set; }

        void ReplaceUnlistedChildNode(
            ExprNode nodeToReplace,
            ExprNode newNode);

        void SetChildNode(
            int index,
            ExprNode newNode);

        ExprForge Forge { get; }
    }
} // end of namespace