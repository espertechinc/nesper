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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.agg.access.sorted
{
	public class AggregationMethodSortedKeyedFactory
	{
		public static AggregationMethodSortedKeyedEval MakeSortedAggregationWithKey(
			ExprEvaluator keyEval,
			AggregationMethodSortedEnum method,
			Type underlyingClass)
		{
			if (method.GetFootprint() != AggregationMethodSortedFootprintEnum.KEYONLY) {
				throw new IllegalStateException("Unrecognized aggregation method " + method);
			}

			if (method == AggregationMethodSortedEnum.CEILINGEVENT) {
				return new AggregationMethodSortedKeyedEval(
					keyEval,
					(map, key) => FirstUnd(map.CeilingEntry(key)),
					(map, key) => FirstBean(map.CeilingEntry(key)),
					(map, key) => FirstColl(map.CeilingEntry(key))
				);
			}

			if (method == AggregationMethodSortedEnum.CEILINGEVENTS) {
				return new AggregationMethodSortedKeyedEval(
					keyEval,
					(map, key) => EventsArrayUnd(map.CeilingEntry(key), underlyingClass),
					(map, key) => null,
					(map, key) => EventsColl(map.CeilingEntry(key))
				);
			}

			if (method == AggregationMethodSortedEnum.CEILINGKEY) {
				return new AggregationMethodSortedKeyedEval(
					keyEval,
					(map, key) => map.CeilingKey(key),
					(map, key) => null,
					(map, key) => null
				);
			}

			if (method == AggregationMethodSortedEnum.FLOOREVENT) {
				return new AggregationMethodSortedKeyedEval(
					keyEval,
					(map, key) => FirstUnd(map.FloorEntry(key)),
					(map, key) => FirstBean(map.FloorEntry(key)),
					(map, key) => FirstColl(map.FloorEntry(key))
				);
			}

			if (method == AggregationMethodSortedEnum.FLOOREVENTS) {
				return new AggregationMethodSortedKeyedEval(
					keyEval,
					(map, key) => EventsArrayUnd(map.FloorEntry(key), underlyingClass),
					(map, key) => null,
					(map, key) => EventsColl(map.FloorEntry(key))
				);
			}

			if (method == AggregationMethodSortedEnum.FLOORKEY) {
				return new AggregationMethodSortedKeyedEval(
					keyEval,
					(map, key) => map.FloorKey(key),
					(map, key) => null,
					(map, key) => null
				);
			}

			if (method == AggregationMethodSortedEnum.LOWEREVENT) {
				return new AggregationMethodSortedKeyedEval(
					keyEval,
					(map, key) => FirstUnd(map.LowerEntry(key)),
					(map, key) => FirstBean(map.LowerEntry(key)),
					(map, key) => FirstColl(map.LowerEntry(key))
				);
			}

			if (method == AggregationMethodSortedEnum.LOWEREVENTS) {
				return new AggregationMethodSortedKeyedEval(
					keyEval,
					(map, key) => EventsArrayUnd(map.LowerEntry(key), underlyingClass),
					(map, key) => null,
					(map, key) => EventsColl(map.LowerEntry(key))
				);
			}

			if (method == AggregationMethodSortedEnum.LOWERKEY) {
				return new AggregationMethodSortedKeyedEval(
					keyEval,
					(map, key) => map.LowerKey(key),
					(map, key) => null,
					(map, key) => null
				);
			}

			if (method == AggregationMethodSortedEnum.HIGHEREVENT) {
				return new AggregationMethodSortedKeyedEval(
					keyEval,
					(map, key) => FirstUnd(map.HigherEntry(key)),
					(map, key) => FirstBean(map.HigherEntry(key)),
					(map, key) => FirstColl(map.HigherEntry(key))
				);
			}

			if (method == AggregationMethodSortedEnum.HIGHEREVENTS) {
				return new AggregationMethodSortedKeyedEval(
					keyEval,
					(map, key) => EventsArrayUnd(map.HigherEntry(key), underlyingClass),
					(map, key) => null,
					(map, key) => EventsColl(map.HigherEntry(key))
				);
			}

			if (method == AggregationMethodSortedEnum.HIGHERKEY) {
				return new AggregationMethodSortedKeyedEval(
					keyEval,
					(map, key) => map.HigherKey(key),
					(map, key) => null,
					(map, key) => null
				);
			}

			if (method == AggregationMethodSortedEnum.GETEVENT) {
				return new AggregationMethodSortedKeyedEval(
					keyEval,
					(map, key) => FirstUnd(map.Get(key)),
					(map, key) => FirstBean(map.Get(key)),
					(map, key) => FirstColl(map.Get(key))
				);
			}

			if (method == AggregationMethodSortedEnum.GETEVENTS) {
				return new AggregationMethodSortedKeyedEval(
					keyEval,
					(map, key) => EventsArrayUnd(map.Get(key), underlyingClass),
					(map, key) => null,
					(map, key) => EventsColl(map.Get(key))
				);
			}

			if (method == AggregationMethodSortedEnum.CONTAINSKEY) {
				return new AggregationMethodSortedKeyedEval(
					keyEval,
					(map, key) => map.ContainsKey(key),
					(map, key) => null,
					(map, key) => null
				);
			}

			throw new IllegalStateException("Unrecognized aggregation method " + method);
		}

		internal static object FirstUnd(KeyValuePair<object, object> entry)
		{
			var @event = AggregatorAccessSortedImpl.CheckedPayloadMayDeque(entry.Value);
			return @event.Underlying;
		}

		internal static object FirstUnd(object value)
		{
			if (value == null) {
				return null;
			}

			EventBean @event = AggregatorAccessSortedImpl.CheckedPayloadMayDeque(value);
			return @event.Underlying;
		}

		internal static object EventsArrayUnd(
			KeyValuePair<object, object> entry,
			Type underlyingClass)
		{
			return AggregatorAccessSortedImpl.CheckedPayloadGetUnderlyingArray(entry.Value, underlyingClass);
		}

		internal static object EventsArrayUnd(
			object value,
			Type underlyingClass)
		{
			return AggregatorAccessSortedImpl.CheckedPayloadGetUnderlyingArray(value, underlyingClass);
		}

		internal static EventBean FirstBean(KeyValuePair<object, object> entry)
		{
			return AggregatorAccessSortedImpl.CheckedPayloadMayDeque(entry.Value);
		}

		internal static EventBean FirstBean(object value)
		{
			if (value == null) {
				return null;
			}

			return AggregatorAccessSortedImpl.CheckedPayloadMayDeque(value);
		}

		internal static ICollection<EventBean> FirstColl(KeyValuePair<object, object> entry)
		{
			return Collections.SingletonList(AggregatorAccessSortedImpl.CheckedPayloadMayDeque(entry.Value));
		}

		internal static ICollection<EventBean> FirstColl(object value)
		{
			if (value == null) {
				return null;
			}

			return Collections.SingletonList(AggregatorAccessSortedImpl.CheckedPayloadMayDeque(value));
		}

		internal static ICollection<EventBean> EventsColl(KeyValuePair<object, object> entry)
		{
			return AggregatorAccessSortedImpl.CheckedPayloadGetCollEvents(entry.Value);
		}

		internal static ICollection<EventBean> EventsColl(object value)
		{
			if (value == null) {
				return null;
			}

			return AggregatorAccessSortedImpl.CheckedPayloadGetCollEvents(value);
		}
	}
} // end of namespace
