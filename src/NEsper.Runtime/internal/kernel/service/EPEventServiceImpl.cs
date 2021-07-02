///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Xml;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.exception;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.avro;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.json.compiletime;
using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.common.@internal.@event.util;
using com.espertech.esper.common.@internal.@event.xml;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.statement.insertintolatch;
using com.espertech.esper.runtime.@internal.timer;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.diagnostics;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading.threadlocal;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.@internal.kernel.thread;
using com.espertech.esper.runtime.@internal.metrics.instrumentation;
using com.espertech.esper.runtime.@internal.schedulesvcimpl;
using com.espertech.esper.runtime.@internal.statementlifesvc;

using static com.espertech.esper.runtime.@internal.kernel.service.EPEventServiceHelper;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
	/// <summary>
	/// Implements runtime interface. Also accepts timer callbacks for synchronizing time events with regular events
	/// sent in.
	/// </summary>
	public class EPEventServiceImpl : EPEventServiceSPI,
		InternalEventRouteDest,
		ITimerCallback,
		EPRuntimeEventProcessWrapped
	{
		private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public const int MAX_FILTER_FAULT_COUNT = 10;

		private EPServicesContext _services;
		private bool _inboundThreading;
		private bool _routeThreading;
		private bool _timerThreading;
		private bool _isLatchStatementInsertStream;
		private bool _isUsingExternalClocking;
		private bool _isPrioritized;
		private volatile UnmatchedListener _unmatchedListener;
		private AtomicLong _routedInternal;
		private AtomicLong _routedExternal;
		private InternalEventRouter _internalEventRouter;
		private IThreadLocal<EPEventServiceThreadLocalEntry> _threadLocals;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="services">references to services</param>
		public EPEventServiceImpl(EPServicesContext services)
		{
			this._services = services;
			_inboundThreading = services.ThreadingService.IsInboundThreading;
			_routeThreading = services.ThreadingService.IsRouteThreading;
			_timerThreading = services.ThreadingService.IsTimerThreading;
			_isLatchStatementInsertStream = this._services.RuntimeSettingsService.ConfigurationRuntime.Threading.IsInsertIntoDispatchPreserveOrder;
			_isUsingExternalClocking = !this._services.RuntimeSettingsService.ConfigurationRuntime.Threading.IsInternalTimerEnabled;
			_isPrioritized = services.RuntimeSettingsService.ConfigurationRuntime.Execution.IsPrioritized;
			_routedInternal = new AtomicLong();
			_routedExternal = new AtomicLong();

			InitThreadLocals();

			services.ThreadingService.InitThreading(RuntimeURI, services);
		}

		public EPServicesContext Services => _services;

		/// <summary>
		/// Sets the route for events to use
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

			if ((ExecutionPathDebugLog.IsDebugEnabled) && (Log.IsDebugEnabled && (ExecutionPathDebugLog.IsTimerDebugEnabled))) {
				Log.Debug(".timerCallback Evaluating scheduled callbacks, time is " + msec);
			}

			AdvanceTime(msec);
		}

		public void SendEventAvro(
			object avroGenericDataDotRecord,
			string avroEventTypeName)
		{
			if (avroGenericDataDotRecord == null) {
				throw new ArgumentException("Invalid null event object");
			}

			if ((ExecutionPathDebugLog.IsDebugEnabled) && (Log.IsDebugEnabled)) {
				Log.Debug(".sendMap Processing event " + avroGenericDataDotRecord);
			}

			if (_inboundThreading) {
				_services.ThreadingService.SubmitInbound(new InboundUnitSendAvro(avroGenericDataDotRecord, avroEventTypeName, this, _services));
			}
			else {
				var eventBean = WrapEventAvro(avroGenericDataDotRecord, avroEventTypeName);
				ProcessWrappedEvent(eventBean);
			}
		}

		public void SendEventJson(
			string json,
			string jsonEventTypeName)
		{
			if (json == null) {
				throw new ArgumentException("Invalid null event object");
			}

			if ((ExecutionPathDebugLog.IsDebugEnabled) && (Log.IsDebugEnabled)) {
				Log.Debug(".sendEventJson Processing event " + json);
			}

			if (_inboundThreading) {
				_services.ThreadingService.SubmitInbound(
					new InboundUnitSendJson(json, jsonEventTypeName, this, _services));
			}
			else {
				var eventBean = WrapEventJson(json, jsonEventTypeName);
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

			if ((ExecutionPathDebugLog.IsDebugEnabled) && (Log.IsDebugEnabled)) {
				Log.Debug(".sendEvent Processing event " + theEvent);
			}

			if (_inboundThreading) {
				_services.ThreadingService.SubmitInbound(
					new InboundUnitSendEvent(theEvent, eventTypeName, this, _services));
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

		public long? NextScheduledTime => _services.SchedulingServiceSPI.NearestTimeHandle;

		private void AdvanceTimeSpanInternal(
			long targetTime,
			long? optionalResolution)
		{
			var currentTime = _services.SchedulingService.Time;

			while (currentTime < targetTime) {

				if ((optionalResolution != null) && (optionalResolution > 0)) {
					currentTime += optionalResolution.Value;
				}
				else {
					long? nearest = _services.SchedulingServiceSPI.NearestTimeHandle;
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
					InstrumentationHelper.Get()
						.QStimulantTime(_services.SchedulingService.Time, currentTime, targetTime, true, optionalResolution, _services.RuntimeURI);
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
				_services.ThreadingService.SubmitInbound(
					new InboundUnitSendDOM(node, eventTypeName, this, _services));
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

			if ((ExecutionPathDebugLog.IsDebugEnabled) && (Log.IsDebugEnabled)) {
				Log.Debug(".sendEventObjectArray Processing event " + propertyValues.RenderAny());
			}

			if (_inboundThreading) {
				_services.ThreadingService.SubmitInbound(
					new InboundUnitSendObjectArray(propertyValues, eventTypeName, this, _services));
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

			if ((ExecutionPathDebugLog.IsDebugEnabled) && (Log.IsDebugEnabled)) {
				Log.Debug(".sendMap Processing event " + map);
			}

			if (_inboundThreading) {
				_services.ThreadingService.SubmitInbound(
					new InboundUnitSendMap(map, mapEventTypeName, this, _services));
			}
			else {
				var eventBean = WrapEventMap(map, mapEventTypeName);
				ProcessWrappedEvent(eventBean);
			}
		}

		public void RouteEventBean(EventBean theEvent)
		{
			_threadLocals.GetOrCreate().DualWorkQueue.BackQueue.AddLast(theEvent);
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

			var threadWorkQueue = _threadLocals.GetOrCreate().DualWorkQueue;
			if (theEvent is NaturalEventBean) {
				theEvent = ((NaturalEventBean) theEvent).OptionalSynthetic;
			}

			_routedInternal.IncrementAndGet();

			if (_isLatchStatementInsertStream) {
				if (addToFront) {
					var latch = epStatementHandle.InsertIntoFrontLatchFactory.NewLatch(theEvent);
					threadWorkQueue.FrontQueue.AddLast(latch);
				}
				else {
					var latch = epStatementHandle.InsertIntoBackLatchFactory.NewLatch(theEvent);
					threadWorkQueue.BackQueue.AddLast(latch);
				}
			}
			else {
				if (addToFront) {
					threadWorkQueue.FrontQueue.AddLast(theEvent);
				}
				else {
					threadWorkQueue.BackQueue.AddLast(theEvent);
				}
			}

			if (InstrumentationHelper.ENABLED) {
				InstrumentationHelper.Get().ARouteBetweenStmt();
			}
		}

		public void ProcessWrappedEvent(EventBean eventBean)
		{
			if (InstrumentationHelper.ENABLED) {
				InstrumentationHelper.Get().QStimulantEvent(eventBean, _services.RuntimeURI);
			}

			var tlEntry = _threadLocals.GetOrCreate();
			if (_internalEventRouter.HasPreprocessing) {
				eventBean = _internalEventRouter.Preprocess(eventBean, tlEntry.ExprEvaluatorContext, InstrumentationHelper.Get());
				if (eventBean == null) {
					return;
				}
			}

			// Acquire main processing lock which locks out statement management
			if (InstrumentationHelper.ENABLED) {
				InstrumentationHelper.Get().QEvent(eventBean, _services.RuntimeURI, true);
			}

			using (_services.EventProcessingRWLock.AcquireReadLock()) {
				try {
					ProcessMatches(eventBean);
				}
				catch (EPException) {
					tlEntry.MatchesArrayThreadLocal.Clear();
					throw;
				}
				catch (Exception ex) {
					tlEntry.MatchesArrayThreadLocal.Clear();
					throw new EPException(ex);
				}
				finally {
					if (InstrumentationHelper.ENABLED) {
						InstrumentationHelper.Get().AEvent();
					}
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

		/// <summary>
		/// Works off the thread's work queue.
		/// </summary>
		public void ProcessThreadWorkQueue()
		{
			var queues = _threadLocals.GetOrCreate().DualWorkQueue;

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
				if (item is InsertIntoLatchSpin) {
					ProcessThreadWorkQueueLatchedSpin((InsertIntoLatchSpin) item);
				}
				else if (item is InsertIntoLatchWait) {
					ProcessThreadWorkQueueLatchedWait((InsertIntoLatchWait) item);
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

		private void ProcessThreadWorkQueueFront(DualWorkQueue<object> queues)
		{
			object item;
			while ((item = queues.FrontQueue.Poll()) != null) {
				if (item is InsertIntoLatchSpin) {
					ProcessThreadWorkQueueLatchedSpin((InsertIntoLatchSpin) item);
				}
				else if (item is InsertIntoLatchWait) {
					ProcessThreadWorkQueueLatchedWait((InsertIntoLatchWait) item);
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

			try {
				using (_services.EventProcessingRWLock.AcquireReadLock()) {
					try {
						ProcessMatches(eventBean);
					}
					catch (Exception) {
						_threadLocals.GetOrCreate().MatchesArrayThreadLocal.Clear();
						throw;
					}
					finally {
						insertIntoLatch.Done();
					}
				}
			}
			finally {
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

			try {
				using (_services.EventProcessingRWLock.AcquireReadLock()) {
					try {
						ProcessMatches(eventBean);
					}
					catch (Exception) {
						_threadLocals.GetOrCreate().MatchesArrayThreadLocal.Clear();
						throw;
					}
					finally {
						insertIntoLatch.Done();
					}
				}
			}
			finally {
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

			try {
				using (_services.EventProcessingRWLock.AcquireReadLock()) {
					try {
						ProcessMatches(eventBean);
					}
					catch (Exception) {
						_threadLocals.GetOrCreate().MatchesArrayThreadLocal.Clear();
						throw;
					}
				}
			}
			finally {
				if (InstrumentationHelper.ENABLED) {
					InstrumentationHelper.Get().AEvent();
				}
			}

			Dispatch();
		}

		protected void ProcessMatches(EventBean theEvent)
		{
			// get matching filters
			var tlEntry = _threadLocals.GetOrCreate();
			var matches = tlEntry.MatchesArrayThreadLocal;
			var ctx = tlEntry.ExprEvaluatorContext;
			var version = _services.FilterService.Evaluate(theEvent, matches, ctx);

			if (ThreadLogUtil.ENABLED_TRACE) {
				ThreadLogUtil.Trace("Found matches for underlying ", matches.Count, theEvent.Underlying);
			}

			if (matches.Count == 0) {
				if (_unmatchedListener != null) {
					_services.EventProcessingRWLock.ReadLock.Release(); // Allow listener to create new statements
					try {
						_unmatchedListener.Invoke(theEvent);
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

			var stmtCallbacks = tlEntry.MatchesPerStmtThreadLocal;
			var matchArray = matches.Array;
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
					else if (callbacks is ArrayDeque<FilterHandleCallback>) {
						var q = (ArrayDeque<FilterHandleCallback>) callbacks;
						q.Add(handleCallback.FilterCallback);
					}
					else {
						ArrayDeque<FilterHandleCallback> q = new ArrayDeque<FilterHandleCallback>();
						q.Add((FilterHandleCallback) callbacks);
						q.Add(handleCallback.FilterCallback);
						stmtCallbacks.Put(handle, q);
					}

					continue;
				}

				if (handle.StatementHandle.MetricsHandle.IsEnabled) {
					var performanceMetrics = PerformanceMetricsHelper.Call(
						() => ProcessStatementFilterSingle(handle, handleCallback, theEvent, version, 0));
					_services.MetricReportingService.AccountTime(handle.StatementHandle.MetricsHandle, performanceMetrics, 1);
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
					var size = 1;
					if (callbackList is ICollection<object>) {
						size = ((ICollection<object>) callbackList).Count;
					}

					var metrics = PerformanceMetricsHelper.Call(
						() => ProcessStatementFilterMultiple(handle, callbackList, theEvent, version, 0), size);

					_services.MetricReportingService.AccountTime(handle.StatementHandle.MetricsHandle, metrics, size);
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
		/// Processing multiple filter matches for a statement.
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
					if (callbackList is ICollection<FilterHandleCallback>) {
						var callbacks = (ICollection<FilterHandleCallback>) callbackList;
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
			catch (Exception ex) {
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
		/// Process a single match.
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
			catch (Exception ex) {
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
			var version = _services.FilterService.Evaluate(
				theEvent,
				callbacksForStatement,
				faultingHandle.StatementId,
				_threadLocals.GetOrCreate().ExprEvaluatorContext);

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
				stmtCallbacks = new SortedDictionary<EPStatementAgentInstanceHandle, object>(EPStatementAgentInstanceHandleComparer.INSTANCE);
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
					else if (callbacks is ArrayDeque<FilterHandleCallback>) {
						var q = (ArrayDeque<FilterHandleCallback>) callbacks;
						q.Add(handleCallback.FilterCallback);
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

		/// <summary>
		/// Dispatch events.
		/// </summary>
		public void Dispatch()
		{
			try {
				_services.DispatchService.Dispatch();
			}
			catch (Exception ex) {
				throw new EPException(ex);
			}
		}

		public bool IsExternalClockingEnabled()
		{
			return _isUsingExternalClocking;
		}

		/// <summary>
		/// Destroy for destroying an runtime instance: sets references to null and clears thread-locals
		/// </summary>
		public void Destroy()
		{
			_services = null;
			RemoveFromThreadLocals();
		}

		public void Initialize()
		{
			InitThreadLocals();
		}

		public void ClearCaches()
		{
			InitThreadLocals();
		}

		public UnmatchedListener UnmatchedListener {
			get => this._unmatchedListener;
			set => this._unmatchedListener = value;
		}

		public long CurrentTime => _services.SchedulingService.Time;

		public string RuntimeURI => _services.RuntimeURI;

		private void RemoveFromThreadLocals()
		{
			_threadLocals?.Remove();
		}

		private void InitThreadLocals()
		{
			RemoveFromThreadLocals();
			_threadLocals = AllocateThreadLocals(
				_isPrioritized,
				_services.RuntimeURI,
				_services.EventBeanService,
				_services.ExceptionHandlingService,
				_services.SchedulingService,
				_services.ImportServiceRuntime.TimeZone,
				_services.ImportServiceRuntime.TimeAbacus,
				_services.VariableManagementService);
		}

		private void ProcessSchedule(long time)
		{
			if (InstrumentationHelper.ENABLED) {
				InstrumentationHelper.Get().QTime(time, _services.RuntimeURI);
			}

			var handles = _threadLocals.GetOrCreate().ScheduleArrayThreadLocal;

			// Evaluation of schedules is protected by an optional scheduling service lock and then the runtime lock
			// We want to stay in this order for allowing the runtime lock as a second-order lock to the
			// services own lock, if it has one.
			using (_services.EventProcessingRWLock.AcquireReadLock()) {
				_services.SchedulingService.Evaluate(handles);
			}

			try {
				using (_services.EventProcessingRWLock.AcquireReadLock()) {
					try {
						ProcessScheduleHandles(handles);
					}
					catch (Exception) {
						handles.Clear();
						throw;
					}
				}
			}
			finally {
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
				var handleArray = handles.Array;
				var handle = (EPStatementHandleCallbackSchedule) handleArray[0];

				if (handle.AgentInstanceHandle.StatementHandle.MetricsHandle.IsEnabled) {
					var metrics = PerformanceMetricsHelper.Call(
						() => ProcessStatementScheduleSingle(handle, _services));

					_services.MetricReportingService.AccountTime(handle.AgentInstanceHandle.StatementHandle.MetricsHandle, metrics, 1);
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

			var matchArray = handles.Array;
			var entryCount = handles.Count;

			// sort multiple matches for the event into statements
			var stmtCallbacks = _threadLocals.GetOrCreate().SchedulePerStmtThreadLocal;
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
				if (entry is ScheduleHandleCallback) {
					var existingCallback = (ScheduleHandleCallback) entry;
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

			foreach (KeyValuePair<EPStatementAgentInstanceHandle, object> entry in stmtCallbacks) {
				var handle = entry.Key;
				var callbackObject = entry.Value;

				if (handle.StatementHandle.MetricsHandle.IsEnabled) {
					var metrics = PerformanceMetricsHelper.Call(
						() => ProcessStatementScheduleMultiple(handle, callbackObject, _services));

					var numInput = (callbackObject is ICollection<ScheduleHandleCallback> callbackCollection) ? callbackCollection.Count : 1;
					_services.MetricReportingService.AccountTime(handle.StatementHandle.MetricsHandle, metrics, numInput);
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

		private EventBean WrapEventJson(
			string json,
			string eventTypeName)
		{
			return _services.EventTypeResolvingBeanFactory.AdapterForJson(json, eventTypeName);
		}

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

		public void RouteEventJson(
			string json,
			string eventTypeName)
		{
			if (json == null) {
				throw new ArgumentException("Invalid null event object");
			}

			var theEvent = _services.EventTypeResolvingBeanFactory.AdapterForJson(json, eventTypeName);
			RouteEventInternal(theEvent);
		}

		public string URI => RuntimeURI;

		public EventSender GetEventSender(string eventTypeName)
		{
			var eventType = _services.EventTypeRepositoryBus.GetTypeByName(eventTypeName);
			if (eventType == null) {
				throw new EventTypeException("Event type named '" + eventTypeName + "' could not be found");
			}

			// handle built-in types
			var threadingService = _services.ThreadingService;
			if (eventType is BeanEventType) {
				return new EventSenderBean(this, (BeanEventType) eventType, _services.EventBeanTypedEventFactory, threadingService);
			}

			if (eventType is MapEventType) {
				return new EventSenderMap(this, (MapEventType) eventType, _services.EventBeanTypedEventFactory, threadingService);
			}

			if (eventType is ObjectArrayEventType) {
				return new EventSenderObjectArray(this, (ObjectArrayEventType) eventType, _services.EventBeanTypedEventFactory, threadingService);
			}

			if (eventType is BaseXMLEventType) {
				return new EventSenderXMLDOM(this, (BaseXMLEventType) eventType, _services.EventBeanTypedEventFactory, threadingService);
			}

			if (eventType is AvroSchemaEventType) {
				return new EventSenderAvro(this, eventType, _services.EventBeanTypedEventFactory, threadingService);
			}

			if (eventType is JsonEventType) {
				return new EventSenderJsonImpl(this, (JsonEventType) eventType, _services.EventBeanTypedEventFactory, threadingService);
			}

			throw new EventTypeException("An event sender for event type named '" + eventTypeName + "' could not be created as the type is not known");
		}

		public IDictionary<DeploymentIdNamePair, long> StatementNearestSchedules {
			get {
				return GetStatementNearestSchedulesInternal(
					_services.SchedulingServiceSPI,
					_services.StatementLifecycleService);
			}
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

		public long NumEventsEvaluated => _services.FilterService.NumEventsEvaluated;

		public void ResetStats()
		{
			_services.FilterService.ResetStats();
			_routedInternal.Set(0);
			_routedExternal.Set(0);
		}

		private static IDictionary<DeploymentIdNamePair, long> GetStatementNearestSchedulesInternal(
			SchedulingServiceSPI schedulingService,
			StatementLifecycleService statementLifecycleSvc)
		{
			var schedulePerStatementId = new Dictionary<int, long>();
			schedulingService.VisitSchedules(
				new ProxyScheduleVisitor() {
					ProcVisit = (visit) => {
						if (schedulePerStatementId.ContainsKey(visit.StatementId)) {
							return;
						}

						schedulePerStatementId.Put(visit.StatementId, visit.Timestamp);
					},
				});

			var result = new Dictionary<DeploymentIdNamePair, long>();
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
			var tlEntry = _threadLocals.GetOrCreate();
			if (_internalEventRouter.HasPreprocessing) {
				var exprEvaluatorContext = tlEntry.ExprEvaluatorContext;
				theEvent = _internalEventRouter.Preprocess(theEvent, exprEvaluatorContext, InstrumentationHelper.Get());
				if (theEvent == null) {
					return;
				}
			}

			tlEntry.DualWorkQueue.BackQueue.AddLast(theEvent);
		}
	}
} // end of namespace
