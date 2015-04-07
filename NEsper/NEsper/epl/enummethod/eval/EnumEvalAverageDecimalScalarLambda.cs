///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.events.arr;

namespace com.espertech.esper.epl.enummethod.eval
{
    public class EnumEvalAverageDecimalScalarLambda
        : EnumEvalBase
        , EnumEval
    {
        private readonly ObjectArrayEventType _resultEventType;
        private readonly MathContext _optionalMathContext;

        public EnumEvalAverageDecimalScalarLambda(ExprEvaluator innerExpression, int streamCountIncoming, ObjectArrayEventType resultEventType, MathContext optionalMathContext)
            : base(innerExpression, streamCountIncoming)
        {
            _resultEventType = resultEventType;
            _optionalMathContext = optionalMathContext;
        }

        public Object EvaluateEnumMethod(EventBean[] eventsLambda, ICollection<object> target, bool isNewData, ExprEvaluatorContext context)
        {
            var agg = new AggregatorAvgDecimal(_optionalMathContext);
            var resultEvent = new ObjectArrayEventBean(new Object[1], _resultEventType);

            var values = target;
            foreach (Object next in values)
            {
                resultEvent.Properties[0] = next;
                eventsLambda[StreamNumLambda] = resultEvent;

                var num = InnerExpression.Evaluate(new EvaluateParams(eventsLambda, isNewData, context));
                if (num == null)
                {
                    continue;
                }
                agg.Enter(num);
            }

            return agg.Value;
        }
    }
}
