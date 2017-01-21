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
using com.espertech.esper.compat;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.events.arr;

namespace com.espertech.esper.epl.enummethod.eval
{
    public class EnumEvalAverageScalarLambda
        : EnumEvalBase
        , EnumEval
    {
        private readonly ObjectArrayEventType _resultEventType;

        public EnumEvalAverageScalarLambda(ExprEvaluator innerExpression,
                                           int streamCountIncoming,
                                           ObjectArrayEventType resultEventType)
            : base(innerExpression, streamCountIncoming)
        {
            _resultEventType = resultEventType;
        }

        public Object EvaluateEnumMethod(EventBean[] eventsLambda,
                                         ICollection<object> target,
                                         bool isNewData,
                                         ExprEvaluatorContext context)
        {
            double sum = 0d;
            int count = 0;
            var resultEvent = new ObjectArrayEventBean(new Object[1], _resultEventType);
            ICollection<object> values = target;
            foreach (object next in values)
            {
                resultEvent.Properties[0] = next;
                eventsLambda[StreamNumLambda] = resultEvent;

                object num = InnerExpression.Evaluate(new EvaluateParams(eventsLambda, isNewData, context));
                if (num == null)
                {
                    continue;
                }
                count++;
                sum += num.AsDouble();
            }

            if (count == 0)
            {
                return null;
            }
            return sum/count;
        }
    }
}