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
using com.espertech.esper.events.arr;

namespace com.espertech.esper.epl.enummethod.eval
{
    public class EnumEvalWhereIndexEvents : EnumEvalBaseIndex, EnumEval {
    
        public EnumEvalWhereIndexEvents(ExprEvaluator innerExpression, int streamNumLambda, ObjectArrayEventType indexEventType)
                    : base(innerExpression, streamNumLambda, indexEventType)
        {
        }
    
        public override Object EvaluateEnumMethod(EventBean[] eventsLambda, ICollection<object> target, bool isNewData, ExprEvaluatorContext context) {
            if (target.IsEmpty()) {
                return target;
            }
    
            var result = new LinkedList<Object>();
            var indexEvent = new ObjectArrayEventBean(new Object[1], IndexEventType);
    
            int count = -1;
            foreach (EventBean next in target) {
                count++;
    
                indexEvent.Properties[0] = count;
                eventsLambda[StreamNumLambda] = next;
                eventsLambda[StreamNumLambda + 1] = indexEvent;
    
                var pass = (bool?) InnerExpression.Evaluate(new EvaluateParams(eventsLambda, isNewData, context));
                if (!pass.HasValue || !pass.Value) {
                    continue;
                }
    
                result.AddLast(next);
            }
    
            return result;
        }
    }
}
