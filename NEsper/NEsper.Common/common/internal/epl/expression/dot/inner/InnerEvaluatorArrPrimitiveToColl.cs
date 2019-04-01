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
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.dot.inner
{
    public class InnerEvaluatorArrPrimitiveToColl : ExprDotEvalRootChildInnerEval
    {
        private readonly ExprEvaluator rootEvaluator;

        public InnerEvaluatorArrPrimitiveToColl(ExprEvaluator rootEvaluator)
        {
            this.rootEvaluator = rootEvaluator;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var array = (Array) rootEvaluator.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
            if (array == null) {
                return null;
            }

            var len = array.Length;
            if (len == 0) {
                return Collections.GetEmptyList<object>();
            }

            if (len == 1) {
                return Collections.SingletonList(array.GetValue(0));
            }

            Deque<object> dq = new ArrayDeque<object>(len);
            for (var i = 0; i < len; i++) {
                dq.Add(array.GetValue(i));
            }

            return dq;
        }

        public ICollection<EventBean> EvaluateGetROCollectionEvents(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return null;
        }

        public ICollection<object> EvaluateGetROCollectionScalar(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return null;
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