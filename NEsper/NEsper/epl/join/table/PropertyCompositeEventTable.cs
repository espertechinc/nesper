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

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.@join.exec.composite;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.join.table
{
    /// <summary>
    /// For use when the index comprises of either two or more ranges or a unique key in 
    /// combination with a range. Expected at least either (A) one key and one range or (B) 
    /// zero keys and 2 ranges.
    /// <list type="bullet">
    /// <item>not applicable for range-only lookups (since there the key can be the value itself</item>
    /// <item>not applicable for multiple nested range as ordering not nested</item>
    /// <item>each add/remove and lookup would also need to construct a key object.</item>
    /// </list>
    /// </summary>
    public class PropertyCompositeEventTable : EventTable
    {
        private readonly CompositeIndexEnterRemove _chain;
        private readonly IList<Type> _optKeyCoercedTypes;
        private readonly IList<Type> _optRangeCoercedTypes;
        private readonly EventTableOrganization _organization;
    
        /// <summary>MapIndex table (sorted and/or keyed, always nested). </summary>
        private readonly IDictionary<Object, Object> _index;
    
        public PropertyCompositeEventTable(bool isHashKeyed, CompositeIndexEnterRemove chain, IList<Type> optKeyCoercedTypes, IList<Type> optRangeCoercedTypes, EventTableOrganization organization)
        {
            _chain = chain;
            _optKeyCoercedTypes = optKeyCoercedTypes;
            _optRangeCoercedTypes = optRangeCoercedTypes;
            _organization = organization;
    
            if (isHashKeyed) {
                _index = new Dictionary<Object, Object>();
            }
            else {
                _index = new OrderedDictionary<Object, Object>();
            }
        }

        public object Index
        {
            get { return _index; }
        }

        public IDictionary<object, object> IndexTable
        {
            get { return _index; }
        }

        public IDictionary<object, object> MapIndex
        {
            get { return _index; }
        }

        public void AddRemove(EventBean[] newData, EventBean[] oldData)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QIndexAddRemove(this, newData, oldData);}
            if (newData != null) {
                foreach (EventBean theEvent in newData) {
                    Add(theEvent);
                }
            }
            if (oldData != null) {
                foreach (EventBean theEvent in oldData) {
                    Remove(theEvent);
                }
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AIndexAddRemove();}
        }
    
        /// <summary>Add an array of events. Same event instance is not added twice. Event properties should be immutable. Allow null passed instead of an empty array. </summary>
        /// <param name="events">to add</param>
        /// <throws>ArgumentException if the event was already existed in the index</throws>
        public void Add(EventBean[] events)
        {
            if (events != null) {
    
                if (InstrumentationHelper.ENABLED && events.Length > 0) {
                    InstrumentationHelper.Get().QIndexAdd(this, events);
                    foreach (EventBean theEvent in events) {
                        Add(theEvent);
                    }
                    InstrumentationHelper.Get().AIndexAdd();
                    return;
                }
    
                foreach (EventBean theEvent in events) {
                    Add(theEvent);
                }
            }
        }
    
        /// <summary>Remove events. </summary>
        /// <param name="events">to be removed, can be null instead of an empty array.</param>
        /// <throws>ArgumentException when the event could not be removed as its not in the index</throws>
        public void Remove(EventBean[] events)
        {
            if (events != null) {
    
                if (InstrumentationHelper.ENABLED && events.Length > 0) {
                    InstrumentationHelper.Get().QIndexRemove(this, events);
                    foreach (EventBean theEvent in events) {
                        Remove(theEvent);
                    }
                    InstrumentationHelper.Get().AIndexRemove();
                    return;
                }
    
                foreach (EventBean theEvent in events) {
                    Remove(theEvent);
                }
            }
        }

        public void Add(EventBean theEvent)
        {
            _chain.Enter(theEvent, _index);
        }

        public void Remove(EventBean theEvent)
        {
            _chain.Remove(theEvent, _index);
        }

        public bool IsEmpty()
        {
            return _index.IsEmpty();
        }

        public IEnumerator<EventBean> GetEnumerator()
        {
            var result = new LinkedHashSet<EventBean>();
            _chain.GetAll(result, _index);
            return result.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Clear()
        {
            _index.Clear();
        }
    
        public override String ToString() {
            return ToQueryPlan();
        }
    
        public String ToQueryPlan()
        {
            return GetType().FullName;
        }

        public IList<Type> OptRangeCoercedTypes
        {
            get { return _optRangeCoercedTypes; }
        }

        public IList<Type> OptKeyCoercedTypes
        {
            get { return _optKeyCoercedTypes; }
        }

        public int? NumberOfEvents
        {
            get { return null; }
        }

        public int NumKeys
        {
            get { return _index.Count; }
        }

        public EventTableOrganization Organization
        {
            get { return _organization; }
        }
    }
}
