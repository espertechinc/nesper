///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.view.internals;

namespace com.espertech.esper.epl.join.@base
{
    /// <summary>
    /// : a method for pre-loading (initializing) join indexes from a filled buffer.
    /// </summary>
    public class JoinPreloadMethodImpl : JoinPreloadMethod
    {
        private readonly int _numStreams;
        private readonly BufferView[] _bufferViews;
        private readonly JoinSetComposer _joinSetComposer;
    
        /// <summary>Ctor. </summary>
        /// <param name="numStreams">number of streams</param>
        /// <param name="joinSetComposer">the composer holding stream indexes</param>
        public JoinPreloadMethodImpl(int numStreams, JoinSetComposer joinSetComposer)
        {
            _numStreams = numStreams;
            _bufferViews = new BufferView[numStreams];
            _joinSetComposer = joinSetComposer;
        }
    
        /// <summary>Sets the buffer for a stream to preload events from. </summary>
        /// <param name="view">buffer</param>
        /// <param name="stream">the stream number for the buffer</param>
        public void SetBuffer(BufferView view, int stream)
        {
            _bufferViews[stream] = view;
        }
    
        public void PreloadFromBuffer(int stream, ExprEvaluatorContext exprEvaluatorContext)
        {
            var preloadEvents = _bufferViews[stream].NewDataBuffer.GetAndFlush();
            var eventsPerStream = new EventBean[_numStreams][];
            eventsPerStream[stream] = preloadEvents;
            _joinSetComposer.Init(eventsPerStream, exprEvaluatorContext);
        }
    
        public void PreloadAggregation(ResultSetProcessor resultSetProcessor)
        {
            var newEvents = _joinSetComposer.StaticJoin();
            var oldEvents = new HashSet<MultiKey<EventBean>>();
            resultSetProcessor.ProcessJoinResult(newEvents, oldEvents, false);
        }
    
        public bool IsPreloading
        {
            get { return true; }
        }
    }
}
