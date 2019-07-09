///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.exception;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.avro;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.common.@internal.@event.util;
using com.espertech.esper.common.@internal.@event.xml;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.common.@internal.metrics.audit;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.statement.insertintolatch;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.magic;
using com.espertech.esper.compat.threading;
using com.espertech.esper.compat.threading.threadlocal;
using com.espertech.esper.container;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.@internal.kernel.thread;
using com.espertech.esper.runtime.@internal.metrics.instrumentation;
using com.espertech.esper.runtime.@internal.metrics.stmtmetrics;
using com.espertech.esper.runtime.@internal.schedulesvcimpl;
using com.espertech.esper.runtime.@internal.statementlifesvc;
using com.espertech.esper.runtime.@internal.timer;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
    using TimerCallback = com.espertech.esper.runtime.@internal.timer.TimerCallback;

    /// <summary>
    ///     Implements runtime interface. Also accepts timer callbacks for synchronizing time events with regular events
    ///     sent in.
    /// </summary>
    public class EPEventServiceImpl : EPEventServiceSPI,
        InternalEventRouteDest,
        ITimerCallback,
        EPRuntimeEventProcessWrapped
    {
        private const int MAX_FILTER_FAULT_COUNT = 10;

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly bool _inboundThreading;
        private readonly bool _isLatchStatementInsertStream;
        private readonly bool _routeThreading;
        private readonly ExprEvaluatorContext _runtimeFilterAndDispatchTimeContext;
        private readonly bool _timerThreading;
        private InternalEventRouter _internalEventRouter;
        private bool _isPrioritized;
        private bool _isUsingExternalClocking;
        private IThreadLocal<ArrayBackedCollection<FilterHandle>> _matchesArrayThreadLocal;
        private IThreadLocal<IDictionary<EPStatementAgentInstanceHandle, object>> _matchesPerStmtThreadLocal;
        private readonly AtomicLong _routedExternal;
        private readonly AtomicLong _routedInternal;
        private IThreadLocal<ArrayBackedCollection<ScheduleHandle>> _scheduleArrayThreadLocal;
        private IThreadLocal<IDictionary<EPStatementAgentInstanceHandle, object>> _schedulePerStmtThreadLocal;

        private EPServicesContext _services;
        private ThreadWorkQueue _threadWorkQueue;
        private volatile UnmatchedListener _unmatchedListener;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="services">references to services</param>
        public EPEventServiceImpl(EPServicesContext services)
        {
            _services = services;
            _inboundThreading = services.ThreadingService.IsInboundThreading;
            _routeThreading = services.ThreadingService.IsRouteThreading;
            _timerThreading = services.ThreadingService.IsTimerThreading;
            _threadWorkQueue = new ThreadWorkQueue(services.Container.ThreadLocalManager());
            _isLatchStatementInsertStream = _services.RuntimeSettingsService.ConfigurationRuntime.Threading.IsInsertIntoDispatchPreserveOrder;
            _isUsingExternalClocking = !_services.RuntimeSettingsService.ConfigurationRuntime.Threading.IsInternalTimerEnabled;
            _isPrioritized = services.RuntimeSettingsService.ConfigurationRuntime.Execution.IsPrioritized;
            _routedInternal = new AtomicLong();
            _routedExternal = new AtomicLong();
            _runtimeFilterAndDispatchTimeContext = new ProxyExprEvaluatorContext {
                ProcTimeProvider = () => throw new UnsupportedOperationException(),
                ProcAgentInstanceId = () => -1,
                ProcContextProperties = () => null,
                ProcStatementName = () => null,
                ProcRuntimeURI = () => null,
                ProcStatementId = () => -1,
                ProcDeploymentId = () => null,
                ProcUserObjectCompileTime = () => null,
                ProcEventBeanService = () => null,
                ProcAgentInstanceLock = () => null,
                ProcExpressionResultCacheService = () => null,
                ProcTableExprEvaluatorContext = () => {
                    throw new UnsupportedOperationException("Table-access evaluation is not supported in this expression");
                },
                ProcAllocateAgentInstanceScriptContext = () => null,
                ProcAuditProvider = () => AuditProviderDefault.INSTANCE,
                ProcInstrumentationProvider = () => InstrumentationCommonDefault.INSTANCE
            };

            InitThreadLocals();

            services.ThreadingService.InitThreading(services, this);
        }

        //JmxGetter(
        //    name = "NumInsertIntoEvents",
        //    description = "Number of inserted-into events")

        //JmxGetter(
        //    name = "NumRoutedEvents",
        //    description = "Number of routed events")

        public void SendEventAvro(
            object avroGenericDataDotRecord,
            string avroEventTypeName)
        {
            if (avroGenericDataDotRecord == null) {
                throw new ArgumentException("Invalid null event object");
            }

            if (ExecutionPathDebugLog.IsDebugEnabled && Log.IsDebugEnabled) {
                Log.Debug(".sendMap Processing event " + avroGenericDataDotRecord);
            }

            if (_inboundThreading) {
                _services.ThreadingService.SubmitInbound(new InboundUnitSendAvro(
                    avroGenericDataDotRecord, avroEventTypeName, this));
            }
            else {
                var eventBean = WrapEventAvro(avroGenericDataDotRecord, avroEventTypeName);
                ProcessWrappedEvent(eventBean);
            }
        }

        public void SendEventBean(
            object theEvent,
            string eventTypeName)
        {
            if (theEvent == null) {
                Log.Error(".sendEvent Null object supplied");
                return;
            }

            if (ExecutionPathDebugLog.IsDebugEnabled && Log.IsDebugEnabled) {
                Log.Debug(".sendEvent Processing event " + theEvent);
            }

            if (_inboundThreading) {
                _services.ThreadingService.SubmitInbound(
                    new InboundUnitSendEvent(theEvent, eventTypeName, this));
            }
            else {
                var eventBean = _services.EventTypeResolvingBeanFactory.AdapterForBean(theEvent, eventTypeName);
                ProcessWrappedEvent(eventBean);
            }
        }

        public void AdvanceTime(long time)
        {
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().QStimulantTime(_services.SchedulingService.Time, time, time, false, null, _services.RuntimeURI);
            }

            _services.SchedulingService.Time = time;

            _services.MetricReportingService.ProcessTimeEvent(time);

            ProcessSchedule(time);

            // Let listeners know of results
            Dispatch();

            // Work off the event queue if any events accumulated in there via a route()
            ProcessThreadWorkQueue();

            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().AStimulantTime();
            }
        }

        public void AdvanceTimeSpan(long targetTime)
        {
            AdvanceTimeSpanInternal(targetTime, null);
        }

        public void AdvanceTimeSpan(
            long targetTime,
            long resolution)
        {
            AdvanceTimeSpanInternal(targetTime, resolution);
        }

        public long? NextScheduledTime => _services.SchedulingService.NearestTimeHandle;

        public void SendEventXMLDOM(
            XmlNode node,
            string eventTypeName)
        {
            if (node == null) {
                Log.Error(".sendEvent Null object supplied");
                return;
            }

            // Process event
            if (_inboundThreading) {
                _services.ThreadingService.SubmitInbound(new InboundUnitSendDOM(node, eventTypeName, this));
            }
            else {
                var eventBean = WrapEventBeanXMLDOM(node, eventTypeName);
                ProcessWrappedEvent(eventBean);
            }
        }

        public void SendEventObjectArray(
            object[] propertyValues,
            string eventTypeName)
        {
            if (propertyValues == null) {
                throw new ArgumentException("Invalid null event object");
            }

            if (ExecutionPathDebugLog.IsDebugEnabled && Log.IsDebugEnabled) {
                Log.Debug(".sendEventObjectArray Processing event " + CompatExtensions.RenderAny(propertyValues));
            }

            if (_inboundThreading) {
                _services.ThreadingService.SubmitInbound(new InboundUnitSendObjectArray(propertyValues, eventTypeName, this));
            }
            else {
                var eventBean = WrapEventObjectArray(propertyValues, eventTypeName);
                ProcessWrappedEvent(eventBean);
            }
        }

        public void SendEventMap(
            IDictionary<string, object> map,
            string mapEventTypeName)
        {
            if (map == null) {
                throw new ArgumentException("Invalid null event object");
            }

            if (ExecutionPathDebugLog.IsDebugEnabled && Log.IsDebugEnabled) {
                Log.Debug(".sendMap Processing event " + map);
            }

            if (_inboundThreading) {
                _services.ThreadingService.SubmitInbound(new InboundUnitSendMap(map, mapEventTypeName, this));
            }
            else {
                var eventBean = WrapEventMap(map, mapEventTypeName);
                ProcessWrappedEvent(eventBean);
            }
        }

        public void RouteEventBean(EventBean theEvent)
        {
            _threadWorkQueue.AddBack(theEvent);
        }

        public void ProcessWrappedEvent(EventBean eventBean)
        {
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().QStimulantEvent(eventBean, _services.RuntimeURI);
            }

            if (_internalEventRouter.HasPreprocessing) {
                eventBean = _internalEventRouter.Preprocess(
                    eventBean, _runtimeFilterAndDispatchTimeContext, InstrumentationHelper.Get());
                if (eventBean == null) {
                    return;
                }
            }

            // Acquire main processing lock which locks out statement management
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().QEvent(eventBean, _services.RuntimeURI, true);
            }

            _services.EventProcessingRWLock.AcquireReadLock();
            try {
                ProcessMatches(eventBean);
            }
            catch (EPException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _matchesArrayThreadLocal.GetOrCreate().Clear();
                throw new EPException(ex);
            }
            finally {
                _services.EventProcessingRWLock.ReleaseReadLock();
                if (InstrumentationHelper.ENABLED) {
                    InstrumentationHelper.Get().AEvent();
                }
            }

            // Dispatch results to listeners
            // Done outside of the read-lock to prevent lockups when listeners create statements
            Dispatch();

            // Work off the event queue if any events accumulated in there via a route() or insert-into
            ProcessThreadWorkQueue();

            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().AStimulantEvent();
            }
        }

        public bool IsExternalClockingEnabled()
        {
            return _isUsingExternalClocking;
        }

        /// <summary>
        ///     Destroy for destroying an runtime instance: sets references to null and clears thread-locals
        /// </summary>
        public void Destroy()
        {
            _services = null;

            RemoveFromThreadLocals();
            _matchesArrayThreadLocal = null;
            _matchesPerStmtThreadLocal = null;
            _scheduleArrayThreadLocal = null;
            _schedulePerStmtThreadLocal = null;
        }

        public void Initialize()
        {
            InitThreadLocals();
            _threadWorkQueue = new ThreadWorkQueue(_services.Container.ThreadLocalManager());
        }

        public void ClearCaches()
        {
            InitThreadLocals();
        }

        public UnmatchedListener UnmatchedListener {
            get => _unmatchedListener;
            set => _unmatchedListener = value;
        }

        public long CurrentTime => _services.SchedulingService.Time;

        public void RouteEventMap(
            IDictionary<string, object> map,
            string eventTypeName)
        {
            if (map == null) {
                throw new ArgumentException("Invalid null event object");
            }

            var theEvent = _services.EventTypeResolvingBeanFactory.AdapterForMap(map, eventTypeName);
            RouteEventInternal(theEvent);
        }

        public void RouteEventBean(
            object @event,
            string eventTypeName)
        {
            if (@event == null) {
                throw new ArgumentException("Invalid null event object");
            }

            var theEvent = _services.EventTypeResolvingBeanFactory.AdapterForBean(@event, eventTypeName);
            RouteEventInternal(theEvent);
        }

        public void RouteEventObjectArray(
            object[] @event,
            string eventTypeName)
        {
            if (@event == null) {
                throw new ArgumentException("Invalid null event object");
            }

            var theEvent = _services.EventTypeResolvingBeanFactory.AdapterForObjectArray(@event, eventTypeName);
            RouteEventInternal(theEvent);
        }

        public void RouteEventXMLDOM(
            XmlNode @event,
            string eventTypeName)
        {
            if (@event == null) {
                throw new ArgumentException("Invalid null event object");
            }

            var theEvent = _services.EventTypeResolvingBeanFactory.AdapterForXMLDOM(@event, eventTypeName);
            RouteEventInternal(theEvent);
        }

        public void RouteEventAvro(
            object avroGenericDataDotRecord,
            string eventTypeName)
        {
            if (avroGenericDataDotRecord == null) {
                throw new ArgumentException("Invalid null event object");
            }

            var theEvent = _services.EventTypeResolvingBeanFactory.AdapterForAvro(avroGenericDataDotRecord, eventTypeName);
            RouteEventInternal(theEvent);
        }

        public EventSender GetEventSender(string eventTypeName)
        {
            var eventType = _services.EventTypeRepositoryBus.GetTypeByName(eventTypeName);
            if (eventType == null) {
                throw new EventTypeException("Event type named '" + eventTypeName + "' could not be found");
            }

            // handle built-in types
            var threadingService = _services.ThreadingService;
            if (eventType is BeanEventType beanEventType) {
                return new EventSenderBean(this, beanEventType,
                    _services.EventBeanTypedEventFactory, threadingService);
            }

            if (eventType is MapEventType mapEventType) {
                return new EventSenderMap(this, mapEventType,
                    _services.EventBeanTypedEventFactory, threadingService);
            }

            if (eventType is ObjectArrayEventType objectArrayEventType) {
                return new EventSenderObjectArray(this, objectArrayEventType,
                    _services.EventBeanTypedEventFactory, threadingService);
            }

            if (eventType is BaseXMLEventType baseXmlEventType) {
                return new EventSenderXMLDOM(this, baseXmlEventType,
                    _services.EventBeanTypedEventFactory, threadingService);
            }

            if (eventType is AvroSchemaEventType) {
                return new EventSenderAvro(this, eventType,
                    _services.EventBeanTypedEventFactory, threadingService);
            }

            throw new EventTypeException(
                "An event sender for event type named '" + eventTypeName + "' could not be created as the type is not known");
        }

        public void ClockInternal()
        {
            // Start internal clock which supplies CurrentTimeEvent events every 100ms
            // This may be done without delay thus the write lock indeed must be reentrant.
            if (_services.ConfigSnapshot.Common.TimeSource.TimeUnit != TimeUnit.MILLISECONDS) {
                throw new EPException("Internal timer requires millisecond time resolution");
            }

            _services.TimerService.StartInternalClock();
            _isUsingExternalClocking = false;
        }

        public void ClockExternal()
        {
            // Stop internal clock, for unit testing and for external clocking
            _services.TimerService.StopInternalClock(true);
            _isUsingExternalClocking = true;
        }

        public void ResetStats()
        {
            _services.FilterService.ResetStats();
            _routedInternal.Set(0);
            _routedExternal.Set(0);
        }

        // Internal route of events via insert-into, holds a statement lock
        public void Route(
            EventBean theEvent,
            EPStatementHandle epStatementHandle,
            bool addToFront)
        {
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().QRouteBetweenStmt(theEvent, epStatementHandle, addToFront);
            }

            if (theEvent is NaturalEventBean naturalEventBean) {
                theEvent = naturalEventBean.OptionalSynthetic;
            }

            _routedInternal.IncrementAndGet();

            if (_isLatchStatementInsertStream) {
                if (addToFront) {
                    var latch = epStatementHandle.InsertIntoFrontLatchFactory.NewLatch(theEvent);
                    _threadWorkQueue.AddFront(latch);
                }
                else {
                    var latch = epStatementHandle.InsertIntoBackLatchFactory.NewLatch(theEvent);
                    _threadWorkQueue.AddBack(latch);
                }
            }
            else {
                if (addToFront) {
                    _threadWorkQueue.AddFront(theEvent);
                }
                else {
                    _threadWorkQueue.AddBack(theEvent);
                }
            }

            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().ARouteBetweenStmt();
            }
        }

        /// <summary>
        ///     Works off the thread's work queue.
        /// </summary>
        public void ProcessThreadWorkQueue()
        {
            var queues = _threadWorkQueue.ThreadQueue;

            if (queues.FrontQueue.IsEmpty()) {
                var haveDispatched = _services.NamedWindowDispatchService.Dispatch();
                if (haveDispatched) {
                    // Dispatch results to listeners
                    Dispatch();

                    if (!queues.FrontQueue.IsEmpty()) {
                        ProcessThreadWorkQueueFront(queues);
                    }
                }
            }
            else {
                ProcessThreadWorkQueueFront(queues);
            }

            object item;
            while ((item = queues.BackQueue.Poll()) != null) {
                if (item is InsertIntoLatchSpin intoLatchSpin) {
                    ProcessThreadWorkQueueLatchedSpin(intoLatchSpin);
                }
                else if (item is InsertIntoLatchWait intoLatchWait) {
                    ProcessThreadWorkQueueLatchedWait(intoLatchWait);
                }
                else {
                    ProcessThreadWorkQueueUnlatched(item);
                }

                var haveDispatched = _services.NamedWindowDispatchService.Dispatch();
                if (haveDispatched) {
                    Dispatch();
                }

                if (!queues.FrontQueue.IsEmpty()) {
                    ProcessThreadWorkQueueFront(queues);
                }
            }
        }

        /// <summary>
        ///     Dispatch events.
        /// </summary>
        public void Dispatch()
        {
            try {
                _services.DispatchService.Dispatch();
            }
            catch (EPException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new EPException(ex);
            }
        }

        public EPServicesContext Services => _services;

        /// <summary>
        ///     Sets the route for events to use
        /// </summary>
        /// <value>router</value>
        public InternalEventRouter InternalEventRouter {
            get => this._internalEventRouter;
            set => this._internalEventRouter = value;
        }


        public long RoutedInternal => _routedInternal.Get();


        public long RoutedExternal => _routedExternal.Get();

        public void TimerCallback()
        {
            var msec = _services.TimeSourceService.TimeMillis;

            if (ExecutionPathDebugLog.IsDebugEnabled && Log.IsDebugEnabled && ExecutionPathDebugLog.IsTimerDebugEnabled) {
                Log.Debug(".timerCallback Evaluating scheduled callbacks, time is " + msec);
            }

            AdvanceTime(msec);
        }

        private void AdvanceTimeSpanInternal(
            long targetTime,
            long? optionalResolution)
        {
            var currentTime = _services.SchedulingService.Time;

            while (currentTime < targetTime) {
                if (optionalResolution != null && optionalResolution > 0) {
                    currentTime += optionalResolution.Value;
                }
                else {
                    var nearest = _services.SchedulingService.NearestTimeHandle;
                    if (nearest == null) {
                        currentTime = targetTime;
                    }
                    else {
                        currentTime = nearest.Value;
                    }
                }

                if (currentTime > targetTime) {
                    currentTime = targetTime;
                }

                if (InstrumentationHelper.ENABLED) {
                    InstrumentationHelper.Get().QStimulantTime(
                        _services.SchedulingService.Time, currentTime, targetTime, true, optionalResolution, _services.RuntimeURI);
                }

                _services.SchedulingService.Time = currentTime;

                ProcessSchedule(currentTime);

                // Let listeners know of results
                Dispatch();

                // Work off the event queue if any events accumulated in there via a route()
                ProcessThreadWorkQueue();

                if (InstrumentationHelper.ENABLED) {
                    InstrumentationHelper.Get().AStimulantTime();
                }
            }

            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().AStimulantTime();
            }
        }

        private void ProcessThreadWorkQueueFront(DualWorkQueue<object> queues)
        {
            object item;
            while ((item = queues.FrontQueue.Poll()) != null) {
                if (item is InsertIntoLatchSpin insertIntoLatchSpin) {
                    ProcessThreadWorkQueueLatchedSpin(insertIntoLatchSpin);
                }
                else if (item is InsertIntoLatchWait insertIntoLatchWait) {
                    ProcessThreadWorkQueueLatchedWait(insertIntoLatchWait);
                }
                else {
                    ProcessThreadWorkQueueUnlatched(item);
                }

                var haveDispatched = _services.NamedWindowDispatchService.Dispatch();
                if (haveDispatched) {
                    Dispatch();
                }
            }
        }

        private void ProcessThreadWorkQueueLatchedWait(InsertIntoLatchWait insertIntoLatch)
        {
            // wait for the latch to complete
            var eventBean = insertIntoLatch.Await();

            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().QEvent(eventBean, _services.RuntimeURI, false);
            }

            _services.EventProcessingRWLock.AcquireReadLock();
            try {
                ProcessMatches(eventBean);
            }
            catch (EPException)
            {
                throw;
            }
            catch (Exception)
            {
                _matchesArrayThreadLocal.GetOrCreate().Clear();
                throw;
            }
            finally {
                insertIntoLatch.Done();
                _services.EventProcessingRWLock.ReleaseReadLock();
                if (InstrumentationHelper.ENABLED) {
                    InstrumentationHelper.Get().AEvent();
                }
            }

            Dispatch();
        }

        private void ProcessThreadWorkQueueLatchedSpin(InsertIntoLatchSpin insertIntoLatch)
        {
            // wait for the latch to complete
            var eventBean = insertIntoLatch.Await();

            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().QEvent(eventBean, _services.RuntimeURI, false);
            }

            _services.EventProcessingRWLock.AcquireReadLock();
            try {
                ProcessMatches(eventBean);
            }
            catch (EPException)
            {
                throw;
            }
            catch (Exception)
            {
                _matchesArrayThreadLocal.GetOrCreate().Clear();
                throw;
            }
            finally {
                insertIntoLatch.Done();
                _services.EventProcessingRWLock.ReleaseReadLock();
                if (InstrumentationHelper.ENABLED) {
                    InstrumentationHelper.Get().AEvent();
                }
            }

            Dispatch();
        }

        private void ProcessThreadWorkQueueUnlatched(object item)
        {
            EventBean eventBean;
            if (item is EventBean) {
                eventBean = (EventBean) item;
            }
            else {
                throw new IllegalStateException("Unexpected item type " + item + " in queue");
            }

            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().QEvent(eventBean, _services.RuntimeURI, false);
            }

            _services.EventProcessingRWLock.AcquireReadLock();
            try {
                ProcessMatches(eventBean);
            }
            catch (EPException)
            {
                throw;
            }
            catch (Exception)
            {
                _matchesArrayThreadLocal.GetOrCreate().Clear();
                throw;
            }
            finally {
                _services.EventProcessingRWLock.ReleaseReadLock();
                if (InstrumentationHelper.ENABLED) {
                    InstrumentationHelper.Get().AEvent();
                }
            }

            Dispatch();
        }

        protected void ProcessMatches(EventBean theEvent)
        {
            // get matching filters
            var matches = _matchesArrayThreadLocal.GetOrCreate();
            var version = _services.FilterService.Evaluate(theEvent, matches);

            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.Trace("Found matches for underlying ", matches.Count, theEvent.Underlying);
            }

            if (matches.Count == 0) {
                if (_unmatchedListener != null) {
                    _services.EventProcessingRWLock.ReleaseReadLock(); // Allow listener to create new statements
                    try {
                        _unmatchedListener.Invoke(theEvent);
                    }
                    catch (EPException) {
                        throw;
                    }
                    catch (Exception ex) {
                        Log.Error("Exception thrown by unmatched listener: " + ex.Message, ex);
                    }
                    finally {
                        // acquire read lock for release by caller
                        _services.EventProcessingRWLock.AcquireReadLock();
                    }
                }

                return;
            }

            var stmtCallbacks = _matchesPerStmtThreadLocal.GetOrCreate();
            object[] matchArray = matches.Array;
            var entryCount = matches.Count;

            for (var i = 0; i < entryCount; i++) {
                var handleCallback = (EPStatementHandleCallbackFilter) matchArray[i];
                var handle = handleCallback.AgentInstanceHandle;

                // Self-joins require that the internal dispatch happens after all streams are evaluated.
                // Priority or preemptive settings also require special ordering.
                if (handle.IsCanSelfJoin || _isPrioritized) {
                    var callbacks = stmtCallbacks.Get(handle);
                    if (callbacks == null) {
                        stmtCallbacks.Put(handle, handleCallback.FilterCallback);
                    }
                    else if (callbacks is ArrayDeque<FilterHandleCallback> callbacksQueue) {
                        callbacksQueue.Add(handleCallback.FilterCallback);
                    }
                    else {
                        var q = new ArrayDeque<FilterHandleCallback>(4);
                        q.Add((FilterHandleCallback) callbacks);
                        q.Add(handleCallback.FilterCallback);
                        stmtCallbacks.Put(handle, q);
                    }

                    continue;
                }

                if (handle.StatementHandle.MetricsHandle.IsEnabled) {
                    var performanceMetric = PerformanceMetricsHelper.Call(
                        () => ProcessStatementFilterSingle(handle, handleCallback, theEvent, version, 0));
                    _services.MetricReportingService.AccountTime(
                        handle.StatementHandle.MetricsHandle, 
                        performanceMetric.ExecTime,
                        performanceMetric.WallTime,
                        performanceMetric.NumInput);
                }
                else {
                    if (_routeThreading) {
                        _services.ThreadingService.SubmitRoute(new RouteUnitSingle(this, handleCallback, theEvent, version));
                    }
                    else {
                        ProcessStatementFilterSingle(handle, handleCallback, theEvent, version, 0);
                    }
                }
            }

            matches.Clear();
            if (stmtCallbacks.IsEmpty()) {
                return;
            }

            foreach (var entry in stmtCallbacks) {
                var handle = entry.Key;
                var callbackList = entry.Value;

                if (handle.StatementHandle.MetricsHandle.IsEnabled) {
                    var performanceMetric = PerformanceMetricsHelper.Call(
                        () => ProcessStatementFilterMultiple(handle, callbackList, theEvent, version, 0));

                    var size = 1;
                    if (callbackList is ICollection<object> callbackCollectionList) {
                        size = callbackCollectionList.Count;
                    }

                    _services.MetricReportingService.AccountTime(
                        handle.StatementHandle.MetricsHandle, 
                        performanceMetric.ExecTime,
                        performanceMetric.WallTime,
                        size);
                }
                else {
                    if (_routeThreading) {
                        _services.ThreadingService.SubmitRoute(new RouteUnitMultiple(this, callbackList, theEvent, handle, version));
                    }
                    else {
                        ProcessStatementFilterMultiple(handle, callbackList, theEvent, version, 0);
                    }
                }

                if (_isPrioritized && handle.IsPreemptive) {
                    break;
                }
            }

            stmtCallbacks.Clear();
        }

        /// <summary>
        ///     Processing multiple schedule matches for a statement.
        /// </summary>
        /// <param name="handle">statement handle</param>
        /// <param name="callbackObject">object containing matches</param>
        /// <param name="services">runtime services</param>
        public static void ProcessStatementScheduleMultiple(
            EPStatementAgentInstanceHandle handle,
            object callbackObject,
            EPServicesContext services)
        {
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().QTimeCP(handle, services.SchedulingService.Time);
            }

            handle.StatementAgentInstanceLock.AcquireWriteLock();
            try {
                if (!handle.IsDestroyed) {
                    if (handle.HasVariables) {
                        services.VariableManagementService.SetLocalVersion();
                    }

                    if (callbackObject is ArrayDeque<ScheduleHandleCallback> callbackList) {
                        foreach (var callback in callbackList) {
                            callback.ScheduledTrigger();
                        }
                    }
                    else {
                        var callback = (ScheduleHandleCallback) callbackObject;
                        callback.ScheduledTrigger();
                    }

                    // internal join processing, if applicable
                    handle.InternalDispatch();
                }
            }
            catch (EPException)
            {
                throw;
            }
            catch (Exception ex)
            {
                services.ExceptionHandlingService.HandleException(ex, handle, ExceptionHandlerExceptionType.PROCESS, null);
            }
            finally {
                if (handle.HasTableAccess) {
                    services.TableExprEvaluatorContext.ReleaseAcquiredLocks();
                }

                handle.StatementAgentInstanceLock.ReleaseWriteLock();

                if (InstrumentationHelper.ENABLED) {
                    InstrumentationHelper.Get().ATimeCP();
                }
            }
        }

        /// <summary>
        ///     Processing multiple filter matches for a statement.
        /// </summary>
        /// <param name="handle">statement handle</param>
        /// <param name="callbackList">object containing callbacks</param>
        /// <param name="theEvent">to process</param>
        /// <param name="version">filter version</param>
        /// <param name="filterFaultCount">filter fault count</param>
        public void ProcessStatementFilterMultiple(
            EPStatementAgentInstanceHandle handle,
            object callbackList,
            EventBean theEvent,
            long version,
            int filterFaultCount)
        {
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().QEventCP(theEvent, handle, _services.SchedulingService.Time);
            }

            handle.StatementAgentInstanceLock.AcquireWriteLock();
            try {
                if (handle.HasVariables) {
                    _services.VariableManagementService.SetLocalVersion();
                }

                if (!handle.IsCurrentFilter(version)) {
                    var handled = false;
                    if (handle.FilterFaultHandler != null) {
                        handled = handle.FilterFaultHandler.HandleFilterFault(theEvent, version);
                    }

                    if (!handled && filterFaultCount < MAX_FILTER_FAULT_COUNT) {
                        HandleFilterFault(handle, theEvent, filterFaultCount);
                    }
                }
                else {
                    if (callbackList is ICollection<FilterHandleCallback> callbacks) {
                        handle.MultiMatchHandler.Handle(callbacks, theEvent);
                    }
                    else {
                        var single = (FilterHandleCallback) callbackList;
                        single.MatchFound(theEvent, null);
                    }

                    // internal join processing, if applicable
                    handle.InternalDispatch();
                }
            }
            catch (EPException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _services.ExceptionHandlingService.HandleException(ex, handle, ExceptionHandlerExceptionType.PROCESS, theEvent);
            }
            finally {
                if (handle.HasTableAccess) {
                    _services.TableExprEvaluatorContext.ReleaseAcquiredLocks();
                }

                handle.StatementAgentInstanceLock.ReleaseWriteLock();
                if (InstrumentationHelper.ENABLED) {
                    InstrumentationHelper.Get().AEventCP();
                }
            }
        }

        /// <summary>
        ///     Process a single match.
        /// </summary>
        /// <param name="handle">statement</param>
        /// <param name="handleCallback">callback</param>
        /// <param name="theEvent">event to indicate</param>
        /// <param name="version">filter version</param>
        /// <param name="filterFaultCount">filter fault count</param>
        public void ProcessStatementFilterSingle(
            EPStatementAgentInstanceHandle handle,
            EPStatementHandleCallbackFilter handleCallback,
            EventBean theEvent,
            long version,
            int filterFaultCount)
        {
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().QEventCP(theEvent, handle, _services.SchedulingService.Time);
            }

            handle.StatementAgentInstanceLock.AcquireWriteLock();
            try {
                if (handle.HasVariables) {
                    _services.VariableManagementService.SetLocalVersion();
                }

                if (!handle.IsCurrentFilter(version)) {
                    var handled = false;
                    if (handle.FilterFaultHandler != null) {
                        handled = handle.FilterFaultHandler.HandleFilterFault(theEvent, version);
                    }

                    if (!handled && filterFaultCount < MAX_FILTER_FAULT_COUNT) {
                        HandleFilterFault(handle, theEvent, filterFaultCount);
                    }
                }
                else {
                    handleCallback.FilterCallback.MatchFound(theEvent, null);
                }

                // internal join processing, if applicable
                handle.InternalDispatch();
            }
            catch (EPException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _services.ExceptionHandlingService.HandleException(ex, handle, ExceptionHandlerExceptionType.PROCESS, theEvent);
            }
            finally {
                if (handle.HasTableAccess) {
                    _services.TableExprEvaluatorContext.ReleaseAcquiredLocks();
                }

                handleCallback.AgentInstanceHandle.StatementAgentInstanceLock.ReleaseWriteLock();
                if (InstrumentationHelper.ENABLED) {
                    InstrumentationHelper.Get().AEventCP();
                }
            }
        }

        protected void HandleFilterFault(
            EPStatementAgentInstanceHandle faultingHandle,
            EventBean theEvent,
            int filterFaultCount)
        {
            var callbacksForStatement = new ArrayDeque<FilterHandle>();
            var version = _services.FilterService.Evaluate(theEvent, callbacksForStatement, faultingHandle.StatementId);

            if (callbacksForStatement.Count == 1) {
                var handleCallback = (EPStatementHandleCallbackFilter) callbacksForStatement.First;
                ProcessStatementFilterSingle(handleCallback.AgentInstanceHandle, handleCallback, theEvent, version, filterFaultCount + 1);
                return;
            }

            if (callbacksForStatement.IsEmpty()) {
                return;
            }

            IDictionary<EPStatementAgentInstanceHandle, object> stmtCallbacks;
            if (_isPrioritized) {
                stmtCallbacks = new SortedDictionary<EPStatementAgentInstanceHandle, object>(EPStatementAgentInstanceHandleComparator.INSTANCE);
            }
            else {
                stmtCallbacks = new Dictionary<EPStatementAgentInstanceHandle, object>();
            }

            foreach (var filterHandle in callbacksForStatement) {
                var handleCallback = (EPStatementHandleCallbackFilter) filterHandle;
                var handle = handleCallback.AgentInstanceHandle;

                if (handle.IsCanSelfJoin || _isPrioritized) {
                    var callbacks = stmtCallbacks.Get(handle);
                    if (callbacks == null) {
                        stmtCallbacks.Put(handle, handleCallback.FilterCallback);
                    }
                    else if (callbacks is ArrayDeque<FilterHandleCallback> q1) {
                        q1.Add(handleCallback.FilterCallback);
                    }
                    else {
                        var q = new ArrayDeque<FilterHandleCallback>(4);
                        q.Add((FilterHandleCallback) callbacks);
                        q.Add(handleCallback.FilterCallback);
                        stmtCallbacks.Put(handle, q);
                    }

                    continue;
                }

                ProcessStatementFilterSingle(handle, handleCallback, theEvent, version, filterFaultCount + 1);
            }

            if (stmtCallbacks.IsEmpty()) {
                return;
            }

            foreach (var entry in stmtCallbacks) {
                var handle = entry.Key;
                var callbackList = entry.Value;

                ProcessStatementFilterMultiple(handle, callbackList, theEvent, version, filterFaultCount + 1);

                if (_isPrioritized && handle.IsPreemptive) {
                    break;
                }
            }
        }

        public string RuntimeURI => _services.RuntimeURI;

        private void RemoveFromThreadLocals()
        {
            _matchesArrayThreadLocal?.Remove();

            _matchesPerStmtThreadLocal?.Remove();

            _scheduleArrayThreadLocal?.Remove();

            _schedulePerStmtThreadLocal?.Remove();
        }

        private void InitThreadLocals()
        {
            RemoveFromThreadLocals();

            _matchesArrayThreadLocal = new FastThreadLocal<ArrayBackedCollection<FilterHandle>>(
                () => new ArrayBackedCollection<FilterHandle>(100));

            _scheduleArrayThreadLocal = new FastThreadLocal<ArrayBackedCollection<ScheduleHandle>>(
                () => new ArrayBackedCollection<ScheduleHandle>(100));

            _matchesPerStmtThreadLocal = new FastThreadLocal<IDictionary<EPStatementAgentInstanceHandle, object>>(
                () => {
                    if (_isPrioritized) {
                        return new SortedDictionary<EPStatementAgentInstanceHandle, object>(
                            EPStatementAgentInstanceHandleComparator.INSTANCE);
                    }

                    return new Dictionary<EPStatementAgentInstanceHandle, object>();
                });

            _schedulePerStmtThreadLocal = new FastThreadLocal<IDictionary<EPStatementAgentInstanceHandle, object>>(
                () => {
                    if (_isPrioritized) {
                        return new SortedDictionary<EPStatementAgentInstanceHandle, object>(
                            EPStatementAgentInstanceHandleComparator.INSTANCE);
                    }

                    return new Dictionary<EPStatementAgentInstanceHandle, object>();
                });
        }

        private void ProcessSchedule(long time)
        {
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().QTime(time, _services.RuntimeURI);
            }

            var handles = _scheduleArrayThreadLocal.GetOrCreate();

            // Evaluation of schedules is protected by an optional scheduling service lock and then the runtimelock
            // We want to stay in this order for allowing the runtimelock as a second-order lock to the
            // services own lock, if it has one.
            _services.EventProcessingRWLock.AcquireReadLock();
            try {
                _services.SchedulingService.Evaluate(handles);
            }
            finally {
                _services.EventProcessingRWLock.ReleaseReadLock();
            }

            _services.EventProcessingRWLock.AcquireReadLock();
            try {
                ProcessScheduleHandles(handles);
            }
            catch (EPException)
            {
                throw;
            }
            catch (Exception)
            {
                handles.Clear();
                throw;
            }
            finally {
                _services.EventProcessingRWLock.ReleaseReadLock();
                if (InstrumentationHelper.ENABLED) {
                    InstrumentationHelper.Get().ATime();
                }
            }
        }

        public void ProcessScheduleHandles(ArrayBackedCollection<ScheduleHandle> handles)
        {
            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.Trace("Found schedules for", handles.Count);
            }

            if (handles.Count == 0) {
                return;
            }

            // handle 1 result separately for performance reasons
            if (handles.Count == 1) {
                object[] handleArray = handles.Array;
                var handle = (EPStatementHandleCallbackSchedule) handleArray[0];

                if (handle.AgentInstanceHandle.StatementHandle.MetricsHandle.IsEnabled) {
                    var performanceMetric = PerformanceMetricsHelper.Call(
                        () => ProcessStatementScheduleSingle(handle, _services));

                    _services.MetricReportingService.AccountTime(
                        handle.AgentInstanceHandle.StatementHandle.MetricsHandle, 
                        performanceMetric.ExecTime,
                        performanceMetric.WallTime,
                        performanceMetric.NumInput);
                }
                else {
                    if (_timerThreading) {
                        _services.ThreadingService.SubmitTimerWork(new TimerUnitSingle(_services, this, handle));
                    }
                    else {
                        ProcessStatementScheduleSingle(handle, _services);
                    }
                }

                handles.Clear();
                return;
            }

            object[] matchArray = handles.Array;
            var entryCount = handles.Count;

            // sort multiple matches for the event into statements
            var stmtCallbacks = _schedulePerStmtThreadLocal.GetOrCreate();
            stmtCallbacks.Clear();
            for (var i = 0; i < entryCount; i++) {
                var handleCallback = (EPStatementHandleCallbackSchedule) matchArray[i];
                var handle = handleCallback.AgentInstanceHandle;
                var callback = handleCallback.ScheduleCallback;

                var entry = stmtCallbacks.Get(handle);

                // This statement has not been encountered before
                if (entry == null) {
                    stmtCallbacks.Put(handle, callback);
                    continue;
                }

                // This statement has been encountered once before
                if (entry is ScheduleHandleCallback existingCallback) {
                    var entriesX = new ArrayDeque<ScheduleHandleCallback>();
                    entriesX.Add(existingCallback);
                    entriesX.Add(callback);
                    stmtCallbacks.Put(handle, entriesX);
                    continue;
                }

                // This statement has been encountered more then once before
                var entries = (ArrayDeque<ScheduleHandleCallback>) entry;
                entries.Add(callback);
            }

            handles.Clear();

            foreach (var entry in stmtCallbacks) {
                var handle = entry.Key;
                var callbackObject = entry.Value;

                if (handle.StatementHandle.MetricsHandle.IsEnabled) {
                    var performanceMetric = PerformanceMetricsHelper.Call(
                        () => ProcessStatementScheduleMultiple(handle, callbackObject, _services));

                    int numInput;
                    if (GenericExtensions.IsGenericCollection(callbackObject.GetType())) {
                        var collection = MagicMarker.SingletonInstance.GetCollection(callbackObject);
                        if (collection == null) {
                            numInput = 0;
                        }
                        else {
                            numInput = collection.Count;
                        }
                    }
                    else {
                        numInput = 1;
                    }

                    _services.MetricReportingService.AccountTime(
                        handle.StatementHandle.MetricsHandle,
                        performanceMetric.ExecTime,
                        performanceMetric.WallTime,
                        numInput);
                }
                else {
                    if (_timerThreading) {
                        _services.ThreadingService.SubmitTimerWork(new TimerUnitMultiple(_services, this, handle, callbackObject));
                    }
                    else {
                        ProcessStatementScheduleMultiple(handle, callbackObject, _services);
                    }
                }

                if (_isPrioritized && handle.IsPreemptive) {
                    break;
                }
            }
        }

        /// <summary>
        ///     Processing single schedule match for a statement.
        /// </summary>
        /// <param name="handle">statement handle</param>
        /// <param name="services">runtime services</param>
        public static void ProcessStatementScheduleSingle(
            EPStatementHandleCallbackSchedule handle,
            EPServicesContext services)
        {
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().QTimeCP(handle.AgentInstanceHandle, services.SchedulingService.Time);
            }

            var statementLock = handle.AgentInstanceHandle.StatementAgentInstanceLock;
            statementLock.AcquireWriteLock();
            try {
                if (!handle.AgentInstanceHandle.IsDestroyed) {
                    if (handle.AgentInstanceHandle.HasVariables) {
                        services.VariableManagementService.SetLocalVersion();
                    }

                    handle.ScheduleCallback.ScheduledTrigger();
                    handle.AgentInstanceHandle.InternalDispatch();
                }
            }
            catch (EPException)
            {
                throw;
            }
            catch (Exception ex)
            {
                services.ExceptionHandlingService.HandleException(ex, handle.AgentInstanceHandle, ExceptionHandlerExceptionType.PROCESS, null);
            }
            finally {
                if (handle.AgentInstanceHandle.HasTableAccess) {
                    services.TableExprEvaluatorContext.ReleaseAcquiredLocks();
                }

                handle.AgentInstanceHandle.StatementAgentInstanceLock.ReleaseWriteLock();

                if (InstrumentationHelper.ENABLED) {
                    InstrumentationHelper.Get().ATimeCP();
                }
            }
        }

        private EventBean WrapEventMap(
            IDictionary<string, object> map,
            string eventTypeName)
        {
            return _services.EventTypeResolvingBeanFactory.AdapterForMap(map, eventTypeName);
        }

        private EventBean WrapEventObjectArray(
            object[] objectArray,
            string eventTypeName)
        {
            return _services.EventTypeResolvingBeanFactory.AdapterForObjectArray(objectArray, eventTypeName);
        }

        private EventBean WrapEventBeanXMLDOM(
            XmlNode node,
            string eventTypeName)
        {
            return _services.EventTypeResolvingBeanFactory.AdapterForXMLDOM(node, eventTypeName);
        }

        private EventBean WrapEventAvro(
            object avroGenericDataDotRecord,
            string eventTypeName)
        {
            return _services.EventTypeResolvingBeanFactory.AdapterForAvro(avroGenericDataDotRecord, eventTypeName);
        }

        public IDictionary<DeploymentIdNamePair, long> StatementNearestSchedules => 
            GetStatementNearestSchedulesInternal(_services.SchedulingService, _services.StatementLifecycleService);

        public long NumEventsEvaluated => _services.FilterService.NumEventsEvaluated;

        private static IDictionary<DeploymentIdNamePair, long> GetStatementNearestSchedulesInternal(
            SchedulingServiceSPI schedulingService,
            StatementLifecycleService statementLifecycleSvc)
        {
            IDictionary<int, long> schedulePerStatementId = new Dictionary<int, long>();
            schedulingService.VisitSchedules(
                new ProxyScheduleVisitor {
                    ProcVisit = visit => {
                        if (schedulePerStatementId.ContainsKey(visit.StatementId)) {
                            return;
                        }

                        schedulePerStatementId.Put(visit.StatementId, visit.Timestamp);
                    }
                });

            IDictionary<DeploymentIdNamePair, long> result = new Dictionary<DeploymentIdNamePair, long>();
            foreach (var schedule in schedulePerStatementId) {
                var spi = statementLifecycleSvc.GetStatementById(schedule.Key);
                if (spi != null) {
                    result.Put(new DeploymentIdNamePair(spi.DeploymentId, spi.Name), schedule.Value);
                }
            }

            return result;
        }

        private void RouteEventInternal(EventBean theEvent)
        {
            if (_internalEventRouter.HasPreprocessing) {
                theEvent = _internalEventRouter.Preprocess(theEvent, _runtimeFilterAndDispatchTimeContext, InstrumentationHelper.Get());
                if (theEvent == null) {
                    return;
                }
            }

            _threadWorkQueue.AddBack(theEvent);
        }
    }
} // end of namespace