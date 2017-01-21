///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.epl.@join.exec.composite;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.join.table
{
	public abstract class PropertyCompositeEventTable : EventTable
	{
	    private readonly IList<Type> _optKeyCoercedTypes;
	    private readonly IList<Type> _optRangeCoercedTypes;
	    private readonly EventTableOrganization _organization;

	    public abstract IDictionary<object, object> IndexTable { get; }
	    public abstract CompositeIndexQueryResultPostProcessor PostProcessor { get; }

	    public abstract void Add(EventBean @event);
	    public abstract void Remove(EventBean @event);
	    public abstract bool IsEmpty();
	    public abstract void Clear();
	    public abstract void Destroy();
        public abstract IEnumerator<EventBean> GetEnumerator();

	    protected PropertyCompositeEventTable(IList<Type> optKeyCoercedTypes, IList<Type> optRangeCoercedTypes, EventTableOrganization organization)
	    {
	        _optKeyCoercedTypes = optKeyCoercedTypes;
	        _optRangeCoercedTypes = optRangeCoercedTypes;
	        _organization = organization;
	    }

	    public void AddRemove(EventBean[] newData, EventBean[] oldData) {
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

	    /// <summary>
	    /// Add an array of events. Same event instance is not added twice. Event properties should be immutable.
	    /// Allow null passed instead of an empty array.
	    /// </summary>
	    /// <param name="events">to add</param>
	    /// <throws>IllegalArgumentException if the event was already existed in the index</throws>
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

	    /// <summary>
	    /// Remove events.
	    /// </summary>
	    /// <param name="events">to be removed, can be null instead of an empty array.</param>
	    /// <throws>IllegalArgumentException when the event could not be removed as its not in the index</throws>
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

	    public override string ToString()
        {
	        return ToQueryPlan();
	    }

	    public string ToQueryPlan()
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

        public virtual object Index
        {
            get { return IndexTable; }
        }

        public EventTableOrganization Organization
	    {
	        get { return _organization; }
	    }

	    public abstract Type ProviderClass { get; }
	    public abstract int NumKeys { get; }

	    IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
	}
} // end of namespace
