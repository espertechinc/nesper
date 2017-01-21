///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client.metric
{
    /// <summary>Reports engine-level instrumentation values. </summary>
    public class EngineMetric : MetricEvent
    {
        /// <summary>Ctor. </summary>
        /// <param name="engineURI">engine URI</param>
        /// <param name="timestamp">engine timestamp</param>
        /// <param name="inputCount">number of input events</param>
        /// <param name="inputCountDelta">number of input events since last</param>
        /// <param name="scheduleDepth">schedule depth</param>
        public EngineMetric(String engineURI,
                            long timestamp,
                            long inputCount,
                            long inputCountDelta,
                            long scheduleDepth)
            : base(engineURI)
        {
            Timestamp = timestamp;
            InputCount = inputCount;
            InputCountDelta = inputCountDelta;
            ScheduleDepth = scheduleDepth;
        }

        /// <summary>Returns input count since engine initialization cumulative. </summary>
        /// <value>input count</value>
        public long InputCount { get; private set; }

        /// <summary>Returns schedule depth. </summary>
        /// <value>schedule depth</value>
        public long ScheduleDepth { get; private set; }

        /// <summary>Returns engine timestamp. </summary>
        /// <value>timestamp</value>
        public long Timestamp { get; private set; }

        /// <summary>Returns input count since last reporting period. </summary>
        /// <value>input count</value>
        public long InputCountDelta { get; private set; }
    }
}