///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.output.polled
{
    /// <summary>
    /// Output condition for output rate limiting that handles when-then expressions for controlling output.
    /// </summary>
    public class OutputConditionPolledExpressionState : OutputConditionPolledState
    {
        public OutputConditionPolledExpressionState(
            int totalNewEventsCount,
            int totalOldEventsCount,
            int totalNewEventsSum,
            int totalOldEventsSum,
            long? lastOutputTimestamp)
        {
            TotalNewEventsCount = totalNewEventsCount;
            TotalOldEventsCount = totalOldEventsCount;
            TotalNewEventsSum = totalNewEventsSum;
            TotalOldEventsSum = totalOldEventsSum;
            LastOutputTimestamp = lastOutputTimestamp;
        }

        public int TotalNewEventsCount { get; set; }

        public int TotalOldEventsCount { get; set; }

        public int TotalNewEventsSum { get; set; }

        public int TotalOldEventsSum { get; set; }

        public long? LastOutputTimestamp { get; set; }
    }
} // end of namespace