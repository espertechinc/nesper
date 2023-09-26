///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.join.strategy;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.epl.join.exec.@base
{
    /// <summary>
    /// Execution node that performs a nested iteration over all child nodes.
    /// <para />Each child node under this node typically represents a table lookup. The implementation
    /// 'hops' from the first child to the next recursively for each row returned by a child.
    /// <para />It passes a 'prototype' row (prefillPath) to each new child which contains the current partial event set.
    /// </summary>
    public class NestedIterationExecNode : ExecNode
    {
        private readonly List<ExecNode> childNodes;
        private readonly int[] nestedStreams;
        private int nestingOrderLength;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="nestedStreams">array of integers defining order of streams in nested join.</param>
        public NestedIterationExecNode(int[] nestedStreams)
        {
            this.nestedStreams = nestedStreams;
            childNodes = new List<ExecNode>();
        }

        /// <summary>
        /// Add a child node.
        /// </summary>
        /// <param name="childNode">to add</param>
        public void AddChildNode(ExecNode childNode)
        {
            childNodes.Add(childNode);
        }

        public override void Process(
            EventBean lookupEvent,
            EventBean[] prefillPath,
            ICollection<EventBean[]> result,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            nestingOrderLength = childNodes.Count;
            RecursiveNestedJoin(lookupEvent, 0, prefillPath, result, exprEvaluatorContext);
        }

        /// <summary>
        /// Recursive method to run through all child nodes and, for each result set tuple returned
        /// by a child node, execute the inner child of the child node until there are no inner child nodes.
        /// </summary>
        /// <param name="lookupEvent">current event to use for lookup by child node</param>
        /// <param name="nestingOrderIndex">index within the child nodes indicating what nesting level we are at</param>
        /// <param name="currentPath">prototype result row to use by child nodes for generating result rows</param>
        /// <param name="result">result tuple rows to be populated</param>
        /// <param name="exprEvaluatorContext">context for expression evalauation</param>
        protected void RecursiveNestedJoin(
            EventBean lookupEvent,
            int nestingOrderIndex,
            EventBean[] currentPath,
            ICollection<EventBean[]> result,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            IList<EventBean[]> nestedResult = new List<EventBean[]>();
            var nestedExecNode = childNodes[nestingOrderIndex];
            nestedExecNode.Process(lookupEvent, currentPath, nestedResult, exprEvaluatorContext);
            var isLastStream = nestingOrderIndex == nestingOrderLength - 1;

            // This is not the last nesting level so no result rows are added. Invoke next nesting level for
            // each event found.
            if (!isLastStream) {
                foreach (var row in nestedResult) {
                    var lookup = row[nestedStreams[nestingOrderIndex]];
                    RecursiveNestedJoin(lookup, nestingOrderIndex + 1, row, result, exprEvaluatorContext);
                }

                return;
            }

            // Loop to add result rows
            foreach (var row in nestedResult) {
                result.Add(row);
            }
        }

        public override void Print(IndentWriter writer)
        {
            writer.WriteLine("NestedIterationExecNode");
            writer.IncrIndent();

            foreach (var child in childNodes) {
                child.Print(writer);
            }

            writer.DecrIndent();
        }
    }
} // end of namespace