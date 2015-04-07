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
using com.espertech.esper.epl.join.exec.@base;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.events;
using com.espertech.esper.filter;

namespace com.espertech.esper.epl.join.exec.composite
{
    using DataMap = IDictionary<string, object>;

    public class CompositeIndexLookupRange
        : CompositeIndexLookup
    {
        private readonly RangeIndexLookupValue _lookupValue;
        private readonly Type _coercionType;
        private CompositeIndexLookup _next;

        public CompositeIndexLookupRange(RangeIndexLookupValue lookupValue, Type coercionType)
        {
            _lookupValue = lookupValue;
            _coercionType = coercionType;
        }

        public void Lookup(IDictionary<object, object> parent, ICollection<EventBean> result)
        {
            if (_lookupValue is RangeIndexLookupValueEquals)
            {
                var equals = (RangeIndexLookupValueEquals)_lookupValue;
                var inner = parent.Get(equals.Value);
                if (_next == null)
                {
                    result.AddAll((ICollection<EventBean>)inner);
                }
                else
                {
                    var innerMap = (IDictionary<object, object>)inner;
                    _next.Lookup(innerMap, result);
                }
                return;
            }

            var lookup = (RangeIndexLookupValueRange)_lookupValue;
            var treeMap = (OrderedDictionary<object, object>)parent;
            var rangeValue = lookup.Value;
            if (lookup.Operator == QueryGraphRangeEnum.RANGE_CLOSED)
            {
                var range = (Range)rangeValue;
                LookupRange(result, treeMap, range.LowEndpoint, true, range.HighEndpoint, true, true);
            }
            else if (lookup.Operator == QueryGraphRangeEnum.RANGE_HALF_OPEN)
            {
                var range = (Range)rangeValue;
                LookupRange(result, treeMap, range.LowEndpoint, true, range.HighEndpoint, false, true);
            }
            else if (lookup.Operator == QueryGraphRangeEnum.RANGE_HALF_CLOSED)
            {
                var range = (Range)rangeValue;
                LookupRange(result, treeMap, range.LowEndpoint, false, range.HighEndpoint, true, true);
            }
            else if (lookup.Operator == QueryGraphRangeEnum.RANGE_OPEN)
            {
                var range = (Range)rangeValue;
                LookupRange(result, treeMap, range.LowEndpoint, false, range.HighEndpoint, false, true);
            }
            else if (lookup.Operator == QueryGraphRangeEnum.NOT_RANGE_CLOSED)
            {
                var range = (Range)rangeValue;
                LookupRangeInverted(result, treeMap, range.LowEndpoint, true, range.HighEndpoint, true);
            }
            else if (lookup.Operator == QueryGraphRangeEnum.NOT_RANGE_HALF_OPEN)
            {
                var range = (Range)rangeValue;
                LookupRangeInverted(result, treeMap, range.LowEndpoint, true, range.HighEndpoint, false);
            }
            else if (lookup.Operator == QueryGraphRangeEnum.NOT_RANGE_HALF_CLOSED)
            {
                var range = (Range)rangeValue;
                LookupRangeInverted(result, treeMap, range.LowEndpoint, false, range.HighEndpoint, true);
            }
            else if (lookup.Operator == QueryGraphRangeEnum.NOT_RANGE_OPEN)
            {
                var range = (Range)rangeValue;
                LookupRangeInverted(result, treeMap, range.LowEndpoint, false, range.HighEndpoint, false);
            }
            else if (lookup.Operator == QueryGraphRangeEnum.GREATER)
            {
                LookupGreater(result, treeMap, rangeValue);
            }
            else if (lookup.Operator == QueryGraphRangeEnum.GREATER_OR_EQUAL)
            {
                LookupGreaterEqual(result, treeMap, rangeValue);
            }
            else if (lookup.Operator == QueryGraphRangeEnum.LESS)
            {
                LookupLess(result, treeMap, rangeValue);
            }
            else if (lookup.Operator == QueryGraphRangeEnum.LESS_OR_EQUAL)
            {
                LookupLessEqual(result, treeMap, rangeValue);
            }
            else
            {
                throw new ArgumentException("Unrecognized operator '" + lookup.Operator + "'");
            }
        }

        public void LookupRange(ICollection<EventBean> result, OrderedDictionary<object, object> propertyIndex, Object keyStart, bool includeStart, Object keyEnd, bool includeEnd, bool allowRangeReversal)
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
            catch (ArgumentException ex)
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
            Normalize(result, submap);
        }

        public void LookupRangeInverted(ICollection<EventBean> result, OrderedDictionary<object, object> propertyIndex, Object keyStart, bool includeStart, Object keyEnd, bool includeEnd)
        {
            if (keyStart == null || keyEnd == null)
            {
                return;
            }
            keyStart = Coerce(keyStart);
            keyEnd = Coerce(keyEnd);
            var submapOne = propertyIndex.Head(keyStart, !includeStart);
            var submapTwo = propertyIndex.Tail(keyEnd, !includeEnd);
            Normalize(result, submapOne, submapTwo);
        }

        public void LookupLess(ICollection<EventBean> result, OrderedDictionary<object, object> propertyIndex, Object keyStart)
        {
            if (keyStart == null)
            {
                return;
            }
            keyStart = Coerce(keyStart);
            Normalize(result, propertyIndex.Head(keyStart));
        }

        public void LookupLessEqual(ICollection<EventBean> result, OrderedDictionary<object, object> propertyIndex, Object keyStart)
        {
            if (keyStart == null)
            {
                return;
            }
            keyStart = Coerce(keyStart);
            Normalize(result, propertyIndex.Head(keyStart, true));
        }

        public void LookupGreaterEqual(ICollection<EventBean> result, OrderedDictionary<object, object> propertyIndex, Object keyStart)
        {
            if (keyStart == null)
            {
                return;
            }
            keyStart = Coerce(keyStart);
            Normalize(result, propertyIndex.Tail(keyStart));
        }

        public void LookupGreater(ICollection<EventBean> result, OrderedDictionary<object, object> propertyIndex, Object keyStart)
        {
            if (keyStart == null)
            {
                return;
            }
            keyStart = Coerce(keyStart);
            Normalize(result, propertyIndex.Tail(keyStart, false));
        }

        private Object Coerce(Object key)
        {
            return EventBeanUtility.Coerce(key, _coercionType);
        }

        private void Normalize(ICollection<EventBean> result, IEnumerable<KeyValuePair<object, object>> submap)
        {
            if (submap.Count() == 0)
            {
                return;
            }
            if (_next == null)
            {
                foreach (KeyValuePair<Object, Object> entry in submap)
                {
                    var set = (ICollection<EventBean>)entry.Value;
                    result.AddAll(set);
                }
            }
            else
            {
                foreach (KeyValuePair<Object, Object> entry in submap)
                {
                    var index = entry.Value as IDictionary<object, object>;
                    _next.Lookup(index, result);
                }
            }
        }

        private void Normalize(ICollection<EventBean> result,
                               IEnumerable<KeyValuePair<object, object>> submapOne,
                               IEnumerable<KeyValuePair<object, object>> submapTwo)
        {
            Normalize(result, submapTwo);
            Normalize(result, submapOne);
        }

        public CompositeIndexLookup Next
        {
            set { _next = value; }
        }
    }
}
