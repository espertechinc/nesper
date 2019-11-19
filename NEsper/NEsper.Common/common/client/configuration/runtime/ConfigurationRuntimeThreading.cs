///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.util;

namespace com.espertech.esper.common.client.configuration.runtime
{
    /// <summary>
    ///     Holds threading settings.
    /// </summary>
    [Serializable]
    public class ConfigurationRuntimeThreading
    {
        /// <summary>
        ///     Ctor - sets up defaults.
        /// </summary>
        internal ConfigurationRuntimeThreading()
        {
            ListenerDispatchTimeout = 1000;
            IsListenerDispatchPreserveOrder = true;
            ListenerDispatchLocking = Locking.SPIN;

            InsertIntoDispatchTimeout = 100;
            IsInsertIntoDispatchPreserveOrder = true;
            InsertIntoDispatchLocking = Locking.SPIN;

            NamedWindowConsumerDispatchTimeout = int.MaxValue;
            IsNamedWindowConsumerDispatchPreserveOrder = true;
            NamedWindowConsumerDispatchLocking = Locking.SPIN;

            IsInternalTimerEnabled = true;
            InternalTimerMsecResolution = 100;

            IsThreadPoolInbound = false;
            IsThreadPoolOutbound = false;
            IsThreadPoolRouteExec = false;
            IsThreadPoolTimerExec = false;

            ThreadPoolTimerExecNumThreads = 2;
            ThreadPoolInboundNumThreads = 2;
            ThreadPoolRouteExecNumThreads = 2;
            ThreadPoolOutboundNumThreads = 2;

            ThreadPoolInboundBlocking = Locking.SUSPEND;
            ThreadPoolTimerExecBlocking = Locking.SUSPEND;
            ThreadPoolRouteExecBlocking = Locking.SUSPEND;
            ThreadPoolOutboundBlocking = Locking.SUSPEND;
        }

        /// <summary>
        ///     Returns true to indicate preserve order for dispatch to listeners,
        ///     or false to indicate not to preserve order
        /// </summary>
        /// <returns>true or false</returns>
        public bool IsListenerDispatchPreserveOrder { get; set; }

        /// <summary>
        ///     Returns the timeout in millisecond to wait for listener code to complete
        ///     before dispatching the next result, if dispatch order is preserved
        /// </summary>
        /// <returns>listener dispatch timeout</returns>
        public long ListenerDispatchTimeout { get; set; }

        /// <summary>
        ///     Returns true to indicate preserve order for inter-statement insert-into,
        ///     or false to indicate not to preserve order
        /// </summary>
        /// <returns>true or false</returns>
        public bool IsInsertIntoDispatchPreserveOrder { get; set; }

        /// <summary>
        ///     Returns true if internal timer is enabled (the default), or false for internal timer disabled.
        /// </summary>
        /// <returns>true for internal timer enabled, false for internal timer disabled</returns>
        public bool IsInternalTimerEnabled { get; set; }

        /// <summary>
        ///     Returns the millisecond resolutuion of the internal timer thread.
        /// </summary>
        /// <returns>number of msec between timer processing intervals</returns>
        public long InternalTimerMsecResolution { get; set; }

        /// <summary>
        ///     Returns the number of milliseconds that a thread may maximally be blocking
        ///     to deliver statement results from a producing statement that employs insert-into
        ///     to a consuming statement.
        /// </summary>
        /// <returns>millisecond timeout for order-of-delivery blocking between statements</returns>
        public int InsertIntoDispatchTimeout { get; set; }

        /// <summary>
        ///     Returns the blocking strategy to use when multiple threads deliver results for
        ///     a single statement to listeners, and the guarantee of order of delivery must be maintained.
        /// </summary>
        /// <returns>is the blocking technique</returns>
        public Locking ListenerDispatchLocking { get; set; }

        /// <summary>
        ///     Returns the blocking strategy to use when multiple threads deliver results for
        ///     a single statement to consuming statements of an insert-into, and the guarantee of order of delivery must be
        ///     maintained.
        /// </summary>
        /// <returns>is the blocking technique</returns>
        public Locking InsertIntoDispatchLocking { get; set; }

        /// <summary>
        ///     Returns true for inbound threading enabled, the default is false for not enabled.
        /// </summary>
        /// <returns>indicator whether inbound threading is enabled</returns>
        public bool IsThreadPoolInbound { get; set; }

        /// <summary>
        ///     Returns true for timer execution threading enabled, the default is false for not enabled.
        /// </summary>
        /// <returns>indicator whether timer execution threading is enabled</returns>
        public bool IsThreadPoolTimerExec { get; set; }

        /// <summary>
        ///     Returns true for route execution threading enabled, the default is false for not enabled.
        /// </summary>
        /// <returns>indicator whether route execution threading is enabled</returns>
        public bool IsThreadPoolRouteExec { get; set; }

        /// <summary>
        ///     Returns true for outbound threading enabled, the default is false for not enabled.
        /// </summary>
        /// <returns>indicator whether outbound threading is enabled</returns>
        public bool IsThreadPoolOutbound { get; set; }

        /// <summary>
        ///     Returns the number of thread in the inbound threading pool.
        /// </summary>
        /// <returns>number of threads</returns>
        public int ThreadPoolInboundNumThreads { get; set; }

        /// <summary>
        ///     Returns the number of thread in the outbound threading pool.
        /// </summary>
        /// <returns>number of threads</returns>
        public int ThreadPoolOutboundNumThreads { get; set; }

        /// <summary>
        ///     Returns the number of thread in the route execution thread pool.
        /// </summary>
        /// <returns>number of threads</returns>
        public int ThreadPoolRouteExecNumThreads { get; set; }

        /// <summary>
        ///     Returns the number of thread in the timer execution threading pool.
        /// </summary>
        /// <returns>number of threads</returns>
        public int ThreadPoolTimerExecNumThreads { get; set; }

        /// <summary>
        ///     Returns true if the runtime-level lock is configured as a fair lock (default is false).
        ///     <para />
        ///     This lock coordinates
        ///     event processing threads (threads that send events) with threads that
        ///     perform administrative functions (threads that start or destroy statements, for example).
        /// </summary>
        /// <returns>true for fair lock</returns>
        public bool IsRuntimeFairlock { get; set; }

        /// <summary>
        ///     In multithreaded environments, this setting controls whether named window dispatches to named window consumers
        ///     preserve
        ///     the order of events inserted and removed such that statements that consume a named windows delta stream
        ///     behave deterministic (true by default).
        /// </summary>
        /// <returns>flag</returns>
        public bool IsNamedWindowConsumerDispatchPreserveOrder { get; set; }

        /// <summary>
        ///     Returns the timeout millisecond value for named window dispatches to named window consumers.
        /// </summary>
        /// <returns>timeout milliseconds</returns>
        public int NamedWindowConsumerDispatchTimeout { get; set; }

        /// <summary>
        ///     Returns the locking strategy value for named window dispatches to named window consumers (default is spin).
        /// </summary>
        /// <returns>strategy</returns>
        public Locking NamedWindowConsumerDispatchLocking { get; set; }

        /// <summary>
        ///     Returns the capacity of the timer execution queue, or null if none defined (the unbounded case, default).
        /// </summary>
        /// <value>capacity or null if none defined</value>
        public int? ThreadPoolTimerExecCapacity { get; set; }

        /// <summary>
        ///     Returns the capacity of the inbound execution queue, or null if none defined (the unbounded case, default).
        /// </summary>
        /// <value>capacity or null if none defined</value>
        public int? ThreadPoolInboundCapacity { get; set; }

        /// <summary>
        ///     Returns the capacity of the route execution queue, or null if none defined (the unbounded case, default).
        /// </summary>
        /// <value>capacity or null if none defined</value>
        public int? ThreadPoolRouteExecCapacity { get; set; }

        /// <summary>
        ///     Returns the capacity of the outbound queue, or null if none defined (the unbounded case, default).
        /// </summary>
        /// <value>capacity or null if none defined</value>
        public int? ThreadPoolOutboundCapacity { get; set; }

        public Locking ThreadPoolInboundBlocking { get; set; }
        public Locking ThreadPoolTimerExecBlocking { get; set; }
        public Locking ThreadPoolRouteExecBlocking { get; set; }
        public Locking ThreadPoolOutboundBlocking { get; set; }
    }
} // end of namespace