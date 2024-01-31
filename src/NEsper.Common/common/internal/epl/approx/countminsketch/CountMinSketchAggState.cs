///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.epl.approx.countminsketch
{
    public class CountMinSketchAggState : AggregationMultiFunctionState
    {
        internal readonly CountMinSketchState state;
        private readonly CountMinSketchAgent agent;
        private readonly CountMinSketchAgentContextAdd add;
        private readonly CountMinSketchAgentContextEstimate estimate;
        private readonly CountMinSketchAgentContextFromBytes fromBytes;

        public CountMinSketchAggState(
            CountMinSketchState state,
            CountMinSketchAgent agent)
        {
            this.state = state;
            this.agent = agent;
            add = new CountMinSketchAgentContextAdd(state);
            estimate = new CountMinSketchAgentContextEstimate(state);
            fromBytes = new CountMinSketchAgentContextFromBytes(state);
        }

        public void ApplyEnter(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            throw new UnsupportedOperationException("values are added through the add method");
        }

        public void ApplyLeave(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            throw new UnsupportedOperationException();
        }

        public void Add(object value)
        {
            add.Value = value;
            agent.Add(add);
        }

        public long? Frequency(object value)
        {
            estimate.Value = value;
            return agent.Estimate(estimate);
        }

        public void Clear()
        {
            throw new UnsupportedOperationException();
        }

        public CountMinSketchState State => state;

        public CountMinSketchTopK[] GetFromBytes()
        {
            var bytes = state.TopKValues;
            if (bytes.IsEmpty()) {
                return Array.Empty<CountMinSketchTopK>();
            }

            var arr = new CountMinSketchTopK[bytes.Count];
            var index = 0;
            foreach (var buf in bytes) {
                var frequency = state.Frequency(buf.Array);
                fromBytes.Bytes = buf.Array;
                var value = agent.FromBytes(fromBytes);
                //if (frequency == null) {
                //    continue;
                //}
                arr[index++] = new CountMinSketchTopK(frequency, value);
            }

            return arr;
        }
    }
} // end of namespace