///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.util;

namespace com.espertech.esper.epl.enummethod.dot
{
    public class PropertyExprEvaluatorScalarArray
        : ExprEvaluatorEnumeration
        , ExprEvaluatorEnumerationGivenEvent
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string _propertyName;
        private readonly int _streamId;
        private readonly EventPropertyGetter _getter;
        private readonly Type _componentType;

        public PropertyExprEvaluatorScalarArray(string propertyName, int streamId, EventPropertyGetter getter, Type componentType)
        {
            _propertyName = propertyName;
            _streamId = streamId;
            _getter = getter;
            _componentType = componentType;
        }

        public ICollection<object> EvaluateGetROCollectionScalar(EvaluateParams evaluateParams)
        {
            EventBean eventInQuestion = evaluateParams.EventsPerStream[_streamId];
            return EvaluateEventGetROCollectionScalar(eventInQuestion, evaluateParams.ExprEvaluatorContext);
        }

        public ICollection<object> EvaluateEventGetROCollectionScalar(EventBean @event, ExprEvaluatorContext context)
        {
            if (@event == null)
            {
                return null;
            }
            return EvaluateGetInterval(@event);
        }

        public Type ComponentTypeCollection
        {
            get { return _componentType; }
        }

        public EventType GetEventTypeCollection(EventAdapterService eventAdapterService, int statementId)
        {
            return null;
        }

        public EventType GetEventTypeSingle(EventAdapterService eventAdapterService, int statementId)
        {
            return null;
        }

        public EventBean EvaluateGetEventBean(EvaluateParams evaluateParams)
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

        public ICollection<EventBean> EvaluateGetROCollectionEvents(EvaluateParams evaluateParams)
        {
            return null;
        }

        private ICollection<object> EvaluateGetInterval(EventBean @event)
        {
            var value = _getter.Get(@event);
            if (value == null)
            {
                return null;
            }
            if (!(value.GetType().IsArray))
            {
                Log.Warn(string.Format("Expected array-type input from property '{0}' but received {1}",
                    _propertyName, value.GetType().GetCleanName()));
                return null;
            }

            return value.Unwrap<object>();
        }
    }
} // end of namespace
