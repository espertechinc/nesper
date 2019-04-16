///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.schedule;

namespace com.espertech.esper.common.@internal.epl.pattern.observer
{
    /// <summary>
    ///     Factory for ISO8601 repeating interval observers that indicate truth when a time point was reached.
    /// </summary>
    public class TimerScheduleObserverFactory : ObserverFactory
    {
        private bool isAllConstant;
        private MatchedEventConvertor optionalConvertor;
        internal int scheduleCallbackId = -1;
        private TimerScheduleSpecCompute scheduleComputer;
        private TimerScheduleSpec spec;

        public bool IsAllConstant {
            set => isAllConstant = value;
        }

        public TimerScheduleSpecCompute ScheduleComputer {
            set => scheduleComputer = value;
        }

        public MatchedEventConvertor OptionalConvertor {
            set => optionalConvertor = value;
        }

        public int ScheduleCallbackId {
            set => scheduleCallbackId = value;
        }

        public EventObserver MakeObserver(
            PatternAgentInstanceContext context,
            MatchedEventMap beginState,
            ObserverEventEvaluator observerEventEvaluator,
            object observerState,
            bool isFilterChildNonQuitting)
        {
            if (isAllConstant) {
                try {
                    spec = scheduleComputer.Compute(
                        optionalConvertor, beginState, context.AgentInstanceContext,
                        context.AgentInstanceContext.ImportServiceRuntime.TimeZone,
                        context.AgentInstanceContext.ImportServiceRuntime.TimeAbacus);
                }
                catch (ScheduleParameterException ex) {
                    throw new EPException(ex.Message, ex);
                }
            }

            return new TimerScheduleObserver(
                ComputeSpecDynamic(beginState, context), beginState, observerEventEvaluator, isFilterChildNonQuitting);
        }

        public bool IsNonRestarting => true;

        public TimerScheduleSpec ComputeSpecDynamic(
            MatchedEventMap beginState,
            PatternAgentInstanceContext context)
        {
            if (spec != null) {
                return spec;
            }

            try {
                return scheduleComputer.Compute(
                    optionalConvertor, beginState, context.AgentInstanceContext,
                    context.StatementContext.ImportServiceRuntime.TimeZone,
                    context.StatementContext.ImportServiceRuntime.TimeAbacus);
            }
            catch (ScheduleParameterException e) {
                throw new EPException("Error computing iso8601 schedule specification: " + e.Message, e);
            }
        }
    }
} // end of namespace