///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.join.table
{
	public class PropertyIndexedEventTableUnique
        : PropertyIndexedEventTable
        , EventTableAsSet
	{
	    private readonly IDictionary<MultiKeyUntyped, EventBean> _propertyIndex;
	    private readonly bool _canClear;

	    public PropertyIndexedEventTableUnique(EventPropertyGetter[] propertyGetters, EventTableOrganization organization)
	        : base(propertyGetters, organization)
        {
	        _propertyIndex = new Dictionary<MultiKeyUntyped, EventBean>();
	        _canClear = true;
	    }

	    public PropertyIndexedEventTableUnique(EventPropertyGetter[] propertyGetters, EventTableOrganization organization, IDictionary<MultiKeyUntyped, EventBean> propertyIndex)
	        : base(propertyGetters, organization)
        {
	        _propertyIndex = propertyIndex;
	        _canClear = false;
	    }

	    /// <summary>
	    /// Remove then add events.
	    /// </summary>
	    /// <param name="newData">to add</param>
	    /// <param name="oldData">to remove</param>
	    public override void AddRemove(EventBean[] newData, EventBean[] oldData)
        {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QIndexAddRemove(this, newData, oldData);}
	        if (oldData != null) {
	            foreach (var theEvent in oldData) {
	                Remove(theEvent);
	            }
	        }
	        if (newData != null) {
	            foreach (var theEvent in newData) {
	                Add(theEvent);
	            }
	        }
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AIndexAddRemove();}
	    }

	    /// <summary>
	    /// Add an array of events. Same event instance is not added twice. Event properties should be immutable.
	    /// Allow null passed instead of an empty array.
	    /// </summary>
	    /// <param name="events">to add</param>
	    /// <throws>IllegalArgumentException if the event was already existed in the index</throws>
	    public override void Add(EventBean[] events)
	    {
	        if (events != null) {

	            if (InstrumentationHelper.ENABLED && events.Length > 0) {
	                InstrumentationHelper.Get().QIndexAdd(this, events);
	                foreach (var theEvent in events) {
	                    Add(theEvent);
	                }
	                InstrumentationHelper.Get().AIndexAdd();
	                return;
	            }

	            foreach (var theEvent in events) {
	                Add(theEvent);
	            }
	        }
	    }

	    /// <summary>
	    /// Remove events.
	    /// </summary>
	    /// <param name="events">to be removed, can be null instead of an empty array.</param>
	    /// <throws>IllegalArgumentException when the event could not be removed as its not in the index</throws>
	    public override void Remove(EventBean[] events)
	    {
	        if (events != null) {

	            if (InstrumentationHelper.ENABLED && events.Length > 0) {
	                InstrumentationHelper.Get().QIndexRemove(this, events);
	                foreach (var theEvent in events) {
	                    Remove(theEvent);
	                }
	                InstrumentationHelper.Get().AIndexRemove();
	                return;
	            }

	            foreach (var theEvent in events) {
	                Remove(theEvent);
	            }
	        }
	    }

	    /// <summary>
	    /// Returns the set of events that have the same property value as the given event.
	    /// </summary>
	    /// <param name="keys">to compare against</param>
	    /// <returns>set of events with property value, or null if none found (never returns zero-sized set)</returns>
	    public override ISet<EventBean> Lookup(object[] keys)
	    {
	        var key = new MultiKeyUntyped(keys);
	        var @event = _propertyIndex.Get(key);
	        if (@event != null) {
	            return Collections.SingletonSet(@event);
	        }
	        return null;
	    }

	    public override void Add(EventBean theEvent)
	    {
	        var key = GetMultiKey(theEvent);

	        var existing = _propertyIndex.Push(key, theEvent);
	        if (existing != null && !existing.Equals(theEvent)) {
	            throw HandleUniqueIndexViolation(Organization.IndexName, key);
	        }
	    }

	    internal static EPException HandleUniqueIndexViolation(string indexName, object key)
        {
	        var indexNameDisplay = indexName == null ? "" : " '" + indexName + "'";
	        throw new EPException("Unique index violation, index" + indexNameDisplay + " is a unique index and key '" + key + "' already exists");
	    }

	    public override void Remove(EventBean theEvent)
	    {
	        var key = GetMultiKey(theEvent);
	        _propertyIndex.Remove(key);
	    }

	    public override bool IsEmpty()
	    {
	        return _propertyIndex.IsEmpty();
	    }

	    public override IEnumerator<EventBean> GetEnumerator()
	    {
	        return _propertyIndex.Values.GetEnumerator();
	    }

	    public override void Clear()
	    {
	        if (_canClear) {
	            _propertyIndex.Clear();
	        }
	    }

	    public override int? NumberOfEvents
	    {
	        get { return _propertyIndex.Count; }
	    }

	    public override int NumKeys
	    {
	        get { return _propertyIndex.Count; }
	    }

	    public override object Index
	    {
	        get { return _propertyIndex; }
	    }

	    public override string ToQueryPlan()
	    {
	        return GetType().Name +
	                " streamNum=" + Organization.StreamNum +
	                " propertyGetters=" + PropertyGetters.Render();
	    }

	    public ISet<EventBean> AllValues()
        {
	        if (_propertyIndex.IsEmpty()) {
	            return Collections.GetEmptySet<EventBean>();
	        }
	        return new HashSet<EventBean>(_propertyIndex.Values);
	    }
	}
} // end of namespace
