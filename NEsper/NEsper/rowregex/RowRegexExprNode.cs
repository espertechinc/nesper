///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.compat.logging;
using com.espertech.esper.util;

namespace com.espertech.esper.rowregex
{
    /// <summary>
    /// Base node for
    /// </summary>
    [Serializable]
    public abstract class RowRegexExprNode : MetaDefItem
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public abstract RowRegexExprNodePrecedenceEnum Precedence { get; }
        public abstract void ToPrecedenceFreeEPL(TextWriter writer);

        /// <summary>
        /// Constructor creates a list of child nodes.
        /// </summary>
        protected RowRegexExprNode()
        {
            ChildNodes = new List<RowRegexExprNode>();
        }

        public virtual void ToEPL(TextWriter writer, RowRegexExprNodePrecedenceEnum parentPrecedence)
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

        /// <summary>
        /// Adds a child node.
        /// </summary>
        /// <param name="childNode">is the child evaluation tree node to add</param>
        public void AddChildNode(RowRegexExprNode childNode)
        {
            ChildNodes.Add(childNode);
        }

        /// <summary>
        /// Returns list of child nodes.
        /// </summary>
        /// <returns>
        /// list of child nodes
        /// </returns>
        public IList<RowRegexExprNode> ChildNodes { get; private set; }

        /// <summary>
        /// Recursively print out all nodes.
        /// </summary>
        /// <param name="prefix">is printed out for naming the printed info</param>
        public void DumpDebug(String prefix)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug(".DumpDebug {0}{1}", prefix, this);
            }
            foreach (RowRegexExprNode node in ChildNodes)
            {
                node.DumpDebug(prefix + "  ");
            }
        }

        public virtual void Accept(RowRegexExprNodeVisitor visitor)
        {
            AcceptChildnodes(visitor, null, 0);
        }

        public virtual void AcceptChildnodes(RowRegexExprNodeVisitor visitor, RowRegexExprNode parent, int level)
        {
            visitor.Visit(this, parent, level);
            foreach (RowRegexExprNode childNode in ChildNodes)
            {
                childNode.AcceptChildnodes(visitor, this, level + 1);
            }
        }

        public virtual void ReplaceChildNode(RowRegexExprNode nodeToReplace, IList<RowRegexExprNode> replacementNodes)
        {
            var newChildNodes = new List<RowRegexExprNode>(ChildNodes.Count - 1 + replacementNodes.Count);
            foreach (RowRegexExprNode node in ChildNodes)
            {
                if (node != nodeToReplace)
                {
                    newChildNodes.Add(node);
                }
                else
                {
                    newChildNodes.AddRange(replacementNodes);
                }
            }

            ChildNodes = newChildNodes;
        }
    }
}
