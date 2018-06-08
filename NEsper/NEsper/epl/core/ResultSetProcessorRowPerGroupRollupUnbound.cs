///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
        private readonly ResultSetProcessorRowPerGroupRollupUnboundHelper _unboundHelper;

        public ResultSetProcessorRowPerGroupRollupUnbound(ResultSetProcessorRowPerGroupRollupFactory prototype, OrderByProcessor orderByProcessor, AggregationService aggregationService, AgentInstanceContext agentInstanceContext)
            : base(prototype, orderByProcessor, aggregationService, agentInstanceContext)
        {
            _unboundHelper = prototype.ResultSetProcessorHelperFactory.MakeRSRowPerGroupRollupSnapshotUnbound(agentInstanceContext, prototype);
        }

        public override void Stop()
        {
            base.Stop();
            _unboundHelper.Destroy();
        }

        public override void ApplyViewResult(EventBean[] newData, EventBean[] oldData)
        {
            var newDataMultiKey = GenerateGroupKeysView(newData, _unboundHelper.Buffer, true);
            var oldDataMultiKey = GenerateGroupKeysView(oldData, _unboundHelper.Buffer, false);

            var aggregationService = AggregationService;
            var agentInstanceContext = AgentInstanceContext;

            // update aggregates
            var eventsPerStream = new EventBean[1];
            if (newData != null)
            {
                for (var i = 0; i < newData.Length; i++)
                {
                    eventsPerStream[0] = newData[i];
                    aggregationService.ApplyEnter(eventsPerStream, newDataMultiKey[i], agentInstanceContext);
                }
            }
            if (oldData != null)
            {
                for (var i = 0; i < oldData.Length; i++)
                {
                    eventsPerStream[0] = oldData[i];
                    aggregationService.ApplyLeave(eventsPerStream, oldDataMultiKey[i], agentInstanceContext);
                }
            }
        }
    
        public override UniformPair<EventBean[]> ProcessViewResult(EventBean[] newData, EventBean[] oldData, bool isSynthesize)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QResultSetProcessGroupedRowPerGroup();}

            var newDataMultiKey = GenerateGroupKeysView(newData, _unboundHelper.Buffer, true);
            var oldDataMultiKey = GenerateGroupKeysView(oldData, _unboundHelper.Buffer, false);
    
            EventBean[] selectOldEvents = null;
            if (Prototype.IsSelectRStream) {
                selectOldEvents = GenerateOutputEventsView(_unboundHelper.Buffer, false, isSynthesize);
            }

            var aggregationService = AggregationService;
            var agentInstanceContext = AgentInstanceContext;
    
            // update aggregates
            var eventsPerStream = new EventBean[1];
            if (newData != null) {
                for (var i = 0; i < newData.Length; i++) {
                    eventsPerStream[0] = newData[i];
                    aggregationService.ApplyEnter(eventsPerStream, newDataMultiKey[i], agentInstanceContext);
                }
            }
            if (oldData != null) {
                for (var i = 0; i < oldData.Length; i++) {
                    eventsPerStream[0] = oldData[i];
                    aggregationService.ApplyLeave(eventsPerStream, oldDataMultiKey[i], agentInstanceContext);
                }
            }
    
            // generate new events using select expressions
            var selectNewEvents = GenerateOutputEventsView(_unboundHelper.Buffer, true, isSynthesize);
    
            if ((selectNewEvents != null) || (selectOldEvents != null)) {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AResultSetProcessGroupedRowPerGroup(selectNewEvents, selectOldEvents);}
                return new UniformPair<EventBean[]>(selectNewEvents, selectOldEvents);
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AResultSetProcessGroupedRowPerGroup(null, null);}
            return null;
        }
    
        public override IEnumerator<EventBean> GetEnumerator(Viewable parent)
        {
            var output = GenerateOutputEventsView(_unboundHelper.Buffer, true, true);
            if (output == null)
                return EnumerationHelper.Empty<EventBean>();
            return ((IEnumerable<EventBean>) output).GetEnumerator();
        }
    }
}
