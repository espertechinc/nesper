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
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.agg.access.sorted
{
	public class AggregationMethodSortedSubmapEval : AggregationMultiFunctionAggregationMethod
	{
		private readonly ExprEvaluator _fromKeyEval;
		private readonly ExprEvaluator _fromInclusiveEval;
		private readonly ExprEvaluator _toKeyEval;
		private readonly ExprEvaluator _toInclusiveEval;
		private readonly Type _underlyingClass;

		public AggregationMethodSortedSubmapEval(
			ExprEvaluator fromKeyEval,
			ExprEvaluator fromInclusiveEval,
			ExprEvaluator toKeyEval,
			ExprEvaluator toInclusiveEval,
			Type underlyingClass)
		{
			_fromKeyEval = fromKeyEval;
			_fromInclusiveEval = fromInclusiveEval;
			_toKeyEval = toKeyEval;
			_toInclusiveEval = toInclusiveEval;
			_underlyingClass = underlyingClass;
		}

		public object GetValue(
			int aggColNum,
			AggregationRow row,
			EventBean[] eventsPerStream,
			bool isNewData,
			ExprEvaluatorContext exprEvaluatorContext)
		{
			AggregationStateSorted sorted = (AggregationStateSorted) row.GetAccessState(aggColNum);
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

			var mapOfArrays = new OrderedListDictionary<object, object>(sorted.Sorted.KeyComparer);
			var submap = sorted.Sorted.Between(fromKey, fromInclusive.Value, toKey, toInclusive.Value);
			foreach (KeyValuePair<object, object> entry in submap) {
				mapOfArrays.Put(entry.Key, AggregatorAccessSortedImpl.CheckedPayloadGetUnderlyingArray(entry.Value, _underlyingClass));
			}

			return mapOfArrays;
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
