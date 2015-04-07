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
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.core
{
    public class ResultSetProcessorRowPerGroupRollupUnbound : ResultSetProcessorRowPerGroupRollup
    {
        public ResultSetProcessorRowPerGroupRollupUnbound(ResultSetProcessorRowPerGroupRollupFactory prototype, OrderByProcessor orderByProcessor, AggregationService aggregationService, AgentInstanceContext agentInstanceContext)
            : base(prototype, orderByProcessor, aggregationService, agentInstanceContext)
        {
        }

        public override void ApplyViewResult(EventBean[] newData, EventBean[] oldData)
        {
            var newDataMultiKey = GenerateGroupKeysView(newData, EventPerGroupBuf, true);
            var oldDataMultiKey = GenerateGroupKeysView(oldData, EventPerGroupBuf, false);

            // update aggregates
            var eventsPerStream = new EventBean[1];
            if (newData != null)
            {
                for (var i = 0; i < newData.Length; i++)
                {
                    eventsPerStream[0] = newData[i];
                    AggregationService.ApplyEnter(eventsPerStream, newDataMultiKey[i], AgentInstanceContext);
                }
            }
            if (oldData != null)
            {
                for (var i = 0; i < oldData.Length; i++)
                {
                    eventsPerStream[0] = oldData[i];
                    AggregationService.ApplyLeave(eventsPerStream, oldDataMultiKey[i], AgentInstanceContext);
                }
            }
        }
    
        public override UniformPair<EventBean[]> ProcessViewResult(EventBean[] newData, EventBean[] oldData, bool isSynthesize)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QResultSetProcessGroupedRowPerGroup();}
    
            var newDataMultiKey = GenerateGroupKeysView(newData, EventPerGroupBuf, true);
            var oldDataMultiKey = GenerateGroupKeysView(oldData, EventPerGroupBuf, false);
    
            EventBean[] selectOldEvents = null;
            if (Prototype.IsSelectRStream) {
                selectOldEvents = GenerateOutputEventsView(EventPerGroupBuf, false, isSynthesize);
            }
    
            // update aggregates
            var eventsPerStream = new EventBean[1];
            if (newData != null) {
                for (var i = 0; i < newData.Length; i++) {
                    eventsPerStream[0] = newData[i];
                    AggregationService.ApplyEnter(eventsPerStream, newDataMultiKey[i], AgentInstanceContext);
                }
            }
            if (oldData != null) {
                for (var i = 0; i < oldData.Length; i++) {
                    eventsPerStream[0] = oldData[i];
                    AggregationService.ApplyLeave(eventsPerStream, oldDataMultiKey[i], AgentInstanceContext);
                }
            }
    
            // generate new events using select expressions
            var selectNewEvents = GenerateOutputEventsView(EventPerGroupBuf, true, isSynthesize);
    
            if ((selectNewEvents != null) || (selectOldEvents != null)) {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AResultSetProcessGroupedRowPerGroup(selectNewEvents, selectOldEvents);}
                return new UniformPair<EventBean[]>(selectNewEvents, selectOldEvents);
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AResultSetProcessGroupedRowPerGroup(null, null);}
            return null;
        }
    
        public override IEnumerator<EventBean> GetEnumerator(Viewable parent)
        {
            var output = GenerateOutputEventsView(EventPerGroupBuf, true, true);
            if (output == null)
                return EnumerationHelper<EventBean>.CreateEmptyEnumerator();
            return ((IEnumerable<EventBean>) output).GetEnumerator();
        }
    }
}
