///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.output.polled
{
    public class OutputConditionPolledCountState : OutputConditionPolledState
    {
        public OutputConditionPolledCountState(
            long eventRate,
            int newEventsCount,
            int oldEventsCount,
            bool isFirst)
        {
            EventRate = eventRate;
            NewEventsCount = newEventsCount;
            OldEventsCount = oldEventsCount;
            IsFirst = isFirst;
        }

        public long EventRate { get; set; }

        public int NewEventsCount { get; set; }

        public int OldEventsCount { get; set; }

        public bool IsFirst { get; set; }
    }
} // end of namespace