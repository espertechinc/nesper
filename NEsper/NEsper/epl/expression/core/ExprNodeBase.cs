///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.visitor;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.core
{
    /// <summary>
    /// Superclass for filter nodes in a filter expression tree. Allow validation against 
    /// stream event types and evaluation of events against filter tree. 
    /// </summary>
    [Serializable]
    public abstract class ExprNodeBase : ExprNode
    {
        private IList<ExprNode> _childNodes;
    
        /// <summary>Constructor creates a list of child nodes. </summary>
        protected ExprNodeBase()
        {
            _childNodes = ExprNodeUtility.EMPTY_EXPR_ARRAY;
        }

        public abstract void ToPrecedenceFreeEPL(TextWriter writer);
    
        public virtual void Accept(ExprNodeVisitor visitor)
        {
            if (visitor.IsVisit(this))
            {
                visitor.Visit(this);
    
                foreach (ExprNode childNode in _childNodes)
                {
                    childNode.Accept(visitor);
                }
            }
        }
    
        public virtual void Accept(ExprNodeVisitorWithParent visitor)
        {
            if (visitor.IsVisit(this))
            {
                visitor.Visit(this, null);
    
                foreach (ExprNode childNode in _childNodes)
                {
                    childNode.AcceptChildnodes(visitor, this);
                }
            }
        }
    
        public virtual void AcceptChildnodes(ExprNodeVisitorWithParent visitor, ExprNode parent)
        {
            if (visitor.IsVisit(this))
            {
                visitor.Visit(this, parent);
    
                foreach (ExprNode childNode in _childNodes)
                {
                    childNode.AcceptChildnodes(visitor, this);
                }
            }
        }
    
        public virtual void AddChildNode(ExprNode childNode)
        {
            if (_childNodes.IsReadOnly)
            {
                _childNodes = new List<ExprNode>(_childNodes);
            }
             
            _childNodes.Add(childNode);
        }
    
        public virtual void AddChildNodes(ICollection<ExprNode> childNodeColl)
        {
            if (_childNodes.IsReadOnly)
            {
                _childNodes = new List<ExprNode>(_childNodes);
            }

            _childNodes.AddAll(childNodeColl);
        }

        public virtual IList<ExprNode> ChildNodes
        {
            get { return _childNodes; }
            set { _childNodes = value; }
        }

        public void SetChildNodes(params ExprNode[] nodes)
        {
            _childNodes = nodes;
        }

        public virtual void ReplaceUnlistedChildNode(ExprNode nodeToReplace, ExprNode newNode)
        {
            // Override to replace child expression nodes that are chained or otherwise not listed as child nodes
        }
    
        public virtual void AddChildNodeToFront(ExprNode childNode)
        {
            _childNodes = (ExprNode[]) CollectionUtil.ArrayExpandAddElements(new ExprNode[] {childNode}, _childNodes);
        }
    
        public virtual void SetChildNode(int index, ExprNode newNode)
        {
            _childNodes[index] = newNode;
        }

        public virtual void ToEPL(TextWriter writer, ExprPrecedenceEnum parentPrecedence)
        {
            if (Precedence.GetLevel() < parentPrecedence.GetLevel())
            {
                writer.Write("(");
                ToPrecedenceFreeEPL(writer);
                writer.Write(")");
            }
            else
            {
                ToPrecedenceFreeEPL(writer);
            }
        }

        public abstract ExprNode Validate(ExprValidationContext validationContext);
        public abstract ExprEvaluator ExprEvaluator { get; }
        public abstract bool IsConstantResult { get; }
        public abstract ExprPrecedenceEnum Precedence { get; }
        public abstract bool EqualsNode(ExprNode node, bool ignoreStreamPrefix);
    }
}
