///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

using com.espertech.esper.common.@internal.statement.thread;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.concurrency;
using com.espertech.esper.compat.threading;
using com.espertech.esper.runtime.@internal.kernel.service;

namespace com.espertech.esper.runtime.@internal.kernel.thread
{
    /// <summary>Engine-level threading services. </summary>
    public interface ThreadingService : ThreadingCommon, IDisposable
    {
        /// <summary>Initialize thread pools. </summary>
        /// <param name="services">engine-level service context</param>
        /// <param name="runtime">runtime</param>
        void InitThreading(
            EPServicesContext services,
            EPEventServiceImpl runtime);

        /// <summary>Returns true for timer execution threading enabled. </summary>
        /// <value>indicator</value>
        bool IsTimerThreading { get; }

        /// <summary>Submit timer execution work unit. </summary>
        /// <param name="timerUnit">unit of work</param>
        void SubmitTimerWork(IRunnable timerUnit);

#if INHERITED
        /// <summary>Returns true for inbound threading enabled. </summary>
        /// <value>indicator</value>
        bool IsInboundThreading { get; }
#endif

        /// <summary>Submit inbound work unit. </summary>
        /// <value>unit of work</value>
        void SubmitInbound(IRunnable unit);

        /// <summary>Returns true for route execution threading enabled. </summary>
        /// <value>indicator</value>
        bool IsRouteThreading { get; }

        /// <summary>Submit route work unit. </summary>
        /// <param name="unit">unit of work</param>
        void SubmitRoute(IRunnable unit);

        /// <summary>Returns true for outbound threading enabled. </summary>
        /// <value>indicator</value>
        bool IsOutboundThreading { get; }

        /// <summary>Submit outbound work unit. </summary>
        /// <param name="unit">unit of work</param>
        void SubmitOutbound(IRunnable unit);

        /// <summary>Returns the outbound queue. </summary>
        /// <value>queue</value>
        IBlockingQueue<Runnable> OutboundQueue { get; }

        /// <summary>Returns the outbound thread pool </summary>
        /// <value>thread pool</value>
        IExecutorService OutboundThreadPool { get; }

        /// <summary>Returns the route queue. </summary>
        /// <value>queue</value>
        IBlockingQueue<Runnable> RouteQueue { get; }

        /// <summary>Returns the route thread pool </summary>
        /// <value>thread pool</value>
        IExecutorService RouteThreadPool { get; }

        /// <summary>Returns the timer queue. </summary>
        /// <value>queue</value>
        IBlockingQueue<Runnable> TimerQueue { get; }

        /// <summary>Returns the timer thread pool </summary>
        /// <value>thread pool</value>
        IExecutorService TimerThreadPool { get; }

        /// <summary>Returns the inbound queue. </summary>
        /// <value>queue</value>
        IBlockingQueue<Runnable> InboundQueue { get; }

        /// <summary>Returns the inbound thread pool </summary>
        /// <value>thread pool</value>
        IExecutorService InboundThreadPool { get; }

        Thread MakeEventSourceThread(string engineURI, string sourceName, Runnable runnable);
    }
}