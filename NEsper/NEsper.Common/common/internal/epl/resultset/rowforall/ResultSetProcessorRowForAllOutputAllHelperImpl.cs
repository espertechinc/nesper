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
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.resultset.rowforall
{
    public class ResultSetProcessorRowForAllOutputAllHelperImpl : ResultSetProcessorRowForAllOutputAllHelper
    {
        private readonly Deque<EventBean> eventsNew = new ArrayDeque<EventBean>(2);
        private readonly Deque<EventBean> eventsOld = new ArrayDeque<EventBean>(2);
        private readonly ResultSetProcessorRowForAll processor;

        public ResultSetProcessorRowForAllOutputAllHelperImpl(ResultSetProcessorRowForAll processor)
        {
            this.processor = processor;
        }

        public void ProcessView(
            EventBean[] newData,
            EventBean[] oldData,
            bool isGenerateSynthetic)
        {
            if (processor.IsSelectRStream) {
                var eventsX = processor.GetSelectListEventsAsArray(false, isGenerateSynthetic, false);
                EventBeanUtility.AddToCollection(eventsX, eventsOld);
            }

            var eventsPerStream = new EventBean[1];
            ResultSetProcessorUtil.ApplyAggViewResult(
                processor.AggregationService,
                processor.ExprEvaluatorContext,
                newData,
                oldData,
                eventsPerStream);

            var events = processor.GetSelectListEventsAsArray(true, isGenerateSynthetic, false);
            EventBeanUtility.AddToCollection(events, eventsNew);
        }

        public void ProcessJoin(
            ISet<MultiKey<EventBean>> newEvents,
            ISet<MultiKey<EventBean>> oldEvents,
            bool isGenerateSynthetic)
        {
            if (processor.IsSelectRStream) {
                var eventsX = processor.GetSelectListEventsAsArray(false, isGenerateSynthetic, true);
                EventBeanUtility.AddToCollection(eventsX, eventsOld);
            }

            ResultSetProcessorUtil.ApplyAggJoinResult(
                processor.AggregationService,
                processor.ExprEvaluatorContext,
                newEvents,
                oldEvents);

            var events = processor.GetSelectListEventsAsArray(true, isGenerateSynthetic, true);
            EventBeanUtility.AddToCollection(events, eventsNew);
        }

        public UniformPair<EventBean[]> OutputView(bool isGenerateSynthetic)
        {
            return Output(isGenerateSynthetic, false);
        }

        public UniformPair<EventBean[]> OutputJoin(bool isGenerateSynthetic)
        {
            return Output(isGenerateSynthetic, true);
        }

        public void Destroy()
        {
            // no action required
        }

        private UniformPair<EventBean[]> Output(
            bool isGenerateSynthetic,
            bool isJoin)
        {
            var oldEvents = EventBeanUtility.ToArrayNullIfEmpty(eventsOld);
            var newEvents = EventBeanUtility.ToArrayNullIfEmpty(eventsNew);

            if (newEvents == null) {
                newEvents = processor.GetSelectListEventsAsArray(true, isGenerateSynthetic, isJoin);
            }

            if (oldEvents == null && processor.IsSelectRStream) {
                oldEvents = processor.GetSelectListEventsAsArray(false, isGenerateSynthetic, isJoin);
            }

            UniformPair<EventBean[]> result = null;
            if (oldEvents != null || newEvents != null) {
                result = new UniformPair<EventBean[]>(newEvents, oldEvents);
            }

            eventsOld.Clear();
            eventsNew.Clear();
            return result;
        }
    }
} // end of namespace