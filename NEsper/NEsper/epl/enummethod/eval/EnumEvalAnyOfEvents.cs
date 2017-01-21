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
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.enummethod.eval
{
    public class EnumEvalAnyOfEvents 
        : EnumEvalBase
        , EnumEval 
    {
        public EnumEvalAnyOfEvents(ExprEvaluator innerExpression, int streamCountIncoming)
            : base(innerExpression, streamCountIncoming)
        {
        }

        public object EvaluateEnumMethod(EventBean[] eventsLambda, ICollection<object> target, bool isNewData, ExprEvaluatorContext context)
        {
            if (target.IsEmpty()) {
                return false;
            }
    
            foreach (EventBean next in target) {
                eventsLambda[StreamNumLambda] = next;

                Object pass = InnerExpression.Evaluate(new EvaluateParams(eventsLambda, isNewData, context));
                if (pass.AsBoolean()) {
                    return true;
                }
            }
    
            return false;
        }
    }
}
