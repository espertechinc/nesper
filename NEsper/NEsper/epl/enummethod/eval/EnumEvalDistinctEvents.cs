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
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.enummethod.eval
{
    public class EnumEvalDistinctEvents
        : EnumEvalBase
        , EnumEval
    {
        public EnumEvalDistinctEvents(ExprEvaluator innerExpression, int streamCountIncoming)
            : base(innerExpression, streamCountIncoming)
        {
        }

        public Object EvaluateEnumMethod(
            EventBean[] eventsLambda,
            ICollection<object> target,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var beans = (ICollection<EventBean>) target;
            if (beans.IsEmpty() || beans.Count == 1)
            {
                return beans;
            }

            var evaluateParams = new EvaluateParams(eventsLambda, isNewData, context);
            var distinct = new LinkedHashMap<IComparable, EventBean>();
            foreach (var next in beans)
            {
                eventsLambda[StreamNumLambda] = next;

                var comparable = (IComparable) InnerExpression.Evaluate(evaluateParams);
                if (!distinct.ContainsKey(comparable))
                {
                    distinct.Put(comparable, next);
                }
            }

            return distinct.Values;
        }
    }
}