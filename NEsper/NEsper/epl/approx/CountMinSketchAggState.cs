///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.approx
{
    public class CountMinSketchAggState : AggregationState
    {
        private readonly CountMinSketchState _state;
        private readonly CountMinSketchAgent _agent;
    
        private readonly CountMinSketchAgentContextAdd _add;
        private readonly CountMinSketchAgentContextEstimate _estimate;
        private readonly CountMinSketchAgentContextFromBytes _fromBytes;
    
        public CountMinSketchAggState(CountMinSketchState state, CountMinSketchAgent agent)
        {
            _state = state;
            _agent = agent;
            _add = new CountMinSketchAgentContextAdd(state);
            _estimate = new CountMinSketchAgentContextEstimate(state);
            _fromBytes = new CountMinSketchAgentContextFromBytes(state);
        }
    
        public void ApplyEnter(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            throw new UnsupportedOperationException("values are added through the add method");
        }
    
        public void ApplyLeave(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            throw new UnsupportedOperationException();
        }
    
        public void Add(object value)
        {
            _add.Value = value;
            _agent.Add(_add);
        }
    
        public long? Frequency(object value)
        {
            _estimate.Value = value;
            return _agent.Estimate(_estimate);
        }
    
        public void Clear()
        {
            throw new UnsupportedOperationException();
        }
    
        public CountMinSketchTopK[] GetFromBytes()
        {
            var bytes = _state.TopKValues;
            if (bytes.IsEmpty()) {
                return new CountMinSketchTopK[0];
            }
            var arr = new CountMinSketchTopK[bytes.Count];
            var index = 0;
            foreach (var buf in bytes) {
                long? frequency = _state.Frequency(buf.Data);
                _fromBytes.Bytes = buf.Data;
                var value = _agent.FromBytes(_fromBytes);
                if (frequency == null) {
                    continue;
                }
                arr[index++] = new CountMinSketchTopK(frequency.Value, value);
            }
            return arr;
        }
    }
}
