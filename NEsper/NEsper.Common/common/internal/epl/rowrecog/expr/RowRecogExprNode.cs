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
using System.Reflection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.rowrecog.expr
{
    /// <summary>
    ///     Base node for
    /// </summary>
    [Serializable]
    public abstract class RowRecogExprNode
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     Constructor creates a list of child nodes.
        /// </summary>
        public RowRecogExprNode()
        {
            ChildNodes = new List<RowRecogExprNode>();
        }

        public abstract RowRecogExprNodePrecedenceEnum Precedence { get; }

        /// <summary>
        ///     Returns list of child nodes.
        /// </summary>
        /// <returns>list of child nodes</returns>
        public IList<RowRecogExprNode> ChildNodes { get; private set; }

        public abstract void ToPrecedenceFreeEPL(TextWriter writer);

        public void ToEPL(
            TextWriter writer,
            RowRecogExprNodePrecedenceEnum parentPrecedence)
        {
            if (Precedence.Level < parentPrecedence.Level) {
                writer.Write("(");
                ToPrecedenceFreeEPL(writer);
                writer.Write(")");
            }
            else {
                ToPrecedenceFreeEPL(writer);
            }
        }

        /// <summary>
        ///     Adds a child node.
        /// </summary>
        /// <param name="childNode">is the child evaluation tree node to add</param>
        public void AddChildNode(RowRecogExprNode childNode)
        {
            ChildNodes.Add(childNode);
        }

        /// <summary>
        ///     Recursively print out all nodes.
        /// </summary>
        /// <param name="prefix">is printed out for naming the printed info</param>
        public void DumpDebug(string prefix)
        {
            if (Log.IsDebugEnabled) {
                Log.Debug(".dumpDebug " + prefix + ToString());
            }

            foreach (var node in ChildNodes) {
                node.DumpDebug(prefix + "  ");
            }
        }

        public void Accept(RowRecogExprNodeVisitor visitor)
        {
            AcceptChildnodes(visitor, null, 0);
        }

        public void AcceptChildnodes(
            RowRecogExprNodeVisitor visitor,
            RowRecogExprNode parent,
            int level)
        {
            visitor.Visit(this, parent, level);
            foreach (var childNode in ChildNodes) {
                childNode.AcceptChildnodes(visitor, this, level + 1);
            }
        }

        public void ReplaceChildNode(
            RowRecogExprNode nodeToReplace,
            IList<RowRecogExprNode> replacementNodes)
        {
            IList<RowRecogExprNode> newChildNodes =
                new List<RowRecogExprNode>(ChildNodes.Count - 1 + replacementNodes.Count);
            foreach (var node in ChildNodes) {
                if (node != nodeToReplace) {
                    newChildNodes.Add(node);
                }
                else {
                    newChildNodes.AddAll(replacementNodes);
                }
            }

            ChildNodes = newChildNodes;
        }
    }
} // end of namespace