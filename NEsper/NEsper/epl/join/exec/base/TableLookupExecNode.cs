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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.join.exec.@base
{
	/// <summary>
	/// Execution node for lookup in a table.
	/// </summary>
	public class TableLookupExecNode : ExecNode
	{
	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="indexedStream">stream indexed for lookup</param>
	    /// <param name="lookupStrategy">strategy to use for lookup (full table/indexed)</param>
	    public TableLookupExecNode(int indexedStream, JoinExecTableLookupStrategy lookupStrategy)
	    {
	        IndexedStream = indexedStream;
	        LookupStrategy = lookupStrategy;
	    }

	    /// <summary>
	    /// Returns strategy for lookup.
	    /// </summary>
	    /// <value>lookup strategy</value>
	    public JoinExecTableLookupStrategy LookupStrategy { get; private set; }

	    public override void Process(EventBean lookupEvent, EventBean[] prefillPath, ICollection<EventBean[]> result, ExprEvaluatorContext exprEvaluatorContext)
	    {
	        // Lookup events
	        var joinedEvents = LookupStrategy.Lookup(lookupEvent, null, exprEvaluatorContext);
	        if (joinedEvents == null) {
	            return;
	        }
	        ProcessResults(prefillPath, result, joinedEvents);
	    }

	    protected void ProcessResults(EventBean[] prefillPath, ICollection<EventBean[]> result, IEnumerable<EventBean> joinedEvents)
        {
	        // Create result row for each found event
	        foreach (EventBean joinedEvent in joinedEvents)
            {
	            EventBean[] events = new EventBean[prefillPath.Length];
	            Array.Copy(prefillPath, 0, events, 0, events.Length);
	            events[IndexedStream] = joinedEvent;
	            result.Add(events);
	        }
	    }

	    /// <summary>
	    /// Returns target stream for lookup.
	    /// </summary>
	    /// <value>indexed stream</value>
	    public int IndexedStream { get; private set; }

	    public override void Print(IndentWriter writer)
	    {
	        writer.WriteLine("TableLookupExecNode indexedStream=" + IndexedStream + " lookup=" + LookupStrategy);
	    }
	}
} // end of namespace
