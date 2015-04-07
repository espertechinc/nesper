///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.time;

namespace com.espertech.esper.epl.view
{
    public sealed class OutputConditionPolledTime : OutputConditionPolled
    {
        private readonly ExprTimePeriod _timePeriod;
        private readonly AgentInstanceContext _context;
        private long? _lastUpdate;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="timePeriod">is the number of minutes or seconds to batch events for, may include variables</param>
        /// <param name="context">is the view context for time scheduling</param>
        public OutputConditionPolledTime(ExprTimePeriod timePeriod,
                                   AgentInstanceContext context)
        {
            if (context == null)
            {
                const string message = "OutputConditionTime requires a non-null view context";
                throw new ArgumentNullException(message);
            }

            _context = context;
            _timePeriod = timePeriod;

            double numSeconds = timePeriod.EvaluateAsSeconds(null, true, context);
            if ((numSeconds < 0.001) && (!timePeriod.HasVariable)) {
                throw new ArgumentException("Output condition by time requires a interval size of at least 1 msec or a variable");
            }
        }

        public bool UpdateOutputCondition(int newEventsCount, int oldEventsCount)
        {
            // If we pull the interval from a variable, then we may need to reschedule
            long msecIntervalSize = _timePeriod.NonconstEvaluator().DeltaMillisecondsUseEngineTime(null, _context);

            long current = _context.TimeProvider.Time;
            if (_lastUpdate == null || current - _lastUpdate >= msecIntervalSize) {
                _lastUpdate = current;
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            return GetType().FullName;
        }
    }
}
