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
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.diagnostics;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading.threadlocal;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.@internal.kernel.service;
using com.espertech.esper.runtime.@internal.kernel.thread;
using com.espertech.esper.runtime.@internal.metrics.instrumentation;
using com.espertech.esper.runtime.@internal.schedulesvcimpl;
using com.espertech.esper.runtime.@internal.statementlifesvc;

using static com.espertech.esper.runtime.@internal.kernel.service.EPEventServiceHelper;
using static com.espertech.esper.runtime.@internal.kernel.service.EPEventServiceImpl;

using EPStatementAgentInstanceHandleComparer = com.espertech.esper.runtime.@internal.kernel.service.EPStatementAgentInstanceHandleComparer; // MAX_FILTER_FAULT_COUNT

namespace com.espertech.esper.runtime.@internal.kernel.stage
{
	public class EPStageEventServiceImpl : EPStageEventServiceSPI,
		InternalEventRouteDest,
		EPRuntimeEventProcessWrapped
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public StageSpecificServices specificServices;
		private StageRuntimeServices runtimeServices;
		private readonly string stageUri;

		private bool inboundThreading;
		private bool routeThreading;
		private bool timerThreading;
		private bool isLatchStatementInsertStream;
		private bool isUsingExternalClocking;
		private bool isPrioritized;
		private volatile UnmatchedListener unmatchedListener;
		private AtomicLong routedInternal;
		private AtomicLong routedExternal;
		private InternalEventRouter internalEventRouter;
		private IThreadLocal<EPEventServiceThreadLocalEntry> threadLocals;

		public EPStageEventServiceImpl(
			StageSpecificServices specificServices,
			StageRuntimeServices runtimeServices,
			string stageUri)
		{
			this.specificServices = specificServices;
			this.runtimeServices = runtimeServices;
			this.stageUri = stageUri;
			this.inboundThreading = specificServices.ThreadingService.IsInboundThreading;
			this.routeThreading = specificServices.ThreadingService.IsRouteThreading;
			this.timerThreading = specificServices.ThreadingService.IsTimerThreading;
			isLatchStatementInsertStream = runtimeServices.RuntimeSettingsService.ConfigurationRuntime.Threading.IsInsertIntoDispatchPreserveOrder;
			isUsingExternalClocking = true;
			isPrioritized = runtimeServices.RuntimeSettingsService.ConfigurationRuntime.Execution.IsPrioritized;
			routedInternal = new AtomicLong();
			routedExternal = new AtomicLong();

			InitThreadLocals();

			specificServices.ThreadingService.InitThreading(stageUri, specificServices);
		}

		public StageSpecificServices SpecificServices => specificServices;

		/// <summary>
		/// Sets the route for events to use
		/// </summary>
		/// <value>router</value>
		public InternalEventRouter InternalEventRouter {
			get => this.internalEventRouter;
			set => this.internalEventRouter = value;
		}

		public long RoutedInternal => routedInternal.Get();

		public long RoutedExternal => routedExternal.Get();

		public void SendEventAvro(
			object avroGenericDataDotRecord,
			string avroEventTypeName)
		{
			if (avroGenericDataDotRecord == null) {
				throw new ArgumentException("Invalid null event object");
			}

			if ((ExecutionPathDebugLog.IsDebugEnabled) && (log.IsDebugEnabled)) {
				log.Debug(".sendMap Processing event " + avroGenericDataDotRecord.ToString());
			}

			if (inboundThreading) {
				specificServices.ThreadingService.SubmitInbound(
					new InboundUnitSendAvro(
						avroGenericDataDotRecord,
						avroEventTypeName,
						this,
						specificServices));
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

			if ((ExecutionPathDebugLog.IsDebugEnabled) && (log.IsDebugEnabled)) {
				log.Debug(".sendEventJson Processing event " + json);
			}

			if (inboundThreading) {
				specificServices.ThreadingService.SubmitInbound(
					new InboundUnitSendJson(json, jsonEventTypeName, this, specificServices));
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
				log.Error(".sendEvent Null object supplied");
				return;
			}

			if ((ExecutionPathDebugLog.IsDebugEnabled) && (log.IsDebugEnabled)) {
				log.Debug(".sendEvent Processing event " + theEvent);
			}

			if (inboundThreading) {
				specificServices.ThreadingService.SubmitInbound(
					new InboundUnitSendEvent(theEvent, eventTypeName, this, specificServices));
			}
			else {
				var eventBean = runtimeServices.EventTypeResolvingBeanFactory.AdapterForBean(theEvent, eventTypeName);
				ProcessWrappedEvent(eventBean);
			}
		}

		public void AdvanceTime(long time)
		{
			if (InstrumentationHelper.ENABLED) {
				InstrumentationHelper.Get().QStimulantTime(specificServices.SchedulingService.Time, time, time, false, null, stageUri);
			}

			specificServices.SchedulingService.Time = time;

			specificServices.MetricReportingService.ProcessTimeEvent(time);

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

		public long? NextScheduledTime => specificServices.SchedulingServiceSPI.NearestTimeHandle;

		private void AdvanceTimeSpanInternal(
			long targetTime,
			long? optionalResolution)
		{
			var currentTime = specificServices.SchedulingService.Time;

			while (currentTime < targetTime) {

				if ((optionalResolution != null) && (optionalResolution > 0)) {
					currentTime += optionalResolution.Value;
				}
				else {
					long? nearest = specificServices.SchedulingServiceSPI.NearestTimeHandle;
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
						.QStimulantTime(specificServices.SchedulingService.Time, currentTime, targetTime, true, optionalResolution, stageUri);
				}

				specificServices.SchedulingService.Time = currentTime;

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
				log.Error(".sendEvent Null object supplied");
				return;
			}

			// Process event
			if (inboundThreading) {
				specificServices.ThreadingService.SubmitInbound(
					new InboundUnitSendDOM(node, eventTypeName, this, specificServices));
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

			if ((ExecutionPathDebugLog.IsDebugEnabled) && (log.IsDebugEnabled)) {
				log.Debug(".sendEventObjectArray Processing event " + propertyValues.RenderAny());
			}

			if (inboundThreading) {
				specificServices.ThreadingService.SubmitInbound(
					new InboundUnitSendObjectArray(propertyValues, eventTypeName, this, specificServices));
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

			if ((ExecutionPathDebugLog.IsDebugEnabled) && (log.IsDebugEnabled)) {
				log.Debug(".sendMap Processing event " + map);
			}

			if (inboundThreading) {
				specificServices.ThreadingService.SubmitInbound(
					new InboundUnitSendMap(map, mapEventTypeName, this, specificServices));
			}
			else {
				var eventBean = WrapEventMap(map, mapEventTypeName);
				ProcessWrappedEvent(eventBean);
			}
		}

		public void RouteEventBean(EventBean theEvent)
		{
			threadLocals.GetOrCreate().DualWorkQueue.BackQueue.AddLast(theEvent);
		}

		// Internal route of events via insert-into, holds a statement lock
		public void Route(
			EventBean theEvent,
			EPStatementHandle epStatementHandle,
			bool addToFront,
			int precedence)
		{
			if (InstrumentationHelper.ENABLED) {
				InstrumentationHelper.Get().QRouteBetweenStmt(theEvent, epStatementHandle, addToFront);
			}

			if (theEvent is NaturalEventBean) {
				theEvent = ((NaturalEventBean) theEvent).OptionalSynthetic;
			}

			routedInternal.IncrementAndGet();
			var threadWorkQueue = threadLocals.GetOrCreate().DualWorkQueue;

			if (isLatchStatementInsertStream) {
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
				InstrumentationHelper.Get().QStimulantEvent(eventBean, stageUri);
			}

			var tlEntry = threadLocals.GetOrCreate();
			if (internalEventRouter.HasPreprocessing) {
				eventBean = internalEventRouter.Preprocess(eventBean, tlEntry.ExprEvaluatorContext, InstrumentationHelper.Get());
				if (eventBean == null) {
					return;
				}
			}

			// Acquire main processing lock which locks out statement management
			if (InstrumentationHelper.ENABLED) {
				InstrumentationHelper.Get().QEvent(eventBean, stageUri, true);
			}

			using (specificServices.EventProcessingRWLock.AcquireReadLock()) {
				try {
					ProcessMatches(eventBean);
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
			var queues = threadLocals.GetOrCreate().DualWorkQueue;

			if (queues.FrontQueue.IsEmpty()) {
				var haveDispatched = runtimeServices.NamedWindowDispatchService.Dispatch();
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

				var haveDispatched = runtimeServices.NamedWindowDispatchService.Dispatch();
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

				var haveDispatched = runtimeServices.NamedWindowDispatchService.Dispatch();
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
				InstrumentationHelper.Get().QEvent(eventBean, stageUri, false);
			}

			using (specificServices.EventProcessingRWLock.AcquireReadLock()) {
				try {
					ProcessMatches(eventBean);
				}
				catch (Exception) {
					threadLocals.GetOrCreate().MatchesArrayThreadLocal.Clear();
					throw;
				}
				finally {
					insertIntoLatch.Done();
					if (InstrumentationHelper.ENABLED) {
						InstrumentationHelper.Get().AEvent();
					}
				}
			}

			Dispatch();
		}

		private void ProcessThreadWorkQueueLatchedSpin(InsertIntoLatchSpin insertIntoLatch)
		{
			// wait for the latch to complete
			var eventBean = insertIntoLatch.Await();

			if (InstrumentationHelper.ENABLED) {
				InstrumentationHelper.Get().QEvent(eventBean, stageUri, false);
			}

			using (specificServices.EventProcessingRWLock.AcquireReadLock()) {
				try {
					ProcessMatches(eventBean);
				}
				catch (Exception) {
					threadLocals.GetOrCreate().MatchesArrayThreadLocal.Clear();
					throw;
				}
				finally {
					insertIntoLatch.Done();
					if (InstrumentationHelper.ENABLED) {
						InstrumentationHelper.Get().AEvent();
					}
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
				InstrumentationHelper.Get().QEvent(eventBean, stageUri, false);
			}

			using (specificServices.EventProcessingRWLock.AcquireReadLock()) {
				try {
					ProcessMatches(eventBean);
				}
				catch (Exception) {
					threadLocals.GetOrCreate().MatchesArrayThreadLocal.Clear();
					throw;
				}
				finally {
					if (InstrumentationHelper.ENABLED) {
						InstrumentationHelper.Get().AEvent();
					}
				}
			}

			Dispatch();
		}

		protected void ProcessMatches(EventBean theEvent)
		{
			// get matching filters
			var tlEntry = threadLocals.GetOrCreate();
			var matches = tlEntry.MatchesArrayThreadLocal;
			var version = specificServices.FilterService.Evaluate(theEvent, matches, tlEntry.ExprEvaluatorContext);

			if (ThreadLogUtil.ENABLED_TRACE) {
				ThreadLogUtil.Trace("Found matches for underlying ", matches.Count, theEvent.Underlying);
			}

			if (matches.Count == 0) {
				if (unmatchedListener != null) {
					specificServices.EventProcessingRWLock.ReadLock.Release(); // Allow listener to create new statements
					try {
						unmatchedListener.Invoke(theEvent);
					}
					catch (Exception ex) {
						log.Error("Exception thrown by unmatched listener: " + ex.Message, ex);
					}
					finally {
						// acquire read lock for release by caller
						specificServices.EventProcessingRWLock.AcquireReadLock();
					}
				}

				return;
			}

			var stmtCallbacks = tlEntry.MatchesPerStmtThreadLocal;
			object[] matchArray = matches.Array;
			var entryCount = matches.Count;

			for (var i = 0; i < entryCount; i++) {
				var handleCallback = (EPStatementHandleCallbackFilter) matchArray[i];
				var handle = handleCallback.AgentInstanceHandle;

				// Self-joins require that the internal dispatch happens after all streams are evaluated.
				// Priority or preemptive settings also require special ordering.
				if (handle.IsCanSelfJoin || isPrioritized) {
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

				if (handle.StatementHandle.MetricsHandle.IsEnabled) {
					var metrics = PerformanceMetricsHelper.Call(
						() => ProcessStatementFilterSingle(handle, handleCallback, theEvent, version, 0));
					specificServices.MetricReportingService.AccountTime(handle.StatementHandle.MetricsHandle, metrics, 1);
				}
				else {
					if (routeThreading) {
						specificServices.ThreadingService.SubmitRoute(
							new RouteUnitSingleStaged(this, handleCallback, theEvent, version));
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
					var metrics = PerformanceMetricsHelper.Call(
						() => ProcessStatementFilterMultiple(handle, callbackList, theEvent, version, 0));

					var size = 1;
					if (callbackList is ICollection<FilterHandleCallback>) {
						size = ((ICollection<FilterHandleCallback>) callbackList).Count;
					}

					specificServices.MetricReportingService.AccountTime(handle.StatementHandle.MetricsHandle, metrics, size);
				}
				else {
					if (routeThreading) {
						specificServices.ThreadingService.SubmitRoute(
							new RouteUnitMultipleStaged(this, callbackList, theEvent, handle, version));
					}
					else {
						ProcessStatementFilterMultiple(handle, callbackList, theEvent, version, 0);
					}
				}

				if (isPrioritized && handle.IsPreemptive) {
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
				InstrumentationHelper.Get().QEventCP(theEvent, handle, specificServices.SchedulingService.Time);
			}

			handle.StatementAgentInstanceLock.AcquireWriteLock();
			try {
				if (handle.HasVariables) {
					runtimeServices.VariableManagementService.SetLocalVersion();
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
				runtimeServices.ExceptionHandlingService.HandleException(ex, handle, ExceptionHandlerExceptionType.PROCESS, theEvent);
			}
			finally {
				if (handle.HasTableAccess) {
					runtimeServices.TableExprEvaluatorContext.ReleaseAcquiredLocks();
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
				InstrumentationHelper.Get().QEventCP(theEvent, handle, specificServices.SchedulingService.Time);
			}

			handle.StatementAgentInstanceLock.AcquireWriteLock();
			try {
				if (handle.HasVariables) {
					runtimeServices.VariableManagementService.SetLocalVersion();
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
				runtimeServices.ExceptionHandlingService.HandleException(ex, handle, ExceptionHandlerExceptionType.PROCESS, theEvent);
			}
			finally {
				if (handle.HasTableAccess) {
					runtimeServices.TableExprEvaluatorContext.ReleaseAcquiredLocks();
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
			var version = specificServices.FilterService.Evaluate(
				theEvent,
				callbacksForStatement,
				faultingHandle.StatementId,
				threadLocals.GetOrCreate().ExprEvaluatorContext);

			if (callbacksForStatement.Count == 1) {
				var handleCallback = (EPStatementHandleCallbackFilter) callbacksForStatement.First;
				ProcessStatementFilterSingle(handleCallback.AgentInstanceHandle, handleCallback, theEvent, version, filterFaultCount + 1);
				return;
			}

			if (callbacksForStatement.IsEmpty()) {
				return;
			}

			IDictionary<EPStatementAgentInstanceHandle, object> stmtCallbacks;
			if (isPrioritized) {
				stmtCallbacks = new SortedDictionary<EPStatementAgentInstanceHandle, object>(
					EPStatementAgentInstanceHandleComparer.INSTANCE);
			}
			else {
				stmtCallbacks = new Dictionary<EPStatementAgentInstanceHandle, object>();
			}

			foreach (var filterHandle in callbacksForStatement) {
				var handleCallback = (EPStatementHandleCallbackFilter) filterHandle;
				var handle = handleCallback.AgentInstanceHandle;

				if (handle.IsCanSelfJoin || isPrioritized) {
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

				if (isPrioritized && handle.IsPreemptive) {
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
				runtimeServices.DispatchService.Dispatch();
			}
			catch (EPException) {
				throw;
			}
			catch (Exception ex) {
				throw new EPException(ex);
			}
		}

		public bool IsExternalClockingEnabled => isUsingExternalClocking;

		/// <summary>
		/// Destroy for destroying an runtime instance: sets references to null and clears thread-locals
		/// </summary>
		public void Destroy()
		{
			runtimeServices = null;
			specificServices = null;
			RemoveFromThreadLocals();
			threadLocals = null;
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
			get => this.unmatchedListener;
			set => this.unmatchedListener = value;
		}

		public long CurrentTime => specificServices.SchedulingService.Time;

		public string RuntimeURI => stageUri;

		private void RemoveFromThreadLocals()
		{
			threadLocals?.Remove();
		}

		private void InitThreadLocals()
		{
			RemoveFromThreadLocals();
			threadLocals = AllocateThreadLocals(
				isPrioritized,
				runtimeServices.RuntimeURI,
				runtimeServices.EventBeanService,
				runtimeServices.ExceptionHandlingService,
				specificServices.SchedulingService);
		}

		private void ProcessSchedule(long time)
		{
			if (InstrumentationHelper.ENABLED) {
				InstrumentationHelper.Get().QTime(time, stageUri);
			}

			var handles = threadLocals.GetOrCreate().ScheduleArrayThreadLocal;

			// Evaluation of schedules is protected by an optional scheduling service lock and then the runtime lock
			// We want to stay in this order for allowing the runtime lock as a second-order lock to the
			// services own lock, if it has one.
			using (specificServices.EventProcessingRWLock.AcquireReadLock()) {
				specificServices.SchedulingService.Evaluate(handles);
			}

			using (specificServices.EventProcessingRWLock.AcquireReadLock()) {
				try {
					ProcessScheduleHandles(handles);
				}
				catch (EPException) {
					handles.Clear();
					throw;
				}
				catch (Exception) {
					handles.Clear();
					throw;
				}
				finally {
					if (InstrumentationHelper.ENABLED) {
						InstrumentationHelper.Get().ATime();
					}
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
					var metrics = PerformanceMetricsHelper.Call(() => ProcessStatementScheduleSingle(handle, specificServices));
					specificServices.MetricReportingService.AccountTime(handle.AgentInstanceHandle.StatementHandle.MetricsHandle, metrics, 1);
				}
				else {
					if (timerThreading) {
						specificServices.ThreadingService.SubmitTimerWork(
							new TimerUnitSingleStaged(specificServices, this, handle));
					}
					else {
						ProcessStatementScheduleSingle(handle, specificServices);
					}
				}

				handles.Clear();
				return;
			}

			object[] matchArray = handles.Array;
			var entryCount = handles.Count;

			// sort multiple matches for the event into statements
			var stmtCallbacks = threadLocals.GetOrCreate().SchedulePerStmtThreadLocal;
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

			foreach (var entry in stmtCallbacks) {
				var handle = entry.Key;
				var callbackObject = entry.Value;

				if (handle.StatementHandle.MetricsHandle.IsEnabled) {
					var metrics = PerformanceMetricsHelper.Call(() => ProcessStatementScheduleMultiple(handle, callbackObject, specificServices));
					var numInput = (callbackObject is ICollection<FilterHandleCallback>) ? ((ICollection<FilterHandleCallback>) callbackObject).Count : 1;
					specificServices.MetricReportingService.AccountTime(handle.StatementHandle.MetricsHandle, metrics, numInput);
				}
				else {
					if (timerThreading) {
						specificServices.ThreadingService.SubmitTimerWork(
							new TimerUnitMultipleStaged(specificServices, this, handle, callbackObject));
					}
					else {
						ProcessStatementScheduleMultiple(handle, callbackObject, specificServices);
					}
				}

				if (isPrioritized && handle.IsPreemptive) {
					break;
				}
			}
		}

		private EventBean WrapEventMap(
			IDictionary<string, object> map,
			string eventTypeName)
		{
			return runtimeServices.EventTypeResolvingBeanFactory.AdapterForMap(map, eventTypeName);
		}

		private EventBean WrapEventObjectArray(
			object[] objectArray,
			string eventTypeName)
		{
			return runtimeServices.EventTypeResolvingBeanFactory.AdapterForObjectArray(objectArray, eventTypeName);
		}

		private EventBean WrapEventBeanXMLDOM(
			XmlNode node,
			string eventTypeName)
		{
			return runtimeServices.EventTypeResolvingBeanFactory.AdapterForXMLDOM(node, eventTypeName);
		}

		private EventBean WrapEventAvro(
			object avroGenericDataDotRecord,
			string eventTypeName)
		{
			return runtimeServices.EventTypeResolvingBeanFactory.AdapterForAvro(avroGenericDataDotRecord, eventTypeName);
		}

		private EventBean WrapEventJson(
			string json,
			string eventTypeName)
		{
			return runtimeServices.EventTypeResolvingBeanFactory.AdapterForJson(json, eventTypeName);
		}

		public void RouteEventMap(
			IDictionary<string, object> map,
			string eventTypeName)
		{
			if (map == null) {
				throw new ArgumentException("Invalid null event object");
			}

			var theEvent = runtimeServices.EventTypeResolvingBeanFactory.AdapterForMap(map, eventTypeName);
			RouteEventInternal(theEvent);
		}

		public void RouteEventBean(
			object @event,
			string eventTypeName)
		{
			if (@event == null) {
				throw new ArgumentException("Invalid null event object");
			}

			var theEvent = runtimeServices.EventTypeResolvingBeanFactory.AdapterForBean(@event, eventTypeName);
			RouteEventInternal(theEvent);
		}

		public void RouteEventObjectArray(
			object[] @event,
			string eventTypeName)
		{
			if (@event == null) {
				throw new ArgumentException("Invalid null event object");
			}

			var theEvent = runtimeServices.EventTypeResolvingBeanFactory.AdapterForObjectArray(@event, eventTypeName);
			RouteEventInternal(theEvent);
		}

		public void RouteEventXMLDOM(
			XmlNode @event,
			string eventTypeName)
		{
			if (@event == null) {
				throw new ArgumentException("Invalid null event object");
			}

			var theEvent = runtimeServices.EventTypeResolvingBeanFactory.AdapterForXMLDOM(@event, eventTypeName);
			RouteEventInternal(theEvent);
		}

		public void RouteEventAvro(
			object avroGenericDataDotRecord,
			string eventTypeName)
		{
			if (avroGenericDataDotRecord == null) {
				throw new ArgumentException("Invalid null event object");
			}

			var theEvent = runtimeServices.EventTypeResolvingBeanFactory.AdapterForAvro(avroGenericDataDotRecord, eventTypeName);
			RouteEventInternal(theEvent);
		}

		public void RouteEventJson(
			string json,
			string eventTypeName)
		{
			if (json == null) {
				throw new ArgumentException("Invalid null event object");
			}

			var theEvent = runtimeServices.EventTypeResolvingBeanFactory.AdapterForJson(json, eventTypeName);
			RouteEventInternal(theEvent);
		}

		public EventSender GetEventSender(string eventTypeName)
		{
			var eventType = runtimeServices.EventTypeRepositoryBus.GetTypeByName(eventTypeName);
			if (eventType == null) {
				throw new EventTypeException("Event type named '" + eventTypeName + "' could not be found");
			}

			// handle built-in types
			var threadingService = specificServices.ThreadingService;
			if (eventType is BeanEventType) {
				return new EventSenderBean(this, (BeanEventType) eventType, runtimeServices.EventBeanTypedEventFactory, threadingService);
			}

			if (eventType is MapEventType) {
				return new EventSenderMap(this, (MapEventType) eventType, runtimeServices.EventBeanTypedEventFactory, threadingService);
			}

			if (eventType is ObjectArrayEventType) {
				return new EventSenderObjectArray(this, (ObjectArrayEventType) eventType, runtimeServices.EventBeanTypedEventFactory, threadingService);
			}

			if (eventType is BaseXMLEventType) {
				return new EventSenderXMLDOM(this, (BaseXMLEventType) eventType, runtimeServices.EventBeanTypedEventFactory, threadingService);
			}

			if (eventType is AvroSchemaEventType) {
				return new EventSenderAvro(this, eventType, runtimeServices.EventBeanTypedEventFactory, threadingService);
			}

			if (eventType is JsonEventType) {
				return new EventSenderJsonImpl(this, (JsonEventType) eventType, runtimeServices.EventBeanTypedEventFactory, threadingService);
			}

			throw new EventTypeException("An event sender for event type named '" + eventTypeName + "' could not be created as the type is not known");
		}

		public IDictionary<DeploymentIdNamePair, long> StatementNearestSchedules =>
			GetStatementNearestSchedulesInternal(
				specificServices.SchedulingServiceSPI,
				runtimeServices.StatementLifecycleService);

		public void ClockInternal()
		{
			throw new UnsupportedOperationException("Not support for stage-provided time processing, only external time is supported");
		}

		public void ClockExternal()
		{
			// no action
		}

		public long NumEventsEvaluated => specificServices.FilterService.NumEventsEvaluated;

		public void ResetStats()
		{
			specificServices.FilterService.ResetStats();
			routedInternal.Set(0);
			routedExternal.Set(0);
		}

		public string URI => stageUri;

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
			var tlEntry = threadLocals.GetOrCreate();
			if (internalEventRouter.HasPreprocessing) {
				theEvent = internalEventRouter.Preprocess(theEvent, tlEntry.ExprEvaluatorContext, InstrumentationHelper.Get());
				if (theEvent == null) {
					return;
				}
			}

			tlEntry.DualWorkQueue.BackQueue.AddLast(theEvent);
		}
	}
} // end of namespace
