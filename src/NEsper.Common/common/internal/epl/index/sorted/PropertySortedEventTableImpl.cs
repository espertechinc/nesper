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
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.join.exec.util;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using Range = com.espertech.esper.common.@internal.filterspec.Range;


namespace com.espertech.esper.common.@internal.epl.index.sorted
{
    /// <summary>
    /// Index that organizes events by the event property values into a single TreeMap sortable non-nested index
    /// with Object keys that store the property values.
    /// </summary>
    public class PropertySortedEventTableImpl : PropertySortedEventTable
    {
        /// <summary>
        /// Index table.
        /// </summary>
        protected readonly IOrderedDictionary<object, ISet<EventBean>> propertyIndex;

        protected readonly ISet<EventBean> nullKeyedValues;

        protected object Coerce(object value)
        {
            if (value != null &&
                factory.valueType != null && 
                factory.valueType != value.GetType()) {
                if (value.IsNumber()) {
                    return TypeHelper.CoerceBoxed(value, factory.valueType);
                }
            }

            return value;
        }

        public PropertySortedEventTableImpl(PropertySortedEventTableFactory factory) : base(factory)
        {
            propertyIndex = new OrderedListDictionary<object, ISet<EventBean>>();
            nullKeyedValues = new LinkedHashSet<EventBean>();
        }

        /// <summary>
        /// Returns the set of events that have the same property value as the given event.
        /// </summary>
        /// <param name="keyStart">to compare against</param>
        /// <param name="keyEnd">to compare against</param>
        /// <param name="allowRangeReversal">indicate whether "a between 60 and 50" should return no results (equivalent to a&amp;gt;= X and a &amp;lt;=Y) or should return results (equivalent to 'between' and 'in'</param>
        /// <returns>set of events with property value, or null if none found (never returns zero-sized set)</returns>
        public override ISet<EventBean> LookupRange(
            object keyStart,
            bool includeStart,
            object keyEnd,
            bool includeEnd,
            bool allowRangeReversal)
        {
            if (keyStart == null || keyEnd == null) {
                return EmptySet<EventBean>.Instance;
            }

            keyStart = Coerce(keyStart);
            keyEnd = Coerce(keyEnd);
            IOrderedDictionary<object, ISet<EventBean>> submap;
            try {
                submap = propertyIndex.Between(keyStart, includeStart, keyEnd, includeEnd);
            }
            catch (ArgumentException ex) {
                if (allowRangeReversal) {
                    submap = propertyIndex.Between(keyEnd, includeStart, keyStart, includeEnd);
                }
                else {
                    return EmptySet<EventBean>.Instance;
                }
            }

            return Normalize(submap);
        }

        public override ICollection<EventBean> LookupRangeColl(
            object keyStart,
            bool includeStart,
            object keyEnd,
            bool includeEnd,
            bool allowRangeReversal)
        {
            if (keyStart == null || keyEnd == null) {
                return EmptySet<EventBean>.Instance;
            }

            keyStart = Coerce(keyStart);
            keyEnd = Coerce(keyEnd);
            IOrderedDictionary<object, ISet<EventBean>> submap;
            try {
                submap = propertyIndex.Between(keyStart, includeStart, keyEnd, includeEnd);
            }
            catch (ArgumentException ex) {
                if (allowRangeReversal) {
                    submap = propertyIndex.Between(keyEnd, includeStart, keyStart, includeEnd);
                }
                else {
                    return EmptySet<EventBean>.Instance;
                }
            }

            return NormalizeCollection(submap);
        }

        public override ISet<EventBean> LookupRangeInverted(
            object keyStart,
            bool includeStart,
            object keyEnd,
            bool includeEnd)
        {
            if (keyStart == null || keyEnd == null) {
                return EmptySet<EventBean>.Instance;
            }

            keyStart = Coerce(keyStart);
            keyEnd = Coerce(keyEnd);
            var submapOne = propertyIndex.Head(keyStart, !includeStart);
            var submapTwo = propertyIndex.Tail(keyEnd, !includeEnd);
            return Normalize(submapOne, submapTwo);
        }

        public override ICollection<EventBean> LookupRangeInvertedColl(
            object keyStart,
            bool includeStart,
            object keyEnd,
            bool includeEnd)
        {
            if (keyStart == null || keyEnd == null) {
                return EmptySet<EventBean>.Instance;
            }

            keyStart = Coerce(keyStart);
            keyEnd = Coerce(keyEnd);
            var submapOne = propertyIndex.Head(keyStart, !includeStart);
            var submapTwo = propertyIndex.Tail(keyEnd, !includeEnd);
            return NormalizeCollection(submapOne, submapTwo);
        }

        public override ISet<EventBean> LookupLess(object keyStart)
        {
            if (keyStart == null) {
                return EmptySet<EventBean>.Instance;
            }

            keyStart = Coerce(keyStart);
            return Normalize(propertyIndex.Head(keyStart));
        }

        public override ICollection<EventBean> LookupLessThenColl(object keyStart)
        {
            if (keyStart == null) {
                return EmptySet<EventBean>.Instance;
            }

            keyStart = Coerce(keyStart);
            return NormalizeCollection(propertyIndex.Head(keyStart));
        }

        public override ISet<EventBean> LookupLessEqual(object keyStart)
        {
            if (keyStart == null) {
                return EmptySet<EventBean>.Instance;
            }

            keyStart = Coerce(keyStart);
            return Normalize(propertyIndex.Head(keyStart, true));
        }

        public override ICollection<EventBean> LookupLessEqualColl(object keyStart)
        {
            if (keyStart == null) {
                return EmptyList<EventBean>.Instance;
            }

            keyStart = Coerce(keyStart);
            return NormalizeCollection(propertyIndex.Head(keyStart, true));
        }

        public override ISet<EventBean> LookupGreaterEqual(object keyStart)
        {
            if (keyStart == null) {
                return EmptySet<EventBean>.Instance;
            }

            keyStart = Coerce(keyStart);
            return Normalize(propertyIndex.Tail(keyStart));
        }

        public override ICollection<EventBean> LookupGreaterEqualColl(object keyStart)
        {
            if (keyStart == null) {
                return EmptyList<EventBean>.Instance;
            }

            keyStart = Coerce(keyStart);
            return NormalizeCollection(propertyIndex.Tail(keyStart));
        }

        public override ISet<EventBean> LookupGreater(object keyStart)
        {
            if (keyStart == null) {
                return EmptySet<EventBean>.Instance;
            }

            keyStart = Coerce(keyStart);
            return Normalize(propertyIndex.Tail(keyStart, false));
        }

        public override ICollection<EventBean> LookupGreaterColl(object keyStart)
        {
            if (keyStart == null) {
                return EmptyList<EventBean>.Instance;
            }

            keyStart = Coerce(keyStart);
            return NormalizeCollection(propertyIndex.Tail(keyStart, false));
        }

        public override int? NumberOfEvents => null;

        public override int NumKeys => propertyIndex.Count;

        public override object Index => propertyIndex;

        public override void Add(
            EventBean theEvent,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var key = GetIndexedValue(theEvent);

            key = Coerce(key);

            if (key == null) {
                nullKeyedValues.Add(theEvent);
                return;
            }

            var events = propertyIndex.Get(key);
            if (events == null) {
                events = new LinkedHashSet<EventBean>();
                propertyIndex.Put(key, events);
            }

            events.Add(theEvent);
        }

        public override void Remove(
            EventBean theEvent,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var key = GetIndexedValue(theEvent);

            if (key == null) {
                nullKeyedValues.Remove(theEvent);
                return;
            }

            key = Coerce(key);

            var events = propertyIndex.Get(key);
            if (events == null) {
                return;
            }

            if (!events.Remove(theEvent)) {
                // Not an error, its possible that an old-data event is artificial (such as for statistics) and
                // thus did not correspond to a new-data event raised earlier.
                return;
            }

            if (events.IsEmpty()) {
                propertyIndex.Remove(key);
            }
        }

        public override bool IsEmpty => propertyIndex.IsEmpty();

        public override IEnumerator<EventBean> GetEnumerator()
        {
            if (nullKeyedValues.IsEmpty()) {
                return PropertySortedEventTableEnumerator.For(propertyIndex);
            }

            return SuperEnumerator.For(
                PropertySortedEventTableEnumerator.For(propertyIndex),
                nullKeyedValues.GetEnumerator());
        }

        public override void Clear()
        {
            propertyIndex.Clear();
        }

        public override void Destroy()
        {
            Clear();
        }

        public override ISet<EventBean> LookupConstantsFAF(RangeIndexLookupValue lookupValueBase)
        {
            var result = LookupConstants(lookupValueBase);
            return result == null ? null : new LinkedHashSet<EventBean>(result);
        }

        private ISet<EventBean> LookupConstants(RangeIndexLookupValue lookupValueBase)
        {
            if (lookupValueBase is RangeIndexLookupValueEquals equals) {
                return propertyIndex.Get(equals.Value);
            }

            var lookupValue = (RangeIndexLookupValueRange)lookupValueBase;
            switch (lookupValue.Operator) {
                case QueryGraphRangeEnum.RANGE_CLOSED: {
                    var range = (Range)lookupValue.Value;
                    return LookupRange(
                        range.LowEndpoint,
                        true,
                        range.HighEndpoint,
                        true,
                        lookupValue.IsAllowRangeReverse);
                }

                case QueryGraphRangeEnum.RANGE_HALF_OPEN: {
                    var range = (Range)lookupValue.Value;
                    return LookupRange(
                        range.LowEndpoint,
                        true,
                        range.HighEndpoint,
                        false,
                        lookupValue.IsAllowRangeReverse);
                }

                case QueryGraphRangeEnum.RANGE_HALF_CLOSED: {
                    var range = (Range)lookupValue.Value;
                    return LookupRange(
                        range.LowEndpoint,
                        false,
                        range.HighEndpoint,
                        true,
                        lookupValue.IsAllowRangeReverse);
                }

                case QueryGraphRangeEnum.RANGE_OPEN: {
                    var range = (Range)lookupValue.Value;
                    return LookupRange(
                        range.LowEndpoint,
                        false,
                        range.HighEndpoint,
                        false,
                        lookupValue.IsAllowRangeReverse);
                }

                case QueryGraphRangeEnum.NOT_RANGE_CLOSED: {
                    var range = (Range)lookupValue.Value;
                    return LookupRangeInverted(range.LowEndpoint, true, range.HighEndpoint, true);
                }

                case QueryGraphRangeEnum.NOT_RANGE_HALF_OPEN: {
                    var range = (Range)lookupValue.Value;
                    return LookupRangeInverted(range.LowEndpoint, true, range.HighEndpoint, false);
                }

                case QueryGraphRangeEnum.NOT_RANGE_HALF_CLOSED: {
                    var range = (Range)lookupValue.Value;
                    return LookupRangeInverted(range.LowEndpoint, false, range.HighEndpoint, true);
                }

                case QueryGraphRangeEnum.NOT_RANGE_OPEN: {
                    var range = (Range)lookupValue.Value;
                    return LookupRangeInverted(range.LowEndpoint, false, range.HighEndpoint, false);
                }

                case QueryGraphRangeEnum.GREATER:
                    return LookupGreater(lookupValue.Value);

                case QueryGraphRangeEnum.GREATER_OR_EQUAL:
                    return LookupGreaterEqual(lookupValue.Value);

                case QueryGraphRangeEnum.LESS:
                    return LookupLess(lookupValue.Value);

                case QueryGraphRangeEnum.LESS_OR_EQUAL:
                    return LookupLessEqual(lookupValue.Value);

                default:
                    throw new ArgumentException("Unrecognized operator '" + lookupValue.Operator + "'");
            }
        }

        public override Type ProviderClass => typeof(PropertySortedEventTable);
    }
} // end of namespace