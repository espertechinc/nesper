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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.join.table
{
	/// <summary>
	/// Index that organizes events by the event property values into hash buckets. Based on a HashMap
	/// with <seealso cref="com.espertech.esper.collection.MultiKeyUntyped" /> keys that store the property values.
	/// </summary>
	public class PropertyIndexedEventTableSingleUnadorned : PropertyIndexedEventTableSingle
	{
	    protected readonly IDictionary<object, ISet<EventBean>> propertyIndex;

	    public PropertyIndexedEventTableSingleUnadorned(EventPropertyGetter propertyGetter, EventTableOrganization organization)
	        : base(propertyGetter, organization)
	    {
	        propertyIndex = new Dictionary<object, ISet<EventBean>>().WithNullSupport();
	    }

	    /// <summary>
	    /// Returns the set of events that have the same property value as the given event.
	    /// </summary>
	    /// <param name="key">to compare against</param>
	    /// <returns>set of events with property value, or null if none found (never returns zero-sized set)</returns>
	    public override ISet<EventBean> Lookup(object key)
	    {
	        return propertyIndex.Get(key);
	    }

	    public override void Add(EventBean theEvent, ExprEvaluatorContext exprEvaluatorContext)
	    {
	        var key = GetKey(theEvent);

	        var events = propertyIndex.Get(key);
	        if (events == null)
	        {
	            events = new LinkedHashSet<EventBean>();
	            propertyIndex.Put(key, events);
	        }

	        events.Add(theEvent);
	    }

	    public override void Remove(EventBean theEvent, ExprEvaluatorContext exprEvaluatorContext)
	    {
	        var key = GetKey(theEvent);

	        var events = propertyIndex.Get(key);
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
	            propertyIndex.Remove(key);
	        }
	    }

	    public override bool IsEmpty()
	    {
	        return propertyIndex.IsEmpty();
	    }

	    public override IEnumerator<EventBean> GetEnumerator()
	    {
	        return propertyIndex.Values
                .SelectMany(eventBeans => eventBeans)
                .GetEnumerator();
	    }

	    public override void Clear()
	    {
	        propertyIndex.Clear();
	    }

	    public override void Destroy() {
	        Clear();
	    }

	    public override int? NumberOfEvents
	    {
	        get { return null; }
	    }

	    public override int NumKeys
	    {
	        get { return propertyIndex.Count; }
	    }

	    public override object Index
	    {
	        get { return propertyIndex; }
	    }

	    public override Type ProviderClass
	    {
	        get { return typeof (PropertyIndexedEventTableSingle); }
	    }
	}
} // end of namespace
