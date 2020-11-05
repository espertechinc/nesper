///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.approx.countminsketch;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.agg.access.countminsketch
{
	public class AgregationMethodCountMinSketchFreq : AggregationMultiFunctionAggregationMethod
	{
		private ExprEvaluator frequencyEval;

		public ExprEvaluator FrequencyEval {
			get => this.frequencyEval;
			set => this.frequencyEval = value;
		}

		public object GetValue(
			int aggColNum,
			AggregationRow row,
			EventBean[] eventsPerStream,
			bool isNewData,
			ExprEvaluatorContext exprEvaluatorContext)
		{
			var value = frequencyEval.Evaluate(eventsPerStream, true, exprEvaluatorContext);
			var state = (CountMinSketchAggState) row.GetAccessState(aggColNum);
			return state.Frequency(value);
		}

		public ICollection<EventBean> GetValueCollectionEvents(
			int aggColNum,
			AggregationRow row,
			EventBean[] eventsPerStream,
			bool isNewData,
			ExprEvaluatorContext exprEvaluatorContext)
		{
			return null;
		}

		public ICollection<object> GetValueCollectionScalar(
			int aggColNum,
			AggregationRow row,
			EventBean[] eventsPerStream,
			bool isNewData,
			ExprEvaluatorContext exprEvaluatorContext)
		{
			return null;
		}

		public EventBean GetValueEventBean(
			int aggColNum,
			AggregationRow row,
			EventBean[] eventsPerStream,
			bool isNewData,
			ExprEvaluatorContext exprEvaluatorContext)
		{
			return null;
		}
	}
} // end of namespace
