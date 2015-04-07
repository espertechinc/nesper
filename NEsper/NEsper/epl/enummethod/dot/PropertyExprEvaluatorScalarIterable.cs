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
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.enummethod.dot
{
    public class PropertyExprEvaluatorScalarIterable
        : ExprEvaluatorEnumeration
        , ExprEvaluatorEnumerationGivenEvent
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly String _propertyName;
        private readonly int _streamId;
        private readonly EventPropertyGetter _getter;
        private readonly Type _componentType;

        public PropertyExprEvaluatorScalarIterable(
            String propertyName,
            int streamId,
            EventPropertyGetter getter,
            Type componentType)
        {
            _propertyName = propertyName;
            _streamId = streamId;
            _getter = getter;
            _componentType = componentType;
        }

        public ICollection<object> EvaluateGetROCollectionScalar(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            return EvaluateInternal<object>(eventsPerStream[_streamId]);
        }

        public ICollection<object> EvaluateEventGetROCollectionScalar(EventBean @event, ExprEvaluatorContext context)
        {
            return EvaluateInternal<object>(@event);
        }

        private ICollection<T> EvaluateInternal<T>(EventBean eventInQuestion)
        {
            var result = _getter.Get(eventInQuestion);
            if (result == null)
            {
                return null;
            }

            var resultType = result.GetType();
            if (result is ICollection<T>)
            {
                return (ICollection<T>) result;
            }

            if (resultType.IsGenericCollection() || resultType is IEnumerable)
            {
                return result.UnwrapWithNulls<T>();
            }

            Log.Warn(
                "Expected iterable-type input from property '" + _propertyName + "' but received " + resultType.FullName);
            return null;
        }

        public EventType GetEventTypeCollection(EventAdapterService eventAdapterService, String statementId)
        {
            return null;
        }

        public Type ComponentTypeCollection
        {
            get { return _componentType; }
        }

        public EventType GetEventTypeSingle(EventAdapterService eventAdapterService, String statementId)
        {
            return null;
        }

        public EventBean EvaluateGetEventBean(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            return null;
        }

        public ICollection<EventBean> EvaluateEventGetROCollectionEvents(EventBean @event, ExprEvaluatorContext context)
        {
            return null;
        }

        public EventBean EvaluateEventGetEventBean(EventBean @event, ExprEvaluatorContext context)
        {
            return null;
        }

        public ICollection<EventBean> EvaluateGetROCollectionEvents(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            return null;
        }
    }
}