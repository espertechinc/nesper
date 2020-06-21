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
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.resultset.rowperevent
{
    public class ResultSetProcessorRowPerEventOutputLastHelperImpl : ResultSetProcessorRowPerEventOutputLastHelper
    {
        private readonly ResultSetProcessorRowPerEvent processor;

        private EventBean lastEventIStreamForOutputLast;
        private EventBean lastEventRStreamForOutputLast;

        public ResultSetProcessorRowPerEventOutputLastHelperImpl(ResultSetProcessorRowPerEvent processor)
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
            UniformPair<EventBean[]> newOldEvents = null;
            if (lastEventIStreamForOutputLast != null) {
                EventBean[] istream = new EventBean[] {lastEventIStreamForOutputLast};
                newOldEvents = new UniformPair<EventBean[]>(istream, null);
            }

            if (lastEventRStreamForOutputLast != null) {
                EventBean[] rstream = new EventBean[] {lastEventRStreamForOutputLast};
                if (newOldEvents == null) {
                    newOldEvents = new UniformPair<EventBean[]>(null, rstream);
                }
                else {
                    newOldEvents.Second = rstream;
                }
            }

            lastEventIStreamForOutputLast = null;
            lastEventRStreamForOutputLast = null;
            return newOldEvents;
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

            if (pair.First != null && pair.First.Length > 0) {
                lastEventIStreamForOutputLast = pair.First[pair.First.Length - 1];
            }

            if (pair.Second != null && pair.Second.Length > 0) {
                lastEventRStreamForOutputLast = pair.Second[pair.Second.Length - 1];
            }
        }
    }
} // end of namespace