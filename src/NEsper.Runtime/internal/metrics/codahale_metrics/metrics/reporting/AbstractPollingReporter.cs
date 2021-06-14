///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;
using com.espertech.esper.compat.concurrency;
using com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.core;

namespace com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.reporting
{
    /// <summary>
    /// An abstract base class for all reporter implementations which periodically poll registered
    /// metrics (e.g., to send the data to another service).
    /// </summary>
    public abstract class AbstractPollingReporter : AbstractReporter, IRunnable
    {
        private readonly IScheduledExecutorService executor;

        /// <summary>
        /// Creates a new <seealso cref="AbstractPollingReporter" /> instance.
        /// </summary>
        /// <param name="registry">the <seealso cref="MetricsRegistry" /> containing the metrics this reporter will report
        /// </param>
        /// <param name="name">the reporter's name</param>
        /// <unknown>@see AbstractReporter#AbstractReporter(MetricsRegistry)</unknown>
        protected AbstractPollingReporter(MetricsRegistry registry, string name) : base(registry)
        {
            this.executor = registry.NewScheduledThreadPool(1, name);
        }

        /// <summary>
        /// Starts the reporter polling at the given period.
        /// </summary>
        /// <param name="period">the amount of time between polls</param>
        /// <param name="unit">the unit for {@code period}</param>
        public virtual void Start(long period, TimeUnit unit)
        {
            executor.ScheduleWithFixedDelay(
                Run,
                TimeUnitHelper.ToTimeSpan(period, unit),
                TimeUnitHelper.ToTimeSpan(period, unit));
        }

        /// <summary>
        /// Shuts down the reporter polling, waiting the specific amount of time for any current polls to
        /// complete.
        /// </summary>
        /// <param name="timeout">the maximum time to wait</param>
        /// <param name="unit">the unit for {@code timeout}</param>
        /// <throws>InterruptedException if interrupted while waiting</throws>
        public void Shutdown(long timeout, TimeUnit unit)
        {
            executor.Shutdown();
            executor.AwaitTermination(TimeUnitHelper.ToTimeSpan(timeout, unit));
        }

        public override void Shutdown()
        {
            executor.Shutdown();
            base.Shutdown();
        }

        /// <summary>
        /// The method called when a poll is scheduled to occur.
        /// </summary>
        public abstract void Run();
    }
} // end of namespace