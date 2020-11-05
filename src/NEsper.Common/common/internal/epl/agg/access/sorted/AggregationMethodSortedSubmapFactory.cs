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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;


namespace com.espertech.esper.common.@internal.epl.agg.access.sorted
{
	public class AggregationMethodSortedSubmapFactory
	{

		public static AggregationMultiFunctionAggregationMethod MakeSortedAggregationSubmap(
			ExprEvaluator fromKeyEval,
			ExprEvaluator fromInclusiveEval,
			ExprEvaluator toKeyEval,
			ExprEvaluator toInclusiveEval,
			AggregationMethodSortedEnum method,
			Type underlyingClass)
		{
			if (method.GetFootprint() != AggregationMethodSortedFootprintEnum.SUBMAP) {
				throw new IllegalStateException("Unrecognized aggregation method " + method);
			}

			if (method == AggregationMethodSortedEnum.EVENTSBETWEEN) {
				return new AggregationMethodSortedEventsBetweenEval(
					fromKeyEval,
					fromInclusiveEval,
					toKeyEval,
					toInclusiveEval,
					submap => UnderlyingEvents(submap, underlyingClass),
					submap => CollEvents(submap)
				);
			}

			if (method == AggregationMethodSortedEnum.SUBMAP) {
				return new AggregationMethodSortedSubmapEval(fromKeyEval, fromInclusiveEval, toKeyEval, toInclusiveEval, underlyingClass);
			}

			throw new IllegalStateException("Unrecognized aggregation method " + method);
		}

		private static ICollection<EventBean> CollEvents(IDictionary<object, object> submap)
		{
			if (submap.IsEmpty()) {
				return EmptyList<EventBean>.Instance;
			}

			ArrayDeque<EventBean> events = new ArrayDeque<EventBean>(4);
			foreach (KeyValuePair<object, object> entry in submap) {
				AggregatorAccessSortedImpl.CheckedPayloadAddAll(events, entry.Value);
			}

			return events;
		}

		private static Array UnderlyingEvents(
			IDictionary<object, object> submap,
			Type underlyingClass)
		{
			if (submap.IsEmpty()) {
				return Arrays.CreateInstanceChecked(underlyingClass, 0);
			}

			ArrayDeque<EventBean> events = new ArrayDeque<EventBean>(4);
			foreach (KeyValuePair<object, object> entry in submap) {
				AggregatorAccessSortedImpl.CheckedPayloadAddAll(events, entry.Value);
			}

			var array = Arrays.CreateInstanceChecked(underlyingClass, events.Count);
			int index = 0;
			foreach (EventBean @event in events) {
				array.SetValue(@event.Underlying, index++);
			}

			return array;
		}
	}
} // end of namespace
