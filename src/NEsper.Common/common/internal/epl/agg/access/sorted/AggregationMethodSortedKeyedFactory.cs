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
                    (
                        map,
                        key) => FirstUnd(map.GreaterThanOrEqualTo(key)),
                    (
                        map,
                        key) => FirstBean(map.GreaterThanOrEqualTo(key)),
                    (
                        map,
                        key) => FirstColl(map.GreaterThanOrEqualTo(key))
                );
            }

            if (method == AggregationMethodSortedEnum.CEILINGEVENTS) {
                return new AggregationMethodSortedKeyedEval(
                    keyEval,
                    (
                        map,
                        key) => EventsArrayUnd(map.GreaterThanOrEqualTo(key), underlyingClass),
                    (
                        map,
                        key) => null,
                    (
                        map,
                        key) => EventsColl(map.GreaterThanOrEqualTo(key))
                );
            }

            if (method == AggregationMethodSortedEnum.CEILINGKEY) {
                return new AggregationMethodSortedKeyedEval(
                    keyEval,
                    (
                        map,
                        key) => map.GreaterThanOrEqualTo(key)?.Key,
                    (
                        map,
                        key) => null,
                    (
                        map,
                        key) => null
                );
            }

            if (method == AggregationMethodSortedEnum.FLOOREVENT) {
                return new AggregationMethodSortedKeyedEval(
                    keyEval,
                    (
                        map,
                        key) => FirstUnd(map.LessThanOrEqualTo(key)),
                    (
                        map,
                        key) => FirstBean(map.LessThanOrEqualTo(key)),
                    (
                        map,
                        key) => FirstColl(map.LessThanOrEqualTo(key))
                );
            }

            if (method == AggregationMethodSortedEnum.FLOOREVENTS) {
                return new AggregationMethodSortedKeyedEval(
                    keyEval,
                    (
                        map,
                        key) => EventsArrayUnd(map.LessThanOrEqualTo(key), underlyingClass),
                    (
                        map,
                        key) => null,
                    (
                        map,
                        key) => EventsColl(map.LessThanOrEqualTo(key))
                );
            }

            if (method == AggregationMethodSortedEnum.FLOORKEY) {
                return new AggregationMethodSortedKeyedEval(
                    keyEval,
                    (
                        map,
                        key) => map.LessThanOrEqualTo(key)?.Key,
                    (
                        map,
                        key) => null,
                    (
                        map,
                        key) => null
                );
            }

            if (method == AggregationMethodSortedEnum.LOWEREVENT) {
                return new AggregationMethodSortedKeyedEval(
                    keyEval,
                    (
                        map,
                        key) => FirstUnd(map.LessThan(key)),
                    (
                        map,
                        key) => FirstBean(map.LessThan(key)),
                    (
                        map,
                        key) => FirstColl(map.LessThan(key))
                );
            }

            if (method == AggregationMethodSortedEnum.LOWEREVENTS) {
                return new AggregationMethodSortedKeyedEval(
                    keyEval,
                    (
                        map,
                        key) => EventsArrayUnd(map.LessThan(key), underlyingClass),
                    (
                        map,
                        key) => null,
                    (
                        map,
                        key) => EventsColl(map.LessThan(key))
                );
            }

            if (method == AggregationMethodSortedEnum.LOWERKEY) {
                return new AggregationMethodSortedKeyedEval(
                    keyEval,
                    (
                        map,
                        key) => map.LessThan(key)?.Key,
                    (
                        map,
                        key) => null,
                    (
                        map,
                        key) => null
                );
            }

            if (method == AggregationMethodSortedEnum.HIGHEREVENT) {
                return new AggregationMethodSortedKeyedEval(
                    keyEval,
                    (
                        map,
                        key) => FirstUnd(map.GreaterThan(key)),
                    (
                        map,
                        key) => FirstBean(map.GreaterThan(key)),
                    (
                        map,
                        key) => FirstColl(map.GreaterThan(key))
                );
            }

            if (method == AggregationMethodSortedEnum.HIGHEREVENTS) {
                return new AggregationMethodSortedKeyedEval(
                    keyEval,
                    (
                        map,
                        key) => EventsArrayUnd(map.GreaterThan(key), underlyingClass),
                    (
                        map,
                        key) => null,
                    (
                        map,
                        key) => EventsColl(map.GreaterThan(key))
                );
            }

            if (method == AggregationMethodSortedEnum.HIGHERKEY) {
                return new AggregationMethodSortedKeyedEval(
                    keyEval,
                    (
                        map,
                        key) => map.GreaterThan(key)?.Key,
                    (
                        map,
                        key) => null,
                    (
                        map,
                        key) => null
                );
            }

            if (method == AggregationMethodSortedEnum.GETEVENT) {
                return new AggregationMethodSortedKeyedEval(
                    keyEval,
                    (
                        map,
                        key) => FirstUnd(map.Get(key)),
                    (
                        map,
                        key) => FirstBean(map.Get(key)),
                    (
                        map,
                        key) => FirstColl(map.Get(key))
                );
            }

            if (method == AggregationMethodSortedEnum.GETEVENTS) {
                return new AggregationMethodSortedKeyedEval(
                    keyEval,
                    (
                        map,
                        key) => EventsArrayUnd(map.Get(key), underlyingClass),
                    (
                        map,
                        key) => null,
                    (
                        map,
                        key) => EventsColl(map.Get(key))
                );
            }

            if (method == AggregationMethodSortedEnum.CONTAINSKEY) {
                return new AggregationMethodSortedKeyedEval(
                    keyEval,
                    (
                        map,
                        key) => map.ContainsKey(key),
                    (
                        map,
                        key) => null,
                    (
                        map,
                        key) => null
                );
            }

            throw new IllegalStateException("Unrecognized aggregation method " + method);
        }

        internal static object FirstUnd(KeyValuePair<object, object>? entry)
        {
            if (entry == null) {
                return null;
            }

            var @event = AggregatorAccessSortedImpl.CheckedPayloadMayDeque(entry.Value.Value);
            return @event.Underlying;
        }

        internal static object FirstUnd(object value)
        {
            if (value == null) {
                return null;
            }

            var @event = AggregatorAccessSortedImpl.CheckedPayloadMayDeque(value);
            return @event.Underlying;
        }

        internal static object EventsArrayUnd(
            KeyValuePair<object, object>? entry,
            Type underlyingClass)
        {
            return entry == null
                ? null
                : AggregatorAccessSortedImpl.CheckedPayloadGetUnderlyingArray(entry.Value.Value, underlyingClass);
        }

        internal static object EventsArrayUnd(
            object value,
            Type underlyingClass)
        {
            return value == null
                ? null
                : AggregatorAccessSortedImpl.CheckedPayloadGetUnderlyingArray(value, underlyingClass);
        }

        internal static EventBean FirstBean(KeyValuePair<object, object>? entry)
        {
            return entry.HasValue ? AggregatorAccessSortedImpl.CheckedPayloadMayDeque(entry.Value.Value) : null;
        }

        internal static EventBean FirstBean(object value)
        {
            return value == null ? null : AggregatorAccessSortedImpl.CheckedPayloadMayDeque(value);
        }

        internal static ICollection<EventBean> FirstColl(KeyValuePair<object, object>? entry)
        {
            return entry == null
                ? null
                : Collections.SingletonList(AggregatorAccessSortedImpl.CheckedPayloadMayDeque(entry.Value.Value));
        }

        internal static ICollection<EventBean> FirstColl(object value)
        {
            return value == null
                ? null
                : Collections.SingletonList(AggregatorAccessSortedImpl.CheckedPayloadMayDeque(value));
        }

        internal static ICollection<EventBean> EventsColl(KeyValuePair<object, object>? entry)
        {
            return entry == null ? null : AggregatorAccessSortedImpl.CheckedPayloadGetCollEvents(entry.Value.Value);
        }

        internal static ICollection<EventBean> EventsColl(object value)
        {
            return value == null ? null : AggregatorAccessSortedImpl.CheckedPayloadGetCollEvents(value);
        }
    }
} // end of namespace