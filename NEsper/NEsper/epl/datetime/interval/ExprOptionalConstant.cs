///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.datetime.interval
{
    public class ExprOptionalConstant
    {
        public readonly static IntervalDeltaExprEvaluatorMax MAXEVAL = 
            new IntervalDeltaExprEvaluatorMax();

        private readonly IntervalDeltaExprEvaluator _evaluator;
        private readonly long? _optionalConstant;

        public ExprOptionalConstant(IntervalDeltaExprEvaluator evaluator, long? optionalConstant)
        {
            _evaluator = evaluator;
            _optionalConstant = optionalConstant;
        }

        public long? OptionalConstant
        {
            get { return _optionalConstant; }
        }

        public IntervalDeltaExprEvaluator Evaluator
        {
            get { return _evaluator; }
        }

        public static ExprOptionalConstant Make(long maxValue)
        {
            return new ExprOptionalConstant(MAXEVAL, maxValue);
        }

        public class IntervalDeltaExprEvaluatorMax : IntervalDeltaExprEvaluator
        {
            public long Evaluate(long reference, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
            {
                return long.MaxValue;
            }
        }
    }
}
