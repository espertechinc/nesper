///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.view.internals;

namespace com.espertech.esper.epl.join.@base
{
    /// <summary>
    /// This class reacts to any new data buffered by registring with the dispatch service. 
    /// When dispatched via execute, it takes the buffered events and hands these to the join
    ///  execution strategy.
    /// </summary>
    public class JoinExecStrategyDispatchable : EPStatementDispatch, BufferObserver
    {
        private readonly JoinExecutionStrategy _joinExecutionStrategy;
        private readonly IDictionary<int, FlushedEventBuffer> _oldStreamBuffer;
        private readonly IDictionary<int, FlushedEventBuffer> _newStreamBuffer;
        private readonly int _numStreams;
    
        private bool _hasNewData;
    
        /// <summary>CTor. </summary>
        /// <param name="joinExecutionStrategy">strategy for executing the join</param>
        /// <param name="numStreams">number of stream</param>
        public JoinExecStrategyDispatchable(JoinExecutionStrategy joinExecutionStrategy, int numStreams)
        {
            _joinExecutionStrategy = joinExecutionStrategy;
            _numStreams = numStreams;
    
            _oldStreamBuffer = new Dictionary<int, FlushedEventBuffer>();
            _newStreamBuffer = new Dictionary<int, FlushedEventBuffer>();
        }
    
        public void Execute()
        {
            if (!_hasNewData)
            {
                return;
            }
            _hasNewData = false;
    
            var oldDataPerStream = new EventBean[_numStreams][];
            var newDataPerStream = new EventBean[_numStreams][];
    
            for (var i = 0; i < _numStreams; i++)
            {
                oldDataPerStream[i] = GetBufferData(_oldStreamBuffer.Get(i));
                newDataPerStream[i] = GetBufferData(_newStreamBuffer.Get(i));
            }
    
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QJoinDispatch(newDataPerStream, oldDataPerStream);}
            _joinExecutionStrategy.Join(newDataPerStream, oldDataPerStream);
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AJoinDispatch();}
        }
    
        private static EventBean[] GetBufferData(FlushedEventBuffer buffer)
        {
            if (buffer == null)
            {
                return null;
            }
            return buffer.GetAndFlush();
        }
    
        public void NewData(int streamId, FlushedEventBuffer newEventBuffer, FlushedEventBuffer oldEventBuffer)
        {
            _hasNewData = true;
            _newStreamBuffer.Put(streamId, newEventBuffer);
            _oldStreamBuffer.Put(streamId, oldEventBuffer);
        }
    }
}
