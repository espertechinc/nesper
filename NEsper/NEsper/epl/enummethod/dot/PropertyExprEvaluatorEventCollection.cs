///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.enummethod.dot
{
    public class PropertyExprEvaluatorEventCollection
        : ExprEvaluatorEnumeration
        , ExprEvaluatorEnumerationGivenEvent
    {
        private readonly string _propertyNameCache;
        private readonly int _streamId;
        private readonly EventType _fragmentType;
        private readonly EventPropertyGetter _getter;
        private readonly bool _disablePropertyExpressionEventCollCache;

        public PropertyExprEvaluatorEventCollection(
            string propertyNameCache,
            int streamId,
            EventType fragmentType,
            EventPropertyGetter getter,
            bool disablePropertyExpressionEventCollCache)
        {
            _propertyNameCache = propertyNameCache;
            _streamId = streamId;
            _fragmentType = fragmentType;
            _getter = getter;
            _disablePropertyExpressionEventCollCache = disablePropertyExpressionEventCollCache;
        }

        public ICollection<EventBean> EvaluateGetROCollectionEvents(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var eventInQuestion = eventsPerStream[_streamId];
            if (eventInQuestion == null)
            {
                return null;
            }
            return EvaluateInternal(eventInQuestion, context);
        }

        public ICollection<EventBean> EvaluateEventGetROCollectionEvents(EventBean @event, ExprEvaluatorContext context)
        {
            if (@event == null)
            {
                return null;
            }
            return EvaluateInternal(@event, context);
        }

        private ICollection<EventBean> EvaluateInternal(EventBean eventInQuestion, ExprEvaluatorContext context)
        {

            if (!_disablePropertyExpressionEventCollCache)
            {
                var cacheEntry = context.ExpressionResultCacheService.GetPropertyColl(
                    _propertyNameCache, eventInQuestion);
                if (cacheEntry != null)
                {
                    return cacheEntry.Result;
                }
            }

            var events = (EventBean[]) _getter.GetFragment(eventInQuestion);
            ICollection<EventBean> coll = events ?? null;
            if (!_disablePropertyExpressionEventCollCache)
            {
                context.ExpressionResultCacheService.SavePropertyColl(_propertyNameCache, eventInQuestion, coll);
            }
            if (coll == null)
            {
                return null;
            }

            return coll;
        }

        public EventType GetEventTypeCollection(EventAdapterService eventAdapterService, int statementId)
        {
            return _fragmentType;
        }

        public ICollection<object> EvaluateGetROCollectionScalar(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return null;
        }

        public Type ComponentTypeCollection
        {
            get { return null; }
        }

        public EventType GetEventTypeSingle(EventAdapterService eventAdapterService, int statementId)
        {
            return null;
        }

        public EventBean EvaluateGetEventBean(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            return null;
        }

        public ICollection<object> EvaluateEventGetROCollectionScalar(EventBean @event, ExprEvaluatorContext context)
        {
            return null;
        }

        public EventBean EvaluateEventGetEventBean(EventBean @event, ExprEvaluatorContext context)
        {
            return null;
        }
    }
} // end of namespace
