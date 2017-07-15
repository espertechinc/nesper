///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.time
{
    public class ExprTimePeriodEvalDeltaConstFactoryMsec : ExprTimePeriodEvalDeltaConstFactory {
        private readonly ExprEvaluator secondsEvaluator;
        private readonly TimeAbacus timeAbacus;
    
        public ExprTimePeriodEvalDeltaConstFactoryMsec(ExprEvaluator secondsEvaluator, TimeAbacus timeAbacus) {
            this.secondsEvaluator = secondsEvaluator;
            this.timeAbacus = timeAbacus;
        }
    
        public ExprTimePeriodEvalDeltaConst Make(string validateMsgName, string validateMsgValue, AgentInstanceContext agentInstanceContext) {
            Number time = (Number) secondsEvaluator.Evaluate(null, true, agentInstanceContext);
            if (!ExprTimePeriodUtil.ValidateTime(time, agentInstanceContext.StatementContext.TimeAbacus)) {
                throw new EPException(ExprTimePeriodUtil.GetTimeInvalidMsg(validateMsgName, validateMsgValue, time));
            }
            return new ExprTimePeriodEvalDeltaConstGivenDelta(timeAbacus.DeltaForSecondsNumber(time));
        }
    }
} // end of namespace
