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
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.enummethod.dot
{
    public class PropertyExprEvaluatorEventSingle
        : ExprEvaluatorEnumeration
        , ExprEvaluatorEnumerationGivenEvent
    {
        private readonly int _streamId;
        private readonly EventType _fragmentType;
        private readonly EventPropertyGetter _getter;
    
        public PropertyExprEvaluatorEventSingle(int streamId, EventType fragmentType, EventPropertyGetter getter)
        {
            _streamId = streamId;
            _fragmentType = fragmentType;
            _getter = getter;
        }
    
        public EventBean EvaluateGetEventBean(EvaluateParams evaluateParams) {
            EventBean eventInQuestion = evaluateParams.EventsPerStream[_streamId];
            if (eventInQuestion == null) {
                return null;
            }
            return (EventBean) _getter.GetFragment(eventInQuestion);
        }
    
        public EventBean EvaluateEventGetEventBean(EventBean @event, ExprEvaluatorContext context) {
            if (@event == null) {
                return null;
            }
            return (EventBean) _getter.GetFragment(@event);
        }
    
        public EventType GetEventTypeCollection(EventAdapterService eventAdapterService, int statementId) {
            return _fragmentType;
        }

        public Type ComponentTypeCollection
        {
            get { return null; }
        }

        public EventType GetEventTypeSingle(EventAdapterService eventAdapterService, int statementId) {
            return null;
        }
    
        public ICollection<EventBean> EvaluateEventGetROCollectionEvents(EventBean @event, ExprEvaluatorContext context) {
            return null;
        }

        public ICollection<object> EvaluateEventGetROCollectionScalar(EventBean @event, ExprEvaluatorContext context)
        {
            return null;
        }
    
        public ICollection<EventBean> EvaluateGetROCollectionEvents(EvaluateParams evaluateParams) {
            return null;
        }

        public ICollection<object> EvaluateGetROCollectionScalar(EvaluateParams evaluateParams)
        {
            return null;
        }
    }
}
