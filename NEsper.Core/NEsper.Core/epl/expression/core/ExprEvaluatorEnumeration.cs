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
using com.espertech.esper.events;

namespace com.espertech.esper.epl.expression.core
{
    /// <summary>
    /// Interface for evaluating of an event tuple.
    /// </summary>
    public interface ExprEvaluatorEnumeration
    {
        EventType GetEventTypeCollection(EventAdapterService eventAdapterService, int statementId);
        ICollection<EventBean> EvaluateGetROCollectionEvents(EvaluateParams evaluateParams);

        Type ComponentTypeCollection { get; }
        ICollection<object> EvaluateGetROCollectionScalar(EvaluateParams evaluateParams);

        EventType GetEventTypeSingle(EventAdapterService eventAdapterService, int statementId);
        EventBean EvaluateGetEventBean(EvaluateParams evaluateParams);
    }
}
