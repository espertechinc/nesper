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
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.agg.access.sorted
{
	public class AggregationMethodSortedEventsBetweenEval : AggregationMultiFunctionAggregationMethod
	{
		private readonly ExprEvaluator _fromKeyEval;
		private readonly ExprEvaluator _fromInclusiveEval;
		private readonly ExprEvaluator _toKeyEval;
		private readonly ExprEvaluator _toInclusiveEval;
		private readonly Func<IDictionary<object, object>, object> _value;
		private readonly Func<IDictionary<object, object>, ICollection<EventBean>> _events;

		public AggregationMethodSortedEventsBetweenEval(
			ExprEvaluator fromKeyEval,
			ExprEvaluator fromInclusiveEval,
			ExprEvaluator toKeyEval,
			ExprEvaluator toInclusiveEval,
			Func<IDictionary<object, object>, object> value,
			Func<IDictionary<object, object>, ICollection<EventBean>> events)
		{
			this._fromKeyEval = fromKeyEval;
			this._fromInclusiveEval = fromInclusiveEval;
			this._toKeyEval = toKeyEval;
			this._toInclusiveEval = toInclusiveEval;
			this._value = value;
			this._events = events;
		}

		public object GetValue(
			int aggColNum,
			AggregationRow row,
			EventBean[] eventsPerStream,
			bool isNewData,
			ExprEvaluatorContext exprEvaluatorContext)
		{
			var submap = GetSubmap(aggColNum, row, eventsPerStream, isNewData, exprEvaluatorContext);
			if (submap == null) {
				return null;
			}

			return _value.Invoke(submap);
		}

		public ICollection<EventBean> GetValueCollectionEvents(
			int aggColNum,
			AggregationRow row,
			EventBean[] eventsPerStream,
			bool isNewData,
			ExprEvaluatorContext exprEvaluatorContext)
		{
			var submap = GetSubmap(aggColNum, row, eventsPerStream, isNewData, exprEvaluatorContext);
			if (submap == null) {
				return null;
			}

			return _events.Invoke(submap);
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

		private IDictionary<object, object> GetSubmap(
			int aggColNum,
			AggregationRow row,
			EventBean[] eventsPerStream,
			bool isNewData,
			ExprEvaluatorContext exprEvaluatorContext)
		{
			var sorted = (AggregationStateSorted) row.GetAccessState(aggColNum);
			var fromKey = _fromKeyEval.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
			if (fromKey == null) {
				return null;
			}

			var fromInclusive = _fromInclusiveEval.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext).AsBoxedBoolean();
			if (fromInclusive == null) {
				return null;
			}

			var toKey = _toKeyEval.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
			if (toKey == null) {
				return null;
			}

			var toInclusive = _toInclusiveEval.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext).AsBoxedBoolean();
			if (toInclusive == null) {
				return null;
			}

			
			return sorted.Sorted.Between(fromKey, fromInclusive.Value, toKey, toInclusive.Value);
		}
	}
} // end of namespace
