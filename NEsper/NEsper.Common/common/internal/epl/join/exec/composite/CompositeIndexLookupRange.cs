///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.join.exec.util;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;

using Range = com.espertech.esper.common.@internal.filterspec.Range;

namespace com.espertech.esper.common.@internal.epl.join.exec.composite
{
    public class CompositeIndexLookupRange : CompositeIndexLookup
    {
        private readonly RangeIndexLookupValue _lookupValue;
        private readonly Type _coercionType;
        private CompositeIndexLookup _next;

        public CompositeIndexLookupRange(
            RangeIndexLookupValue lookupValue,
            Type coercionType)
        {
            this._lookupValue = lookupValue;
            this._coercionType = coercionType;
        }

        public void Lookup(
            IDictionary<object, CompositeIndexEntry> parent,
            ISet<EventBean> result,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            if (_lookupValue is RangeIndexLookupValueEquals equals) {
                var inner = parent.Get(equals.Value);
                if (_next == null) {
                    result.AddAll(inner.AssertCollection());
                }
                else {
                    _next.Lookup(inner.AssertIndex(), result, postProcessor);
                }

                return;
            }

            var lookup = (RangeIndexLookupValueRange) _lookupValue;
            var treeMap = (OrderedDictionary<object, CompositeIndexEntry>) parent;
            var rangeValue = lookup.Value;
            if (lookup.Operator == QueryGraphRangeEnum.RANGE_CLOSED) {
                var range = (Range) rangeValue;
                LookupRange(result, treeMap, range.LowEndpoint, true, range.HighEndpoint, true, true, postProcessor);
            }
            else if (lookup.Operator == QueryGraphRangeEnum.RANGE_HALF_OPEN) {
                var range = (Range) rangeValue;
                LookupRange(result, treeMap, range.LowEndpoint, true, range.HighEndpoint, false, true, postProcessor);
            }
            else if (lookup.Operator == QueryGraphRangeEnum.RANGE_HALF_CLOSED) {
                var range = (Range) rangeValue;
                LookupRange(result, treeMap, range.LowEndpoint, false, range.HighEndpoint, true, true, postProcessor);
            }
            else if (lookup.Operator == QueryGraphRangeEnum.RANGE_OPEN) {
                var range = (Range) rangeValue;
                LookupRange(result, treeMap, range.LowEndpoint, false, range.HighEndpoint, false, true, postProcessor);
            }
            else if (lookup.Operator == QueryGraphRangeEnum.NOT_RANGE_CLOSED) {
                var range = (Range) rangeValue;
                LookupRangeInverted(result, treeMap, range.LowEndpoint, true, range.HighEndpoint, true, postProcessor);
            }
            else if (lookup.Operator == QueryGraphRangeEnum.NOT_RANGE_HALF_OPEN) {
                var range = (Range) rangeValue;
                LookupRangeInverted(result, treeMap, range.LowEndpoint, true, range.HighEndpoint, false, postProcessor);
            }
            else if (lookup.Operator == QueryGraphRangeEnum.NOT_RANGE_HALF_CLOSED) {
                var range = (Range) rangeValue;
                LookupRangeInverted(result, treeMap, range.LowEndpoint, false, range.HighEndpoint, true, postProcessor);
            }
            else if (lookup.Operator == QueryGraphRangeEnum.NOT_RANGE_OPEN) {
                var range = (Range) rangeValue;
                LookupRangeInverted(
                    result,
                    treeMap,
                    range.LowEndpoint,
                    false,
                    range.HighEndpoint,
                    false,
                    postProcessor);
            }
            else if (lookup.Operator == QueryGraphRangeEnum.GREATER) {
                LookupGreater(result, treeMap, rangeValue, postProcessor);
            }
            else if (lookup.Operator == QueryGraphRangeEnum.GREATER_OR_EQUAL) {
                LookupGreaterEqual(result, treeMap, rangeValue, postProcessor);
            }
            else if (lookup.Operator == QueryGraphRangeEnum.LESS) {
                LookupLess(result, treeMap, rangeValue, postProcessor);
            }
            else if (lookup.Operator == QueryGraphRangeEnum.LESS_OR_EQUAL) {
                LookupLessEqual(result, treeMap, rangeValue, postProcessor);
            }
            else {
                throw new ArgumentException("Unrecognized operator '" + lookup.Operator + "'");
            }
        }

        public void LookupRange(
            ISet<EventBean> result,
            OrderedDictionary<object, CompositeIndexEntry> propertyIndex,
            object keyStart,
            bool includeStart,
            object keyEnd,
            bool includeEnd,
            bool allowRangeReversal,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            if (keyStart == null || keyEnd == null) {
                return;
            }

            keyStart = Coerce(keyStart);
            keyEnd = Coerce(keyEnd);
            IDictionary<object, CompositeIndexEntry> submap;
            try {
                submap = propertyIndex.Between(keyStart, includeStart, keyEnd, includeEnd);
            }
            catch (ArgumentException) {
                if (allowRangeReversal) {
                    submap = propertyIndex.Between(keyEnd, includeStart, keyStart, includeEnd);
                }
                else {
                    return;
                }
            }

            Normalize(result, submap, postProcessor);
        }

        public void LookupRangeInverted(
            ISet<EventBean> result,
            OrderedDictionary<object, CompositeIndexEntry> propertyIndex,
            object keyStart,
            bool includeStart,
            object keyEnd,
            bool includeEnd,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            if (keyStart == null || keyEnd == null) {
                return;
            }

            keyStart = Coerce(keyStart);
            keyEnd = Coerce(keyEnd);
            var submapOne = propertyIndex.Head(keyStart, !includeStart);
            var submapTwo = propertyIndex.Tail(keyEnd, !includeEnd);
            Normalize(result, submapOne, submapTwo, postProcessor);
        }

        public void LookupLess(
            ISet<EventBean> result,
            OrderedDictionary<object, CompositeIndexEntry> propertyIndex,
            object keyStart,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            if (keyStart == null) {
                return;
            }

            keyStart = Coerce(keyStart);
            Normalize(result, propertyIndex.Head(keyStart), postProcessor);
        }

        public void LookupLessEqual(
            ISet<EventBean> result,
            OrderedDictionary<object, CompositeIndexEntry> propertyIndex,
            object keyStart,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            if (keyStart == null) {
                return;
            }

            keyStart = Coerce(keyStart);
            Normalize(result, propertyIndex.Head(keyStart, true), postProcessor);
        }

        public void LookupGreaterEqual(
            ISet<EventBean> result,
            OrderedDictionary<object, CompositeIndexEntry> propertyIndex,
            object keyStart,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            if (keyStart == null) {
                return;
            }

            keyStart = Coerce(keyStart);
            Normalize(result, propertyIndex.Tail(keyStart), postProcessor);
        }

        public void LookupGreater(
            ISet<EventBean> result,
            OrderedDictionary<object, CompositeIndexEntry> propertyIndex,
            object keyStart,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            if (keyStart == null) {
                return;
            }

            keyStart = Coerce(keyStart);
            Normalize(result, propertyIndex.Tail(keyStart, false), postProcessor);
        }

        private object Coerce(object key)
        {
            return EventBeanUtility.Coerce(key, _coercionType);
        }

        private void Normalize(
            ISet<EventBean> result,
            IDictionary<object, CompositeIndexEntry> submap,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            if (submap.Count == 0) {
                return;
            }

            if (_next == null) {
                if (postProcessor != null) {
                    foreach (KeyValuePair<object, CompositeIndexEntry> entry in submap) {
                        postProcessor.Add(entry.Value.AssertCollection(), result);
                    }
                }
                else {
                    foreach (KeyValuePair<object, CompositeIndexEntry> entry in submap) {
                        ICollection<EventBean> set = entry.Value.AssertCollection();
                        result.AddAll(set);
                    }
                }
            }
            else {
                foreach (KeyValuePair<object, CompositeIndexEntry> entry in submap) {
                    var index = entry.Value.AssertIndex();
                    _next.Lookup(index, result, postProcessor);
                }
            }
        }

        private void Normalize(
            ISet<EventBean> result,
            IDictionary<object, CompositeIndexEntry> submapOne,
            IDictionary<object, CompositeIndexEntry> submapTwo,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            Normalize(result, submapTwo, postProcessor);
            Normalize(result, submapOne, postProcessor);
        }

        public CompositeIndexLookup Next {
            set { this._next = value; }
        }
    }
} // end of namespace