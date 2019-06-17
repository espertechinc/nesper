///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;

namespace com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.core
{
    /// <summary>
    ///     An object which maintains mean and exponentially-weighted rate.
    /// </summary>
    public interface Metered : Metric
    {
        /// <summary>
        ///     Returns the meter's rate unit.
        /// </summary>
        /// <value>the meter's rate unit</value>
        TimeUnit RateUnit { get; }

        /// <summary>
        ///     Returns the type of events the meter is measuring.
        /// </summary>
        /// <value>the meter's event type</value>
        string EventType { get; }

        /// <summary>
        ///     Returns the number of events which have been marked.
        /// </summary>
        /// <value>the number of events which have been marked</value>
        long Count { get; }

        /// <summary>
        ///     Returns the fifteen-minute exponentially-weighted moving average rate at which events have
        ///     occurred since the meter was created.
        ///     This rate has the same exponential decay factor as the fifteen-minute load average in the
        ///     {@code top} Unix command.
        /// </summary>
        /// <value>
        ///     the fifteen-minute exponentially-weighted moving average rate at which events have
        ///     occurred since the meter was
        ///     created
        /// </value>
        double FifteenMinuteRate { get; }

        /// <summary>
        ///     Returns the five-minute exponentially-weighted moving average rate at which events have
        ///     occurred since the meter was created.
        ///     This rate has the same exponential decay factor as the five-minute load average in the {@code
        ///     top} Unix command.
        /// </summary>
        /// <value>
        ///     the five-minute exponentially-weighted moving average rate at which events have
        ///     occurred since the meter was created
        /// </value>
        double FiveMinuteRate { get; }

        /// <summary>
        ///     Returns the mean rate at which events have occurred since the meter was created.
        /// </summary>
        /// <value>the mean rate at which events have occurred since the meter was created</value>
        double MeanRate { get; }

        /// <summary>
        ///     Returns the one-minute exponentially-weighted moving average rate at which events have
        ///     occurred since the meter was created.
        ///     This rate has the same exponential decay factor as the one-minute load average in the {@code
        ///     top} Unix command.
        /// </summary>
        /// <value>
        ///     the one-minute exponentially-weighted moving average rate at which events have occurred
        ///     since the meter was created
        /// </value>
        double OneMinuteRate { get; }
    }
} // end of namespace