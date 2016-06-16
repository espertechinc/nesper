///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.@join.exec.@base;
using com.espertech.esper.epl.@join.plan;
using com.espertech.esper.events;
using com.espertech.esper.filter;

namespace com.espertech.esper.epl.join.exec.composite
{
    public class CompositeIndexLookupRange : CompositeIndexLookup
    {
        private readonly RangeIndexLookupValue _lookupValue;
        private readonly Type _coercionType;
        private CompositeIndexLookup _next;

        public CompositeIndexLookupRange(RangeIndexLookupValue lookupValue, Type coercionType)
        {
            _lookupValue = lookupValue;
            _coercionType = coercionType;
        }

        public void Lookup(
            IDictionary<object, object> parent,
            ICollection<EventBean> result,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            if (_lookupValue is RangeIndexLookupValueEquals)
            {
                var equals = (RangeIndexLookupValueEquals) _lookupValue;
                var inner = parent.Get(equals.Value);
                if (_next == null)
                {
                    result.AddAll((ICollection<EventBean>) inner);
                }
                else
                {
                    var innerMap = (IDictionary<object, object>) inner;
                    _next.Lookup(innerMap, result, postProcessor);
                }
                return;
            }

            var lookup = (RangeIndexLookupValueRange) _lookupValue;
            var treeMap = (OrderedDictionary<object, object>) parent;
            var rangeValue = lookup.Value;
            switch (lookup.Operator)
            {
                case QueryGraphRangeEnum.RANGE_CLOSED:
                {
                    var range = (Range) rangeValue;
                    LookupRange(result, treeMap, range.LowEndpoint, true, range.HighEndpoint, true, true, postProcessor);
                }
                    break;
                case QueryGraphRangeEnum.RANGE_HALF_OPEN:
                {
                    var range = (Range) rangeValue;
                    LookupRange(
                        result, treeMap, range.LowEndpoint, true, range.HighEndpoint, false, true, postProcessor);
                }
                    break;
                case QueryGraphRangeEnum.RANGE_HALF_CLOSED:
                {
                    var range = (Range) rangeValue;
                    LookupRange(
                        result, treeMap, range.LowEndpoint, false, range.HighEndpoint, true, true, postProcessor);
                }
                    break;
                case QueryGraphRangeEnum.RANGE_OPEN:
                {
                    var range = (Range) rangeValue;
                    LookupRange(
                        result, treeMap, range.LowEndpoint, false, range.HighEndpoint, false, true, postProcessor);
                }
                    break;
                case QueryGraphRangeEnum.NOT_RANGE_CLOSED:
                {
                    var range = (Range) rangeValue;
                    LookupRangeInverted(
                        result, treeMap, range.LowEndpoint, true, range.HighEndpoint, true, postProcessor);
                }
                    break;
                case QueryGraphRangeEnum.NOT_RANGE_HALF_OPEN:
                {
                    var range = (Range) rangeValue;
                    LookupRangeInverted(
                        result, treeMap, range.LowEndpoint, true, range.HighEndpoint, false, postProcessor);
                }
                    break;
                case QueryGraphRangeEnum.NOT_RANGE_HALF_CLOSED:
                {
                    var range = (Range) rangeValue;
                    LookupRangeInverted(
                        result, treeMap, range.LowEndpoint, false, range.HighEndpoint, true, postProcessor);
                }
                    break;
                case QueryGraphRangeEnum.NOT_RANGE_OPEN:
                {
                    var range = (Range) rangeValue;
                    LookupRangeInverted(
                        result, treeMap, range.LowEndpoint, false, range.HighEndpoint, false, postProcessor);
                }
                    break;
                case QueryGraphRangeEnum.GREATER:
                    LookupGreater(result, treeMap, rangeValue, postProcessor);
                    break;
                case QueryGraphRangeEnum.GREATER_OR_EQUAL:
                    LookupGreaterEqual(result, treeMap, rangeValue, postProcessor);
                    break;
                case QueryGraphRangeEnum.LESS:
                    LookupLess(result, treeMap, rangeValue, postProcessor);
                    break;
                case QueryGraphRangeEnum.LESS_OR_EQUAL:
                    LookupLessEqual(result, treeMap, rangeValue, postProcessor);
                    break;
                default:
                    throw new ArgumentException("Unrecognized operator '" + lookup.Operator + "'");
            }
        }

        public void LookupRange(
            ICollection<EventBean> result,
            OrderedDictionary<object, object> propertyIndex,
            object keyStart,
            bool includeStart,
            object keyEnd,
            bool includeEnd,
            bool allowRangeReversal,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            if (keyStart == null || keyEnd == null)
            {
                return;
            }
            keyStart = Coerce(keyStart);
            keyEnd = Coerce(keyEnd);
            IEnumerable<KeyValuePair<object, object>> submap;
            try
            {
                submap = propertyIndex.Between(keyStart, includeStart, keyEnd, includeEnd);
            }
            catch (ArgumentException)
            {
                if (allowRangeReversal)
                {
                    submap = propertyIndex.Between(keyEnd, includeStart, keyStart, includeEnd);
                }
                else
                {
                    return;
                }
            }
            Normalize(result, submap, postProcessor);
        }

        public void LookupRangeInverted(
            ICollection<EventBean> result,
            OrderedDictionary<object, object> propertyIndex,
            object keyStart,
            bool includeStart,
            object keyEnd,
            bool includeEnd,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            if (keyStart == null || keyEnd == null)
            {
                return;
            }
            keyStart = Coerce(keyStart);
            keyEnd = Coerce(keyEnd);
            var submapOne = propertyIndex.Head(keyStart, !includeStart);
            var submapTwo = propertyIndex.Tail(keyEnd, !includeEnd);
            Normalize(result, submapOne, submapTwo, postProcessor);
        }

        public void LookupLess(
            ICollection<EventBean> result,
            OrderedDictionary<object, object> propertyIndex,
            object keyStart,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            if (keyStart == null)
            {
                return;
            }
            keyStart = Coerce(keyStart);
            Normalize(result, propertyIndex.Head(keyStart), postProcessor);
        }

        public void LookupLessEqual(
            ICollection<EventBean> result,
            OrderedDictionary<object, object> propertyIndex,
            object keyStart,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            if (keyStart == null)
            {
                return;
            }
            keyStart = Coerce(keyStart);
            Normalize(result, propertyIndex.Head(keyStart, true), postProcessor);
        }

        public void LookupGreaterEqual(
            ICollection<EventBean> result,
            OrderedDictionary<object, object> propertyIndex,
            object keyStart,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            if (keyStart == null)
            {
                return;
            }
            keyStart = Coerce(keyStart);
            Normalize(result, propertyIndex.Tail(keyStart), postProcessor);
        }

        public void LookupGreater(
            ICollection<EventBean> result,
            OrderedDictionary<object, object> propertyIndex,
            object keyStart,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            if (keyStart == null)
            {
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
            ICollection<EventBean> result,
            IEnumerable<KeyValuePair<object, object>> submap,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            if (!submap.Any())
            {
                return;
            }
            if (_next == null)
            {
                if (postProcessor != null)
                {
                    foreach (var entry in submap)
                    {
                        postProcessor.Add(entry.Value, result);
                    }
                }
                else
                {
                    foreach (var entry in submap)
                    {
                        var set = (ISet<EventBean>) entry.Value;
                        result.AddAll(set);
                    }
                }
            }
            else
            {
                foreach (var entry in submap)
                {
                    var index = (OrderedDictionary<object, object>) entry.Value;
                    _next.Lookup(index, result, postProcessor);
                }
            }
        }

        private void Normalize(
            ICollection<EventBean> result,
            IEnumerable<KeyValuePair<object, object>> submapOne,
            IEnumerable<KeyValuePair<object, object>> submapTwo,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            Normalize(result, submapTwo, postProcessor);
            Normalize(result, submapOne, postProcessor);
        }

        public void SetNext(CompositeIndexLookup next)
        {
            _next = next;
        }
    }
} // end of namespace
