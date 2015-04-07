///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.events;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.join.table
{
    /// <summary>
    /// MapIndex that organizes events by the event property values into hash buckets. 
    /// Based on a HashMap with <seealso cref="com.espertech.esper.collection.MultiKeyUntyped"/> 
    /// keys that store the property values. Takes a list of property names as parameter. Doesn't 
    /// care which event type the events have as long as the properties exist. If the same event
    /// is added twice, the class throws an exception on add.
    /// </summary>
    public class PropertyIndexedEventTable : EventTable
    {
        internal readonly EventPropertyGetter[] PropertyGetters;
        private readonly EventTableOrganization _organization;
        protected internal readonly IDictionary<MultiKeyUntyped, ISet<EventBean>> PropertyIndex;
    
        public PropertyIndexedEventTable(EventPropertyGetter[] propertyGetters, EventTableOrganization organization)
        {
            PropertyGetters = propertyGetters;
            _organization = organization;
            PropertyIndex = new Dictionary<MultiKeyUntyped, ISet<EventBean>>();
        }
    
        /// <summary>Determine multikey for index access. </summary>
        /// <param name="theEvent">to get properties from for key</param>
        /// <returns>multi key</returns>
        protected virtual MultiKeyUntyped GetMultiKey(EventBean theEvent)
        {
            return EventBeanUtility.GetMultiKey(theEvent, PropertyGetters);
        }
    
        public virtual void AddRemove(EventBean[] newData, EventBean[] oldData)
        {
            Instrument.With(
                i => i.QIndexAddRemove(this, newData, oldData),
                i => i.AIndexAddRemove(),
                () =>
                {
                    if (newData != null)
                        newData.ForEach(Add);
                    if (oldData != null)
                        oldData.ForEach(Remove);
                });
        }

        /// <summary>
        /// Add an array of events. Same event instance is not added twice. Event properties should be immutable. Allow null passed instead of an empty array.
        /// </summary>
        /// <param name="events">to add</param>
        /// <throws>ArgumentException if the event was already existed in the index</throws>
        public virtual void Add(EventBean[] events)
        {
            if (events != null && events.Length > 0)
            {
                Instrument.With(
                    i => i.QIndexAdd(this, events),
                    i => i.AIndexAdd(),
                    () => events.ForEach(Add));
            }
        }
    
        /// <summary>Remove events. </summary>
        /// <param name="events">to be removed, can be null instead of an empty array.</param>
        /// <throws>ArgumentException when the event could not be removed as its not in the index</throws>
        public virtual void Remove(EventBean[] events)
        {
            if (events != null && events.Length > 0)
            {
                Instrument.With(
                    i => i.QIndexRemove(this, events),
                    i => i.AIndexRemove(),
                    () => events.ForEach(Remove));
            }
        }

        /// <summary>Returns the set of events that have the same property value as the given event. </summary>
        /// <param name="keys">to compare against</param>
        /// <returns>set of events with property value, or null if none found (never returns zero-sized set)</returns>
        public virtual ISet<EventBean> Lookup(object[] keys)
        {
            var key = new MultiKeyUntyped(keys);
            return PropertyIndex.Get(key);
        }

        public virtual void Add(EventBean theEvent)
        {
            var key = GetMultiKey(theEvent);
    
            var events = PropertyIndex.Get(key);
            if (events == null)
            {
                events = new LinkedHashSet<EventBean>();
                PropertyIndex.Put(key, events);
            }
    
            events.Add(theEvent);
        }

        public virtual void Remove(EventBean theEvent)
        {
            var key = GetMultiKey(theEvent);
    
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

        public virtual bool IsEmpty()
        {
            return PropertyIndex.IsEmpty();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual IEnumerator<EventBean> GetEnumerator()
        {
            return PropertyIndex.SelectMany(entry => entry.Value).GetEnumerator();        }

        public virtual void Clear()
        {
            PropertyIndex.Clear();
        }

        public virtual string ToQueryPlan()
        {
            return GetType().Name +
                    " streamNum=" + _organization.StreamNum +
                    " propertyGetters=" + PropertyGetters.Render();
        }

        public virtual int? NumberOfEvents
        {
            get { return null; }
        }

        public virtual int NumKeys
        {
            get { return PropertyIndex.Count; }
        }

        public virtual object Index
        {
            get { return PropertyIndex; }
        }

        public virtual EventTableOrganization Organization
        {
            get { return _organization; }
        }
    }
}
