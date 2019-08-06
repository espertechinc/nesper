///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.pattern.guard
{
    public class TimerWithinOrMaxCountGuardFactory : GuardFactory
    {
        private ExprEvaluator countEval;

        private PatternDeltaCompute deltaCompute;
        private MatchedEventConvertor optionalConvertor;

        public PatternDeltaCompute DeltaCompute {
            set => deltaCompute = value;
        }

        public MatchedEventConvertor OptionalConvertor {
            set => optionalConvertor = value;
        }

        public ExprEvaluator CountEval {
            set => countEval = value;
        }

        public int ScheduleCallbackId { get; set; } = -1;

        public Guard MakeGuard(
            PatternAgentInstanceContext context,
            MatchedEventMap beginState,
            Quitable quitable,
            object guardState)
        {
            return new TimerWithinOrMaxCountGuard(
                ComputeTime(beginState, context),
                ComputeNumCountTo(beginState, context),
                quitable);
        }

        public long ComputeTime(
            MatchedEventMap beginState,
            PatternAgentInstanceContext context)
        {
            return deltaCompute.ComputeDelta(beginState, context);
        }

        public int ComputeNumCountTo(
            MatchedEventMap beginState,
            PatternAgentInstanceContext context)
        {
            var events = optionalConvertor == null ? null : optionalConvertor.Invoke(beginState);
            var numCountToVal = PatternExpressionUtil.EvaluateChecked(
                "Timer-Within-Or-Max-Count guard",
                countEval,
                events,
                context.AgentInstanceContext);
            if (null == numCountToVal) {
                throw new EPException("Timer-within-or-max second parameter evaluated to a null-value");
            }

            return numCountToVal.AsInt();
        }
    }
} // end of namespace