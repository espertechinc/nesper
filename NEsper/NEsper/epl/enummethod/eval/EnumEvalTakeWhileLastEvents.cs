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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;


namespace com.espertech.esper.epl.enummethod.eval
{
    public class EnumEvalTakeWhileLastEvents
        : EnumEvalBase
        , EnumEval
    {
    
        public EnumEvalTakeWhileLastEvents(ExprEvaluator innerExpression, int streamCountIncoming) 
            : base(innerExpression, streamCountIncoming)
        {
        }

        public object EvaluateEnumMethod(EventBean[] eventsLambda, ICollection<object> target, bool isNewData, ExprEvaluatorContext context)
        {
            if (target.IsEmpty()) {
                return target;
            }
    
            if (target.Count == 1) {
                EventBean item = target.OfType<EventBean>().FirstOrDefault();
                eventsLambda[StreamNumLambda] = item;

                Object pass = InnerExpression.Evaluate(new EvaluateParams(eventsLambda, isNewData, context));
                if (!pass.AsBoolean())
                {
                    return new EventBean[0];
                }
                return new[] {item};
            }

            var all = target.OfType<EventBean>().ToArray();
            var result = new LinkedList<EventBean>();
    
            for (int i = all.Length - 1; i >= 0; i--) {
                eventsLambda[StreamNumLambda] = all[i];

                Object pass = InnerExpression.Evaluate(new EvaluateParams(eventsLambda, isNewData, context));
                if (!pass.AsBoolean())
                {
                    break;
                }
    
                result.AddFirst(all[i]);
            }
    
            return result;
        }
    }
}
