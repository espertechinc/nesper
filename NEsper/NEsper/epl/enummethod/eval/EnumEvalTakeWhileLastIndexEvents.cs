///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    public class EnumEvalTakeWhileLastIndexEvents 
        : EnumEvalBaseIndex
        , EnumEval
    {
        public EnumEvalTakeWhileLastIndexEvents(ExprEvaluator innerExpression, int streamNumLambda, ObjectArrayEventType indexEventType)
            : base(innerExpression, streamNumLambda, indexEventType)
        {
        }
    
        public override Object EvaluateEnumMethod(EventBean[] eventsLambda, ICollection<object> target, bool isNewData, ExprEvaluatorContext context)
        {
            if (target.IsEmpty()) {
                return target;
            }
            var indexEvent = new ObjectArrayEventBean(new Object[1], IndexEventType);
            if (target.Count == 1)
            {
                EventBean item = (EventBean) target.First();
                indexEvent.Properties[0] = 0;
                eventsLambda[StreamNumLambda] = item;
                eventsLambda[StreamNumLambda + 1] = indexEvent;
    
                var pass = (bool?)InnerExpression.Evaluate(new EvaluateParams(eventsLambda, isNewData, context));
                if (!pass.GetValueOrDefault(false))
                {
                    return Collections.GetEmptyList<EventBean>();
                }
                return Collections.SingletonList(item);
            }
    
            var size = target.Count;
            var all = new EventBean[size];
            var count = 0;
            foreach (EventBean item in target) {
                all[count++] = item;
            }

            var result = new LinkedList<Object>();
            var index = 0;
            for (int i = all.Length - 1; i >= 0; i--) {
    
                indexEvent.Properties[0] = index++;
                eventsLambda[StreamNumLambda] = all[i];
                eventsLambda[StreamNumLambda + 1] = indexEvent;
    
                var pass = (bool?)InnerExpression.Evaluate(new EvaluateParams(eventsLambda, isNewData, context));
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
