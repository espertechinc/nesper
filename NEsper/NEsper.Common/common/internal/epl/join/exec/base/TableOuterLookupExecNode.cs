///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.@join.strategy;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.exec.@base
{
    /// <summary>
    ///     Execution node for lookup in a table for outer joins. This execution node thus generates
    ///     rows even if no joined events could be found, the joined table events are set to null if no
    ///     joined events are found.
    /// </summary>
    public class TableOuterLookupExecNode : ExecNode
    {
        protected JoinExecTableLookupStrategy lookupStrategy;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="indexedStream">stream indexed for lookup</param>
        /// <param name="lookupStrategy">strategy to use for lookup (full table/indexed)</param>
        public TableOuterLookupExecNode(
            int indexedStream,
            JoinExecTableLookupStrategy lookupStrategy)
        {
            IndexedStream = indexedStream;
            this.lookupStrategy = lookupStrategy;
        }

        /// <summary>
        ///     Returns strategy for lookup.
        /// </summary>
        /// <returns>lookup strategy</returns>
        public JoinExecTableLookupStrategy LookupStrategy => lookupStrategy;

        /// <summary>
        ///     Returns target stream for lookup.
        /// </summary>
        /// <returns>indexed stream</returns>
        public int IndexedStream { get; }

        public override void Process(
            EventBean lookupEvent,
            EventBean[] prefillPath,
            ICollection<EventBean[]> result,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            // Lookup events
            var joinedEvents = lookupStrategy.Lookup(lookupEvent, null, exprEvaluatorContext);

            // process
            ProcessResults(prefillPath, result, joinedEvents);
        }

        protected void ProcessResults(
            EventBean[] prefillPath,
            ICollection<EventBean[]> result,
            ISet<EventBean> joinedEvents)
        {
            // If no events are found, since this is an outer join, create a result row leaving the
            // joined event as null.
            if (joinedEvents == null || joinedEvents.IsEmpty()) {
                var events = new EventBean[prefillPath.Length];
                Array.Copy(prefillPath, 0, events, 0, events.Length);
                result.Add(events);

                return;
            }

            // Create result row for each found event
            foreach (var joinedEvent in joinedEvents) {
                var events = new EventBean[prefillPath.Length];
                Array.Copy(prefillPath, 0, events, 0, events.Length);
                events[IndexedStream] = joinedEvent;
                result.Add(events);
            }
        }

        public override void Print(IndentWriter writer)
        {
            writer.WriteLine("TableOuterLookupExecNode indexedStream=" + IndexedStream + " lookup=" + lookupStrategy);
        }
    }
} // end of namespace