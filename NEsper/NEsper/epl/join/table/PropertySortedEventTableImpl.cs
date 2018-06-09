///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.@join.exec.@base;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.filter;

namespace com.espertech.esper.epl.join.table
{
    /// <summary>
    /// Index that organizes events by the event property values into a single TreeMap sortable non-nested index
    /// with Object keys that store the property values.
    /// </summary>
    public class PropertySortedEventTableImpl : PropertySortedEventTable
    {
        private static readonly ISet<EventBean> EmptySet = Collections.GetEmptySet<EventBean>();
        private static readonly IList<EventBean> EmptyList = Collections.GetEmptyList<EventBean>();

        /// <summary>
        /// Index table.
        /// </summary>
        private readonly OrderedDictionary<object, ISet<EventBean>> _propertyIndex;
        private readonly ISet<EventBean> _nullKeyedValues;

        // override in a subclass
        protected virtual object Coerce(object value)
        {
            return value;
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        public PropertySortedEventTableImpl(EventPropertyGetter propertyGetter, EventTableOrganization organization)
            : base(propertyGetter, organization)
        {
            _propertyIndex = new OrderedDictionary<object, ISet<EventBean>>();
            _nullKeyedValues = new LinkedHashSet<EventBean>();
        }

        /// <summary>
        /// Returns the set of events that have the same property value as the given event.
        /// </summary>
        /// <param name="keyStart">to compare against</param>
        /// <param name="includeStart">if set to <c>true</c> [include start].</param>
        /// <param name="keyEnd">to compare against</param>
        /// <param name="includeEnd">if set to <c>true</c> [include end].</param>
        /// <param name="allowRangeReversal">indicate whether "a between 60 and 50" should return no results (equivalent to a&gt;= X and a &lt;=Y) or should return results (equivalent to 'between' and 'in'</param>
        /// <returns>
        /// set of events with property value, or null if none found (never returns zero-sized set)
        /// </returns>
        public override ISet<EventBean> LookupRange(object keyStart, bool includeStart, object keyEnd, bool includeEnd, bool allowRangeReversal)
        {
            if (keyStart == null || keyEnd == null)
            {
                return EmptySet;
            }
            keyStart = Coerce(keyStart);
            keyEnd = Coerce(keyEnd);
            IDictionary<object, ISet<EventBean>> submap;
            try
            {
                submap = _propertyIndex.Between(keyStart, includeStart, keyEnd, includeEnd);
            }
            catch (ArgumentException)
            {
                if (allowRangeReversal)
                {
                    submap = _propertyIndex.Between(keyEnd, includeStart, keyStart, includeEnd);
                }
                else
                {
                    return EmptySet;
                }
            }
            return Normalize(submap);
        }

        public override ICollection<EventBean> LookupRangeColl(object keyStart, bool includeStart, object keyEnd, bool includeEnd, bool allowRangeReversal)
        {
            if (keyStart == null || keyEnd == null)
            {
                return EmptyList;
            }
            keyStart = Coerce(keyStart);
            keyEnd = Coerce(keyEnd);
            IDictionary<object, ISet<EventBean>> submap;
            try
            {
                submap = _propertyIndex.Between(keyStart, includeStart, keyEnd, includeEnd);
            }
            catch (ArgumentException)
            {
                if (allowRangeReversal)
                {
                    submap = _propertyIndex.Between(keyEnd, includeStart, keyStart, includeEnd);
                }
                else
                {
                    return EmptyList;
                }
            }
            return NormalizeCollection(submap);
        }

        public override ISet<EventBean> LookupRangeInverted(object keyStart, bool includeStart, object keyEnd, bool includeEnd)
        {
            if (keyStart == null || keyEnd == null)
            {
                return EmptySet;
            }
            keyStart = Coerce(keyStart);
            keyEnd = Coerce(keyEnd);
            var submapOne = _propertyIndex.Head(keyStart, !includeStart);
            var submapTwo = _propertyIndex.Tail(keyEnd, !includeEnd);
            return Normalize(submapOne, submapTwo);
        }

        public override ICollection<EventBean> LookupRangeInvertedColl(object keyStart, bool includeStart, object keyEnd, bool includeEnd)
        {
            if (keyStart == null || keyEnd == null)
            {
                return EmptySet;
            }
            keyStart = Coerce(keyStart);
            keyEnd = Coerce(keyEnd);
            var submapOne = _propertyIndex.Head(keyStart, !includeStart);
            var submapTwo = _propertyIndex.Tail(keyEnd, !includeEnd);
            return NormalizeCollection(submapOne, submapTwo);
        }

        public override ISet<EventBean> LookupLess(object keyStart)
        {
            if (keyStart == null)
            {
                return EmptySet;
            }
            keyStart = Coerce(keyStart);
            return Normalize(_propertyIndex.Head(keyStart));
        }

        public override ICollection<EventBean> LookupLessThenColl(object keyStart)
        {
            if (keyStart == null)
            {
                return EmptyList;
            }
            keyStart = Coerce(keyStart);
            return NormalizeCollection(_propertyIndex.Head(keyStart));
        }

        public override ISet<EventBean> LookupLessEqual(object keyStart)
        {
            if (keyStart == null)
            {
                return EmptySet;
            }
            keyStart = Coerce(keyStart);
            return Normalize(_propertyIndex.Head(keyStart, true));
        }

        public override ICollection<EventBean> LookupLessEqualColl(object keyStart)
        {
            if (keyStart == null)
            {
                return EmptyList;
            }
            keyStart = Coerce(keyStart);
            return NormalizeCollection(_propertyIndex.Head(keyStart, true));
        }

        public override ISet<EventBean> LookupGreaterEqual(object keyStart)
        {
            if (keyStart == null)
            {
                return EmptySet;
            }
            keyStart = Coerce(keyStart);
            return Normalize(_propertyIndex.Tail(keyStart));
        }

        public override ICollection<EventBean> LookupGreaterEqualColl(object keyStart)
        {
            if (keyStart == null)
            {
                return EmptyList;
            }
            keyStart = Coerce(keyStart);
            return NormalizeCollection(_propertyIndex.Tail(keyStart));
        }

        public override ISet<EventBean> LookupGreater(object keyStart)
        {
            if (keyStart == null)
            {
                return EmptySet;
            }
            keyStart = Coerce(keyStart);
            return Normalize(_propertyIndex.Tail(keyStart, false));
        }

        public override ICollection<EventBean> LookupGreaterColl(object keyStart)
        {
            if (keyStart == null)
            {
                return EmptyList;
            }
            keyStart = Coerce(keyStart);
            return NormalizeCollection(_propertyIndex.Tail(keyStart, false));
        }

        public override int? NumberOfEvents
        {
            get { return null; }
        }

        public override int NumKeys
        {
            get { return _propertyIndex.Count; }
        }

        public override object Index
        {
            get { return _propertyIndex; }
        }

        public override void Add(EventBean theEvent, ExprEvaluatorContext exprEvaluatorContext)
        {
            var key = GetIndexedValue(theEvent);

            key = Coerce(key);

            if (key == null)
            {
                _nullKeyedValues.Add(theEvent);
                return;
            }

            var events = _propertyIndex.TryInsert(
                key, () => new LinkedHashSet<EventBean>());

            events.Add(theEvent);
        }

        public override void Remove(EventBean theEvent, ExprEvaluatorContext exprEvaluatorContext)
        {
            var key = GetIndexedValue(theEvent);

            if (key == null)
            {
                _nullKeyedValues.Remove(theEvent);
                return;
            }

            key = Coerce(key);

            var events = _propertyIndex.Get(key);
            if (events == null)
            {
                return;
            }

            if (!events.Remove(theEvent))
            {
                // Not an error, its possible that an old-data event is artificial (such as for statistics) and
                // thus did not correspond to a new-data event raised earlier.
                return;
            }

            if (events.IsEmpty())
            {
                _propertyIndex.Remove(key);
            }
        }

        public override bool IsEmpty()
        {
            return _propertyIndex.IsEmpty();
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            if (_nullKeyedValues.IsEmpty())
            {
                return _propertyIndex.Values
                    .SelectMany(eventBeans => eventBeans)
                    .GetEnumerator();
            }

            return Enumerable
                .Union(_propertyIndex.Values.SelectMany(eventBeans => eventBeans), _nullKeyedValues)
                .GetEnumerator();
        }

        public override void Clear()
        {
            _propertyIndex.Clear();
        }

        public override void Destroy()
        {
            Clear();
        }

        public override ISet<EventBean> LookupConstants(RangeIndexLookupValue lookupValueBase)
        {
            if (lookupValueBase is RangeIndexLookupValueEquals)
            {
                var equals = (RangeIndexLookupValueEquals)lookupValueBase;
                return _propertyIndex.Get(equals.Value);
            }

            var lookupValue = (RangeIndexLookupValueRange)lookupValueBase;
            switch (lookupValue.Operator)
            {
                case QueryGraphRangeEnum.RANGE_CLOSED:
                    {
                        var range = (Range)lookupValue.Value;
                        return LookupRange(range.LowEndpoint, true, range.HighEndpoint, true, lookupValue.IsAllowRangeReverse);
                    }
                case QueryGraphRangeEnum.RANGE_HALF_OPEN:
                    {
                        var range = (Range)lookupValue.Value;
                        return LookupRange(range.LowEndpoint, true, range.HighEndpoint, false, lookupValue.IsAllowRangeReverse);
                    }
                case QueryGraphRangeEnum.RANGE_HALF_CLOSED:
                    {
                        var range = (Range)lookupValue.Value;
                        return LookupRange(range.LowEndpoint, false, range.HighEndpoint, true, lookupValue.IsAllowRangeReverse);
                    }
                case QueryGraphRangeEnum.RANGE_OPEN:
                    {
                        var range = (Range)lookupValue.Value;
                        return LookupRange(range.LowEndpoint, false, range.HighEndpoint, false, lookupValue.IsAllowRangeReverse);
                    }
                case QueryGraphRangeEnum.NOT_RANGE_CLOSED:
                    {
                        var range = (Range)lookupValue.Value;
                        return LookupRangeInverted(range.LowEndpoint, true, range.HighEndpoint, true);
                    }
                case QueryGraphRangeEnum.NOT_RANGE_HALF_OPEN:
                    {
                        var range = (Range)lookupValue.Value;
                        return LookupRangeInverted(range.LowEndpoint, true, range.HighEndpoint, false);
                    }
                case QueryGraphRangeEnum.NOT_RANGE_HALF_CLOSED:
                    {
                        var range = (Range)lookupValue.Value;
                        return LookupRangeInverted(range.LowEndpoint, false, range.HighEndpoint, true);
                    }
                case QueryGraphRangeEnum.NOT_RANGE_OPEN:
                    {
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

        public override Type ProviderClass
        {
            get { return typeof(PropertySortedEventTable); }
        }
    }
} // end of namespace
