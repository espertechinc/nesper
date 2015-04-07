///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;
using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.service;

namespace com.espertech.esper.core.thread
{
    /// <summary>
    /// Implementation for engine-level threading.
    /// </summary>
    public class ThreadingServiceImpl : ThreadingService
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ConfigurationEngineDefaults.Threading _config;
        private readonly bool _isTimerThreading;
        private readonly bool _isInboundThreading;
        private readonly bool _isRouteThreading;
        private readonly bool _isOutboundThreading;

        private IBlockingQueue<Runnable> _timerQueue;
        private IBlockingQueue<Runnable> _inboundQueue;
        private IBlockingQueue<Runnable> _routeQueue;
        private IBlockingQueue<Runnable> _outboundQueue;
    
        private IExecutorService _timerThreadPool;
        private IExecutorService _inboundThreadPool;
        private IExecutorService _routeThreadPool;
        private IExecutorService _outboundThreadPool;

        /// <summary>Ctor. </summary>
        /// <param name="threadingConfig">configuration</param>
        public ThreadingServiceImpl(ConfigurationEngineDefaults.Threading threadingConfig)
        {
            _config = threadingConfig;
            if (ThreadingOption.IsThreadingEnabled)
            {
                _isTimerThreading = threadingConfig.IsThreadPoolTimerExec;
                _isInboundThreading = threadingConfig.IsThreadPoolInbound;
                _isRouteThreading = threadingConfig.IsThreadPoolRouteExec;
                _isOutboundThreading = threadingConfig.IsThreadPoolOutbound;
            }
            else
            {
                _isTimerThreading = false;
                _isInboundThreading = false;
                _isRouteThreading = false;
                _isOutboundThreading = false;
            }
        }

        public bool IsRouteThreading
        {
            get { return _isRouteThreading; }
        }

        public bool IsInboundThreading
        {
            get { return _isInboundThreading; }
        }

        public bool IsTimerThreading
        {
            get { return _isTimerThreading; }
        }

        public bool IsOutboundThreading
        {
            get { return _isOutboundThreading; }
        }

        public void InitThreading(EPServicesContext services, EPRuntimeImpl runtime)
        {
            if (_isInboundThreading)
            {
                _inboundQueue = MakeQueue(_config.ThreadPoolInboundCapacity, _config.ThreadPoolInboundBlocking);
                _inboundThreadPool = GetThreadPool(services.EngineURI, "Inbound", _inboundQueue, _config.ThreadPoolInboundNumThreads);
            }
    
            if (_isTimerThreading)
            {
                _timerQueue = MakeQueue(_config.ThreadPoolTimerExecCapacity, _config.ThreadPoolTimerExecBlocking);
                _timerThreadPool = GetThreadPool(services.EngineURI, "TimerExec", _timerQueue, _config.ThreadPoolTimerExecNumThreads);
            }
    
            if (_isRouteThreading)
            {
                _routeQueue = MakeQueue(_config.ThreadPoolRouteExecCapacity, _config.ThreadPoolRouteExecBlocking);
                _routeThreadPool = GetThreadPool(services.EngineURI, "RouteExec", _routeQueue, _config.ThreadPoolRouteExecNumThreads);
            }
    
            if (_isOutboundThreading)
            {
                _outboundQueue = MakeQueue(_config.ThreadPoolOutboundCapacity, _config.ThreadPoolOutboundBlocking);
                _outboundThreadPool = GetThreadPool(services.EngineURI, "Outbound", _outboundQueue, _config.ThreadPoolOutboundNumThreads);
            }
        }
    
        private static IBlockingQueue<Runnable> MakeQueue(int? threadPoolTimerExecCapacity, ConfigurationEngineDefaults.Threading.Locking blocking)
        {
            if ((threadPoolTimerExecCapacity == null) ||
                (threadPoolTimerExecCapacity <= 0) ||
                (threadPoolTimerExecCapacity == int.MaxValue))
            {
                return blocking == ConfigurationEngineDefaults.Threading.Locking.SPIN
                    ? (IBlockingQueue<Runnable>) new ImperfectBlockingQueue<Runnable>()
                    : (IBlockingQueue<Runnable>) new LinkedBlockingQueue<Runnable>();
            }

            return blocking == ConfigurationEngineDefaults.Threading.Locking.SPIN
                       ? (IBlockingQueue<Runnable>)new ImperfectBlockingQueue<Runnable>(threadPoolTimerExecCapacity.Value)
                       : (IBlockingQueue<Runnable>)new BoundBlockingQueue<Runnable>(threadPoolTimerExecCapacity.Value);
        }

        /// <summary>Submit route work unit. </summary>
        /// <param name="unit">unit of work</param>
        public void SubmitRoute(Runnable unit)
        {
            _routeQueue.Push(unit);
        }

        /// <summary>Submit inbound work unit. </summary>
        /// <value>unit of work</value>
        public void SubmitInbound(Runnable unit)
        {
            _inboundQueue.Push(unit);
        }

        public void SubmitOutbound(Runnable unit)
        {
            _outboundQueue.Push(unit);
        }

        public void SubmitTimerWork(Runnable unit)
        {
            _timerQueue.Push(unit);
        }

        public IBlockingQueue<Runnable> OutboundQueue
        {
            get { return _outboundQueue; }
        }

        public IExecutorService OutboundThreadPool
        {
            get { return _outboundThreadPool; }
        }

        public IBlockingQueue<Runnable> RouteQueue
        {
            get { return _routeQueue; }
        }

        public IExecutorService RouteThreadPool
        {
            get { return _routeThreadPool; }
        }

        public IBlockingQueue<Runnable> TimerQueue
        {
            get { return _timerQueue; }
        }

        public IExecutorService TimerThreadPool
        {
            get { return _timerThreadPool; }
        }

        public IBlockingQueue<Runnable> InboundQueue
        {
            get { return _inboundQueue; }
        }

        public IExecutorService InboundThreadPool
        {
            get { return _inboundThreadPool; }
        }

        public void Dispose()
        {
            lock (this)
            {
                if (_timerThreadPool != null)
                {
                    StopPool(_timerThreadPool, _timerQueue, "TimerExec");
                }
                if (_routeThreadPool != null)
                {
                    StopPool(_routeThreadPool, _routeQueue, "RouteExec");
                }
                if (_outboundThreadPool != null)
                {
                    StopPool(_outboundThreadPool, _outboundQueue, "Outbound");
                }
                if (_inboundThreadPool != null)
                {
                    StopPool(_inboundThreadPool, _inboundQueue, "Inbound");
                }

                _timerThreadPool = null;
                _routeThreadPool = null;
                _outboundThreadPool = null;
                _inboundThreadPool = null;
            }
        }

        private static IExecutorService GetThreadPool(String engineURI, String name, IBlockingQueue<Runnable> queue, int numThreads)
        {
            if (Log.IsInfoEnabled)
            {
                Log.Info("Starting pool " + name + " with " + numThreads + " threads");
            }

            if (engineURI == null)
            {
                engineURI = "default";
            }

            return new DedicatedExecutorService(name, numThreads, queue);
        }

        public Thread MakeEventSourceThread(String engineURI, String sourceName, Runnable runnable)
        {
            if (engineURI == null)
            {
                engineURI = "default";
            }

            var threadGroupName = "com.espertech.esper." + engineURI + "-source-" + sourceName;
            var thread = new Thread(() => runnable());
            return thread;
        }

        private static void StopPool(IExecutorService executorService, IBlockingQueue<Runnable> queue, String name)
        {
            if (Log.IsInfoEnabled)
            {
                Log.Info("Shutting down pool " + name);
            }
    
            queue.Clear();
            executorService.Shutdown();
            executorService.AwaitTermination(new TimeSpan(0, 0, 10));
        }
    }
}
