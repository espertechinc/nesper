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
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.resultset.rowperevent
{
    public class ResultSetProcessorRowPerEventOutputAllHelperImpl : ResultSetProcessorRowPerEventOutputAllHelper
    {
        private readonly ResultSetProcessorRowPerEvent processor;
        private readonly Deque<EventBean> eventsOld = new ArrayDeque<EventBean>(2);
        private readonly Deque<EventBean> eventsNew = new ArrayDeque<EventBean>(2);

        public ResultSetProcessorRowPerEventOutputAllHelperImpl(ResultSetProcessorRowPerEvent processor)
        {
            this.processor = processor;
        }

        public void ProcessView(
            EventBean[] newData,
            EventBean[] oldData,
            bool isGenerateSynthetic)
        {
            UniformPair<EventBean[]> pair = processor.ProcessViewResult(newData, oldData, isGenerateSynthetic);
            Apply(pair);
        }

        public void ProcessJoin(
            ISet<MultiKeyArrayOfKeys<EventBean>> newEvents,
            ISet<MultiKeyArrayOfKeys<EventBean>> oldEvents,
            bool isGenerateSynthetic)
        {
            UniformPair<EventBean[]> pair = processor.ProcessJoinResult(newEvents, oldEvents, isGenerateSynthetic);
            Apply(pair);
        }

        public UniformPair<EventBean[]> Output()
        {
            EventBean[] oldEvents = EventBeanUtility.ToArrayNullIfEmpty(eventsOld);
            EventBean[] newEvents = EventBeanUtility.ToArrayNullIfEmpty(eventsNew);

            UniformPair<EventBean[]> result = null;
            if (oldEvents != null || newEvents != null) {
                result = new UniformPair<EventBean[]>(newEvents, oldEvents);
            }

            eventsOld.Clear();
            eventsNew.Clear();
            return result;
        }

        public void Destroy()
        {
            // no action required
        }

        private void Apply(UniformPair<EventBean[]> pair)
        {
            if (pair == null) {
                return;
            }

            EventBeanUtility.AddToCollection(pair.First, eventsNew);
            EventBeanUtility.AddToCollection(pair.Second, eventsOld);
        }
    }
} // end of namespace