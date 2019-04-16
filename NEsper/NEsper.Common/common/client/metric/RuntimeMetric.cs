///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.metric
{
    /// <summary>
    ///     Reports runtime-level instrumentation values.
    /// </summary>
    public class RuntimeMetric : MetricEvent
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="runtimeURI">runtime URI</param>
        /// <param name="timestamp">runtime timestamp</param>
        /// <param name="inputCount">number of input events</param>
        /// <param name="inputCountDelta">number of input events since last</param>
        /// <param name="scheduleDepth">schedule depth</param>
        public RuntimeMetric(
            string runtimeURI,
            long timestamp,
            long inputCount,
            long inputCountDelta,
            long scheduleDepth)
            : base(runtimeURI)
        {
            Timestamp = timestamp;
            InputCount = inputCount;
            InputCountDelta = inputCountDelta;
            ScheduleDepth = scheduleDepth;
        }

        /// <summary>
        ///     Returns input count since runtime initialization, cumulative.
        /// </summary>
        /// <value>input count</value>
        public long InputCount { get; }

        /// <summary>
        ///     Returns schedule depth.
        /// </summary>
        /// <value>schedule depth</value>
        public long ScheduleDepth { get; }

        /// <summary>
        ///     Returns runtime timestamp.
        /// </summary>
        /// <value>timestamp</value>
        public long Timestamp { get; }

        /// <summary>
        ///     Returns input count since last reporting period.
        /// </summary>
        /// <value>input count</value>
        public long InputCountDelta { get; }
    }
} // end of namespace