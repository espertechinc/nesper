///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.join.exec.@base
{
    /// <summary>
    /// Execution node for lookup in a table for outer joins. This execution node thus generates 
    /// rows even if no joined events could be found, the joined table events are set to null if 
    /// no joined events are found.
    /// </summary>
    public class TableOuterLookupExecNode : ExecNode
    {
        /// <summary>Ctor. </summary>
        /// <param name="indexedStream">stream indexed for lookup</param>
        /// <param name="lookupStrategy">strategy to use for lookup (full table/indexed)</param>
        public TableOuterLookupExecNode(int indexedStream, JoinExecTableLookupStrategy lookupStrategy)
        {
            IndexedStream = indexedStream;
            LookupStrategy = lookupStrategy;
        }

        /// <summary>Returns strategy for lookup. </summary>
        /// <value>lookup strategy</value>
        public JoinExecTableLookupStrategy LookupStrategy { get; private set; }

        /// <summary>Returns target stream for lookup. </summary>
        /// <value>indexed stream</value>
        public int IndexedStream { get; private set; }

        public override void Process(EventBean lookupEvent, EventBean[] prefillPath, ICollection<EventBean[]> result, ExprEvaluatorContext exprEvaluatorContext)
        {
            // Lookup events
            ICollection<EventBean> joinedEvents = LookupStrategy.Lookup(lookupEvent, null, exprEvaluatorContext);
    
            // process
            ProcessResults(prefillPath, result, joinedEvents);
        }

        protected void ProcessResults(EventBean[] prefillPath, ICollection<EventBean[]> result, ICollection<EventBean> joinedEvents)
        {
            // If no events are found, since this is an outer join, create a result row leaving the
            // joined event as null.
            if ((joinedEvents == null) || (joinedEvents.IsEmpty()))
            {
                var events = new EventBean[prefillPath.Length];
                Array.Copy(prefillPath, 0, events, 0, events.Length);
                result.Add(events);
    
                return;
            }
    
            // Create result row for each found event
            foreach (EventBean joinedEvent in joinedEvents)
            {
                var events = new EventBean[prefillPath.Length];
                Array.Copy(prefillPath, 0, events, 0, events.Length);
                events[IndexedStream] = joinedEvent;
                result.Add(events);
            }
        }


        public override void Print(IndentWriter writer)
        {
            writer.WriteLine("TableOuterLookupExecNode indexedStream=" + IndexedStream + " lookup=" + LookupStrategy);
        }
    }
}
