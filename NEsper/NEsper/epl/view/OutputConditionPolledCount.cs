///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.variable;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.view
{
    /// <summary>
    /// Output limit condition that is satisfied when either
    /// the total number of new events arrived or the total number
    /// of old events arrived is greater than a preset value.
    /// </summary>
    public sealed class OutputConditionPolledCount : OutputConditionPolled
    {
        private readonly OutputConditionPolledCountFactory _factory;
        private readonly OutputConditionPolledCountState _state;
        private readonly VariableReader _optionalVariableReader;

        public OutputConditionPolledCount(OutputConditionPolledCountFactory factory, OutputConditionPolledCountState state, VariableReader optionalVariableReader)
        {
            _factory = factory;
            _state = state;
            _optionalVariableReader = optionalVariableReader;
        }

        public OutputConditionPolledState State => _state;

        public bool UpdateOutputCondition(int newDataCount, int oldDataCount)
        {
            if (_optionalVariableReader != null)
            {
                var value = _optionalVariableReader.Value;
                if (value != null)
                {
                    _state.EventRate = value.AsLong();
                }
            }

            _state.NewEventsCount = _state.NewEventsCount + newDataCount;
            _state.OldEventsCount = _state.OldEventsCount + oldDataCount;

            if (IsSatisfied || _state.IsFirst)
            {
                if ((ExecutionPathDebugLog.IsEnabled) && (log.IsDebugEnabled))
                {
                    log.Debug(".updateOutputCondition() condition satisfied");
                }
                _state.IsFirst = false;
                _state.NewEventsCount = 0;
                _state.OldEventsCount = 0;
                return true;
            }

            return false;
        }

        private bool IsSatisfied => (_state.NewEventsCount >= _state.EventRate) || (_state.OldEventsCount >= _state.EventRate);

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
