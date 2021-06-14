///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.output.polled
{
    /// <summary>
    /// Output limit condition that is satisfied when either
    /// the total number of new events arrived or the total number
    /// of old events arrived is greater than a preset value.
    /// </summary>
    public sealed class OutputConditionPolledCount : OutputConditionPolled
    {
        private readonly OutputConditionPolledCountState state;
        private readonly VariableReader optionalVariableReader;

        public OutputConditionPolledCount(
            OutputConditionPolledCountState state,
            VariableReader optionalVariableReader)
        {
            this.state = state;
            this.optionalVariableReader = optionalVariableReader;
        }

        OutputConditionPolledState OutputConditionPolled.State => State;

        public OutputConditionPolledCountState State {
            get => state;
        }

        public bool UpdateOutputCondition(
            int newDataCount,
            int oldDataCount)
        {
            object value = optionalVariableReader?.Value;
            if (value != null) {
                state.EventRate = value.AsInt64();
            }

            state.NewEventsCount = state.NewEventsCount + newDataCount;
            state.OldEventsCount = state.OldEventsCount + oldDataCount;

            if (IsSatisfied() || state.IsFirst) {
                if ((ExecutionPathDebugLog.IsDebugEnabled) && (log.IsDebugEnabled)) {
                    log.Debug(".updateOutputCondition() condition satisfied");
                }

                state.IsFirst = false;
                state.NewEventsCount = 0;
                state.OldEventsCount = 0;
                return true;
            }

            return false;
        }

        private bool IsSatisfied()
        {
            return (state.NewEventsCount >= state.EventRate) || (state.OldEventsCount >= state.EventRate);
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(OutputConditionPolledCount));
    }
} // end of namespace