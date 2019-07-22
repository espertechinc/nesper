///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.filterspec;

namespace com.espertech.esper.common.@internal.epl.pattern.observer
{
    /// <summary>
    ///     Factory for making observer instances.
    /// </summary>
    public class TimerIntervalObserverFactory : ObserverFactory
    {
        internal PatternDeltaCompute deltaCompute;

        internal int scheduleCallbackId;

        public PatternDeltaCompute DeltaCompute {
            get => deltaCompute;
            set => deltaCompute = value;
        }

        public int ScheduleCallbackId {
            get => scheduleCallbackId;
            set => scheduleCallbackId = value;
        }

        public EventObserver MakeObserver(
            PatternAgentInstanceContext context,
            MatchedEventMap beginState,
            ObserverEventEvaluator observerEventEvaluator,
            object observerState,
            bool isFilterChildNonQuitting)
        {
            return new TimerIntervalObserver(
                deltaCompute.ComputeDelta(beginState, context),
                beginState,
                observerEventEvaluator);
        }

        public bool IsNonRestarting => false;
    }
} // end of namespace