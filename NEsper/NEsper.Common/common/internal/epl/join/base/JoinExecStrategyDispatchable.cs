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
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.common.@internal.view.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.@base
{
	/// <summary>
	/// This class reacts to any new data buffered by registring with the dispatch service.
	/// When dispatched via execute, it takes the buffered events and hands these to the join execution strategy.
	/// </summary>
	public class JoinExecStrategyDispatchable : BufferObserver, EPStatementDispatch {
	    private readonly JoinExecutionStrategy joinExecutionStrategy;
	    private readonly IDictionary<int, FlushedEventBuffer> oldStreamBuffer;
	    private readonly IDictionary<int, FlushedEventBuffer> newStreamBuffer;
	    private readonly int numStreams;
	    private readonly AgentInstanceContext agentInstanceContext;

	    private bool hasNewData;

	    public JoinExecStrategyDispatchable(JoinExecutionStrategy joinExecutionStrategy, int numStreams, AgentInstanceContext agentInstanceContext) {
	        this.joinExecutionStrategy = joinExecutionStrategy;
	        this.numStreams = numStreams;
	        this.agentInstanceContext = agentInstanceContext;

	        oldStreamBuffer = new Dictionary<int, FlushedEventBuffer>();
	        newStreamBuffer = new Dictionary<int, FlushedEventBuffer>();
	    }

	    public void Execute() {
	        if (!hasNewData) {
	            return;
	        }
	        hasNewData = false;

	        EventBean[][] oldDataPerStream = new EventBean[numStreams][];
	        EventBean[][] newDataPerStream = new EventBean[numStreams][];

	        for (int i = 0; i < numStreams; i++) {
	            oldDataPerStream[i] = GetBufferData(oldStreamBuffer.Get(i));
	            newDataPerStream[i] = GetBufferData(newStreamBuffer.Get(i));
	        }

	        InstrumentationCommon instrumentationCommon = agentInstanceContext.InstrumentationProvider;
	        if (instrumentationCommon.Activated()) {
	            instrumentationCommon.QJoinDispatch(newDataPerStream, oldDataPerStream);
	            joinExecutionStrategy.Join(newDataPerStream, oldDataPerStream);
	            instrumentationCommon.AJoinDispatch();
	            return;
	        }

	        joinExecutionStrategy.Join(newDataPerStream, oldDataPerStream);
	    }

	    private static EventBean[] GetBufferData(FlushedEventBuffer buffer) {
	        if (buffer == null) {
	            return null;
	        }
	        return buffer.GetAndFlush();
	    }

	    public void NewData(int streamId, FlushedEventBuffer newEventBuffer, FlushedEventBuffer oldEventBuffer) {
	        hasNewData = true;
	        newStreamBuffer.Put(streamId, newEventBuffer);
	        oldStreamBuffer.Put(streamId, oldEventBuffer);
	    }
	}
} // end of namespace