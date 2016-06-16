///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.epl.core
{
	public class ResultSetProcessorAggregateGroupedOutputLastHelperImpl : ResultSetProcessorAggregateGroupedOutputLastHelper
    {
	    private readonly ResultSetProcessorAggregateGrouped _processor;

	    private readonly IDictionary<object, EventBean> _outputLastUnordGroupNew;
	    private readonly IDictionary<object, EventBean> _outputLastUnordGroupOld;

	    public ResultSetProcessorAggregateGroupedOutputLastHelperImpl(ResultSetProcessorAggregateGrouped processor) {
	        _processor = processor;
	        _outputLastUnordGroupNew = new LinkedHashMap<object, EventBean>();
	        _outputLastUnordGroupOld = new LinkedHashMap<object, EventBean>();
	    }

	    public void ProcessView(EventBean[] newData, EventBean[] oldData, bool isGenerateSynthetic) {
	        object[] newDataMultiKey = _processor.GenerateGroupKeys(newData, true);
	        object[] oldDataMultiKey = _processor.GenerateGroupKeys(oldData, false);

	        if (newData != null)
	        {
	            // apply new data to aggregates
	            int count = 0;
	            foreach (EventBean aNewData in newData)
	            {
	                object mk = newDataMultiKey[count];
	                _processor.EventsPerStreamOneStream[0] = aNewData;
	                _processor.AggregationService.ApplyEnter(_processor.EventsPerStreamOneStream, mk, _processor.AgentInstanceContext);
	                count++;
	            }
	        }
	        if (oldData != null)
	        {
	            // apply old data to aggregates
	            int count = 0;
	            foreach (EventBean anOldData in oldData)
	            {
	                _processor.EventsPerStreamOneStream[0] = anOldData;
	                _processor.AggregationService.ApplyLeave(_processor.EventsPerStreamOneStream, oldDataMultiKey[count], _processor.AgentInstanceContext);
	                count++;
	            }
	        }

	        if (_processor.Prototype.IsSelectRStream) {
	            _processor.GenerateOutputBatchedViewPerKey(oldData, oldDataMultiKey, false, isGenerateSynthetic, _outputLastUnordGroupOld, null);
	        }
	        _processor.GenerateOutputBatchedViewPerKey(newData, newDataMultiKey, false, isGenerateSynthetic, _outputLastUnordGroupNew, null);
	    }

	    public void ProcessJoin(ISet<MultiKey<EventBean>> newData, ISet<MultiKey<EventBean>> oldData, bool isGenerateSynthetic) {
	        object[] newDataMultiKey = _processor.GenerateGroupKeys(newData, true);
	        object[] oldDataMultiKey = _processor.GenerateGroupKeys(oldData, false);

	        if (newData != null) {
	            // apply new data to aggregates
	            int count = 0;
	            foreach (MultiKey<EventBean> aNewData in newData)
	            {
	                object mk = newDataMultiKey[count];
	                _processor.AggregationService.ApplyEnter(aNewData.Array, mk, _processor.AgentInstanceContext);
	                count++;
	            }
	        }
	        if (oldData != null)
	        {
	            // apply old data to aggregates
	            int count = 0;
	            foreach (MultiKey<EventBean> anOldData in oldData)
	            {
	                _processor.AggregationService.ApplyLeave(anOldData.Array, oldDataMultiKey[count], _processor.AgentInstanceContext);
	                count++;
	            }
	        }

	        if (_processor.Prototype.IsSelectRStream) {
	            _processor.GenerateOutputBatchedJoinPerKey(oldData, oldDataMultiKey, false, isGenerateSynthetic, _outputLastUnordGroupOld, null);
	        }
	        _processor.GenerateOutputBatchedJoinPerKey(newData, newDataMultiKey, false, isGenerateSynthetic, _outputLastUnordGroupNew, null);
	    }

	    public UniformPair<EventBean[]> OutputView(bool isSynthesize) {
	        return ContinueOutputLimitedLastNonBuffered();
	    }

	    public UniformPair<EventBean[]> OutputJoin(bool isSynthesize) {
	        return ContinueOutputLimitedLastNonBuffered();
	    }

	    public void Remove(object key) {
	        // no action required
	    }

	    public void Destroy() {
	        // no action required
	    }

	    private UniformPair<EventBean[]> ContinueOutputLimitedLastNonBuffered() {
	        EventBean[] newEventsArr = (_outputLastUnordGroupNew.IsEmpty()) ? null : _outputLastUnordGroupNew.Values.ToArrayOrNull();
	        EventBean[] oldEventsArr = null;
	        if (_processor.Prototype.IsSelectRStream) {
	            oldEventsArr = (_outputLastUnordGroupOld.IsEmpty()) ? null : _outputLastUnordGroupOld.Values.ToArrayOrNull();
	        }
	        if ((newEventsArr == null) && (oldEventsArr == null)) {
	            return null;
	        }
	        _outputLastUnordGroupNew.Clear();
	        _outputLastUnordGroupOld.Clear();
	        return new UniformPair<EventBean[]>(newEventsArr, oldEventsArr);
	    }
	}
} // end of namespace
