///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.events.arr;

namespace com.espertech.esper.epl.enummethod.eval
{
    public class EnumEvalTakeWhileLastIndexScalar
        : EnumEvalBaseScalarIndex
        , EnumEval
    {
        public EnumEvalTakeWhileLastIndexScalar(ExprEvaluator innerExpression, int streamNumLambda, ObjectArrayEventType evalEventType, ObjectArrayEventType indexEventType)
            : base(innerExpression, streamNumLambda, evalEventType, indexEventType)
        {
        }

        public override Object EvaluateEnumMethod(EventBean[] eventsLambda, ICollection<object> target, bool isNewData, ExprEvaluatorContext context)
        {
            if (target.IsEmpty())
            {
                return target;
            }

            var evalEvent = new ObjectArrayEventBean(new Object[1], EvalEventType);
            var indexEvent = new ObjectArrayEventBean(new Object[1], IndexEventType);

            if (target.Count == 1)
            {
                Object item = target.First();

                evalEvent.Properties[0] = item;
                eventsLambda[StreamNumLambda] = evalEvent;

                indexEvent.Properties[0] = 0;
                eventsLambda[StreamNumLambda + 1] = indexEvent;

                var pass = (bool?) InnerExpression.Evaluate(new EvaluateParams(eventsLambda, isNewData, context));
                if (!pass.GetValueOrDefault(false))
                {
                    return Collections.GetEmptyList<object>();
                }
                return Collections.SingletonList(item);
            }

            var size = target.Count;
            var all = new Object[size];
            var count = 0;
            foreach (Object item in target)
            {
                all[count++] = item;
            }

            var result = new LinkedList<Object>();
            int index = 0;
            for (int i = all.Length - 1; i >= 0; i--)
            {

                evalEvent.Properties[0] = all[i];
                eventsLambda[StreamNumLambda] = evalEvent;

                indexEvent.Properties[0] = index++;
                eventsLambda[StreamNumLambda + 1] = indexEvent;

                var pass = (bool?) InnerExpression.Evaluate(new EvaluateParams(eventsLambda, isNewData, context));
                if (!pass.GetValueOrDefault(false))
                {
                    break;
                }
                result.AddFirst(all[i]);
            }

            return result;
        }
    }
}
