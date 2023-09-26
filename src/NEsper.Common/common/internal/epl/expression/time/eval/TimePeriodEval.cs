///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.expression.time.eval
{
    public interface TimePeriodEval
    {
        TimePeriod TimePeriodEval(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext);
    }

    public class ProxyTimePeriodEval : TimePeriodEval
    {
        public delegate TimePeriod TimePeriodEvalFunc(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext);

        public ProxyTimePeriodEval()
        {
        }

        public ProxyTimePeriodEval(TimePeriodEvalFunc procTimePeriodEvalFunc)
        {
            ProcTimePeriodEvalFunc = procTimePeriodEvalFunc;
        }

        public TimePeriodEvalFunc ProcTimePeriodEvalFunc { get; set; }

        public TimePeriod TimePeriodEval(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return ProcTimePeriodEvalFunc.Invoke(eventsPerStream, isNewData, exprEvaluatorContext);
        }
    }
} // end of namespace