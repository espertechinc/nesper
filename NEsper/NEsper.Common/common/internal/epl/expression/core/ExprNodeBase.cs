///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    /// <summary>
    ///     Superclass for filter nodes in a filter expression tree. Allow
    ///     validation against stream event types and evaluation of events against filter tree.
    /// </summary>
    [Serializable]
    public abstract class ExprNodeBase : ExprNode
    {
        private static readonly ExprNode[] EMPTY_EXPR_ARRAY = new ExprNode[0];

        public ExprNode[] ChildNodes { get; set; }

        public abstract ExprPrecedenceEnum Precedence { get; }
        public abstract ExprForge Forge { get; }

        /// <summary>
        ///     Constructor creates a list of child nodes.
        /// </summary>
        protected ExprNodeBase()
        {
            ChildNodes = EMPTY_EXPR_ARRAY;
        }

        public virtual void Accept(ExprNodeVisitor visitor)
        {
            if (visitor.IsVisit(this)) {
                visitor.Visit(this);

                foreach (var childNode in ChildNodes) {
                    childNode.Accept(visitor);
                }
            }
        }

        public virtual void Accept(ExprNodeVisitorWithParent visitor)
        {
            if (visitor.IsVisit(this)) {
                visitor.Visit(this, null);

                foreach (var childNode in ChildNodes) {
                    childNode.AcceptChildnodes(visitor, this);
                }
            }
        }

        public virtual void AcceptChildnodes(
            ExprNodeVisitorWithParent visitor,
            ExprNode parent)
        {
            if (visitor.IsVisit(this)) {
                visitor.Visit(this, parent);

                foreach (var childNode in ChildNodes) {
                    childNode.AcceptChildnodes(visitor, this);
                }
            }
        }

        public void AddChildNode(ExprNode childNode)
        {
            ChildNodes = (ExprNode[]) CollectionUtil.ArrayExpandAddSingle(ChildNodes, childNode);
        }

        public void AddChildNodes(ICollection<ExprNode> childNodeColl)
        {
            ChildNodes = (ExprNode[]) CollectionUtil.ArrayExpandAddElements(ChildNodes, childNodeColl);
        }

        public virtual void ReplaceUnlistedChildNode(
            ExprNode nodeToReplace,
            ExprNode newNode)
        {
            // Override to replace child expression nodes that are chained or otherwise not listed as child nodes
        }

        public void SetChildNode(
            int index,
            ExprNode newNode)
        {
            ChildNodes[index] = newNode;
        }

        public virtual void ToEPL(
            TextWriter writer,
            ExprPrecedenceEnum parentPrecedence)
        {
            if (this.Precedence.GetLevel() < parentPrecedence.GetLevel()) {
                writer.Write("(");
                ToPrecedenceFreeEPL(writer);
                writer.Write(")");
            }
            else {
                ToPrecedenceFreeEPL(writer);
            }
        }

        public abstract bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix);

        public abstract void ToPrecedenceFreeEPL(TextWriter writer);
        public abstract ExprNode Validate(ExprValidationContext validationContext);

        public void AddChildNodeToFront(ExprNode childNode)
        {
            ChildNodes = CollectionUtil.ArrayExpandAddElements<ExprNode>(new[] {childNode}, ChildNodes);
        }

        protected internal static void CheckValidated(ExprForge forge)
        {
            if (forge == null) {
                throw CheckValidatedException();
            }
        }

        protected internal static IllegalStateException CheckValidatedException()
        {
            return new IllegalStateException("Expression has not been validated");
        }
    }
} // end of namespace