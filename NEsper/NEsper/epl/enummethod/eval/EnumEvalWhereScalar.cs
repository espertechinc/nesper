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
    public class EnumEvalWhereScalar 
        : EnumEvalBaseScalar
        , EnumEval
    {
        public EnumEvalWhereScalar(ExprEvaluator innerExpression, int streamCountIncoming, ObjectArrayEventType type)
            : base(innerExpression, streamCountIncoming, type)
        {
        }
    
        public override Object EvaluateEnumMethod(EventBean[] eventsLambda, ICollection<object> target, bool isNewData, ExprEvaluatorContext context)
        {
            if (target.IsEmpty()) {
                return target;
            }
    
            var result = new LinkedList<Object>();
            var evalEvent = new ObjectArrayEventBean(new Object[1], Type);
    
            foreach (Object next in target) {
    
                evalEvent.Properties[0] = next;
                eventsLambda[StreamNumLambda] = evalEvent;
    
                var pass = (bool?) InnerExpression.Evaluate(new EvaluateParams(eventsLambda, isNewData, context));
                if (!pass.GetValueOrDefault(false)) {
                    continue;
                }
    
                result.AddLast(next);
            }
    
            return result;
        }
    }
}
