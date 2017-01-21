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
    public class EnumEvalAggregateScalar : EnumEvalAggregateBase, EnumEval {
    
        private readonly ObjectArrayEventType _evalEventType;
    
        public EnumEvalAggregateScalar(ExprEvaluator initialization, ExprEvaluator innerExpression, int streamNumLambda, ObjectArrayEventType resultEventType, ObjectArrayEventType evalEventType)
            : base(initialization, innerExpression, streamNumLambda, resultEventType)
        {
            _evalEventType = evalEventType;
        }

        public Object EvaluateEnumMethod(EventBean[] eventsLambda, ICollection<object> target, bool isNewData, ExprEvaluatorContext context)
        {
            var initializationValue = Initialization.Evaluate(new EvaluateParams(eventsLambda, isNewData, context));
    
            if (target.IsEmpty()) {
                return initializationValue;
            }
    
            var resultEvent = new ObjectArrayEventBean(new Object[1], ResultEventType);
            var evalEvent = new ObjectArrayEventBean(new Object[1], _evalEventType);
    
            foreach (Object next in target) {
    
                resultEvent.Properties[0] = initializationValue;
                evalEvent.Properties[0] = next;
                eventsLambda[StreamNumLambda] = resultEvent;
                eventsLambda[StreamNumLambda + 1] = evalEvent;
    
                initializationValue = InnerExpression.Evaluate(new EvaluateParams(eventsLambda, isNewData, context));
            }
    
            return initializationValue;
        }
    }
}
