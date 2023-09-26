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
            var pair = processor.ProcessViewResult(newData, oldData, isGenerateSynthetic);
            Apply(pair);
        }

        public void ProcessJoin(
            ISet<MultiKeyArrayOfKeys<EventBean>> newEvents,
            ISet<MultiKeyArrayOfKeys<EventBean>> oldEvents,
            bool isGenerateSynthetic)
        {
            var pair = processor.ProcessJoinResult(newEvents, oldEvents, isGenerateSynthetic);
            Apply(pair);
        }

        public UniformPair<EventBean[]> Output()
        {
            UniformPair<EventBean[]> newOldEvents = null;
            if (lastEventIStreamForOutputLast != null) {
                var istream = new EventBean[] { lastEventIStreamForOutputLast };
                newOldEvents = new UniformPair<EventBean[]>(istream, null);
            }

            if (lastEventRStreamForOutputLast != null) {
                var rstream = new EventBean[] { lastEventRStreamForOutputLast };
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
                lastEventIStreamForOutputLast = pair.First[^1];
            }

            if (pair.Second != null && pair.Second.Length > 0) {
                lastEventRStreamForOutputLast = pair.Second[^1];
            }
        }
    }
} // end of namespace