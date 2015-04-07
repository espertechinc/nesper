///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.@join.exec.@base;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.filter;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.join.table
{
    /// <summary>
    /// MapIndex that organizes events by the event property values into a single TreeMap 
    /// sortable non-nested index with Object keys that store the property values.
    /// </summary>
    public class PropertySortedEventTable : EventTable
    {
        internal readonly EventPropertyGetter PropertyGetter;

        private readonly EventTableOrganization _organization;
    
        /// <summary>MapIndex table. </summary>
        internal readonly OrderedDictionary<Object, ISet<EventBean>> PropertyIndex;
    
        internal readonly LinkedHashSet<EventBean> NullKeyedValues;
    
        // override in a subclass
        protected virtual object Coerce(Object value) {
            return value;
        }
    
        /// <summary>Ctor. </summary>
        public PropertySortedEventTable(EventPropertyGetter propertyGetter, EventTableOrganization organization)
        {
            _organization = organization;
            PropertyGetter = propertyGetter;
            PropertyIndex = new OrderedDictionary<Object, ISet<EventBean>>();
            NullKeyedValues = new LinkedHashSet<EventBean>();
        }

        /// <summary>
        /// Returns the organization.
        /// </summary>
        public EventTableOrganization Organization
        {
            get { return _organization; }
        }

        /// <summary>Determine multikey for index access. </summary>
        /// <param name="theEvent">to get properties from for key</param>
        /// <returns>multi key</returns>
        protected Object GetIndexedValue(EventBean theEvent)
        {
            return PropertyGetter.Get(theEvent);
        }
    
        public void AddRemove(EventBean[] newData, EventBean[] oldData) {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QIndexAddRemove(this, newData, oldData);}
            if (newData != null) {
                for (int ii = 0; ii < newData.Length; ii++) {
                    Add(newData[ii]);
                }
            }
            if (oldData != null) {
                for (int ii = 0; ii < oldData.Length; ii++) {
                    Remove(oldData[ii]);
                }
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AIndexAddRemove();}
        }
    
        /// <summary>Add an array of events. Same event instance is not added twice. Event properties should be immutable. Allow null passed instead of an empty array. </summary>
        /// <param name="events">to add</param>
        /// <throws>ArgumentException if the event was already existed in the index</throws>
        public void Add(EventBean[] events)
        {
            if (events != null)
            {
                if (InstrumentationHelper.ENABLED && events.Length > 0) {
                    InstrumentationHelper.Get().QIndexAdd(this, events);
                    foreach (var theEvent in events) {
                        Add(theEvent);
                    }
                    InstrumentationHelper.Get().AIndexAdd();
                    return;
                }

                for (int ii = 0; ii < events.Length; ii++) {
                    Add(events[ii]);
                }
            }
        }
    
        /// <summary>Remove events. </summary>
        /// <param name="events">to be removed, can be null instead of an empty array.</param>
        /// <throws>ArgumentException when the event could not be removed as its not in the index</throws>
        public void Remove(EventBean[] events)
        {
            if (events != null)
            {
                if (InstrumentationHelper.ENABLED && events.Length > 0) {
                    InstrumentationHelper.Get().QIndexRemove(this, events);
                    foreach (var theEvent in events) {
                        Remove(theEvent);
                    }
                    InstrumentationHelper.Get().AIndexRemove();
                    return;
                }

                for (int ii = 0; ii < events.Length; ii++) {
                    Remove(events[ii]);
                }
            }
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
        public ISet<EventBean> LookupRange(
            object keyStart,
            bool includeStart,
            object keyEnd,
            bool includeEnd,
            bool allowRangeReversal)
        {
            if (keyStart == null || keyEnd == null)
            {
                return Collections.GetEmptySet<EventBean>();
            }
            keyStart = Coerce(keyStart);
            keyEnd = Coerce(keyEnd);
            IDictionary<object, ISet<EventBean>> submap;
            try
            {
                submap = PropertyIndex.Between(keyStart, includeStart, keyEnd, includeEnd);
            }
            catch (ArgumentException)
            {
                if (allowRangeReversal)
                {
                    submap = PropertyIndex.Between(keyEnd, includeStart, keyStart, includeEnd);
                }
                else
                {
                    return Collections.GetEmptySet<EventBean>();
                }
            }
            return Normalize(submap);
        }

        public ISet<EventBean> LookupRangeColl(Object keyStart, bool includeStart, Object keyEnd, bool includeEnd, bool allowRangeReversal)
        {
            if (keyStart == null || keyEnd == null)
            {
                return Collections.GetEmptySet<EventBean>();
            }
            keyStart = Coerce(keyStart);
            keyEnd = Coerce(keyEnd);
            IDictionary<object, ISet<EventBean>> submap;
            try {
                submap = PropertyIndex.Between(keyStart, includeStart, keyEnd, includeEnd);
            }
            catch (ArgumentException) {
                if (allowRangeReversal) {
                    submap = PropertyIndex.Between(keyEnd, includeStart, keyStart, includeEnd);
                }
                else {
                    return Collections.GetEmptySet<EventBean>();
                }
            }
            return NormalizeCollection(submap);
        }
    
        public ISet<EventBean> LookupRangeInverted(Object keyStart, bool includeStart, Object keyEnd, bool includeEnd) {
            if (keyStart == null || keyEnd == null) {
                return Collections.GetEmptySet<EventBean>();
            }
            keyStart = Coerce(keyStart);
            keyEnd = Coerce(keyEnd);
            var submapOne = PropertyIndex.Head(keyStart, !includeStart);
            var submapTwo = PropertyIndex.Tail(keyEnd, !includeEnd);
            return Normalize(submapOne, submapTwo);
        }
    
        public ISet<EventBean> LookupRangeInvertedColl(Object keyStart, bool includeStart, Object keyEnd, bool includeEnd) {
            if (keyStart == null || keyEnd == null) {
                return Collections.GetEmptySet<EventBean>();
            }
            keyStart = Coerce(keyStart);
            keyEnd = Coerce(keyEnd);
            var submapOne = PropertyIndex.Head(keyStart, !includeStart);
            var submapTwo = PropertyIndex.Tail(keyEnd, !includeEnd);
            return NormalizeCollection(submapOne, submapTwo);
        }
    
        public ISet<EventBean> LookupLess(Object keyStart) {
            if (keyStart == null) {
                return Collections.GetEmptySet<EventBean>();
            }
            keyStart = Coerce(keyStart);
            return Normalize(PropertyIndex.Head(keyStart));
        }
    
        public ISet<EventBean> LookupLessThenColl(Object keyStart) {
            if (keyStart == null) {
                return Collections.GetEmptySet<EventBean>();
            }
            keyStart = Coerce(keyStart);
            return NormalizeCollection(PropertyIndex.Head(keyStart));
        }
    
        public ISet<EventBean> LookupLessEqual(Object keyStart) {
            if (keyStart == null) {
                return Collections.GetEmptySet<EventBean>();
            }
            keyStart = Coerce(keyStart);
            return Normalize(PropertyIndex.Head(keyStart, true));
        }
    
        public ISet<EventBean> LookupLessEqualColl(Object keyStart) {
            if (keyStart == null) {
                return Collections.GetEmptySet<EventBean>();
            }
            keyStart = Coerce(keyStart);
            return NormalizeCollection(PropertyIndex.Head(keyStart, true));
        }

        public ISet<EventBean> LookupGreaterEqual(Object keyStart)
        {
            if (keyStart == null) {
                return Collections.GetEmptySet<EventBean>();
            }
            keyStart = Coerce(keyStart);
            return Normalize(PropertyIndex.Tail(keyStart));
        }
    
        public ISet<EventBean> LookupGreaterEqualColl(Object keyStart) {
            if (keyStart == null) {
                return Collections.GetEmptySet<EventBean>();
            }
            keyStart = Coerce(keyStart);
            return NormalizeCollection(PropertyIndex.Tail(keyStart));
        }
    
        public ISet<EventBean> LookupGreater(Object keyStart) {
            if (keyStart == null) {
                return Collections.GetEmptySet<EventBean>();
            }
            keyStart = Coerce(keyStart);
            return Normalize(PropertyIndex.Tail(keyStart, false));
        }
    
        public ISet<EventBean> LookupGreaterColl(Object keyStart) {
            if (keyStart == null) {
                return Collections.GetEmptySet<EventBean>();
            }
            keyStart = Coerce(keyStart);
            return NormalizeCollection(PropertyIndex.Tail(keyStart, false));
        }

        public int? NumberOfEvents
        {
            get { return null; }
        }

        public int NumKeys
        {
            get { return PropertyIndex.Count; }
        }

        public object Index
        {
            get { return PropertyIndex; }
        }

        private ISet<EventBean> Normalize(IDictionary<Object,ISet<EventBean>> submap)
        {
            if (submap.Count == 0) {
                return null;
            }
            if (submap.Count == 1) {
                return submap.Get(submap.First().Key);
            }
            ISet<EventBean> result = new LinkedHashSet<EventBean>();
            foreach (var entry in submap) {
                result.AddAll(entry.Value);
            }
            return result;
        }
    
        private ISet<EventBean> NormalizeCollection(IDictionary<Object, ISet<EventBean>> submap)
        {
            if (submap.Count == 0) {
                return null;
            }
            if (submap.Count == 1) {
                return submap.Get(submap.First().Key);
            }
            var result = new LinkedHashSet<EventBean>();
            foreach (var entry in submap) {
                result.AddAll(entry.Value);
            }
            return result;
        }
    
        private ISet<EventBean> NormalizeCollection(IDictionary<object, ISet<EventBean>> submapOne, IDictionary<object, ISet<EventBean>> submapTwo)
        {
            if (submapOne.Count == 0) {
                return NormalizeCollection(submapTwo);
            }
            if (submapTwo.Count == 0) {
                return NormalizeCollection(submapOne);
            }
            var result = new LinkedHashSet<EventBean>();
            foreach (var entry in submapOne) {
                result.AddAll(entry.Value);
            }
            foreach (var entry in submapTwo) {
                result.AddAll(entry.Value);
            }
            return result;
        }

        private ISet<EventBean> Normalize(
            IDictionary<Object, ISet<EventBean>> submapOne, 
            IDictionary<object, ISet<EventBean>> submapTwo)
        {
            if (submapOne.Count == 0) {
                return Normalize(submapTwo);
            }
            if (submapTwo.Count == 0) {
                return Normalize(submapOne);
            }
            ISet<EventBean> result = new LinkedHashSet<EventBean>();
            foreach (var entry in submapOne) {
                result.AddAll(entry.Value);
            }
            foreach (var entry in submapTwo) {
                result.AddAll(entry.Value);
            }
            return result;
        }
    
        public void Add(EventBean theEvent)
        {
            var key = GetIndexedValue(theEvent);
    
            key = Coerce(key);
    
            if (key == null) {
                NullKeyedValues.Add(theEvent);
                return;
            }
    
            var events = PropertyIndex.Get(key);
            if (events == null)
            {
                events = new LinkedHashSet<EventBean>();
                PropertyIndex.Put(key, events);
            }
    
            events.Add(theEvent);
        }

        public void Remove(EventBean theEvent)
        {
            var key = GetIndexedValue(theEvent);
    
            if (key == null) {
                NullKeyedValues.Remove(theEvent);
                return;
            }
    
            key = Coerce(key);
    
            var events = PropertyIndex.Get(key);
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
                PropertyIndex.Remove(key);
            }
        }

        public bool IsEmpty()
        {
            return PropertyIndex.IsEmpty();
        }

        public IEnumerator<EventBean> GetEnumerator()
        {
            if (NullKeyedValues.IsEmpty()) {
                return PropertySortedEventTableEnumerator.Create(PropertyIndex);
            }

            return PropertySortedEventTableEnumerator.CreateEnumerable(PropertyIndex)
                .Merge(NullKeyedValues)
                .Where(t => t.A != null && t.B != null)
                .Select(t => t.B)
                .GetEnumerator();        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Clear()
        {
            PropertyIndex.Clear();
        }
    
        public String ToQueryPlan() {
            return GetType().Name +
                    " streamNum=" + _organization.StreamNum +
                    " propertyGetter=" + PropertyGetter;
        }

        public ISet<EventBean> LookupConstants(RangeIndexLookupValue lookupValueBase)
        {
            if (lookupValueBase is RangeIndexLookupValueEquals) {
                var equals = (RangeIndexLookupValueEquals) lookupValueBase;
                return PropertyIndex.Get(equals.Value);    
            }
    
            var lookupValue = (RangeIndexLookupValueRange) lookupValueBase;
            switch (lookupValue.Operator)
            {
                case QueryGraphRangeEnum.RANGE_CLOSED:
                {
                    var range = (Range) lookupValue.Value;
                    return LookupRange(range.LowEndpoint, true, range.HighEndpoint, true, lookupValue.IsAllowRangeReverse);
                }
                case QueryGraphRangeEnum.RANGE_HALF_OPEN:
                {
                    var range = (Range) lookupValue.Value;
                    return LookupRange(range.LowEndpoint, true, range.HighEndpoint, false, lookupValue.IsAllowRangeReverse);
                }
                case QueryGraphRangeEnum.RANGE_HALF_CLOSED:
                {
                    var range = (Range) lookupValue.Value;
                    return LookupRange(range.LowEndpoint, false, range.HighEndpoint, true, lookupValue.IsAllowRangeReverse);
                }
                case QueryGraphRangeEnum.RANGE_OPEN:
                {
                    var range = (Range) lookupValue.Value;
                    return LookupRange(range.LowEndpoint, false, range.HighEndpoint, false, lookupValue.IsAllowRangeReverse);
                }
                case QueryGraphRangeEnum.NOT_RANGE_CLOSED:
                {
                    var range = (Range) lookupValue.Value;
                    return LookupRangeInverted(range.LowEndpoint, true, range.HighEndpoint, true);
                }
                case QueryGraphRangeEnum.NOT_RANGE_HALF_OPEN:
                {
                    var range = (Range) lookupValue.Value;
                    return LookupRangeInverted(range.LowEndpoint, true, range.HighEndpoint, false);
                }
                case QueryGraphRangeEnum.NOT_RANGE_HALF_CLOSED:
                {
                    var range = (Range) lookupValue.Value;
                    return LookupRangeInverted(range.LowEndpoint, false, range.HighEndpoint, true);
                }
                case QueryGraphRangeEnum.NOT_RANGE_OPEN:
                {
                    var range = (Range) lookupValue.Value;
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
    }
}
