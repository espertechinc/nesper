///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportCallEvent
    {
        public SupportCallEvent(
            long callId,
            string source,
            string destination,
            long startTime,
            long endTime)
        {
            CallId = callId;
            Source = source;
            Dest = destination;
            StartTime = startTime;
            EndTime = endTime;
        }

        public long CallId { get; }

        public string Source { get; }

        public string Dest { get; }

        public long StartTime { get; }

        public long EndTime { get; }
    }
} // end of namespace