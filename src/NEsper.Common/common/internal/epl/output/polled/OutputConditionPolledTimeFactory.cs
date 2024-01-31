///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.time.eval;

namespace com.espertech.esper.common.@internal.epl.output.polled
{
    public sealed class OutputConditionPolledTimeFactory : OutputConditionPolledFactory
    {
        internal readonly TimePeriodCompute timePeriodCompute;

        public OutputConditionPolledTimeFactory(TimePeriodCompute timePeriodCompute)
        {
            this.timePeriodCompute = timePeriodCompute;
        }

        public OutputConditionPolled MakeNew(ExprEvaluatorContext exprEvaluatorContext)
        {
            return new OutputConditionPolledTime(this, exprEvaluatorContext, new OutputConditionPolledTimeState(null));
        }

        public OutputConditionPolled MakeFromState(
            ExprEvaluatorContext exprEvaluatorContext,
            OutputConditionPolledState state)
        {
            var timeState = (OutputConditionPolledTimeState)state;
            return new OutputConditionPolledTime(this, exprEvaluatorContext, timeState);
        }
    }
} // end of namespace