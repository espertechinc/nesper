///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.join.table
{
	/// <summary>
	/// Unique index.
	/// </summary>
	public class PropertyIndexedEventTableSingleUnique 
        : PropertyIndexedEventTableSingle
        , EventTableAsSet
	{
	    private readonly IDictionary<object, EventBean> _propertyIndex;
	    private readonly bool _canClear;

	    public PropertyIndexedEventTableSingleUnique(EventPropertyGetter propertyGetter, EventTableOrganization organization)
	        : base(propertyGetter, organization)
	    {
            _propertyIndex = new NullableDictionary<object, EventBean>();
	        _canClear = true;
	    }

	    public PropertyIndexedEventTableSingleUnique(EventPropertyGetter propertyGetter, EventTableOrganization organization, IDictionary<object, EventBean> propertyIndex)
	        : base(propertyGetter, organization)
        {
	        _propertyIndex = propertyIndex;
	        _canClear = false;
	    }

	    public override ISet<EventBean> Lookup(object key)
	    {
	        EventBean @event = _propertyIndex.Get(key);
	        if (@event != null) {
	            return Collections.SingletonSet(@event);
	        }
	        return null;
	    }

	    public override int NumKeys
	    {
	        get { return _propertyIndex.Count; }
	    }

	    public override object Index
	    {
	        get { return _propertyIndex; }
	    }

        /// <summary>
        /// Remove then add events.
        /// </summary>
        /// <param name="newData">to add</param>
        /// <param name="oldData">to remove</param>
        /// <param name="exprEvaluatorContext">The expr evaluator context.</param>
        public override void AddRemove(EventBean[] newData, EventBean[] oldData, ExprEvaluatorContext exprEvaluatorContext)
        {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QIndexAddRemove(this, newData, oldData);}
	        if (oldData != null) {
	            foreach (EventBean theEvent in oldData) {
	                Remove(theEvent, exprEvaluatorContext);
	            }
	        }
	        if (newData != null) {
	            foreach (EventBean theEvent in newData) {
	                Add(theEvent, exprEvaluatorContext);
	            }
	        }
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AIndexAddRemove();}
	    }

	    public override void Add(EventBean theEvent, ExprEvaluatorContext exprEvaluatorContext)
	    {
	        var key = GetKey(theEvent);
	        
            var existing = _propertyIndex.Push(key, theEvent);
	        if (existing != null && !existing.Equals(theEvent)) {
	            throw PropertyIndexedEventTableUnique.HandleUniqueIndexViolation(organization.IndexName, key);
	        }
	    }

	    public override void Remove(EventBean theEvent, ExprEvaluatorContext exprEvaluatorContext)
	    {
	        object key = GetKey(theEvent);
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

	    public override void Destroy() {
	        Clear();
	    }

	    public override string ToString()
	    {
	        return ToQueryPlan();
	    }

	    public override int? NumberOfEvents
	    {
	        get { return _propertyIndex.Count; }
	    }

	    public ISet<EventBean> AllValues
	    {
	        get
	        {
	            if (_propertyIndex.IsEmpty())
	            {
	                return Collections.GetEmptySet<EventBean>();
	            }
	            return new HashSet<EventBean>(_propertyIndex.Values);
	        }
	    }

	    public override Type ProviderClass
	    {
	        get { return typeof (PropertyIndexedEventTableSingleUnique); }
	    }
	}
} // end of namespace
