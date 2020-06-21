///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.core;

namespace com.espertech.esper.common.@internal.epl.expression.dot.inner
{
    public class InnerEvaluatorEnumerableEventBean : ExprDotEvalRootChildInnerEval
    {
        private readonly ExprEnumerationEval rootLambdaEvaluator;
        private readonly EventType eventType;

        public InnerEvaluatorEnumerableEventBean(
            ExprEnumerationEval rootLambdaEvaluator,
            EventType eventType)
        {
            this.rootLambdaEvaluator = rootLambdaEvaluator;
            this.eventType = eventType;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return rootLambdaEvaluator.EvaluateGetEventBean(eventsPerStream, isNewData, context);
        }

        public ICollection<EventBean> EvaluateGetROCollectionEvents(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return rootLambdaEvaluator.EvaluateGetROCollectionEvents(eventsPerStream, isNewData, context);
        }

        public ICollection<object> EvaluateGetROCollectionScalar(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return rootLambdaEvaluator.EvaluateGetROCollectionScalar(eventsPerStream, isNewData, context);
        }

        public EventBean EvaluateGetEventBean(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return rootLambdaEvaluator.EvaluateGetEventBean(eventsPerStream, isNewData, context);
        }
    }
} // end of namespace