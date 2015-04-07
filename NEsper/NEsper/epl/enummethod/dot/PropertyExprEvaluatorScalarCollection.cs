///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.enummethod.dot
{
	public class PropertyExprEvaluatorScalarCollection
        : ExprEvaluatorEnumeration
        , ExprEvaluatorEnumerationGivenEvent
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

	    private readonly string _propertyName;
	    private readonly int _streamId;
	    private readonly EventPropertyGetter _getter;
	    private readonly Type _componentType;

	    public PropertyExprEvaluatorScalarCollection(string propertyName, int streamId, EventPropertyGetter getter, Type componentType)
        {
	        _propertyName = propertyName;
	        _streamId = streamId;
	        _getter = getter;
	        _componentType = componentType;
	    }

	    public ICollection<EventBean> EvaluateGetROCollectionEvents(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
	        return null;
	    }

	    public ICollection<object> EvaluateGetROCollectionScalar(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
	        return EvaluateInternal<object>(eventsPerStream[_streamId]);
	    }

	    public ICollection<EventBean> EvaluateEventGetROCollectionEvents(EventBean @event, ExprEvaluatorContext context)
        {
	        return EvaluateInternal<EventBean>(@event);
	    }

	    private ICollection<T> EvaluateInternal<T>(EventBean @event)
        {
	        var result = _getter.Get(@event);
	        if (result == null)
            {
	            return null;
	        }

	        var resultAsCollection = result as ICollection<object>;
            if (resultAsCollection == null)
            {
	            Log.Warn("Expected collection-type input from property '" + _propertyName + "' but received " + result.GetType());
	            return null;
	        }

	        return resultAsCollection.Unwrap<T>();
        }

	    public Type ComponentTypeCollection
	    {
	        get { return _componentType; }
	    }

	    public ICollection<object> EvaluateEventGetROCollectionScalar(EventBean @event, ExprEvaluatorContext context)
        {
	        return null;
	    }

	    public EventBean EvaluateEventGetEventBean(EventBean @event, ExprEvaluatorContext context)
        {
	        return null;
	    }

	    public EventType GetEventTypeCollection(EventAdapterService eventAdapterService, string statementId)
        {
	        return null;
	    }

	    public EventType GetEventTypeSingle(EventAdapterService eventAdapterService, string statementId)
        {
	        return null;
	    }

	    public EventBean EvaluateGetEventBean(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
	        return null;
	    }
	}
} // end of namespace
