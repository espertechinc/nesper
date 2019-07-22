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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.dot.inner
{
    public class InnerEvaluatorEnumerableScalarCollection : ExprDotEvalRootChildInnerEval
    {
        private readonly ExprEnumerationEval rootLambdaEvaluator;
        private readonly Type componentType;

        public InnerEvaluatorEnumerableScalarCollection(
            ExprEnumerationEval rootLambdaEvaluator,
            Type componentType)
        {
            this.rootLambdaEvaluator = rootLambdaEvaluator;
            this.componentType = componentType;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return rootLambdaEvaluator.EvaluateGetROCollectionScalar(eventsPerStream, isNewData, exprEvaluatorContext);
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
            return null;
        }
    }
} // end of namespace