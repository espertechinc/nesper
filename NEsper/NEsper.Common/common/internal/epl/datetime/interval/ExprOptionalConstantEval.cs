///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.datetime.interval
{
    public class ExprOptionalConstantEval
    {
        public static readonly IntervalDeltaExprEvaluatorMax MAXEVAL = new IntervalDeltaExprEvaluatorMax();

        public ExprOptionalConstantEval(IntervalDeltaExprEvaluator evaluator, long? optionalConstant)
        {
            Evaluator = evaluator;
            OptionalConstant = optionalConstant;
        }

        public IntervalDeltaExprEvaluator Evaluator { get; }

        public long? OptionalConstant { get; }

        public static ExprOptionalConstantEval Make(long maxValue)
        {
            return new ExprOptionalConstantEval(MAXEVAL, maxValue);
        }

        public class IntervalDeltaExprEvaluatorMax : IntervalDeltaExprEvaluator
        {
            public long Evaluate(
                long reference, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
            {
                return Int64.MaxValue;
            }
        }
    }
} // end of namespace