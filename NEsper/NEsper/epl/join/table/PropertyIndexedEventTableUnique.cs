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
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.join.table
{
	public class PropertyIndexedEventTableUnique : PropertyIndexedEventTable, EventTableAsSet
	{
	    internal readonly IDictionary<MultiKeyUntyped, EventBean> _propertyIndex;
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
        /// <param name="exprEvaluatorContext">The expr evaluator context.</param>
        public override void AddRemove(EventBean[] newData, EventBean[] oldData, ExprEvaluatorContext exprEvaluatorContext)
        {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QIndexAddRemove(this, newData, oldData);}
	        if (oldData != null) {
	            foreach (var theEvent in oldData) {
	                Remove(theEvent, exprEvaluatorContext);
	            }
	        }
	        if (newData != null) {
	            foreach (var theEvent in newData) {
	                Add(theEvent, exprEvaluatorContext);
	            }
	        }
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AIndexAddRemove();}
	    }

	    public override ISet<EventBean> Lookup(object[] keys)
	    {
	        var key = new MultiKeyUntyped(keys);
	        var @event = _propertyIndex.Get(key);
	        if (@event != null) {
	            return Collections.SingletonSet(@event);
	        }
	        return null;
	    }

	    public override void Add(EventBean theEvent, ExprEvaluatorContext exprEvaluatorContext)
	    {
	        var key = GetMultiKey(theEvent);
	        var existing = _propertyIndex.Push(key, theEvent);
	        if (existing != null && !existing.Equals(theEvent)) {
	            throw HandleUniqueIndexViolation(organization.IndexName, key);
	        }
	    }

	    public static EPException HandleUniqueIndexViolation(string indexName, object key)
        {
	        var indexNameDisplay = indexName == null ? "" : " '" + indexName + "'";
	        throw new EPException("Unique index violation, index" + indexNameDisplay + " is a unique index and key '" + key + "' already exists");
	    }

	    public override void Remove(EventBean theEvent, ExprEvaluatorContext exprEvaluatorContext)
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

	    public override void Destroy()
        {
	        Clear();
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
	        get { return typeof (PropertyIndexedEventTableUnique); }
	    }
	}
} // end of namespace
