///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.strategy
{
    /// <summary>
    ///     Query strategy for building a join tuple set by using an execution node tree.
    /// </summary>
    public class ExecNodeQueryStrategy : QueryStrategy
    {
        /// <summary>
        ///     CTor.
        /// </summary>
        /// <param name="forStream">stream the strategy is for</param>
        /// <param name="numStreams">number of streams in total</param>
        /// <param name="execNode">execution node for building join tuple set</param>
        public ExecNodeQueryStrategy(int forStream, int numStreams, ExecNode execNode)
        {
            ForStream = forStream;
            NumStreams = numStreams;
            ExecNode = execNode;
        }

        /// <summary>
        ///     Return stream number this strategy is for.
        /// </summary>
        /// <value>stream num</value>
        protected int ForStream { get; }

        /// <summary>
        ///     Returns the total number of streams.
        /// </summary>
        /// <value>number of streams</value>
        protected int NumStreams { get; }

        /// <summary>
        ///     Returns execution node.
        /// </summary>
        /// <value>execution node</value>
        protected ExecNode ExecNode { get; }

        public void Lookup(
            EventBean[] lookupEvents,
            ICollection<MultiKey<EventBean>> joinSet,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (lookupEvents == null || lookupEvents.Length == 0) {
                return;
            }

            var results = new ArrayDeque<EventBean[]>();
            foreach (var theEvent in lookupEvents) {
                // Set up prototype row
                var prototype = new EventBean[NumStreams];
                prototype[ForStream] = theEvent;

                // Perform execution
                ExecNode.Process(theEvent, prototype, results, exprEvaluatorContext);

                // Convert results into unique set
                foreach (var row in results) {
                    joinSet.Add(new MultiKey<EventBean>(row));
                }

                results.Clear();
            }
        }
    }
} // end of namespace