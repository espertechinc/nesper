///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.context;
using com.espertech.esper.client.dataflow;
using com.espertech.esper.client.hook;
using com.espertech.esper.client.soda;
using com.espertech.esper.client.time;
using com.espertech.esper.client.util;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.context.mgr;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.start;
using com.espertech.esper.core.thread;
using com.espertech.esper.epl.annotation;
using com.espertech.esper.epl.declexpr;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.subquery;
using com.espertech.esper.epl.expression.visitor;
using com.espertech.esper.epl.metric;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.spec.util;
using com.espertech.esper.epl.variable;
using com.espertech.esper.events.util;
using com.espertech.esper.filter;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.schedule;
using com.espertech.esper.util;

namespace com.espertech.esper.core.service
{
    using DataMap = IDictionary<string, object>;
    using VariableNotFoundException = com.espertech.esper.client.VariableNotFoundException;

    /// <summary>
    /// Implements runtime interface. Also accepts timer callbacks for synchronizing time events with regular events sent in.
    /// </summary>
    public class EPRuntimeImpl
        : EPRuntimeSPI
        , EPRuntimeEventSender
        , InternalEventRouteDest
    {
        private EPServicesContext _services;
        private readonly bool _isLatchStatementInsertStream;
        private bool _isUsingExternalClocking;
        private readonly bool _isPrioritized;
        private readonly AtomicLong _routedInternal;
        private readonly AtomicLong _routedExternal;
        private EventRenderer _eventRenderer;
        private InternalEventRouter _internalEventRouter;
        private readonly ExprEvaluatorContext _engineFilterAndDispatchTimeContext;
        private ThreadWorkQueue _threadWorkQueue;

        public event EventHandler<UnmatchedEventArgs> UnmatchedEvent;

        /// <summary>
        /// Data that remains local to the thread.
        /// </summary>

        private IThreadLocal<ThreadLocalData> _threadLocalData;

        #region Nested type: ThreadLocalData

        /// <summary>
        /// Group of data that is associated with the thread.
        /// </summary>

        private class ThreadLocalData
        {
            internal List<FilterHandle> MatchesArrayThreadLocal;
            internal IDictionary<EPStatementAgentInstanceHandle, Object> MatchesPerStmtThreadLocal;
            internal ArrayBackedCollection<ScheduleHandle> ScheduleArrayThreadLocal;
            internal IDictionary<EPStatementAgentInstanceHandle, Object> SchedulePerStmtThreadLocal;
        }

        /// <summary>
        /// Gets the local data.
        /// </summary>
        /// <value>The local data.</value>

#if NET45
        //[MethodImplOptions.AggressiveInlining]
#endif
        private ThreadLocalData ThreadData
        {
            get => _threadLocalData.GetOrCreate();
        }

        private ArrayBackedCollection<ScheduleHandle> ScheduleArray
        {
            get => ThreadData.ScheduleArrayThreadLocal;
        }

        private IDictionary<EPStatementAgentInstanceHandle, Object> SchedulePerStmt
        {
            get => ThreadData.SchedulePerStmtThreadLocal;
        }

        #endregion

        /// <summary>Constructor. </summary>
        /// <param name="services">references to services</param>
        public EPRuntimeImpl(EPServicesContext services)
        {
            _services = services;
            _threadWorkQueue = new ThreadWorkQueue(services.ThreadLocalManager);
            _isLatchStatementInsertStream = _services.EngineSettingsService.EngineSettings.Threading.IsInsertIntoDispatchPreserveOrder;
            _isUsingExternalClocking = !_services.EngineSettingsService.EngineSettings.Threading.IsInternalTimerEnabled;
            _isPrioritized = services.EngineSettingsService.EngineSettings.Execution.IsPrioritized;
            _routedInternal = new AtomicLong();
            _routedExternal = new AtomicLong();

            var expressionResultCacheService = services.ExpressionResultCacheSharable;
            _engineFilterAndDispatchTimeContext = new ProxyExprEvaluatorContext
            {
                ProcContainer = () => services.Container,
                ProcTimeProvider = () => services.SchedulingService,
                ProcExpressionResultCacheService = () => expressionResultCacheService,
                ProcAgentInstanceId = () => -1,
                ProcContextProperties = () => null,
                ProcAllocateAgentInstanceScriptContext = () => null,
                ProcStatementName = () => null,
                ProcEngineURI = () => null,
                ProcStatementId = () => -1,
                ProcAgentInstanceLock = () => null,
                ProcStatementType = () => null,
                ProcTableExprEvaluatorContext = () =>
                {
                    throw new UnsupportedOperationException("Table-access evaluation is not supported in this expression");
                },
                ProcStatementUserObject = () => null
            };

            InitThreadLocals();

            services.ThreadingService.InitThreading(services, this);
        }

        /// <summary>
        /// Removes all unmatched event handlers.
        /// </summary>

        public void RemoveAllUnmatchedEventHandlers()
        {
            UnmatchedEvent = null;
        }

        /// <summary>
        /// Creates a local data object.
        /// </summary>
        /// <returns></returns>

        private ThreadLocalData CreateLocalData()
        {
            var threadLocalData = new ThreadLocalData
            {
                MatchesArrayThreadLocal = new List<FilterHandle>(100),
                ScheduleArrayThreadLocal = new ArrayBackedCollection<ScheduleHandle>(100)
            };

            if (_isPrioritized)
            {
                threadLocalData.MatchesPerStmtThreadLocal =
                    new OrderedDictionary<EPStatementAgentInstanceHandle, Object>(new EPStatementAgentInstanceHandlePrioritySort());
                threadLocalData.SchedulePerStmtThreadLocal =
                    new OrderedDictionary<EPStatementAgentInstanceHandle, Object>(new EPStatementAgentInstanceHandlePrioritySort());
            }
            else
            {
                threadLocalData.MatchesPerStmtThreadLocal =
                    new Dictionary<EPStatementAgentInstanceHandle, Object>(10000);
                threadLocalData.SchedulePerStmtThreadLocal =
                    new Dictionary<EPStatementAgentInstanceHandle, Object>(10000);
            }

            return threadLocalData;
        }


        /// <summary>Sets the route for events to use </summary>
        /// <value>router</value>
        public InternalEventRouter InternalEventRouter
        {
            set => _internalEventRouter = value;
        }

        public long RoutedInternal
        {
            get => _routedInternal.Get();
        }

        public long RoutedExternal
        {
            get => _routedExternal.Get();
        }

        public void TimerCallback()
        {
            var msec = _services.TimeSource.GetTimeMillis();

            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled && (ExecutionPathDebugLog.IsTimerDebugEnabled)))
            {
                Log.Debug(".timerCallback Evaluating scheduled callbacks, time is " + msec);
            }

            SendEvent(new CurrentTimeEvent(msec));
        }

        public void SendEventAvro(Object avroGenericDataDotRecord, String avroEventTypeName)
        {
            if (avroGenericDataDotRecord == null)
            {
                throw new ArgumentException("Invalid null event object", "avroGenericDataDotRecord");
            }

            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(".sendMap Processing event " + avroGenericDataDotRecord.ToString());
            }

            if ((ThreadingOption.IsThreadingEnabled) && (_services.ThreadingService.IsInboundThreading))
            {
                _services.ThreadingService.SubmitInbound(
                    new InboundUnitSendAvro(avroGenericDataDotRecord, avroEventTypeName, _services, this).Run);
            }
            else
            {
                // Process event
                EventBean eventBean = WrapEventAvro(avroGenericDataDotRecord, avroEventTypeName);
                ProcessWrappedEvent(eventBean);
            }
        }

        public void SendEvent(Object theEvent)
        {
            if (theEvent == null)
            {
                Log.Error(".sendEvent Null object supplied");
                return;
            }

            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                if ((!(theEvent is CurrentTimeEvent)) || (ExecutionPathDebugLog.IsTimerDebugEnabled))
                {
                    Log.Debug(".sendEvent Processing event " + theEvent);
                }
            }

            // Process event
            if ((ThreadingOption.IsThreadingEnabledValue) && (_services.ThreadingService.IsInboundThreading))
            {
                _services.ThreadingService.SubmitInbound(
                    new InboundUnitSendEvent(theEvent, this).Run);
            }
            else
            {
                ProcessEvent(theEvent);
            }
        }

        public void SendEvent(XmlNode document)
        {
            if (document == null)
            {
                Log.Error(".sendEvent Null object supplied");
                return;
            }

            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(".sendEvent Processing DOM node event " + document);
            }

            // Process event
            if ((ThreadingOption.IsThreadingEnabledValue) && (_services.ThreadingService.IsInboundThreading))
            {
                _services.ThreadingService.SubmitInbound(
                    new InboundUnitSendDOM(document, _services, this).Run);
            }
            else
            {
                // Get it wrapped up, process event
                var eventBean = WrapEvent(document);
                ProcessEvent(eventBean);
            }
        }

        /// <summary>
        /// Send an event represented by a LINQ element to the event stream processing runtime.
        /// <para/>
        /// Use the route method for sending events into the runtime from within
        /// event handler code. to avoid the possibility of a stack overflow due to nested calls to
        /// SendEvent.
        /// </summary>
        /// <param name="element">The element.</param>

        public void SendEvent(XElement element)
        {
            if (element == null)
            {
                Log.Error(".sendEvent Null object supplied");
                return;
            }

            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(".sendEvent Processing DOM node event {0}", element);
            }

            // Process event
            if ((ThreadingOption.IsThreadingEnabledValue) && (_services.ThreadingService.IsInboundThreading))
            {
                _services.ThreadingService.SubmitInbound(
                    new InboundUnitSendLINQ(element, _services, this).Run);
            }
            else
            {
                // Get it wrapped up, process event
                var eventBean = WrapEvent(element);
                ProcessEvent(eventBean);
            }
        }

        public EventBean WrapEvent(XmlNode node)
        {
            return _services.EventAdapterService.AdapterForDOM(node);
        }

        public void Route(XmlNode document)
        {
            if (document == null)
            {
                Log.Error(".sendEvent Null object supplied");
                return;
            }

            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(".sendEvent Processing DOM node event " + document);
            }

            // Get it wrapped up, process event
            var eventBean = _services.EventAdapterService.AdapterForDOM(document);
            _threadWorkQueue.AddBack(eventBean);
        }

        public void RouteAvro(Object avroGenericDataDotRecord, String avroEventTypeName) 
        {
            if (avroGenericDataDotRecord == null) {
                Log.Error(".sendEvent Null object supplied");
                return;
            }

            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(".sendEvent Processing Avro event " + avroGenericDataDotRecord);
            }

            // Get it wrapped up, process event
            EventBean eventBean = _services.EventAdapterService.AdapterForAvro(avroGenericDataDotRecord, avroEventTypeName);
            _threadWorkQueue.AddBack(eventBean);
        }

        public EventBean WrapEvent(XElement node)
        {
            return _services.EventAdapterService.AdapterForDOM(node);
        }

        public EventBean WrapEventAvro(Object avroGenericDataDotRecord, String eventTypeName)
        {
            return _services.EventAdapterService.AdapterForAvro(avroGenericDataDotRecord, eventTypeName);
        }

        public void Route(XElement element)
        {
            if (element == null)
            {
                Log.Fatal(".sendEvent Null object supplied");
                return;
            }

            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(".sendEvent Processing DOM node event " + element);
            }

            // Get it wrapped up, process event
            var eventBean = _services.EventAdapterService.AdapterForDOM(element);
            _threadWorkQueue.AddBack(eventBean);
        }

        public void SendEvent(DataMap map, String mapEventTypeName)
        {
            if (map == null)
            {
                throw new ArgumentException("Invalid null event object");
            }

            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(".sendMap Processing event " + map);
            }

            if ((ThreadingOption.IsThreadingEnabledValue) && (_services.ThreadingService.IsInboundThreading))
            {
                _services.ThreadingService.SubmitInbound(
                    new InboundUnitSendMap(map, mapEventTypeName, _services, this).Run);
            }
            else
            {
                // Process event
                var eventBean = WrapEvent(map, mapEventTypeName);
                ProcessWrappedEvent(eventBean);
            }
        }

        public EventBean WrapEvent(DataMap map, String eventTypeName)
        {
            return _services.EventAdapterService.AdapterForMap(map, eventTypeName);
        }

        public void SendEvent(Object[] propertyValues, String objectArrayEventTypeName)
        {
            if (propertyValues == null)
            {
                throw new ArgumentException("Invalid null event object");
            }

            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(".sendMap Processing event " + propertyValues.Render());
            }

            if ((ThreadingOption.IsThreadingEnabledValue) && (_services.ThreadingService.IsInboundThreading))
            {
                _services.ThreadingService.SubmitInbound(
                    new InboundUnitSendObjectArray(propertyValues, objectArrayEventTypeName, _services, this).Run);
            }
            else
            {
                // Process event
                var eventBean = WrapEvent(propertyValues, objectArrayEventTypeName);
                ProcessWrappedEvent(eventBean);
            }
        }

        public EventBean WrapEvent(Object[] objectArray, String eventTypeName)
        {
            return _services.EventAdapterService.AdapterForObjectArray(objectArray, eventTypeName);
        }

        public void Route(DataMap map, String eventTypeName)
        {
            if (map == null)
            {
                throw new ArgumentException("Invalid null event object");
            }

            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(".route Processing event " + map);
            }

            // Process event
            var theEvent = _services.EventAdapterService.AdapterForMap(map, eventTypeName);
            if (_internalEventRouter.HasPreprocessing)
            {
                theEvent = _internalEventRouter.Preprocess(theEvent, _engineFilterAndDispatchTimeContext);
                if (theEvent == null)
                {
                    return;
                }
            }
            _threadWorkQueue.AddBack(theEvent);
        }

        public void Route(Object[] objectArray, String eventTypeName)
        {
            if (objectArray == null)
            {
                throw new ArgumentException("Invalid null event object");
            }

            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(".route Processing event " + objectArray.Render());
            }

            // Process event
            var theEvent = _services.EventAdapterService.AdapterForObjectArray(objectArray, eventTypeName);
            if (_internalEventRouter.HasPreprocessing)
            {
                theEvent = _internalEventRouter.Preprocess(theEvent, _engineFilterAndDispatchTimeContext);
                if (theEvent == null)
                {
                    return;
                }
            }
            _threadWorkQueue.AddBack(theEvent);
        }

        public long NumEventsEvaluated
        {
            get => _services.FilterService.NumEventsEvaluated;
        }

        public void ResetStats()
        {
            _services.FilterService.ResetStats();
            _routedInternal.Set(0);
            _routedExternal.Set(0);
        }

        public void RouteEventBean(EventBean theEvent)
        {
            _threadWorkQueue.AddBack(theEvent);
        }

        public void Route(Object theEvent)
        {
            _routedExternal.IncrementAndGet();

            if (_internalEventRouter.HasPreprocessing)
            {
                var eventBean = _services.EventAdapterService.AdapterForObject(theEvent);
                theEvent = _internalEventRouter.Preprocess(eventBean, _engineFilterAndDispatchTimeContext);
                if (theEvent == null)
                {
                    return;
                }
            }

            _threadWorkQueue.AddBack(theEvent);
        }

        // Internal route of events via insert-into, holds a statement lock
        public void Route(EventBean theEvent, EPStatementHandle epStatementHandle, bool addToFront)
        {
            if (InstrumentationHelper.ENABLED)
                InstrumentationHelper.Get().QRouteBetweenStmt(theEvent, epStatementHandle, addToFront);
            try
            {
                _routedInternal.IncrementAndGet();

                if (_isLatchStatementInsertStream)
                {
                    if (addToFront)
                    {
                        var latch = epStatementHandle.InsertIntoFrontLatchFactory.NewLatch(theEvent);
                        _threadWorkQueue.AddFront(latch);
                    }
                    else
                    {
                        var latch = epStatementHandle.InsertIntoBackLatchFactory.NewLatch(theEvent);
                        _threadWorkQueue.AddBack(latch);
                    }
                }
                else
                {
                    if (addToFront)
                    {
                        _threadWorkQueue.AddFront(theEvent);
                    }
                    else
                    {
                        _threadWorkQueue.AddBack(theEvent);
                    }
                }
            }
            finally
            {
                if (InstrumentationHelper.ENABLED)
                    InstrumentationHelper.Get().ARouteBetweenStmt();
            }
        }

        /// <summary>Process an unwrapped event. </summary>
        /// <param name="theEvent">to process.</param>
        public void ProcessEvent(Object theEvent)
        {
            if (theEvent is TimerEvent)
            {
                ProcessTimeEvent((TimerEvent)theEvent);
                return;
            }

            EventBean eventBean;

            if (theEvent is EventBean)
            {
                eventBean = (EventBean)theEvent;
            }
            else
            {
                eventBean = WrapEvent(theEvent);
            }

            ProcessWrappedEvent(eventBean);
        }

        public EventBean WrapEvent(Object theEvent)
        {
            return _services.EventAdapterService.AdapterForObject(theEvent);
        }

        public void ProcessWrappedEvent(EventBean eventBean)
        {
            if (InstrumentationHelper.ENABLED)
                InstrumentationHelper.Get().QStimulantEvent(eventBean, _services.EngineURI);

            try
            {
                if (_internalEventRouter.HasPreprocessing)
                {
                    eventBean = _internalEventRouter.Preprocess(eventBean, _engineFilterAndDispatchTimeContext);
                    if (eventBean == null)
                    {
                        return;
                    }
                }

                // Acquire main processing lock which locks out statement management
                if (InstrumentationHelper.ENABLED)
                    InstrumentationHelper.Get().QEvent(eventBean, _services.EngineURI, true);

                try
                {
                    using (_services.EventProcessingRWLock.AcquireReadLock()) {
                        try {
                            ProcessMatches(eventBean);
                        }
                        catch (EPException) {
                            throw;
                        }
                        catch (Exception ex) {
                            ThreadData.MatchesArrayThreadLocal.Clear();
                            throw new EPException(ex);
                        }
                    }
                }
                finally
                {
                    if (InstrumentationHelper.ENABLED)
                        InstrumentationHelper.Get().AEvent();
                }

                // Dispatch results to listeners
                // Done outside of the read-lock to prevent lockups when listeners create statements
                Dispatch();

                // Work off the event queue if any events accumulated in there via a Route() or insert-into
                ProcessThreadWorkQueue();
            }
            finally
            {
                if (InstrumentationHelper.ENABLED)
                    InstrumentationHelper.Get().AStimulantEvent();
            }
        }

        private void ProcessTimeEvent(TimerEvent theEvent)
        {
            if (theEvent is TimerControlEvent)
            {
                var timerControlEvent = (TimerControlEvent)theEvent;
                if (timerControlEvent.ClockType == TimerControlEvent.ClockTypeEnum.CLOCK_INTERNAL)
                {
                    // Start internal clock which supplies CurrentTimeEvent events every 100ms
                    // This may be done without delay thus the write lock indeed must be reentrant.
                    if (_services.ConfigSnapshot.EngineDefaults.TimeSource.TimeUnit != TimeUnit.MILLISECONDS)
                    {
                        throw new EPException("Internal timer requires millisecond time resolution");
                    }

                    _services.TimerService.StartInternalClock();
                    _isUsingExternalClocking = false;
                }
                else
                {
                    // Stop internal clock, for unit testing and for external clocking
                    _services.TimerService.StopInternalClock(true);
                    _isUsingExternalClocking = true;
                }

                return;
            }

            if (theEvent is CurrentTimeEvent)
            {
                var current = (CurrentTimeEvent)theEvent;
                var timeInMillis = current.Time;

                if (InstrumentationHelper.ENABLED)
                    InstrumentationHelper.Get().QStimulantTime(timeInMillis, _services.EngineURI);

                try
                {
                    // Evaluation of all time events is protected from statement management
                    if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled) && (ExecutionPathDebugLog.IsTimerDebugEnabled))
                    {
                        Log.Debug(".processTimeEvent Setting time and evaluating schedules for time " + timeInMillis);
                    }

                    if (_isUsingExternalClocking && (timeInMillis == _services.SchedulingService.Time))
                    {
                        if (Log.IsWarnEnabled)
                        {
                            Log.Warn("Duplicate time event received for currentTime " + timeInMillis);
                        }
                    }
                    _services.SchedulingService.Time = timeInMillis;

                    if (MetricReportingPath.IsMetricsEnabled)
                    {
                        _services.MetricsReportingService.ProcessTimeEvent(timeInMillis);
                    }

                    ProcessSchedule(timeInMillis);

                    // Let listeners know of results
                    Dispatch();

                    // Work off the event queue if any events accumulated in there via a Route()
                    ProcessThreadWorkQueue();
                }
                finally
                {
                    if (InstrumentationHelper.ENABLED)
                        InstrumentationHelper.Get().AStimulantTime();
                }

                return;
            }

            // handle time span
            var span = (CurrentTimeSpanEvent)theEvent;
            var targetTime = span.TargetTime;
            var currentTime = _services.SchedulingService.Time;
            var optionalResolution = span.OptionalResolution;

            if (_isUsingExternalClocking && (targetTime < currentTime))
            {
                if (Log.IsWarnEnabled)
                {
                    Log.Warn("Past or current time event received for currentTime " + targetTime);
                }
            }

            // Evaluation of all time events is protected from statement management
            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled) && (ExecutionPathDebugLog.IsTimerDebugEnabled))
            {
                Log.Debug(".processTimeEvent Setting time span and evaluating schedules for time " + targetTime + " optional resolution " + span.OptionalResolution);
            }

            while (currentTime < targetTime)
            {
                if ((optionalResolution != null) && (optionalResolution > 0))
                {
                    currentTime += optionalResolution.Value;
                }
                else
                {
                    var nearest = _services.SchedulingService.NearestTimeHandle;
                    currentTime = nearest == null ? targetTime : nearest.Value;
                }
                if (currentTime > targetTime)
                {
                    currentTime = targetTime;
                }

                if (InstrumentationHelper.ENABLED)
                    InstrumentationHelper.Get().QStimulantTime(currentTime, _services.EngineURI);

                try
                {
                    // Evaluation of all time events is protected from statement management
                    if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled) &&
                        (ExecutionPathDebugLog.IsTimerDebugEnabled))
                    {
                        Log.Debug(".processTimeEvent Setting time and evaluating schedules for time " + currentTime);
                    }

                    _services.SchedulingService.Time = currentTime;

                    if (MetricReportingPath.IsMetricsEnabled)
                    {
                        _services.MetricsReportingService.ProcessTimeEvent(currentTime);
                    }

                    ProcessSchedule(currentTime);

                    // Let listeners know of results
                    Dispatch();

                    // Work off the event queue if any events accumulated in there via a Route()
                    ProcessThreadWorkQueue();
                }
                finally
                {
                    if (InstrumentationHelper.ENABLED)
                        InstrumentationHelper.Get().AStimulantTime();
                }
            }
        }

        private void ProcessSchedule(long time)
        {
            if (InstrumentationHelper.ENABLED)
                InstrumentationHelper.Get().QTime(time, _services.EngineURI);

            try
            {
                var handles = ScheduleArray;

                // Evaluation of schedules is protected by an optional scheduling service lock and then the engine lock
                // We want to stay in this order for allowing the engine lock as a second-order lock to the
                // services own lock, if it has one.
                using (_services.EventProcessingRWLock.AcquireReadLock())
                {
                    _services.SchedulingService.Evaluate(handles);
                }

                using (_services.EventProcessingRWLock.AcquireReadLock())
                {
                    try
                    {
                        ProcessScheduleHandles(handles);
                    }
                    catch (Exception)
                    {
                        handles.Clear();
                        throw;
                    }
                }
            }
            finally
            {
                if (InstrumentationHelper.ENABLED)
                    InstrumentationHelper.Get().ATime();
            }
        }

        public void ProcessScheduleHandles(ArrayBackedCollection<ScheduleHandle> handles)
        {
            if (ThreadLogUtil.ENABLED_TRACE)
            {
                ThreadLogUtil.Trace("Found schedules for", handles.Count);
            }

            if (handles.Count == 0)
            {
                return;
            }

            // handle 1 result separately for performance reasons
            if (handles.Count == 1)
            {
                var handleArray = handles.Array;
                var handle = (EPStatementHandleCallback)handleArray[0];

                if ((MetricReportingPath.IsMetricsEnabled) && (handle.AgentInstanceHandle.StatementHandle.MetricsHandle.IsEnabled))
                {
                    handle.AgentInstanceHandle.StatementHandle.MetricsHandle.Call(
                        _services.MetricsReportingService.PerformanceCollector,
                        () => ProcessStatementScheduleSingle(handle, _services));
                }
                else
                {
                    if ((ThreadingOption.IsThreadingEnabledValue) && (_services.ThreadingService.IsTimerThreading))
                    {
                        _services.ThreadingService.SubmitTimerWork(
                            new TimerUnitSingle(_services, this, handle).Run);
                    }
                    else
                    {
                        ProcessStatementScheduleSingle(handle, _services);
                    }
                }

                handles.Clear();
                return;
            }

            var matchArray = handles.Array;
            var entryCount = handles.Count;

            // sort multiple matches for the event into statements
            var stmtCallbacks = SchedulePerStmt;
            stmtCallbacks.Clear();
            for (var i = 0; i < entryCount; i++)
            {
                var handleCallback = (EPStatementHandleCallback)matchArray[i];
                var handle = handleCallback.AgentInstanceHandle;
                var callback = handleCallback.ScheduleCallback;

                var entry = stmtCallbacks.Get(handle);

                // This statement has not been encountered before
                if (entry == null)
                {
                    stmtCallbacks.Put(handle, callback);
                    continue;
                }

                // This statement has been encountered once before
                if (entry is ScheduleHandleCallback)
                {
                    var existingCallback = (ScheduleHandleCallback)entry;
                    var stmtEntries = new LinkedList<ScheduleHandleCallback>();
                    stmtEntries.AddLast(existingCallback);
                    stmtEntries.AddLast(callback);
                    stmtCallbacks.Put(handle, stmtEntries);
                    continue;
                }

                // This statement has been encountered more then once before
                var entries = (LinkedList<ScheduleHandleCallback>)entry;
                entries.AddLast(callback);
            }
            handles.Clear();

            foreach (var entry in stmtCallbacks)
            {
                var handle = entry.Key;
                var callbackObject = entry.Value;

                if ((MetricReportingPath.IsMetricsEnabled) && (handle.StatementHandle.MetricsHandle.IsEnabled))
                {
                    var numInput = callbackObject is ICollection ? ((ICollection)callbackObject).Count : 0;

                    handle.StatementHandle.MetricsHandle.Call(
                        _services.MetricsReportingService.PerformanceCollector,
                        () => ProcessStatementScheduleMultiple(handle, callbackObject, _services),
                        numInput);
                }
                else
                {
                    if ((ThreadingOption.IsThreadingEnabledValue) && (_services.ThreadingService.IsTimerThreading))
                    {
                        _services.ThreadingService.SubmitTimerWork(
                            new TimerUnitMultiple(_services, this, handle, callbackObject).Run);
                    }
                    else
                    {
                        ProcessStatementScheduleMultiple(handle, callbackObject, _services);
                    }
                }

                if ((_isPrioritized) && (handle.IsPreemptive))
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Works off the thread's work queue.
        /// </summary>
        public void ProcessThreadWorkQueue()
        {
            var queues = _threadWorkQueue.ThreadQueue;

            if (queues.FrontQueue.Peek() == null)
            {
                var haveDispatched = _services.NamedWindowDispatchService.Dispatch();
                if (haveDispatched)
                {
                    // Dispatch results to listeners
                    Dispatch();

                    if (queues.FrontQueue.Peek() != null)
                    {
                        ProcessThreadWorkQueueFront(queues);
                    }
                }
            }
            else
            {
                ProcessThreadWorkQueueFront(queues);
            }

            Object item;
            while ((item = queues.BackQueue.Poll()) != null)
            {
                if (item is InsertIntoLatchSpin)
                {
                    ProcessThreadWorkQueueLatchedSpin((InsertIntoLatchSpin)item);
                }
                else if (item is InsertIntoLatchWait)
                {
                    ProcessThreadWorkQueueLatchedWait((InsertIntoLatchWait)item);
                }
                else
                {
                    ProcessThreadWorkQueueUnlatched(item);
                }

                var haveDispatched = _services.NamedWindowDispatchService.Dispatch();
                if (haveDispatched)
                {
                    Dispatch();
                }

                if (queues.FrontQueue.Peek() != null)
                {
                    ProcessThreadWorkQueueFront(queues);
                }
            }
        }

        private void ProcessThreadWorkQueueFront(DualWorkQueue<object> queues)
        {
            Object item;
            while ((item = queues.FrontQueue.Poll()) != null)
            {
                if (item is InsertIntoLatchSpin)
                {
                    ProcessThreadWorkQueueLatchedSpin((InsertIntoLatchSpin)item);
                }
                else if (item is InsertIntoLatchWait)
                {
                    ProcessThreadWorkQueueLatchedWait((InsertIntoLatchWait)item);
                }
                else
                {
                    ProcessThreadWorkQueueUnlatched(item);
                }

                var haveDispatched = _services.NamedWindowDispatchService.Dispatch();
                if (haveDispatched)
                {
                    Dispatch();
                }
            }
        }

        private void ProcessThreadWorkQueueLatchedWait(InsertIntoLatchWait insertIntoLatch)
        {
            // wait for the latch to complete
            var eventBean = insertIntoLatch.Await();

            if (InstrumentationHelper.ENABLED)
                InstrumentationHelper.Get().QEvent(eventBean, _services.EngineURI, false);

            try
            {
                using (_services.EventProcessingRWLock.AcquireReadLock())
                {
                    try
                    {
                        ProcessMatches(eventBean);
                    }
                    catch (Exception)
                    {
                        ThreadData.MatchesArrayThreadLocal.Clear();
                        throw;
                    }
                    finally
                    {
                        insertIntoLatch.Done();
                    }
                }
            }
            finally
            {
                if (InstrumentationHelper.ENABLED)
                    InstrumentationHelper.Get().AEvent();
            }

            Dispatch();
        }

        private void ProcessThreadWorkQueueLatchedSpin(InsertIntoLatchSpin insertIntoLatch)
        {
            // wait for the latch to complete
            var eventBean = insertIntoLatch.Await();

            if (InstrumentationHelper.ENABLED)
                InstrumentationHelper.Get().QEvent(eventBean, _services.EngineURI, false);

            try
            {
                using (_services.EventProcessingRWLock.AcquireReadLock())
                {
                    try
                    {
                        ProcessMatches(eventBean);
                    }
                    catch (Exception)
                    {
                        ThreadData.MatchesArrayThreadLocal.Clear();
                        throw;
                    }
                    finally
                    {
                        insertIntoLatch.Done();
                    }
                }
            }
            finally
            {
                if (InstrumentationHelper.ENABLED)
                    InstrumentationHelper.Get().AEvent();
            }

            Dispatch();
        }

        private void ProcessThreadWorkQueueUnlatched(Object item)
        {
            EventBean eventBean;
            if (item is EventBean)
            {
                eventBean = (EventBean)item;
            }
            else
            {
                eventBean = _services.EventAdapterService.AdapterForObject(item);
            }

            if (InstrumentationHelper.ENABLED)
                InstrumentationHelper.Get().QEvent(eventBean, _services.EngineURI, false);

            try
            {
                using (_services.EventProcessingRWLock.AcquireReadLock())
                {
                    try
                    {
                        ProcessMatches(eventBean);
                    }
                    catch (Exception)
                    {
                        ThreadData.MatchesArrayThreadLocal.Clear();
                        throw;
                    }
                }
            }
            finally
            {
                if (InstrumentationHelper.ENABLED)
                    InstrumentationHelper.Get().AEvent();
            }

            Dispatch();
        }

        protected internal void ProcessMatches(EventBean theEvent)
        {
            var localData = ThreadData;

            // get matching filters
            var matches = localData.MatchesArrayThreadLocal;
            var version = _services.FilterService.Evaluate(theEvent, matches);

            if (ThreadLogUtil.ENABLED_TRACE)
            {
                ThreadLogUtil.Trace("Found matches for underlying ", matches.Count, theEvent.Underlying);
            }

            if (matches.Count == 0)
            {
                if (UnmatchedEvent != null)
                {
                    using (_services.EventProcessingRWLock.ReadLock.ReleaseAcquire()) // Allow listener to create new statements
                    {
                        try
                        {
                            UnmatchedEvent.Invoke(this, new UnmatchedEventArgs(theEvent));
                        }
                        catch (Exception ex)
                        {
                            Log.Error("Exception thrown by unmatched listener: " + ex.Message, ex);
                        }
                    }
                }
                return;
            }

            var stmtCallbacks = localData.MatchesPerStmtThreadLocal;
            var matchesCount = matches.Count;

            for (var i = 0; i < matchesCount; i++)
            {
                var handleCallback = (EPStatementHandleCallback)matches[i];
                var handle = handleCallback.AgentInstanceHandle;

                // Self-joins require that the internal dispatch happens after all streams are evaluated.
                // Priority or preemptive settings also require special ordering.
                if (handle.CanSelfJoin || _isPrioritized)
                {
                    object callbacks;
                    if (!stmtCallbacks.TryGetValue(handle, out callbacks))
                    {
                        stmtCallbacks.Put(handle, handleCallback.FilterCallback);
                    }
                    else if (callbacks is LinkedList<FilterHandleCallback>)
                    {
                        var q = (LinkedList<FilterHandleCallback>)callbacks;
                        q.AddLast(handleCallback.FilterCallback);
                    }
                    else
                    {
                        var q = new LinkedList<FilterHandleCallback>();
                        q.AddLast((FilterHandleCallback)callbacks);
                        q.AddLast(handleCallback.FilterCallback);
                        stmtCallbacks.Put(handle, q);
                    }

                    continue;
                }

                if ((MetricReportingPath.IsMetricsEnabled) && (handle.StatementHandle.MetricsHandle.IsEnabled))
                {
                    handle.StatementHandle.MetricsHandle.Call(
                        _services.MetricsReportingService.PerformanceCollector,
                        () => ProcessStatementFilterSingle(handle, handleCallback, theEvent, version));
                }
                else
                {
                    if ((ThreadingOption.IsThreadingEnabledValue) && (_services.ThreadingService.IsRouteThreading))
                    {
                        _services.ThreadingService.SubmitRoute(
                            new RouteUnitSingle(this, handleCallback, theEvent, version).Run);
                    }
                    else
                    {
                        ProcessStatementFilterSingle(handle, handleCallback, theEvent, version);
                    }
                }
            }

            matches.Clear();

            if (stmtCallbacks.Count == 0)
            {
                return;
            }

            foreach (var entry in stmtCallbacks)
            {
                var handle = entry.Key;
                var callbackList = entry.Value;

                if ((MetricReportingPath.IsMetricsEnabled) && (handle.StatementHandle.MetricsHandle.IsEnabled))
                {
                    var count = 1;
                    if (callbackList is ICollection)
                        count = ((ICollection)callbackList).Count;

                    handle.StatementHandle.MetricsHandle.Call(
                        _services.MetricsReportingService.PerformanceCollector,
                        () => ProcessStatementFilterMultiple(handle, callbackList, theEvent, version),
                        count);
                }
                else
                {
                    if ((ThreadingOption.IsThreadingEnabledValue) && (_services.ThreadingService.IsRouteThreading))
                    {
                        _services.ThreadingService.SubmitRoute(
                            new RouteUnitMultiple(this, callbackList, theEvent, handle, version).Run);
                    }
                    else
                    {
                        ProcessStatementFilterMultiple(handle, callbackList, theEvent, version);
                    }

                    if ((_isPrioritized) && (handle.IsPreemptive))
                    {
                        break;
                    }
                }
            }
            stmtCallbacks.Clear();
        }

        /// <summary>
        /// Processing multiple schedule matches for a statement.
        /// </summary>
        /// <param name="handle">statement handle</param>
        /// <param name="callbackObject">object containing matches</param>
        /// <param name="services">engine services</param>
        public static void ProcessStatementScheduleMultiple(EPStatementAgentInstanceHandle handle, Object callbackObject, EPServicesContext services)
        {
            if (InstrumentationHelper.ENABLED)
                InstrumentationHelper.Get().QTimeCP(handle, services.SchedulingService.Time);

            try
            {
                using (handle.StatementAgentInstanceLock.AcquireWriteLock())
                {
                    try
                    {
                        if (!handle.IsDestroyed)
                        {
                            if (handle.HasVariables)
                            {
                                services.VariableService.SetLocalVersion();
                            }

                            if (callbackObject is LinkedList<ScheduleHandleCallback>)
                            {
                                var callbackList = (LinkedList<ScheduleHandleCallback>)callbackObject;
                                foreach (var callback in callbackList)
                                {
                                    callback.ScheduledTrigger(services.EngineLevelExtensionServicesContext);
                                }
                            }
                            else
                            {
                                var callback = (ScheduleHandleCallback)callbackObject;
                                callback.ScheduledTrigger(services.EngineLevelExtensionServicesContext);
                            }

                            // internal join processing, if applicable
                            handle.InternalDispatch();
                        }
                    }
                    catch (Exception ex)
                    {
                        services.ExceptionHandlingService.HandleException(ex, handle, ExceptionHandlerExceptionType.PROCESS, null);
                    }
                    finally
                    {
                        if (handle.HasTableAccess)
                        {
                            services.TableService.TableExprEvaluatorContext.ReleaseAcquiredLocks();
                        }
                    }
                }
            }
            finally
            {
                if (InstrumentationHelper.ENABLED)
                    InstrumentationHelper.Get().ATimeCP();
            }
        }

        /// <summary>
        /// Processing single schedule matche for a statement.
        /// </summary>
        /// <param name="handle">statement handle</param>
        /// <param name="services">engine services</param>
        public static void ProcessStatementScheduleSingle(EPStatementHandleCallback handle, EPServicesContext services)
        {
            if (InstrumentationHelper.ENABLED)
                InstrumentationHelper.Get().QTimeCP(handle.AgentInstanceHandle, services.SchedulingService.Time);

            try
            {
                using (handle.AgentInstanceHandle.StatementAgentInstanceLock.AcquireWriteLock())
                {
                    try
                    {
                        if (!handle.AgentInstanceHandle.IsDestroyed)
                        {
                            if (handle.AgentInstanceHandle.HasVariables)
                            {
                                services.VariableService.SetLocalVersion();
                            }

                            handle.ScheduleCallback.ScheduledTrigger(services.EngineLevelExtensionServicesContext);
                            handle.AgentInstanceHandle.InternalDispatch();
                        }
                    }
                    catch (Exception ex)
                    {
                        services.ExceptionHandlingService.HandleException(ex, handle.AgentInstanceHandle, ExceptionHandlerExceptionType.PROCESS, null);
                    }
                    finally
                    {
                        if (handle.AgentInstanceHandle.HasTableAccess)
                        {
                            services.TableService.TableExprEvaluatorContext.ReleaseAcquiredLocks();
                        }
                    }
                }
            }
            finally
            {
                if (InstrumentationHelper.ENABLED)
                    InstrumentationHelper.Get().ATimeCP();
            }
        }

        /// <summary>
        /// Processing multiple filter matches for a statement.
        /// </summary>
        /// <param name="handle">statement handle</param>
        /// <param name="callbackList">object containing callbacks</param>
        /// <param name="theEvent">to process</param>
        /// <param name="version">filter version</param>
        public void ProcessStatementFilterMultiple(EPStatementAgentInstanceHandle handle, Object callbackList, EventBean theEvent, long version)
        {
            if (InstrumentationHelper.ENABLED)
                InstrumentationHelper.Get().QEventCP(theEvent, handle, _services.SchedulingService.Time);

            try
            {
                using (handle.StatementAgentInstanceLock.AcquireWriteLock())
                {
                    try
                    {
                        if (handle.HasVariables)
                        {
                            _services.VariableService.SetLocalVersion();
                        }
                        if (!handle.IsCurrentFilter(version))
                        {
                            var handled = false;
                            if (handle.FilterFaultHandler != null)
                            {
                                handled = handle.FilterFaultHandler.HandleFilterFault(theEvent, version);
                            }
                            if (!handled)
                            {
                                HandleFilterFault(handle, theEvent);
                            }
                        }
                        else
                        {
                            if (callbackList is ICollection<FilterHandleCallback>)
                            {
                                var callbacks = (ICollection<FilterHandleCallback>)callbackList;
                                handle.MultiMatchHandler.Handle(callbacks, theEvent);
                            }
                            else
                            {
                                var single = (FilterHandleCallback)callbackList;
                                single.MatchFound(theEvent, null);
                            }

                            // internal join processing, if applicable
                            handle.InternalDispatch();
                        }
                    }
                    catch (Exception ex)
                    {
                        _services.ExceptionHandlingService.HandleException(ex, handle, ExceptionHandlerExceptionType.PROCESS, theEvent);
                    }
                    finally
                    {
                        if (handle.HasTableAccess)
                        {
                            _services.TableService.TableExprEvaluatorContext.ReleaseAcquiredLocks();
                        }
                    }
                }
            }
            finally
            {
                if (InstrumentationHelper.ENABLED)
                    InstrumentationHelper.Get().AEventCP();
            }
        }

        /// <summary>Process a single match. </summary>
        /// <param name="handle">statement</param>
        /// <param name="handleCallback">callback</param>
        /// <param name="theEvent">event to indicate</param>
        /// <param name="version">filter version</param>
        public void ProcessStatementFilterSingle(EPStatementAgentInstanceHandle handle, EPStatementHandleCallback handleCallback, EventBean theEvent, long version)
        {
            if (InstrumentationHelper.ENABLED)
                InstrumentationHelper.Get().QEventCP(theEvent, handle, _services.SchedulingService.Time);

            try
            {
                using (handle.StatementAgentInstanceLock.AcquireWriteLock())
                {
                    try
                    {
                        if (handle.HasVariables)
                        {
                            _services.VariableService.SetLocalVersion();
                        }
                        if (!handle.IsCurrentFilter(version))
                        {
                            var handled = false;
                            if (handle.FilterFaultHandler != null)
                            {
                                handled = handle.FilterFaultHandler.HandleFilterFault(theEvent, version);
                            }
                            if (!handled)
                            {
                                HandleFilterFault(handle, theEvent);
                            }
                        }
                        else
                        {
                            handleCallback.FilterCallback.MatchFound(theEvent, null);
                        }

                        // internal join processing, if applicable
                        handle.InternalDispatch();
                    }
                    catch (Exception ex)
                    {
                        _services.ExceptionHandlingService.HandleException(ex, handle, ExceptionHandlerExceptionType.PROCESS, theEvent);
                    }
                    finally
                    {
                        if (handle.HasTableAccess)
                        {
                            _services.TableService.TableExprEvaluatorContext.ReleaseAcquiredLocks();
                        }
                    }
                }
            }
            finally
            {
                if (InstrumentationHelper.ENABLED)
                    InstrumentationHelper.Get().AEventCP();
            }
        }

        protected internal void HandleFilterFault(EPStatementAgentInstanceHandle faultingHandle, EventBean theEvent)
        {
            var callbacksForStatement = new ArrayDeque<FilterHandle>();
            var version = _services.FilterService.Evaluate(theEvent, callbacksForStatement, faultingHandle.StatementId);

            if (callbacksForStatement.Count == 1)
            {
                var handleCallback = (EPStatementHandleCallback)callbacksForStatement.First;
                ProcessStatementFilterSingle(handleCallback.AgentInstanceHandle, handleCallback, theEvent, version);
                return;
            }
            if (callbacksForStatement.Count == 0)
            {
                return;
            }

            IDictionary<EPStatementAgentInstanceHandle, Object> stmtCallbacks;
            if (_isPrioritized)
            {
                stmtCallbacks = new SortedDictionary<EPStatementAgentInstanceHandle, Object>(EPStatementAgentInstanceHandleComparator.Instance);
            }
            else
            {
                stmtCallbacks = new Dictionary<EPStatementAgentInstanceHandle, Object>();
            }

            foreach (var filterHandle in callbacksForStatement)
            {
                var handleCallback = (EPStatementHandleCallback)filterHandle;
                var handle = handleCallback.AgentInstanceHandle;

                if (handle.CanSelfJoin || _isPrioritized)
                {
                    var callbacks = stmtCallbacks.Get(handle);
                    if (callbacks == null)
                    {
                        stmtCallbacks.Put(handle, handleCallback.FilterCallback);
                    }
                    else if (callbacks is LinkedList<FilterHandleCallback>)
                    {
                        var q = (LinkedList<FilterHandleCallback>)callbacks;
                        q.AddLast(handleCallback.FilterCallback);
                    }
                    else
                    {
                        var q = new LinkedList<FilterHandleCallback>();
                        q.AddLast((FilterHandleCallback)callbacks);
                        q.AddLast(handleCallback.FilterCallback);
                        stmtCallbacks.Put(handle, q);
                    }
                    continue;
                }

                ProcessStatementFilterSingle(handle, handleCallback, theEvent, version);
            }

            if (stmtCallbacks.IsEmpty())
            {
                return;
            }

            foreach (var entry in stmtCallbacks)
            {
                var handle = entry.Key;
                var callbackList = entry.Value;

                ProcessStatementFilterMultiple(handle, callbackList, theEvent, version);

                if ((_isPrioritized) && (handle.IsPreemptive))
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Dispatch events.
        /// </summary>
        public void Dispatch()
        {
            try
            {
                _services.DispatchService.Dispatch();
            }
            catch (Exception ex)
            {
                throw new EPException(ex);
            }
        }

        public bool IsExternalClockingEnabled
        {
            get => _isUsingExternalClocking;
        }

        /// <summary>
        /// Dispose for destroying an engine instance: sets references to null and clears thread-locals
        /// </summary>
        public void Dispose()
        {
            _services = null;
            _threadLocalData.Dispose();
            _threadLocalData = null;
        }

        public void Initialize()
        {
            InitThreadLocals();
            _threadWorkQueue = new ThreadWorkQueue(
                _services.ThreadLocalManager);
        }

        public void ClearCaches()
        {
            InitThreadLocals();
        }

        public void SetVariableValue(String variableName, Object variableValue)
        {
            VariableMetaData metaData = _services.VariableService.GetVariableMetaData(variableName);
            CheckVariable(variableName, metaData, true, false);

            using (_services.VariableService.ReadWriteLock.AcquireWriteLock())
            {
                _services.VariableService.CheckAndWrite(variableName, EPStatementStartMethodConst.DEFAULT_AGENT_INSTANCE_ID, variableValue);
                _services.VariableService.Commit();
            }
        }

        public void SetVariableValue(IDictionary<String, Object> variableValues)
        {
            SetVariableValueInternal(variableValues, EPStatementStartMethodConst.DEFAULT_AGENT_INSTANCE_ID, false);
        }

        public void SetVariableValue(IDictionary<String, Object> variableValues, int agentInstanceId)
        {
            SetVariableValueInternal(variableValues, agentInstanceId, true);
        }

        public Object GetVariableValue(String variableName)
        {
            _services.VariableService.SetLocalVersion();
            VariableMetaData metaData = _services.VariableService.GetVariableMetaData(variableName);
            if (metaData == null)
            {
                throw new VariableNotFoundException("Variable by name '" + variableName + "' has not been declared");
            }
            if (metaData.ContextPartitionName != null)
            {
                throw new VariableNotFoundException("Variable by name '" + variableName + "' has been declared for context '" + metaData.ContextPartitionName + "' and cannot be read without context partition selector");
            }
            VariableReader reader = _services.VariableService.GetReader(variableName, EPStatementStartMethodConst.DEFAULT_AGENT_INSTANCE_ID);
            Object value = reader.Value;
            if (value == null || reader.VariableMetaData.EventType == null)
            {
                return value;
            }
            return ((EventBean)value).Underlying;
        }

        public IDictionary<String, IList<ContextPartitionVariableState>> GetVariableValue(ISet<String> variableNames, ContextPartitionSelector contextPartitionSelector)
        {
            _services.VariableService.SetLocalVersion();
            String contextPartitionName = null;
            foreach (String variableName in variableNames)
            {
                VariableMetaData metaData = _services.VariableService.GetVariableMetaData(variableName);
                if (metaData == null)
                {
                    throw new VariableNotFoundException("Variable by name '" + variableName + "' has not been declared");
                }
                if (metaData.ContextPartitionName == null)
                {
                    throw new VariableNotFoundException("Variable by name '" + variableName + "' is a global variable and not context-partitioned");
                }
                if (contextPartitionName == null)
                {
                    contextPartitionName = metaData.ContextPartitionName;
                }
                else
                {
                    if (!contextPartitionName.Equals(metaData.ContextPartitionName))
                    {
                        throw new VariableNotFoundException("Variable by name '" + variableName + "' is a declared for context '" + metaData.ContextPartitionName + "' however the expected context is '" + contextPartitionName + "'");
                    }
                }
            }
            ContextManager contextManager = _services.ContextManagementService.GetContextManager(contextPartitionName);
            if (contextManager == null)
            {
                throw new VariableNotFoundException("Context by name '" + contextPartitionName + "' cannot be found");
            }
            IDictionary<int, ContextPartitionDescriptor> contextPartitions = contextManager.ExtractPaths(contextPartitionSelector).ContextPartitionInformation;
            if (contextPartitions.IsEmpty())
            {
                return Collections.GetEmptyMap<string, IList<ContextPartitionVariableState>>();
            }
            IDictionary<String, IList<ContextPartitionVariableState>> statesMap = new Dictionary<String, IList<ContextPartitionVariableState>>();
            foreach (String variableName in variableNames)
            {
                var states = new List<ContextPartitionVariableState>();
                statesMap.Put(variableName, states);
                foreach (var entry in contextPartitions)
                {
                    VariableReader reader = _services.VariableService.GetReader(variableName, entry.Key);
                    Object value = reader.Value;
                    if (value != null && reader.VariableMetaData.EventType != null)
                    {
                        value = ((EventBean)value).Underlying;
                    }
                    states.Add(new ContextPartitionVariableState(entry.Key, entry.Value.Identifier, value));
                }
            }
            return statesMap;
        }

        public IDictionary<String, Object> GetVariableValue(ICollection<String> variableNames)
        {
            _services.VariableService.SetLocalVersion();
            IDictionary<String, Object> values = new Dictionary<String, Object>();
            foreach (var variableName in variableNames)
            {
                VariableMetaData metaData = _services.VariableService.GetVariableMetaData(variableName);
                CheckVariable(variableName, metaData, false, false);
                VariableReader reader = _services.VariableService.GetReader(variableName, EPStatementStartMethodConst.DEFAULT_AGENT_INSTANCE_ID);
                if (reader == null)
                {
                    throw new VariableNotFoundException("Variable by name '" + variableName + "' has not been declared");
                }

                var value = reader.Value;
                if (value != null && reader.VariableMetaData.EventType != null)
                {
                    value = ((EventBean)value).Underlying;
                }
                values.Put(variableName, value);
            }
            return values;
        }

        public DataMap VariableValueAll
        {
            get
            {
                _services.VariableService.SetLocalVersion();
                IDictionary<String, VariableReader> variables = _services.VariableService.VariableReadersNonCP;
                var values = new Dictionary<String, Object>();
                foreach (var entry in variables)
                {
                    var value = entry.Value.Value;
                    values.Put(entry.Value.VariableMetaData.VariableName, value);
                }
                return values;
            }
        }

        public IDictionary<string, Type> VariableTypeAll
        {
            get
            {
                IDictionary<String, VariableReader> variables =
                    _services.VariableService.VariableReadersNonCP;
                var values = new Dictionary<String, Type>();
                foreach (var entry in variables)
                {
                    var type = entry.Value.VariableMetaData.VariableType;
                    values.Put(entry.Value.VariableMetaData.VariableName, type);
                }
                return values;
            }
        }

        public Type GetVariableType(String variableName)
        {
            VariableMetaData metaData = _services.VariableService.GetVariableMetaData(variableName);
            if (metaData == null)
            {
                return null;
            }
            return metaData.VariableType;
        }

        public EPOnDemandQueryResult ExecuteQuery(String epl, ContextPartitionSelector[] contextPartitionSelectors)
        {
            if (contextPartitionSelectors == null)
            {
                throw new ArgumentException("No context partition selectors provided");
            }
            return ExecuteQueryInternal(epl, null, null, contextPartitionSelectors);
        }

        public EPOnDemandQueryResult ExecuteQuery(String epl)
        {
            return ExecuteQueryInternal(epl, null, null, null);
        }

        public EPOnDemandQueryResult ExecuteQuery(EPStatementObjectModel model)
        {
            return ExecuteQueryInternal(null, model, null, null);
        }

        public EPOnDemandQueryResult ExecuteQuery(EPStatementObjectModel model, ContextPartitionSelector[] contextPartitionSelectors)
        {
            if (contextPartitionSelectors == null)
            {
                throw new ArgumentException("No context partition selectors provided");
            }
            return ExecuteQueryInternal(null, model, null, contextPartitionSelectors);
        }

        public EPOnDemandQueryResult ExecuteQuery(EPOnDemandPreparedQueryParameterized parameterizedQuery)
        {
            return ExecuteQueryInternal(null, null, parameterizedQuery, null);
        }

        public EPOnDemandQueryResult ExecuteQuery(EPOnDemandPreparedQueryParameterized parameterizedQuery, ContextPartitionSelector[] contextPartitionSelectors)
        {
            return ExecuteQueryInternal(null, null, parameterizedQuery, contextPartitionSelectors);
        }

        private EPOnDemandQueryResult ExecuteQueryInternal(String epl, EPStatementObjectModel model, EPOnDemandPreparedQueryParameterized parameterizedQuery, ContextPartitionSelector[] contextPartitionSelectors)
        {
            try
            {
                var executeMethod = GetExecuteMethod(epl, model, parameterizedQuery);
                var result = executeMethod.Execute(contextPartitionSelectors);
                return new EPQueryResultImpl(result);
            }
            catch (EPStatementException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var message = "Error executing statement: " + ex.Message;
                Log.Info(message, ex);
                throw new EPStatementException(message, epl, ex);
            }
        }

        public EPOnDemandPreparedQuery PrepareQuery(String epl)
        {
            return PrepareQueryInternal(epl, null);
        }

        public EPOnDemandPreparedQuery PrepareQuery(EPStatementObjectModel model)
        {
            return PrepareQueryInternal(null, model);
        }

        public EPOnDemandPreparedQueryParameterized PrepareQueryWithParameters(String epl)
        {
            // compile to specification
            var stmtName = UuidGenerator.Generate();
            var statementSpec = EPAdministratorHelper.CompileEPL(epl, epl, true, stmtName, _services, SelectClauseStreamSelectorEnum.ISTREAM_ONLY);

            // map to object model thus finding all substitution parameters and their indexes
            var unmapped = StatementSpecMapper.Unmap(statementSpec);

            // the prepared statement is the object model plus a list of substitution parameters
            // map to specification will refuse any substitution parameters that are unfilled
            return new EPPreparedStatementImpl(unmapped.ObjectModel, unmapped.SubstitutionParams, epl);
        }

        private EPOnDemandPreparedQuery PrepareQueryInternal(String epl, EPStatementObjectModel model)
        {
            try
            {
                var startMethod = GetExecuteMethod(epl, model, null);
                return new EPPreparedQueryImpl(startMethod, epl);
            }
            catch (EPStatementException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var message = "Error executing statement: " + ex.Message;
                Log.Debug(message, ex);
                throw new EPStatementException(message, epl);
            }
        }

        private EPPreparedExecuteMethod GetExecuteMethod(String epl, EPStatementObjectModel model, EPOnDemandPreparedQueryParameterized parameterizedQuery)
        {
            var stmtName = UuidGenerator.Generate();
            var stmtId = -1;

            try
            {
                StatementSpecRaw spec;
                if (epl != null)
                {
                    spec = EPAdministratorHelper.CompileEPL(epl, epl, true, stmtName, _services, SelectClauseStreamSelectorEnum.ISTREAM_ONLY);
                }
                else if (model != null) {
                    spec = StatementSpecMapper.Map(
                        _services.Container,
                        model,
                        _services.EngineImportService,
                        _services.VariableService,
                        _services.ConfigSnapshot,
                        _services.SchedulingService,
                        _services.EngineURI,
                        _services.PatternNodeFactory,
                        _services.NamedWindowMgmtService,
                        _services.ContextManagementService,
                        _services.ExprDeclaredService,
                        _services.TableService);
                    epl = model.ToEPL();
                }
                else
                {
                    var prepared = (EPPreparedStatementImpl)parameterizedQuery;
                    spec = StatementSpecMapper.Map(
                        _services.Container,
                        prepared.Model,
                        _services.EngineImportService,
                        _services.VariableService,
                        _services.ConfigSnapshot,
                        _services.SchedulingService,
                        _services.EngineURI,
                        _services.PatternNodeFactory,
                        _services.NamedWindowMgmtService,
                        _services.ContextManagementService,
                        _services.ExprDeclaredService,
                        _services.TableService);
                    epl = prepared.OptionalEPL ?? prepared.Model.ToEPL();
                }

                var annotations = AnnotationUtil.CompileAnnotations(spec.Annotations, _services.EngineImportService, epl);
                var writesToTables = StatementLifecycleSvcUtil.IsWritesToTables(spec, _services.TableService);
                var statementContext = _services.StatementContextFactory.MakeContext(
                    stmtId, stmtName, epl, StatementType.SELECT, _services, null, true, annotations, null, true, spec, Collections.GetEmptyList<ExprSubselectNode>(), writesToTables, null);

                // walk subselects, alias expressions, declared expressions, dot-expressions
                ExprNodeSubselectDeclaredDotVisitor visitor;
                try
                {
                    visitor = StatementSpecRawAnalyzer.WalkSubselectAndDeclaredDotExpr(spec);
                }
                catch (ExprValidationException ex)
                {
                    throw new EPStatementException(ex.Message, epl);
                }

                var compiledSpec = StatementLifecycleSvcImpl.Compile(
                    spec, epl, statementContext, false, true, annotations, visitor.Subselects,
                    Collections.GetEmptyList<ExprDeclaredNode>(),
                    spec.TableExpressions,
                    _services);

                if (compiledSpec.InsertIntoDesc != null)
                {
                    return new EPPreparedExecuteIUDInsertInto(compiledSpec, _services, statementContext);
                }
                else if (compiledSpec.FireAndForgetSpec == null)
                {   // null indicates a select-statement, same as continuous query
                    if (compiledSpec.UpdateSpec != null)
                    {
                        throw new EPStatementException("Provided EPL expression is a continuous query expression (not an on-demand query), please use the administrator createEPL API instead", epl);
                    }
                    return new EPPreparedExecuteMethodQuery(compiledSpec, _services, statementContext);
                }
                else if (compiledSpec.FireAndForgetSpec is FireAndForgetSpecDelete)
                {
                    return new EPPreparedExecuteIUDSingleStreamDelete(compiledSpec, _services, statementContext);
                }
                else if (compiledSpec.FireAndForgetSpec is FireAndForgetSpecUpdate)
                {
                    return new EPPreparedExecuteIUDSingleStreamUpdate(compiledSpec, _services, statementContext);
                }
                else
                {
                    throw new IllegalStateException("Unrecognized FAF code " + compiledSpec.FireAndForgetSpec);
                }
            }
            catch (EPStatementException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var message = "Error executing statement: " + ex.Message;
                Log.Debug(message, ex);
                throw new EPStatementException(message, ex, epl);
            }
        }

        public EventSender GetEventSender(String eventTypeName)
        {
            return _services.EventAdapterService.GetStaticTypeEventSender(
                this, eventTypeName, 
                _services.ThreadingService, 
                _services.LockManager);
        }

        public EventSender GetEventSender(Uri[] uri)
        {
            return _services.EventAdapterService.GetDynamicTypeEventSender(this, uri, _services.ThreadingService);
        }

        public EventRenderer EventRenderer
        {
            get => _eventRenderer ?? (_eventRenderer = new EventRendererImpl());
        }

        public long CurrentTime
        {
            get => _services.SchedulingService.Time;
        }

        public long? NextScheduledTime
        {
            get => _services.SchedulingService.NearestTimeHandle;
        }

        public IDictionary<string, long> StatementNearestSchedules
        {
            get => GetStatementNearestSchedulesInternal(_services.SchedulingService, _services.StatementLifecycleSvc);
        }

        internal static IDictionary<string, long> GetStatementNearestSchedulesInternal(SchedulingServiceSPI schedulingService, StatementLifecycleSvc statementLifecycleSvc)
        {
            var schedulePerStatementId = new Dictionary<int, long>();
            schedulingService.VisitSchedules(visit =>
            {
                if (schedulePerStatementId.ContainsKey(visit.StatementId))
                {
                    return;
                }
                schedulePerStatementId.Put(visit.StatementId, visit.Timestamp);
            });

            var result = new Dictionary<String, long>();
            foreach (var schedule in schedulePerStatementId)
            {
                var stmtName = statementLifecycleSvc.GetStatementNameById(schedule.Key);
                if (stmtName != null)
                {
                    result.Put(stmtName, schedule.Value);
                }
            }
            return result;
        }

        public ExceptionHandlingService ExceptionHandlingService
        {
            get => _services.ExceptionHandlingService;
        }

        public string EngineURI
        {
            get => _services.EngineURI;
        }

        public EPDataFlowRuntime DataFlowRuntime
        {
            get => _services.DataFlowService;
        }

        private void RemoveFromThreadLocals()
        {
            if (_threadLocalData != null)
            {
                _threadLocalData.Dispose();
                _threadLocalData = _services.ThreadLocalManager.Create(CreateLocalData);
            }
        }

        private void InitThreadLocals()
        {
            RemoveFromThreadLocals();
            _threadLocalData = _services.ThreadLocalManager.Create(CreateLocalData);
        }

        private void CheckVariable(String variableName, VariableMetaData metaData, bool settable, bool requireContextPartitioned)
        {
            if (metaData == null)
            {
                throw new VariableNotFoundException("Variable by name '" + variableName + "' has not been declared");
            }
            if (!requireContextPartitioned)
            {
                if (metaData.ContextPartitionName != null)
                {
                    throw new VariableNotFoundException("Variable by name '" + variableName + "' has been declared for context '" + metaData.ContextPartitionName + "' and cannot be set without context partition selectors");
                }
            }
            else
            {
                if (metaData.ContextPartitionName == null)
                {
                    throw new VariableNotFoundException("Variable by name '" + variableName + "' is a global variable and not context-partitioned");
                }
            }
            if (settable && metaData.IsConstant)
            {
                throw new VariableConstantValueException("Variable by name '" + variableName + "' is declared as constant and may not be assigned a new value");
            }
        }

        private void SetVariableValueInternal(
            IDictionary<String, Object> variableValues,
            int agentInstanceId,
            bool requireContextPartitioned)
        {
            // verify
            foreach (var entry in variableValues)
            {
                String variableName = entry.Key;
                VariableMetaData metaData = _services.VariableService.GetVariableMetaData(variableName);
                CheckVariable(variableName, metaData, true, requireContextPartitioned);
            }

            // set values
            using (_services.VariableService.ReadWriteLock.AcquireWriteLock())
            {
                foreach (var entry in variableValues)
                {
                    String variableName = entry.Key;
                    try
                    {
                        _services.VariableService.CheckAndWrite(variableName, agentInstanceId, entry.Value);
                    }
                    catch (Exception)
                    {
                        _services.VariableService.Rollback();
                        throw;
                    }
                }
                _services.VariableService.Commit();
            }
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
