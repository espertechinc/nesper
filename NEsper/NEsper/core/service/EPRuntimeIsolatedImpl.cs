///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.client.time;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.context.util;
using com.espertech.esper.filter;
using com.espertech.esper.schedule;
using com.espertech.esper.util;

using DataMap = System.Collections.Generic.IDictionary<string, object>;
using TypeMap = System.Collections.Generic.IDictionary<string, System.Type>;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// Implementation for isolated runtime.
    /// </summary>
    public class EPRuntimeIsolatedImpl
        : EPRuntimeIsolatedSPI
        , InternalEventRouteDest
        , EPRuntimeEventSender
    {
        private readonly EPServicesContext _unisolatedServices;
        private EPIsolationUnitServices _services;
        private readonly bool _isSubselectPreeval;
        private readonly bool _isPrioritized;
        private readonly bool _isLatchStatementInsertStream;
        private readonly ThreadWorkQueue _threadWorkQueue;

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
            internal IDictionary<EPStatementAgentInstanceHandle, LinkedList<FilterHandleCallback>> MatchesPerStmtThreadLocal;
            internal ArrayBackedCollection<ScheduleHandle> ScheduleArrayThreadLocal;
            internal IDictionary<EPStatementAgentInstanceHandle, Object> SchedulePerStmtThreadLocal;
        }

        /// <summary>
        /// Gets the local data.
        /// </summary>
        /// <value>The local data.</value>
        private ThreadLocalData LocalData
        {
            get { return _threadLocalData.GetOrCreate(); }
        }

        /// <summary>
        /// Gets the schedule array.
        /// </summary>
        /// <value>The schedule array.</value>
        private ArrayBackedCollection<ScheduleHandle> ScheduleArray
        {
            get { return LocalData.ScheduleArrayThreadLocal; }
        }

        /// <summary>
        /// Gets the schedule per statement.
        /// </summary>
        /// <value>The schedule per statement.</value>
        private IDictionary<EPStatementAgentInstanceHandle, object> SchedulePerStmt
        {
            get { return LocalData.SchedulePerStmtThreadLocal; }
        }

        #endregion

        /// <summary>
        /// Creates a local data object.
        /// </summary>
        /// <returns></returns>
        private ThreadLocalData CreateLocalData()
        {
            var threadLocalData = new ThreadLocalData();
            threadLocalData.MatchesArrayThreadLocal =
                new List<FilterHandle>(100);
            threadLocalData.ScheduleArrayThreadLocal =
                new ArrayBackedCollection<ScheduleHandle>(100);

            if (_isPrioritized)
            {
                threadLocalData.MatchesPerStmtThreadLocal =
                    new OrderedDictionary<EPStatementAgentInstanceHandle, LinkedList<FilterHandleCallback>>(new EPStatementAgentInstanceHandlePrioritySort());
                threadLocalData.SchedulePerStmtThreadLocal =
                    new OrderedDictionary<EPStatementAgentInstanceHandle, Object>(new EPStatementAgentInstanceHandlePrioritySort(true));
            }
            else
            {
                threadLocalData.MatchesPerStmtThreadLocal =
                    new Dictionary<EPStatementAgentInstanceHandle, LinkedList<FilterHandleCallback>>(10000);
                threadLocalData.SchedulePerStmtThreadLocal =
                    new Dictionary<EPStatementAgentInstanceHandle, Object>(10000);
            }

            return threadLocalData;
        }

        /// <summary>Ctor. </summary>
        /// <param name="svc">isolated services</param>
        /// <param name="unisolatedSvc">engine services</param>
        public EPRuntimeIsolatedImpl(EPIsolationUnitServices svc, EPServicesContext unisolatedSvc)
        {
            _services = svc;
            _unisolatedServices = unisolatedSvc;
            _threadWorkQueue = new ThreadWorkQueue(unisolatedSvc.ThreadLocalManager);

            _isSubselectPreeval = unisolatedSvc.EngineSettingsService.EngineSettings.Expression.IsSelfSubselectPreeval;
            _isPrioritized = unisolatedSvc.EngineSettingsService.EngineSettings.Execution.IsPrioritized;
            _isLatchStatementInsertStream = unisolatedSvc.EngineSettingsService.EngineSettings.Threading.IsInsertIntoDispatchPreserveOrder;

            _threadLocalData = unisolatedSvc.ThreadLocalManager.Create(CreateLocalData);
        }

        public void SendEvent(Object theEvent)
        {
            if (theEvent == null)
            {
                Log.Error(".SendEvent Null object supplied");
                return;
            }

            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                if ((!(theEvent is CurrentTimeEvent)) || (ExecutionPathDebugLog.IsTimerDebugEnabled))
                {
                    Log.Debug(".SendEvent Processing event " + theEvent);
                }
            }

            // Process event
            ProcessEvent(theEvent);
        }

        public void SendEvent(XElement element)
        {
            if (element == null)
            {
                Log.Error(".SendEvent Null object supplied");
                return;
            }

            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(".SendEvent Processing DOM node event {0}", element);
            }

            // Get it wrapped up, process event
            EventBean eventBean = _unisolatedServices.EventAdapterService.AdapterForDOM(element);
            ProcessEvent(eventBean);
        }

        public void SendEvent(XmlNode document)
        {
            if (document == null)
            {
                Log.Error(".SendEvent Null object supplied");
                return;
            }

            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(".SendEvent Processing DOM node event " + document);
            }

            // Get it wrapped up, process event
            EventBean eventBean = _unisolatedServices.EventAdapterService.AdapterForDOM(document);
            ProcessEvent(eventBean);
        }

        /// <summary>Route a XML docment event </summary>
        /// <param name="document">to route</param>
        /// <throws>EPException if routing failed</throws>
        public void Route(XmlNode document)
        {
            if (document == null)
            {
                Log.Error(".SendEvent Null object supplied");
                return;
            }

            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(".SendEvent Processing DOM node event " + document);
            }

            // Get it wrapped up, process event
            EventBean eventBean = _unisolatedServices.EventAdapterService.AdapterForDOM(document);
            _threadWorkQueue.AddBack(eventBean);
        }

        public void SendEvent(DataMap map, String eventTypeName)
        {
            if (map == null)
            {
                throw new ArgumentException("Invalid null event object");
            }

            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(".sendMap Processing event " + map);
            }

            // Process event
            EventBean eventBean = _unisolatedServices.EventAdapterService.AdapterForMap(map, eventTypeName);
            ProcessWrappedEvent(eventBean);
        }

        public void SendEvent(Object[] objectarray, String objectArrayEventTypeName)
        {
            if (objectarray == null)
            {
                throw new ArgumentException("Invalid null event object");
            }

            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(".sendEvent Processing event {0}", objectarray);
            }

            // Process event
            EventBean eventBean = _unisolatedServices.EventAdapterService.AdapterForObjectArray(objectarray, objectArrayEventTypeName);
            ProcessWrappedEvent(eventBean);
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
                eventBean = _unisolatedServices.EventAdapterService.AdapterForObject(theEvent);
            }

            ProcessWrappedEvent(eventBean);
        }

        /// <summary>Process a wrapped event. </summary>
        /// <param name="eventBean">to process</param>
        public void ProcessWrappedEvent(EventBean eventBean)
        {
            // Acquire main processing lock which locks out statement management
            using (_unisolatedServices.EventProcessingRWLock.AcquireReadLock())
            {
                try
                {
                    ProcessMatches(eventBean);
                }
                catch (EPException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    LocalData.MatchesArrayThreadLocal.Clear();
                    throw new EPException(ex);
                }
            }

            // Dispatch results to listeners
            // Done outside of the read-lock to prevent lockups when listeners create statements
            Dispatch();

            // Work off the event queue if any events accumulated in there via a Route() or insert-into
            ProcessThreadWorkQueue();
        }

        private void ProcessTimeEvent(TimerEvent theEvent)
        {
            if (theEvent is TimerControlEvent)
            {
                var tce = (TimerControlEvent)theEvent;
                if (tce.ClockType == TimerControlEvent.ClockTypeEnum.CLOCK_INTERNAL)
                {
                    Log.Warn("Timer control events are not processed by the isolated runtime as the setting is always external timer.");
                }
                return;
            }

            // Evaluation of all time events is protected from statement management
            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled) && (ExecutionPathDebugLog.IsTimerDebugEnabled))
            {
                Log.Debug(".processTimeEvent Setting time and evaluating schedules");
            }

            long currentTime;

            if (theEvent is CurrentTimeEvent)
            {
                var current = (CurrentTimeEvent)theEvent;

                currentTime = current.Time;
                if (currentTime == _services.SchedulingService.Time)
                {
                    Log.Warn("Duplicate time event received for currentTime {0}", currentTime);
                }

                _services.SchedulingService.Time = currentTime;

                ProcessSchedule();

                // Let listeners know of results
                Dispatch();

                // Work off the event queue if any events accumulated in there via a route()
                ProcessThreadWorkQueue();

                return;
            }

            // handle time span
            var span = (CurrentTimeSpanEvent)theEvent;
            var targetTime = span.TargetTime;
            var optionalResolution = span.OptionalResolution;
            currentTime = _services.SchedulingService.Time;

            if (targetTime < currentTime)
            {
                Log.Warn("Past or current time event received for currentTime {0}", targetTime);
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
                    if (nearest == null)
                    {
                        currentTime = targetTime;
                    }
                    else
                    {
                        currentTime = nearest.Value;
                    }
                }
                if (currentTime > targetTime)
                {
                    currentTime = targetTime;
                }

                // Evaluation of all time events is protected from statement management
                if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled) &&
                    (ExecutionPathDebugLog.IsTimerDebugEnabled))
                {
                    Log.Debug(".processTimeEvent Setting time and evaluating schedules for time " + currentTime);
                }

                _services.SchedulingService.Time = currentTime;

                ProcessSchedule();

                // Let listeners know of results
                Dispatch();

                // Work off the event queue if any events accumulated in there via a route()
                ProcessThreadWorkQueue();
            }
        }

        private void ProcessSchedule()
        {
            ArrayBackedCollection<ScheduleHandle> handles = ScheduleArray;

            // Evaluation of schedules is protected by an optional scheduling service lock and then the engine lock
            // We want to stay in this order for allowing the engine lock as a second-order lock to the
            // services own lock, if it has one.
            using (_unisolatedServices.EventProcessingRWLock.AcquireReadLock())
            {
                _services.SchedulingService.Evaluate(handles);
            }
            using (_unisolatedServices.EventProcessingRWLock.AcquireReadLock())
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

        private void ProcessScheduleHandles(ArrayBackedCollection<ScheduleHandle> handles)
        {
            if (ThreadLogUtil.ENABLED_TRACE)
            {
                ThreadLogUtil.Trace("Found schedules for", handles.Count);
            }

            if (handles.Count == 0)
            {
                return;
            }

            // handle 1 result separatly for performance reasons
            if (handles.Count == 1)
            {
                Object[] handleArray = handles.Array;
                var handle = (EPStatementHandleCallback)handleArray[0];

                EPRuntimeImpl.ProcessStatementScheduleSingle(handle, _unisolatedServices);

                handles.Clear();
                return;
            }

            Object[] matchArray = handles.Array;
            int entryCount = handles.Count;

            LinkedList<ScheduleHandleCallback> entries;

            // sort multiple matches for the event into statements
            var stmtCallbacks = SchedulePerStmt;
            stmtCallbacks.Clear();
            for (int i = 0; i < entryCount; i++)
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
                    entries = new LinkedList<ScheduleHandleCallback>();
                    entries.AddLast(existingCallback);
                    entries.AddLast(callback);
                    stmtCallbacks.Put(handle, entries);
                    continue;
                }

                // This statement has been encountered more then once before
                entries = (LinkedList<ScheduleHandleCallback>)entry;
                entries.AddLast(callback);
            }
            handles.Clear();

            foreach (var entry in stmtCallbacks)
            {
                var handle = entry.Key;
                var callbackObject = entry.Value;

                EPRuntimeImpl.ProcessStatementScheduleMultiple(handle, callbackObject, _unisolatedServices);

                if ((_isPrioritized) && (handle.IsPreemptive))
                {
                    break;
                }
            }
        }

        /// <summary>Works off the thread's work queue. </summary>
        public void ProcessThreadWorkQueue()
        {
            DualWorkQueue<Object> queues = _threadWorkQueue.ThreadQueue;

            Object item;
            if (queues.FrontQueue.IsEmpty())
            {
                bool haveDispatched = _unisolatedServices.NamedWindowDispatchService.Dispatch();
                if (haveDispatched)
                {
                    // Dispatch results to listeners
                    Dispatch();
                    if (!queues.FrontQueue.IsEmpty())
                    {
                        ProcessThreadWorkQueueFront(queues);
                    }
                }
            }
            else
            {
                ProcessThreadWorkQueueFront(queues);
            }

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

                bool haveDispatched = _unisolatedServices.NamedWindowDispatchService.Dispatch();
                if (haveDispatched)
                {
                    Dispatch();
                }

                if (!queues.FrontQueue.IsEmpty())
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

                bool haveDispatched = _unisolatedServices.NamedWindowDispatchService.Dispatch();
                if (haveDispatched)
                {
                    Dispatch();
                }
            }
        }

        private void ProcessThreadWorkQueueLatchedWait(InsertIntoLatchWait insertIntoLatch)
        {
            // wait for the latch to complete
            EventBean eventBean = insertIntoLatch.Await();

            using (_unisolatedServices.EventProcessingRWLock.AcquireReadLock())
            {
                try
                {
                    ProcessMatches(eventBean);
                }
                catch (Exception)
                {
                    LocalData.MatchesArrayThreadLocal.Clear();
                    throw;
                }
                finally
                {
                    insertIntoLatch.Done();
                }
            }

            Dispatch();
        }

        private void ProcessThreadWorkQueueLatchedSpin(InsertIntoLatchSpin insertIntoLatch)
        {
            // wait for the latch to complete
            EventBean eventBean = insertIntoLatch.Await();

            using (_unisolatedServices.EventProcessingRWLock.AcquireReadLock())
            {
                try
                {
                    ProcessMatches(eventBean);
                }
                catch (Exception)
                {
                    LocalData.MatchesArrayThreadLocal.Clear();
                    throw;
                }
                finally
                {
                    insertIntoLatch.Done();
                }
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
                eventBean = _unisolatedServices.EventAdapterService.AdapterForObject(item);
            }

            using (_unisolatedServices.EventProcessingRWLock.AcquireReadLock())
            {
                try
                {
                    ProcessMatches(eventBean);
                }
                catch (Exception)
                {
                    LocalData.MatchesArrayThreadLocal.Clear();
                    throw;
                }
            }

            Dispatch();
        }

        private void ProcessMatches(EventBean theEvent)
        {
            var localData = LocalData;

            // get matching filters
            var matches = localData.MatchesArrayThreadLocal;
            _services.FilterService.Evaluate(theEvent, matches);

            if (ThreadLogUtil.ENABLED_TRACE)
            {
                ThreadLogUtil.Trace("Found matches for underlying ", matches.Count, theEvent.Underlying);
            }

            if (matches.Count == 0)
            {
                return;
            }

            var stmtCallbacks = localData.MatchesPerStmtThreadLocal;
            int entryCount = matches.Count;

            for (int i = 0; i < entryCount; i++)
            {
                var handleCallback = (EPStatementHandleCallback)matches[i];
                var handle = handleCallback.AgentInstanceHandle;

                // Self-joins require that the internal dispatch happens after all streams are evaluated.
                // Priority or preemptive settings also require special ordering.
                if (handle.CanSelfJoin || _isPrioritized)
                {
                    var callbacks = stmtCallbacks.Get(handle);
                    if (callbacks == null)
                    {
                        callbacks = new LinkedList<FilterHandleCallback>();
                        stmtCallbacks.Put(handle, callbacks);
                    }
                    callbacks.AddLast(handleCallback.FilterCallback);
                    continue;
                }

                ProcessStatementFilterSingle(handle, handleCallback, theEvent);
            }
            matches.Clear();
            if (stmtCallbacks.IsEmpty())
            {
                return;
            }

            foreach (var entry in stmtCallbacks)
            {
                var handle = entry.Key;
                var callbackList = entry.Value;

                ProcessStatementFilterMultiple(handle, callbackList, theEvent);

                if ((_isPrioritized) && (handle.IsPreemptive))
                {
                    break;
                }
            }
            stmtCallbacks.Clear();
        }

        /// <summary>Processing multiple filter matches for a statement. </summary>
        /// <param name="handle">statement handle</param>
        /// <param name="callbackList">object containing callbacks</param>
        /// <param name="theEvent">to process</param>
        public void ProcessStatementFilterMultiple(EPStatementAgentInstanceHandle handle, ICollection<FilterHandleCallback> callbackList, EventBean theEvent)
        {
            using (handle.StatementAgentInstanceLock.AcquireWriteLock())
            {
                try
                {
                    if (handle.HasVariables)
                    {
                        _unisolatedServices.VariableService.SetLocalVersion();
                    }

                    handle.MultiMatchHandler.Handle(callbackList, theEvent);

                    // internal join processing, if applicable
                    handle.InternalDispatch();
                }
                catch (Exception ex)
                {
                    _unisolatedServices.ExceptionHandlingService.HandleException(ex, handle, ExceptionHandlerExceptionType.PROCESS, theEvent);
                }
                finally
                {
                    if (handle.HasTableAccess)
                    {
                        _unisolatedServices.TableService.TableExprEvaluatorContext.ReleaseAcquiredLocks();
                    }
                }
            }
        }

        /// <summary>Process a single match. </summary>
        /// <param name="handle">statement</param>
        /// <param name="handleCallback">callback</param>
        /// <param name="theEvent">event to indicate</param>
        public void ProcessStatementFilterSingle(EPStatementAgentInstanceHandle handle, EPStatementHandleCallback handleCallback, EventBean theEvent)
        {
            using (handle.StatementAgentInstanceLock.AcquireWriteLock())
            {
                try
                {
                    if (handle.HasVariables)
                    {
                        _unisolatedServices.VariableService.SetLocalVersion();
                    }

                    handleCallback.FilterCallback.MatchFound(theEvent, null);

                    // internal join processing, if applicable
                    handle.InternalDispatch();
                }
                catch (Exception ex)
                {
                    _unisolatedServices.ExceptionHandlingService.HandleException(ex, handle, ExceptionHandlerExceptionType.PROCESS, theEvent);
                }
                finally
                {
                    if (handle.HasTableAccess)
                    {
                        _unisolatedServices.TableService.TableExprEvaluatorContext.ReleaseAcquiredLocks();
                    }
                }
            }
        }

        /// <summary>Dispatch events. </summary>
        public void Dispatch()
        {
            try
            {
                _unisolatedServices.DispatchService.Dispatch();
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

        /// <summary>
        /// Dispose for destroying an engine instance: sets references to null and clears thread-locals
        /// </summary>
        public void Dispose()
        {
            _services = null;
            _threadLocalData = null;
        }

        public long CurrentTime
        {
            get { return _services.SchedulingService.Time; }
        }

        // Internal route of events via insert-into, holds a statement lock
        public void Route(EventBean theEvent, EPStatementHandle epStatementHandle, bool addToFront)
        {
            if (_isLatchStatementInsertStream)
            {
                if (addToFront)
                {
                    Object latch = epStatementHandle.InsertIntoFrontLatchFactory.NewLatch(theEvent);
                    _threadWorkQueue.AddFront(latch);
                }
                else
                {
                    Object latch = epStatementHandle.InsertIntoBackLatchFactory.NewLatch(theEvent);
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

        public EventSender GetEventSender(String eventTypeName)
        {
            return _unisolatedServices.EventAdapterService.GetStaticTypeEventSender(
                this, eventTypeName,
                _unisolatedServices.ThreadingService,
                _unisolatedServices.LockManager);
        }

        public EventSender GetEventSender(Uri[] uri)
        {
            return _unisolatedServices.EventAdapterService.GetDynamicTypeEventSender(
                this, uri, _unisolatedServices.ThreadingService);
        }

        public void RouteEventBean(EventBean theEvent)
        {
            _threadWorkQueue.AddBack(theEvent);
        }

        public InternalEventRouter InternalEventRouter
        {
            set { throw new UnsupportedOperationException("Isolated runtime does not route itself"); }
        }


        public long? NextScheduledTime
        {
            get { return _services.SchedulingService.NearestTimeHandle; }
        }

        public IDictionary<string, long> StatementNearestSchedules
        {
            get
            {
                return EPRuntimeImpl.GetStatementNearestSchedulesInternal(
                    _services.SchedulingService,
                    _unisolatedServices.StatementLifecycleSvc);
            }
        }

        public string EngineURI
        {
            get { return _unisolatedServices.EngineURI; }
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
