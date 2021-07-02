///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.collection;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    /// <summary>
    ///     Interface for evaluating of an event re. enumeration.
    /// </summary>
    public interface ExprEnumerationGivenEvent
    {
        ICollection<EventBean> EvaluateEventGetROCollectionEvents(
            EventBean @event,
            ExprEvaluatorContext context);

        ICollection<object> EvaluateEventGetROCollectionScalar(
            EventBean @event,
            ExprEvaluatorContext context);

        EventBean EvaluateEventGetEventBean(
            EventBean @event,
            ExprEvaluatorContext context);
    }
    
        public class ProxyExprEnumerationGivenEvent : ExprEnumerationGivenEvent
    {
        public delegate FlexCollection EvaluateEventGetROCollectionEventsFunc(
            EventBean @event,
            ExprEvaluatorContext context);

        public delegate FlexCollection EvaluateEventGetROCollectionScalarFunc(
            EventBean @event,
            ExprEvaluatorContext context);

        public delegate EventBean EvaluateEventGetEventBeanFunc(
            EventBean @event,
            ExprEvaluatorContext context);

        public EvaluateEventGetROCollectionEventsFunc procEvaluateEventGetRoCollectionEvents;
        public EvaluateEventGetROCollectionScalarFunc procEvaluateEventGetRoCollectionScalar;
        public EvaluateEventGetEventBeanFunc procEvaluateEventGetEventBean;

        public ProxyExprEnumerationGivenEvent()
        {
        }

        public ProxyExprEnumerationGivenEvent(
            EvaluateEventGetROCollectionEventsFunc procEvaluateEventGetROCollectionEvents,
            EvaluateEventGetROCollectionScalarFunc procEvaluateEventGetROCollectionScalar,
            EvaluateEventGetEventBeanFunc procEvaluateEventGetEventBean)
        {
            procEvaluateEventGetRoCollectionEvents = procEvaluateEventGetROCollectionEvents;
            procEvaluateEventGetRoCollectionScalar = procEvaluateEventGetROCollectionScalar;
            this.procEvaluateEventGetEventBean = procEvaluateEventGetEventBean;
        }

        public ICollection<EventBean> EvaluateEventGetROCollectionEvents(
            EventBean @event,
            ExprEvaluatorContext context)
        {
            return procEvaluateEventGetRoCollectionEvents(@event, context).EventBeanCollection;
        }

        public ICollection<object> EvaluateEventGetROCollectionScalar(
            EventBean @event,
            ExprEvaluatorContext context)
        {
            return procEvaluateEventGetRoCollectionScalar(@event, context).ValueCollection;
        }

        public EventBean EvaluateEventGetEventBean(
            EventBean @event,
            ExprEvaluatorContext context)
        {
            return procEvaluateEventGetEventBean(@event, context);
        }
    }
} // end of namespace