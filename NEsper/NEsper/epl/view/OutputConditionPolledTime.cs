///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.core.context.util;

namespace com.espertech.esper.epl.view
{
    public sealed class OutputConditionPolledTime : OutputConditionPolled
    {
        private readonly OutputConditionPolledTimeFactory _factory;
        private readonly AgentInstanceContext _context;
        private readonly OutputConditionPolledTimeState _state;

        public OutputConditionPolledTime(
            OutputConditionPolledTimeFactory factory,
            AgentInstanceContext context,
            OutputConditionPolledTimeState state)
        {
            _factory = factory;
            _context = context;
            _state = state;
        }

        public OutputConditionPolledState State => _state;

        public bool UpdateOutputCondition(int newEventsCount, int oldEventsCount)
        {
            // If we pull the interval from a variable, then we may need to reschedule
            var msecIntervalSize = _factory.TimePeriod.NonconstEvaluator().DeltaUseEngineTime(null, _context);
            var current = _context.TimeProvider.Time;
            if (_state.LastUpdate == null || current - _state.LastUpdate >= msecIntervalSize)
            {
                _state.LastUpdate = current;
                return true;
            }
            return false;
        }
    }
} // end of namespace
