///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    /// <summary>
    ///     Interface for evaluating of an event tuple.
    /// </summary>
    public interface ExprEnumerationEval
    {
        ICollection<EventBean> EvaluateGetROCollectionEvents(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context);

        ICollection<object> EvaluateGetROCollectionScalar(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context);

        EventBean EvaluateGetEventBean(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context);
    }

    public class ProxyExprEnumerationEval : ExprEnumerationEval
    {
        public Func<EventBean[], bool, ExprEvaluatorContext, ICollection<EventBean>> procEvaluateGetRoCollectionEvents;
        public Func<EventBean[], bool, ExprEvaluatorContext, ICollection<object>> procEvaluateGetRoCollectionScalar;
        public Func<EventBean[], bool, ExprEvaluatorContext, EventBean> procEvaluateGetEventBean;

        public ICollection<EventBean> EvaluateGetROCollectionEvents(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return procEvaluateGetRoCollectionEvents(
                eventsPerStream,
                isNewData,
                context);
        }

        public ICollection<object> EvaluateGetROCollectionScalar(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return procEvaluateGetRoCollectionScalar(
                eventsPerStream,
                isNewData,
                context);
        }

        public EventBean EvaluateGetEventBean(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return procEvaluateGetEventBean(
                eventsPerStream,
                isNewData,
                context);
        }
    }
} // end of namespace