///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.IO;

using com.espertech.esper.epl.expression.visitor;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.core
{
    public interface ExprNode
        : ExprValidator
        , MetaDefItem
    {
        ExprEvaluator ExprEvaluator { get; }

        /// <summary>
        /// Writes the expression node to the writer using the specified precedence.
        /// </summary>
        void ToEPL(TextWriter writer, ExprPrecedenceEnum parentPrecedence);

        /// <summary>
        /// Returns precedence
        /// </summary>
        ExprPrecedenceEnum Precedence { get; }

        /// <summary>
        /// Returns true if the expression node's evaluation value doesn't depend on any events data, as must be determined at validation time, 
        /// which is bottom-up and therefore reliably allows each node to determine constant value.
        /// </summary>
        /// <value>
        /// true for constant evaluation value, false for non-constant evaluation value
        /// </value>
        bool IsConstantResult { get; }

        /// <summary>
        /// Return true if a expression node semantically equals the current node, or false if not. <para/>Concrete implementations should compare 
        /// the type and any additional information that impact the evaluation of a node.
        /// </summary>
        /// <param name="node">to compare to</param>
        /// <param name="ignoreStreamPrefix">when the equals-comparison can ignore prefix of event properties</param>
        /// <returns>
        /// true if semantically equal, or false if not equals
        /// </returns>
        bool EqualsNode(ExprNode node, bool ignoreStreamPrefix);

        /// <summary>
        /// Accept the visitor. The visitor will first visit the parent then visit all child nodes, then their child nodes. <para/>The visitor can 
        /// decide to skip child nodes by returning false in isVisit.
        /// </summary>
        /// <param name="visitor">to visit each node and each child node.</param>
        void Accept(ExprNodeVisitor visitor);

        /// <summary>
        /// Accept the visitor. The visitor will first visit the parent then visit all child nodes, then their child nodes. <para/>The visitor can 
        /// decide to skip child nodes by returning false in isVisit.
        /// </summary>
        /// <param name="visitor">to visit each node and each child node.</param>
        void Accept(ExprNodeVisitorWithParent visitor);
    
        /// <summary>Accept a visitor that receives both parent and child node. </summary>
        /// <param name="visitor">to apply</param>
        /// <param name="parent">node</param>
        void AcceptChildnodes(ExprNodeVisitorWithParent visitor, ExprNode parent);
    
        /// <summary>Adds a child node. </summary>
        /// <param name="childNode">is the child evaluation tree node to add</param>
        void AddChildNode(ExprNode childNode);
    
        /// <summary>Adds child nodes. </summary>
        /// <param name="childNodes">are the child evaluation tree node to add</param>
        void AddChildNodes(ICollection<ExprNode> childNodes);

        /// <summary>Returns list of child nodes. </summary>
        /// <value>list of child nodes</value>
        IList<ExprNode> ChildNodes { get; set; }

        void ReplaceUnlistedChildNode(ExprNode nodeToReplace, ExprNode newNode);

        void SetChildNodes(params ExprNode[] nodes);
        void SetChildNode(int index, ExprNode newNode);
    }
}
