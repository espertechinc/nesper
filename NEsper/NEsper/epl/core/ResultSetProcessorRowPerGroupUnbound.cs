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
using com.espertech.esper.view;

namespace com.espertech.esper.epl.core
{
    public class ResultSetProcessorRowPerGroupUnbound : ResultSetProcessorRowPerGroup, AggregationRowRemovedCallback
    {
        protected readonly IDictionary<Object, EventBean> GroupReps = new LinkedHashMap<Object, EventBean>();
    
        public ResultSetProcessorRowPerGroupUnbound(ResultSetProcessorRowPerGroupFactory prototype, SelectExprProcessor selectExprProcessor, OrderByProcessor orderByProcessor, AggregationService aggregationService, AgentInstanceContext agentInstanceContext)
                    : base(prototype, selectExprProcessor, orderByProcessor, aggregationService, agentInstanceContext)
        {
            aggregationService.SetRemovedCallback(this);
        }
    
        public override void ApplyViewResult(EventBean[] newData, EventBean[] oldData)
        {
            EventBean[] eventsPerStream = new EventBean[1];
            if (newData != null)
            {
                var groupReps = GroupReps;
                var newDataLength = newData.Length;
                for(int ii = 0 ; ii < newDataLength ; ii++)
                {
                    eventsPerStream[0] = newData[ii];
                    Object mk = GenerateGroupKey(eventsPerStream, true);
                    groupReps[mk] = eventsPerStream[0];
                    AggregationService.ApplyEnter(eventsPerStream, mk, AgentInstanceContext);
                }
            }
            if (oldData != null)
            {
                var oldDataLength = oldData.Length;
                for (int ii  = 0; ii < oldDataLength; ii++)
                {
                    eventsPerStream[0] = oldData[ii];
                    Object mk = GenerateGroupKey(eventsPerStream, false);
                    AggregationService.ApplyLeave(eventsPerStream, mk, AgentInstanceContext);
                }
            }
        }

        public override UniformPair<EventBean[]> ProcessViewResult(EventBean[] newData, EventBean[] oldData, bool isSynthesize)
        {
            // Generate group-by keys for all events, collect all keys in a set for later event generation
            IDictionary<Object, EventBean> keysAndEvents = new Dictionary<Object, EventBean>();
            Object[] newDataMultiKey = GenerateGroupKeys(newData, keysAndEvents, true);
            Object[] oldDataMultiKey = GenerateGroupKeys(oldData, keysAndEvents, false);
    
            EventBean[] selectOldEvents = null;
            if (Prototype.IsSelectRStream)
            {
                selectOldEvents = GenerateOutputEventsView(keysAndEvents, false, isSynthesize);
            }
    
            // update aggregates
            EventBean[] eventsPerStream = new EventBean[1];
            if (newData != null)
            {
                // apply new data to aggregates
                for (int i = 0; i < newData.Length; i++)
                {
                    eventsPerStream[0] = newData[i];
                    GroupReps.Put(newDataMultiKey[i], eventsPerStream[0]);
                    AggregationService.ApplyEnter(eventsPerStream, newDataMultiKey[i], AgentInstanceContext);
                }
            }
            if (oldData != null)
            {
                // apply old data to aggregates
                for (int i = 0; i < oldData.Length; i++)
                {
                    eventsPerStream[0] = oldData[i];
                    AggregationService.ApplyLeave(eventsPerStream, oldDataMultiKey[i], AgentInstanceContext);
                }
            }
    
            // generate new events using select expressions
            EventBean[] selectNewEvents = GenerateOutputEventsView(keysAndEvents, true, isSynthesize);
    
            if ((selectNewEvents != null) || (selectOldEvents != null))
            {
                return new UniformPair<EventBean[]>(selectNewEvents, selectOldEvents);
            }
            return null;
        }
    
        public override IEnumerator<EventBean> GetEnumerator(Viewable parent)
        {
            if (OrderByProcessor == null)
            {
                return ResultSetRowPerGroupEnumerator.New(GroupReps.Values, this, AggregationService, AgentInstanceContext);
            }
            return GetEnumeratorSorted(GroupReps.Values.GetEnumerator());
        }
    
        public override void Removed(Object optionalGroupKeyPerRow)
        {
            GroupReps.Remove(optionalGroupKeyPerRow);
        }
    }
}
