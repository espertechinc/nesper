///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
        private bool _isAllConstant;
        private MatchedEventConvertor _optionalConvertor;
        private int _scheduleCallbackId = -1;
        private TimerScheduleSpecCompute _scheduleComputer;
        private TimerScheduleSpec _spec;

        public bool IsAllConstant {
            get => _isAllConstant;
            set => _isAllConstant = value;
        }

        public TimerScheduleSpecCompute ScheduleComputer {
            get => _scheduleComputer;
            set => _scheduleComputer = value;
        }

        public MatchedEventConvertor OptionalConvertor {
            get => _optionalConvertor;
            set => _optionalConvertor = value;
        }

        public int ScheduleCallbackId {
            get => _scheduleCallbackId;
            set => _scheduleCallbackId = value;
        }

        public EventObserver MakeObserver(
            PatternAgentInstanceContext context,
            MatchedEventMap beginState,
            ObserverEventEvaluator observerEventEvaluator,
            object observerState,
            bool isFilterChildNonQuitting)
        {
            if (_isAllConstant) {
                try {
                    _spec = _scheduleComputer.Compute(
                        _optionalConvertor,
                        beginState,
                        context.AgentInstanceContext,
                        context.AgentInstanceContext.ImportServiceRuntime.TimeZone,
                        context.AgentInstanceContext.ImportServiceRuntime.TimeAbacus);
                }
                catch (ScheduleParameterException ex) {
                    throw new EPException(ex.Message, ex);
                }
            }

            return new TimerScheduleObserver(
                ComputeSpecDynamic(beginState, context),
                beginState,
                observerEventEvaluator,
                isFilterChildNonQuitting);
        }

        public bool IsNonRestarting => true;

        public TimerScheduleSpec ComputeSpecDynamic(
            MatchedEventMap beginState,
            PatternAgentInstanceContext context)
        {
            if (_spec != null) {
                return _spec;
            }

            try {
                return _scheduleComputer.Compute(
                    _optionalConvertor,
                    beginState,
                    context.AgentInstanceContext,
                    context.StatementContext.ImportServiceRuntime.TimeZone,
                    context.StatementContext.ImportServiceRuntime.TimeAbacus);
            }
            catch (ScheduleParameterException e) {
                throw new EPException("Error computing iso8601 schedule specification: " + e.Message, e);
            }
        }
    }
} // end of namespace