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

namespace com.espertech.esper.common.@internal.epl.resultset.rowforall
{
    public class ResultSetProcessorRowForAllOutputLastHelperImpl : ResultSetProcessorRowForAllOutputLastHelper
    {
        private readonly ResultSetProcessorRowForAll processor;
        private EventBean[] lastEventRStreamForOutputLast;

        public ResultSetProcessorRowForAllOutputLastHelperImpl(ResultSetProcessorRowForAll processor)
        {
            this.processor = processor;
        }

        public void ProcessView(
            EventBean[] newData,
            EventBean[] oldData,
            bool isGenerateSynthetic)
        {
            if (processor.IsSelectRStream && lastEventRStreamForOutputLast == null) {
                lastEventRStreamForOutputLast = processor.GetSelectListEventsAsArray(false, isGenerateSynthetic, false);
            }

            var eventsPerStream = new EventBean[1];
            ResultSetProcessorUtil.ApplyAggViewResult(
                processor.AggregationService,
                processor.ExprEvaluatorContext,
                newData,
                oldData,
                eventsPerStream);
        }

        public void ProcessJoin(
            ISet<MultiKey<EventBean>> newEvents,
            ISet<MultiKey<EventBean>> oldEvents,
            bool isGenerateSynthetic)
        {
            if (processor.IsSelectRStream && lastEventRStreamForOutputLast == null) {
                lastEventRStreamForOutputLast = processor.GetSelectListEventsAsArray(false, isGenerateSynthetic, true);
            }

            ResultSetProcessorUtil.ApplyAggJoinResult(
                processor.AggregationService,
                processor.ExprEvaluatorContext,
                newEvents,
                oldEvents);
        }

        public UniformPair<EventBean[]> OutputView(bool isSynthesize)
        {
            return ContinueOutputLimitedLastNonBuffered(isSynthesize);
        }

        public UniformPair<EventBean[]> OutputJoin(bool isSynthesize)
        {
            return ContinueOutputLimitedLastNonBuffered(isSynthesize);
        }

        public void Destroy()
        {
            // no action required
        }

        private UniformPair<EventBean[]> ContinueOutputLimitedLastNonBuffered(bool isSynthesize)
        {
            var events = processor.GetSelectListEventsAsArray(true, isSynthesize, false);
            var result = new UniformPair<EventBean[]>(events, null);

            if (processor.IsSelectRStream && lastEventRStreamForOutputLast == null) {
                lastEventRStreamForOutputLast = processor.GetSelectListEventsAsArray(false, isSynthesize, false);
            }

            if (lastEventRStreamForOutputLast != null) {
                result.Second = lastEventRStreamForOutputLast;
                lastEventRStreamForOutputLast = null;
            }

            return result;
        }
    }
} // end of namespace