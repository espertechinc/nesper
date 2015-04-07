///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.enummethod.eval
{
    public class EnumEvalAverageEvents 
        : EnumEvalBase
        , EnumEval
    {
        public EnumEvalAverageEvents(ExprEvaluator innerExpression, int streamCountIncoming)
            : base(innerExpression, streamCountIncoming)
        {
        }

        public object EvaluateEnumMethod(EventBean[] eventsLambda, ICollection<object> target, bool isNewData, ExprEvaluatorContext context)
        {
            double sum = 0d;
            int count = 0;
    
            var beans = target;
            foreach (EventBean next in target) {
                eventsLambda[StreamNumLambda] = next;

                var num = InnerExpression.Evaluate(new EvaluateParams(eventsLambda, isNewData, context));
                if (num == null) {
                    continue;
                }
                count++;
                sum += num.AsDouble();
            }
    
            if (count == 0) {
                return null;
            }
            return sum / count;
        }
    }
}
