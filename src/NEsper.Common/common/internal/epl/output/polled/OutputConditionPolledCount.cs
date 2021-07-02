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
        private readonly OutputConditionPolledCountState _state;
        private readonly VariableReader _optionalVariableReader;

        public OutputConditionPolledCount(
            OutputConditionPolledCountState state,
            VariableReader optionalVariableReader)
        {
            _state = state;
            _optionalVariableReader = optionalVariableReader;
        }

        OutputConditionPolledState OutputConditionPolled.State => State;

        public OutputConditionPolledCountState State {
            get => _state;
        }

        public bool UpdateOutputCondition(
            int newDataCount,
            int oldDataCount)
        {
            object value = _optionalVariableReader?.Value;
            if (value != null) {
                _state.EventRate = value.AsInt64();
            }

            _state.NewEventsCount = _state.NewEventsCount + newDataCount;
            _state.OldEventsCount = _state.OldEventsCount + oldDataCount;

            if (IsSatisfied() || _state.IsFirst) {
                if ((ExecutionPathDebugLog.IsDebugEnabled) && (Log.IsDebugEnabled)) {
                    Log.Debug(".updateOutputCondition() condition satisfied");
                }

                _state.IsFirst = false;
                _state.NewEventsCount = 0;
                _state.OldEventsCount = 0;
                return true;
            }

            return false;
        }

        private bool IsSatisfied()
        {
            return (_state.NewEventsCount >= _state.EventRate) || (_state.OldEventsCount >= _state.EventRate);
        }

        private static readonly ILog Log = LogManager.GetLogger(typeof(OutputConditionPolledCount));
    }
} // end of namespace