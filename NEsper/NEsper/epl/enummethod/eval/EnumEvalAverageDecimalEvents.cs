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
using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.enummethod.eval
{
    public class EnumEvalAverageDecimalEvents 
        : EnumEvalBase
        , EnumEval
    {
        private readonly MathContext _optionalMathContext;

        public EnumEvalAverageDecimalEvents(ExprEvaluator innerExpression, int streamCountIncoming, MathContext optionalMathContext)
            : base(innerExpression, streamCountIncoming)
        {
            _optionalMathContext = optionalMathContext;
        }

        public object EvaluateEnumMethod(EventBean[] eventsLambda, ICollection<object> target, bool isNewData, ExprEvaluatorContext context)
        {
            var agg = new AggregatorAvgDecimal(_optionalMathContext);
            foreach (EventBean next in target) {
                eventsLambda[StreamNumLambda] = next;

                var num = InnerExpression.Evaluate(new EvaluateParams(eventsLambda, isNewData, context));
                if (num == null) {
                    continue;
                }
                agg.Enter(num);
            }
    
            return agg.Value;
        }
    }
}
