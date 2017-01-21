///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.time
{
    public class ExprTimePeriodEvalDeltaNonConstMsec : ExprTimePeriodEvalDeltaNonConst
    {
        private readonly ExprTimePeriodImpl _exprTimePeriod;
    
        public ExprTimePeriodEvalDeltaNonConstMsec(ExprTimePeriodImpl exprTimePeriod) {
            this._exprTimePeriod = exprTimePeriod;
        }
    
        public long DeltaMillisecondsAdd(long currentTime, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
            double d = _exprTimePeriod.EvaluateAsSeconds(eventsPerStream, isNewData, context);
            return (long) Math.Round(d * 1000d);
        }
    
        public long DeltaMillisecondsSubtract(long currentTime, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
            return DeltaMillisecondsAdd(currentTime, eventsPerStream, isNewData, context);
        }
    
        public long DeltaMillisecondsUseEngineTime(EventBean[] eventsPerStream, AgentInstanceContext agentInstanceContext) {
            return DeltaMillisecondsAdd(0, eventsPerStream, true, agentInstanceContext);
        }
    
        public ExprTimePeriodEvalDeltaResult DeltaMillisecondsAddWReference(long current, long reference, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
            long msec = DeltaMillisecondsAdd(current, eventsPerStream, isNewData, context);
            return new ExprTimePeriodEvalDeltaResult(
                ExprTimePeriodEvalDeltaConstMsec.DeltaMillisecondsAddWReference(current, reference, msec), 
                reference);
        }
    }
}
