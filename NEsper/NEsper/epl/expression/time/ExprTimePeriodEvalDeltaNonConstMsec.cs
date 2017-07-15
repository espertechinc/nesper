///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.time
{
    public class ExprTimePeriodEvalDeltaNonConstMsec : ExprTimePeriodEvalDeltaNonConst
    {
        private readonly ExprTimePeriodImpl _exprTimePeriod;

        public ExprTimePeriodEvalDeltaNonConstMsec(ExprTimePeriodImpl exprTimePeriod)
        {
            this._exprTimePeriod = exprTimePeriod;
        }

        public long DeltaAdd(
            long currentTime,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            double d = _exprTimePeriod.EvaluateAsSeconds(eventsPerStream, isNewData, context);
            return _exprTimePeriod.TimeAbacus.DeltaForSecondsDouble(d);
        }

        public long DeltaSubtract(
            long currentTime,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return DeltaAdd(currentTime, eventsPerStream, isNewData, context);
        }

        public long DeltaUseEngineTime(EventBean[] eventsPerStream, AgentInstanceContext agentInstanceContext)
        {
            return DeltaAdd(0, eventsPerStream, true, agentInstanceContext);
        }

        public ExprTimePeriodEvalDeltaResult DeltaAddWReference(
            long current,
            long reference,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            long msec = DeltaAdd(current, eventsPerStream, isNewData, context);
            return new ExprTimePeriodEvalDeltaResult(
                ExprTimePeriodEvalDeltaConstGivenDelta.DeltaAddWReference(current, reference, msec), reference);
        }
    }
} // end of namespace
