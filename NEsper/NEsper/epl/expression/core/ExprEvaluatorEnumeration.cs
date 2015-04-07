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
using com.espertech.esper.events;

namespace com.espertech.esper.epl.expression.core
{
    /// <summary>
    /// Interface for evaluating of an event tuple.
    /// </summary>
    public interface ExprEvaluatorEnumeration
    {
        EventType GetEventTypeCollection(EventAdapterService eventAdapterService, String statementId);
        ICollection<EventBean> EvaluateGetROCollectionEvents(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context);

        Type ComponentTypeCollection { get; }
        ICollection<object> EvaluateGetROCollectionScalar(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context);

        EventType GetEventTypeSingle(EventAdapterService eventAdapterService, String statementId);
        EventBean EvaluateGetEventBean(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context);
    }
}
