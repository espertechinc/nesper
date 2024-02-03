///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.hook.exception;
using com.espertech.esper.common.client.hook.expr;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.enummethod.cache;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading.threadlocal;
using com.espertech.esper.container;
using com.espertech.esper.runtime.@internal.metrics.instrumentation;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
	public class EPEventServiceHelper
	{
		/// <summary>
		/// Processing multiple schedule matches for a statement.
		/// </summary>
		/// <param name="handle">statement handle</param>
		/// <param name="callbackObject">object containing matches</param>
		/// <param name="services">runtime services</param>
		public static void ProcessStatementScheduleMultiple(
			EPStatementAgentInstanceHandle handle,
			object callbackObject,
			EPServicesEvaluation services)
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

					if (callbackObject is ArrayDeque<ScheduleHandleCallback>) {
						ArrayDeque<ScheduleHandleCallback> callbackList = (ArrayDeque<ScheduleHandleCallback>) callbackObject;
						foreach (ScheduleHandleCallback callback in callbackList) {
							callback.ScheduledTrigger();
						}
					}
					else {
						ScheduleHandleCallback callback = (ScheduleHandleCallback) callbackObject;
						callback.ScheduledTrigger();
					}

					// internal join processing, if applicable
					handle.InternalDispatch();
				}
			}
			catch (Exception ex) {
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
		/// Processing single schedule matche for a statement.
		/// </summary>
		/// <param name="handle">statement handle</param>
		/// <param name="services">runtime services</param>
		public static void ProcessStatementScheduleSingle(
			EPStatementHandleCallbackSchedule handle,
			EPServicesEvaluation services)
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
			catch (Exception ex) {
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

		public static IThreadLocal<EPEventServiceThreadLocalEntry> AllocateThreadLocals(
			IContainer container,
			bool isPrioritized,
			string runtimeURI,
			Configuration configuration,
			EventBeanService eventBeanService,
			ExceptionHandlingService exceptionHandlingService,
			SchedulingService schedulingService,
			TimeZoneInfo timeZone,
			TimeAbacus timeAbacus,
			VariableManagementService variableManagementService)
		{
			var expressionResultCacheService = new ExpressionResultCacheService(
				configuration.Runtime.Execution.DeclaredExprValueCacheSize,
				container.ThreadLocalManager());
				
			//return new SystemThreadLocal<EPEventServiceThreadLocalEntry>(
			return new FastThreadLocal<EPEventServiceThreadLocalEntry>(
				() => {
					ArrayBackedCollection<FilterHandle> filterHandles = new ArrayBackedCollection<FilterHandle>(100);
					ArrayBackedCollection<ScheduleHandle> scheduleHandles = new ArrayBackedCollection<ScheduleHandle>(100);

					IDictionary<EPStatementAgentInstanceHandle, object> matchesPerStmt;
					IDictionary<EPStatementAgentInstanceHandle, object> schedulesPerStmt;
					if (isPrioritized) {
						matchesPerStmt = new SortedDictionary<EPStatementAgentInstanceHandle, object>(EPStatementAgentInstanceHandleComparer.INSTANCE);
						schedulesPerStmt = new SortedDictionary<EPStatementAgentInstanceHandle, object>(EPStatementAgentInstanceHandleComparer.INSTANCE);
					}
					else {
						matchesPerStmt = new Dictionary<EPStatementAgentInstanceHandle, object>();
						schedulesPerStmt = new Dictionary<EPStatementAgentInstanceHandle, object>();
					}

					ExprEvaluatorContext runtimeFilterAndDispatchTimeContext = new EPEventServiceExprEvaluatorContext(
						runtimeURI,
						eventBeanService,
						exceptionHandlingService,
						expressionResultCacheService,
						schedulingService,
						timeZone,
						timeAbacus,
						variableManagementService);
					
					WorkQueue workQueue;
					bool eventPrecedence = configuration.Runtime.Execution.IsPrecedenceEnabled;
					bool insertIntoLatching = configuration.Runtime.Threading.IsInsertIntoDispatchPreserveOrder;
					if (!eventPrecedence) {
						if (insertIntoLatching) {
							// the default work queue may or may not latch depending on statement stateless ascpect and lock-type configuration
							workQueue = new WorkQueueNoPrecedenceMayLatch();
						} else {
							workQueue = new WorkQueueNoPrecedenceNoLatch();
						}
					} else {
						if (!insertIntoLatching) {
							workQueue = new WorkQueueWPrecedenceNoLatch();
						} else {
							workQueue = new WorkQueueWPrecedenceMayLatch();
						}
					}
					
					return new EPEventServiceThreadLocalEntry(
						workQueue,
						filterHandles,
						scheduleHandles,
						matchesPerStmt,
						schedulesPerStmt,
						runtimeFilterAndDispatchTimeContext);
				});
		}
	}
} // end of namespace
