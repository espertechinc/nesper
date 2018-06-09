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
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.join.table
{
	/// <summary>
	/// Index that organizes events by the event property values into hash buckets. Based on a HashMap
	/// with <seealso cref="com.espertech.esper.collection.MultiKeyUntyped" /> keys that store the property values.
	/// Takes a list of property names as parameter. Doesn't care which event type the events have as long as the properties
	/// exist. If the same event is added twice, the class throws an exception on add.
	/// </summary>
	public abstract class PropertyIndexedEventTable : EventTable
	{
	    protected readonly EventPropertyGetter[] propertyGetters;
	    protected readonly EventTableOrganization organization;

	    public abstract ISet<EventBean> Lookup(object[] keys);

	    public abstract void Add(EventBean @event, ExprEvaluatorContext exprEvaluatorContext);
	    public abstract void Remove(EventBean @event, ExprEvaluatorContext exprEvaluatorContext);
	    public abstract bool IsEmpty();
	    public abstract void Clear();
	    public abstract void Destroy();
	    public abstract IEnumerator<EventBean> GetEnumerator();

	    IEnumerator IEnumerable.GetEnumerator()
	    {
	        return GetEnumerator();
	    }

	    protected PropertyIndexedEventTable(EventPropertyGetter[] propertyGetters, EventTableOrganization organization)
        {
	        this.propertyGetters = propertyGetters;
	        this.organization = organization;
	    }

	    /// <summary>
	    /// Determine multikey for index access.
	    /// </summary>
	    /// <param name="theEvent">to get properties from for key</param>
	    /// <returns>multi key</returns>
	    protected virtual MultiKeyUntyped GetMultiKey(EventBean theEvent)
	    {
	        return EventBeanUtility.GetMultiKey(theEvent, propertyGetters);
	    }

	    public virtual void AddRemove(EventBean[] newData, EventBean[] oldData, ExprEvaluatorContext exprEvaluatorContext)
        {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QIndexAddRemove(this, newData, oldData);}
	        if (newData != null) {
	            foreach (EventBean theEvent in newData) {
	                Add(theEvent, exprEvaluatorContext);
	            }
	        }
	        if (oldData != null) {
	            foreach (EventBean theEvent in oldData) {
	                Remove(theEvent, exprEvaluatorContext);
	            }
	        }
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AIndexAddRemove();}
	    }

        /// <summary>
        /// Add an array of events. Same event instance is not added twice. Event properties should be immutable.
        /// Allow null passed instead of an empty array.
        /// </summary>
        /// <param name="events">to add</param>
        /// <param name="exprEvaluatorContext">The expr evaluator context.</param>
        /// <throws>ArgumentException if the event was already existed in the index</throws>
        public virtual void Add(EventBean[] events, ExprEvaluatorContext exprEvaluatorContext)
	    {
	        if (events != null) {

	            if (InstrumentationHelper.ENABLED && events.Length > 0) {
	                InstrumentationHelper.Get().QIndexAdd(this, events);
	                foreach (EventBean theEvent in events) {
	                    Add(theEvent, exprEvaluatorContext);
	                }
	                InstrumentationHelper.Get().AIndexAdd();
	                return;
	            }

	            foreach (EventBean theEvent in events) {
	                Add(theEvent, exprEvaluatorContext);
	            }
	        }
	    }

        /// <summary>
        /// Remove events.
        /// </summary>
        /// <param name="events">to be removed, can be null instead of an empty array.</param>
        /// <param name="exprEvaluatorContext"></param>
        /// <throws>ArgumentException when the event could not be removed as its not in the index</throws>
        public virtual void Remove(EventBean[] events, ExprEvaluatorContext exprEvaluatorContext)
	    {
	        if (events != null) {

	            if (InstrumentationHelper.ENABLED && events.Length > 0) {
	                InstrumentationHelper.Get().QIndexRemove(this, events);
	                foreach (EventBean theEvent in events) {
	                    Remove(theEvent, exprEvaluatorContext);
	                }
	                InstrumentationHelper.Get().AIndexRemove();
	                return;
	            }

	            foreach (EventBean theEvent in events) {
	                Remove(theEvent, exprEvaluatorContext);
	            }
	        }
	    }

	    public string ToQueryPlan()
	    {
	        return this.GetType().Name +
	                " streamNum=" + organization.StreamNum +
	                " propertyGetters=" + CompatExtensions.Render(propertyGetters);
	    }

	    public EventTableOrganization Organization
	    {
	        get { return organization; }
	    }

	    public abstract Type ProviderClass { get; }
	    public abstract int? NumberOfEvents { get; }
	    public abstract int NumKeys { get; }
	    public abstract object Index { get; }
	}
} // end of namespace
