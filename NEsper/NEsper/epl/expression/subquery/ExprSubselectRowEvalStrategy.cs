///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.subquery
{
    public interface ExprSubselectRowEvalStrategy
    {
        object Evaluate(EventBean[] eventsPerStream, bool newData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext exprEvaluatorContext, ExprSubselectRowNode exprSubselectRowNode);
        ICollection<EventBean> EvaluateGetCollEvents(EventBean[] eventsPerStream, bool newData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext context, ExprSubselectRowNode parent);
        ICollection<object> EvaluateGetCollScalar(EventBean[] eventsPerStream, bool newData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext context, ExprSubselectRowNode parent);
        object[] TypableEvaluate(EventBean[] eventsPerStream, bool newData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext exprEvaluatorContext, ExprSubselectRowNode parent);
        object[][] TypableEvaluateMultirow(EventBean[] eventsPerStream, bool newData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext exprEvaluatorContext, ExprSubselectRowNode parent);
        EventBean EvaluateGetEventBean(EventBean[] eventsPerStream, bool newData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext exprEvaluatorContext, ExprSubselectRowNode exprSubselectRowNode);
    }
}
