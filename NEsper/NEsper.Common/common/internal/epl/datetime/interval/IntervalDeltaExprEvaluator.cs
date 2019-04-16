///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
    public interface IntervalDeltaExprEvaluator
    {
        long Evaluate(
            long reference,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context);
    }

    public class ProxyIntervalDeltaExprEvaluator : IntervalDeltaExprEvaluator
    {
        public Func<long, EventBean[], bool, ExprEvaluatorContext, long> ProcEvaluate { get; set; }

        public long Evaluate(
            long reference,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return ProcEvaluate.Invoke(
                reference, eventsPerStream, isNewData, context);
        }
    }
}