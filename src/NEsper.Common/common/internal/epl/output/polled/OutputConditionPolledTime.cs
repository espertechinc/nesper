///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.output.polled
{
    public sealed class OutputConditionPolledTime : OutputConditionPolled
    {
        private readonly OutputConditionPolledTimeFactory _factory;
        private readonly ExprEvaluatorContext _context;
        private readonly OutputConditionPolledTimeState _state;

        public OutputConditionPolledTime(
            OutputConditionPolledTimeFactory factory,
            ExprEvaluatorContext context,
            OutputConditionPolledTimeState state)
        {
            _factory = factory;
            _context = context;
            _state = state;
        }

        public OutputConditionPolledState State {
            get => _state;
        }

        public bool UpdateOutputCondition(
            int newEventsCount,
            int oldEventsCount)
        {
            // If we pull the interval from a variable, then we may need to reschedule
            long msecIntervalSize = _factory.timePeriodCompute.DeltaAdd(_context.TimeProvider.Time, null, true, _context);

            long current = _context.TimeProvider.Time;
            if (_state.LastUpdate == null || current - _state.LastUpdate >= msecIntervalSize) {
                _state.LastUpdate = current;
                return true;
            }

            return false;
        }
    }
} // end of namespace