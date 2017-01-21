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
using com.espertech.esper.epl.rettype;

namespace com.espertech.esper.epl.expression.dot
{
    public interface ExprDotEvalRootChildInnerEval
    {
        object Evaluate(EvaluateParams evaluateParams);
        ICollection<EventBean> EvaluateGetROCollectionEvents(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context);
        ICollection<object> EvaluateGetROCollectionScalar(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context);
        EventBean EvaluateGetEventBean(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context);

        EventType EventTypeCollection { get; }
        EventType EventTypeSingle { get; }
        Type ComponentTypeCollection { get; }

        EPType TypeInfo { get; }
    }
}
