///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.join.exec.@base
{
    /// <summary>
    /// Execution node that performs a nested iteration over all child nodes. 
    /// <para/>
    /// Each child node under this node typically represents a table lookup. The 
    /// implementation 'hops' from the first child to the next recursively for each row 
    /// returned by a child.
    /// <para/>
    /// It passes a 'Prototype' row (prefillPath) to each new child which contains the 
    /// current partial event set.
    /// </summary>
    public class NestedIterationExecNode : ExecNode
    {
        private readonly List<ExecNode> _childNodes;
        private readonly int[] _nestedStreams;
        private int _nestingOrderLength;
    
        /// <summary>Ctor. </summary>
        /// <param name="nestedStreams">array of integers defining order of streams in nested join.</param>
        public NestedIterationExecNode(int[] nestedStreams)
        {
            _nestedStreams = nestedStreams;
            _childNodes = new List<ExecNode>();
        }
    
        /// <summary>Add a child node. </summary>
        /// <param name="childNode">to add</param>
        public void AddChildNode(ExecNode childNode)
        {
            _childNodes.Add(childNode);
        }
    
        public override void Process(EventBean lookupEvent, EventBean[] prefillPath, ICollection<EventBean[]> result, ExprEvaluatorContext exprEvaluatorContext)
        {
            _nestingOrderLength = _childNodes.Count;
            RecursiveNestedJoin(lookupEvent, 0, prefillPath, result, exprEvaluatorContext);
        }
    
        /// <summary>Recursive method to run through all child nodes and, for each result set tuple returned by a child node, execute the inner child of the child node until there are no inner child nodes. </summary>
        /// <param name="lookupEvent">current event to use for lookup by child node</param>
        /// <param name="nestingOrderIndex">index within the child nodes indicating what nesting level we are at</param>
        /// <param name="currentPath">Prototype result row to use by child nodes for generating result rows</param>
        /// <param name="result">result tuple rows to be populated</param>
        /// <param name="exprEvaluatorContext">context for expression evalauation</param>
        protected void RecursiveNestedJoin(EventBean lookupEvent, int nestingOrderIndex, EventBean[] currentPath, ICollection<EventBean[]> result, ExprEvaluatorContext exprEvaluatorContext)
        {
            var nestedResult = new List<EventBean[]>();
            var nestedExecNode = _childNodes[nestingOrderIndex];
            nestedExecNode.Process(lookupEvent, currentPath, nestedResult, exprEvaluatorContext);
            bool isLastStream = (nestingOrderIndex == _nestingOrderLength - 1);

            unchecked
            {
                var nestedResultCount = nestedResult.Count;

                // This is not the last nesting level so no result rows are added. Invoke next nesting level for
                // each event found.
                if (!isLastStream)
                {
                    for (int ii = 0; ii < nestedResultCount; ii++)
                    {
                        EventBean[] row = nestedResult[ii];
                        EventBean lookup = row[_nestedStreams[nestingOrderIndex]];
                        RecursiveNestedJoin(lookup, nestingOrderIndex + 1, row, result, exprEvaluatorContext);
                    }
                    return;
                }

                // Loop to add result rows
                for (int ii = 0; ii < nestedResultCount; ii++)
                {
                    result.Add(nestedResult[ii]);
                }
            }
        }
    
        public override void Print(IndentWriter writer)
        {
            writer.WriteLine("NestedIterationExecNode");
            writer.IncrIndent();
    
            foreach (ExecNode child in _childNodes)
            {
                child.Print(writer);
            }
            writer.DecrIndent();
        }
    }
}
