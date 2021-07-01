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
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.view.util;

namespace com.espertech.esper.common.@internal.epl.join.@base
{
    /// <summary>
    ///     Implements a method for pre-loading (initializing) join indexes from a filled buffer.
    /// </summary>
    public class JoinPreloadMethodImpl : JoinPreloadMethod
    {
        private readonly BufferView[] bufferViews;
        private readonly JoinSetComposer joinSetComposer;
        private readonly int numStreams;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="numStreams">number of streams</param>
        /// <param name="joinSetComposer">the composer holding stream indexes</param>
        public JoinPreloadMethodImpl(
            int numStreams,
            JoinSetComposer joinSetComposer)
        {
            this.numStreams = numStreams;
            bufferViews = new BufferView[numStreams];
            this.joinSetComposer = joinSetComposer;
        }

        /// <summary>
        ///     Sets the buffer for a stream to preload events from.
        /// </summary>
        /// <param name="view">buffer</param>
        /// <param name="stream">the stream number for the buffer</param>
        public void SetBuffer(
            BufferView view,
            int stream)
        {
            bufferViews[stream] = view;
        }

        public void PreloadFromBuffer(
            int stream,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            EventBean[] preloadEvents = bufferViews[stream].NewDataBuffer.GetAndFlush();
            var eventsPerStream = new EventBean[numStreams][];
            eventsPerStream[stream] = preloadEvents;
            joinSetComposer.Init(eventsPerStream, exprEvaluatorContext);
        }

        public void PreloadAggregation(ResultSetProcessor resultSetProcessor)
        {
            var newEvents = joinSetComposer.StaticJoin();
            ISet<MultiKeyArrayOfKeys<EventBean>> oldEvents = new HashSet<MultiKeyArrayOfKeys<EventBean>>();
            resultSetProcessor.ProcessJoinResult(newEvents, oldEvents, false);
        }

        public bool IsPreloading => true;
    }
} // end of namespace