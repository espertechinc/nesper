///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.agg.access.sorted
{
	public class AggregationMethodSortedKeyedEval : AggregationMultiFunctionAggregationMethod
	{

		private readonly ExprEvaluator _keyEval;
		private readonly Func<IOrderedDictionary<object, object>, object, object> _value;
		private readonly Func<IOrderedDictionary<object, object>, object, EventBean> _event;
		private readonly Func<IOrderedDictionary<object, object>, object, ICollection<EventBean>> _events;

		public AggregationMethodSortedKeyedEval(
			ExprEvaluator keyEval,
			Func<IOrderedDictionary<object, object>, object, object> value,
			Func<IOrderedDictionary<object, object>, object, EventBean> @event,
			Func<IOrderedDictionary<object, object>, object, ICollection<EventBean>> events)
		{
			this._keyEval = keyEval;
			this._value = value;
			this._event = @event;
			this._events = events;
		}

		public object GetValue(
			int aggColNum,
			AggregationRow row,
			EventBean[] eventsPerStream,
			bool isNewData,
			ExprEvaluatorContext exprEvaluatorContext)
		{
			var sorted = (AggregationStateSorted) row.GetAccessState(aggColNum);
			var key = _keyEval.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
			if (key == null) {
				return null;
			}

			return _value.Invoke(sorted.Sorted, key);
		}

		public ICollection<EventBean> GetValueCollectionEvents(
			int aggColNum,
			AggregationRow row,
			EventBean[] eventsPerStream,
			bool isNewData,
			ExprEvaluatorContext exprEvaluatorContext)
		{
			var sorted = (AggregationStateSorted) row.GetAccessState(aggColNum);
			var key = _keyEval.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
			if (key == null) {
				return null;
			}

			return _events.Invoke(sorted.Sorted, key);
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
			var sorted = (AggregationStateSorted) row.GetAccessState(aggColNum);
			var key = _keyEval.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
			if (key == null) {
				return null;
			}

			return _event.Invoke(sorted.Sorted, key);
		}
	}
} // end of namespace
