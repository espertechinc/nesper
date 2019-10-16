///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.exception;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.namedwindow.consume;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.metrics.stmtmetrics;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.diagnostics;
using com.espertech.esper.compat.threading;
using com.espertech.esper.compat.threading.locks;
using com.espertech.esper.compat.threading.threadlocal;
using com.espertech.esper.runtime.@internal.metrics.instrumentation;
using com.espertech.esper.runtime.@internal.metrics.stmtmetrics;

namespace com.espertech.esper.runtime.@internal.namedwindow
{
    /// <summary>
    ///     This service hold for each named window a dedicated processor and a lock to the named window.
    ///     This lock is shared between the named window and on-delete statements.
    /// </summary>
    public class NamedWindowDispatchServiceImpl : NamedWindowDispatchService
    {
        private readonly IReaderWriterLock eventProcessingRWLock;
        private readonly ExceptionHandlingService exceptionHandlingService;
        private readonly bool isPrioritized;
        private readonly MetricReportingService metricReportingService;
        private readonly SchedulingService schedulingService;
        private readonly TableManagementService tableManagementService;
        private readonly VariableManagementService variableService;

        private readonly IThreadLocal<DispatchesTL> threadLocal = 
            new SlimThreadLocal<DispatchesTL>(() => new DispatchesTL());

        public NamedWindowDispatchServiceImpl(
            SchedulingService schedulingService,
            VariableManagementService variableService,
            TableManagementService tableManagementService,
            bool isPrioritized,
            IReaderWriterLock eventProcessingRWLock,
            ExceptionHandlingService exceptionHandlingService,
            MetricReportingService metricReportingService)
        {
            this.schedulingService = schedulingService;
            this.variableService = variableService;
            this.tableManagementService = tableManagementService;
            this.isPrioritized = isPrioritized;
            this.eventProcessingRWLock = eventProcessingRWLock;
            this.exceptionHandlingService = exceptionHandlingService;
            this.metricReportingService = metricReportingService;
        }

        public void Destroy()
        {
            threadLocal.Remove();
        }

        public void AddDispatch(
            NamedWindowConsumerLatchFactory latchFactory,
            NamedWindowDeltaData delta,
            IDictionary<EPStatementAgentInstanceHandle, IList<NamedWindowConsumerView>> consumers)
        {
            var latch = latchFactory.NewLatch(delta, consumers);
            threadLocal.GetOrCreate().Dispatches.Add(latch);
        }

        public bool Dispatch()
        {
            var dispatchesTL = threadLocal.GetOrCreate();
            if (dispatchesTL.Dispatches.IsEmpty()) {
                return false;
            }

            while (!dispatchesTL.Dispatches.IsEmpty()) {
                // Acquire main processing lock which locks out statement management
                if (InstrumentationHelper.ENABLED) {
                    InstrumentationHelper.Get().QNamedWindowDispatch(exceptionHandlingService.RuntimeURI);
                }

                using (eventProcessingRWLock.AcquireReadLock()) {
                    try {
                        // since dispatches can cause dispatches, copy the contents
                        dispatchesTL.Current.AddAll(dispatchesTL.Dispatches);
                        dispatchesTL.Dispatches.Clear();
                        ProcessDispatches(dispatchesTL.Current, dispatchesTL.Work, dispatchesTL.DispatchesPerStmt);
                    }
                    catch (EPException) {
                        throw;
                    }
                    catch (Exception ex) {
                        throw new EPException(ex);
                    }
                    finally {
                        dispatchesTL.Current.Clear();
                        if (InstrumentationHelper.ENABLED) {
                            InstrumentationHelper.Get().ANamedWindowDispatch();
                        }
                    }
                }
            }

            return true;
        }

        private void ProcessDispatches(
            ArrayDeque<NamedWindowConsumerLatch> dispatches,
            ArrayDeque<NamedWindowConsumerLatch> work,
            IDictionary<EPStatementAgentInstanceHandle, object> dispatchesPerStmt)
        {
            if (dispatches.Count == 1) {
                var latch = dispatches.First;
                try {
                    latch.Await();
                    var newData = latch.DeltaData.NewData;
                    var oldData = latch.DeltaData.OldData;

                    if (metricReportingService.IsMetricsReportingEnabled) {
                        foreach (var entry in latch.DispatchTo) {
                            var handle = entry.Key;
                            if (handle.StatementHandle.MetricsHandle.IsEnabled) {
                                var performanceMetric = PerformanceMetricsHelper.Call(
                                    () => ProcessHandle(handle, entry.Value, newData, oldData), 1);
                                metricReportingService.AccountTime(
                                    handle.StatementHandle.MetricsHandle,
                                    performanceMetric, performanceMetric.NumInput);
                            }
                            else {
                                ProcessHandle(handle, entry.Value, newData, oldData);
                            }

                            if (isPrioritized && handle.IsPreemptive) {
                                break;
                            }
                        }
                    }
                    else {
                        foreach (var entry in latch.DispatchTo) {
                            var handle = entry.Key;
                            ProcessHandle(handle, entry.Value, newData, oldData);

                            if (isPrioritized && handle.IsPreemptive) {
                                break;
                            }
                        }
                    }
                }
                finally {
                    latch.Done();
                }

                return;
            }

            // Multiple different-result dispatches to same or different statements are needed in two situations:
            // a) an event comes in, triggers two insert-into statements inserting into the same named window and the window produces 2 results
            // b) a time batch is grouped in the named window, and a timer fires for both groups at the same time producing more then one result
            // c) two on-merge/update/delete statements fire for the same arriving event each updating the named window
            // Most likely all dispatches go to different statements since most statements are not joins of
            // named windows that produce results at the same time. Therefore sort by statement handle.
            // We need to process in N-element chains to preserve dispatches that are next to each other for the same thread.
            while (!dispatches.IsEmpty()) {
                // the first latch always gets awaited
                var first = dispatches.RemoveFirst();
                first.Await();
                work.Add(first);

                // determine which further latches are in this chain and add these, skipping await for any latches in the chain

                dispatches.RemoveWhere(
                    (next, continuation) => {
                        var result = next.Earlier == null || work.Contains(next.Earlier);
                        continuation.Value = !result;
                        return result;
                    },
                    work.Add);

#if false
                var enumerator = dispatches.GetEnumerator();
                while (enumerator.MoveNext()) {
                    var next = enumerator.Current;
                    var earlier = next.Earlier;
                    if (earlier == null || work.Contains(earlier)) {
                        work.Add(next);
                        enumerator.Remove();
                    }
                    else {
                        break;
                    }
                }
#endif

                ProcessDispatches(work, dispatchesPerStmt);
            }
        }

        private void ProcessDispatches(
            ArrayDeque<NamedWindowConsumerLatch> dispatches,
            IDictionary<EPStatementAgentInstanceHandle, object> dispatchesPerStmt)
        {
            try {
                foreach (var latch in dispatches) {
                    foreach (var entry in latch.DispatchTo) {
                        var handle = entry.Key;
                        var perStmtObj = dispatchesPerStmt.Get(handle);
                        if (perStmtObj == null) {
                            dispatchesPerStmt.Put(handle, latch);
                        }
                        else if (perStmtObj is IList<NamedWindowConsumerLatch> windowConsumerLatches) {
                            windowConsumerLatches.Add(latch);
                        }
                        else {
                            // convert from object to list
                            var unitObj = (NamedWindowConsumerLatch) perStmtObj;
                            IList<NamedWindowConsumerLatch> list = new List<NamedWindowConsumerLatch>();
                            list.Add(unitObj);
                            list.Add(latch);
                            dispatchesPerStmt.Put(handle, list);
                        }
                    }
                }

                // Dispatch - with or without metrics reporting
                if (metricReportingService.IsMetricsReportingEnabled) {
                    foreach (var entry in dispatchesPerStmt) {
                        var handle = entry.Key;
                        var perStmtObj = entry.Value;

                        // dispatch of a single result to the statement
                        if (perStmtObj is NamedWindowConsumerLatch) {
                            var unit = (NamedWindowConsumerLatch) perStmtObj;
                            var newData = unit.DeltaData.NewData;
                            var oldData = unit.DeltaData.OldData;

                            if (handle.StatementHandle.MetricsHandle.IsEnabled) {
                                var performanceMetric = PerformanceMetricsHelper.Call(
                                    () => ProcessHandle(handle, unit.DispatchTo.Get(handle), newData, oldData));

                                metricReportingService.AccountTime(
                                    handle.StatementHandle.MetricsHandle, 
                                    performanceMetric, performanceMetric.NumInput);
                            }
                            else {
                                var entries = unit.DispatchTo;
                                var items = entries.Get(handle);
                                if (items != null) {
                                    ProcessHandle(handle, items, newData, oldData);
                                }
                            }

                            if (isPrioritized && handle.IsPreemptive) {
                                break;
                            }

                            continue;
                        }

                        // dispatch of multiple results to a the same statement, need to aggregate per consumer view
                        var deltaPerConsumer = GetDeltaPerConsumer(perStmtObj, handle);
                        if (handle.StatementHandle.MetricsHandle.IsEnabled) {
                            var performanceMetric = PerformanceMetricsHelper.Call(
                                () => ProcessHandleMultiple(handle, deltaPerConsumer));
                            metricReportingService.AccountTime(
                                handle.StatementHandle.MetricsHandle, 
                                performanceMetric, performanceMetric.NumInput);
                        }
                        else {
                            ProcessHandleMultiple(handle, deltaPerConsumer);
                        }

                        if (isPrioritized && handle.IsPreemptive) {
                            break;
                        }
                    }
                }
                else {
                    foreach (var entry in dispatchesPerStmt) {
                        var handle = entry.Key;
                        var perStmtObj = entry.Value;

                        // dispatch of a single result to the statement
                        if (perStmtObj is NamedWindowConsumerLatch) {
                            var unit = (NamedWindowConsumerLatch) perStmtObj;
                            var newData = unit.DeltaData.NewData;
                            var oldData = unit.DeltaData.OldData;

                            ProcessHandle(handle, unit.DispatchTo.Get(handle), newData, oldData);

                            if (isPrioritized && handle.IsPreemptive) {
                                break;
                            }

                            continue;
                        }

                        // dispatch of multiple results to a the same statement, need to aggregate per consumer view
                        var deltaPerConsumer = GetDeltaPerConsumer(perStmtObj, handle);
                        ProcessHandleMultiple(handle, deltaPerConsumer);

                        if (isPrioritized && handle.IsPreemptive) {
                            break;
                        }
                    }
                }
            }
            finally {
                foreach (var latch in dispatches) {
                    latch.Done();
                }

                dispatchesPerStmt.Clear();
                dispatches.Clear();
            }
        }

        private void ProcessHandleMultiple(
            EPStatementAgentInstanceHandle handle,
            IDictionary<NamedWindowConsumerView, NamedWindowDeltaData> deltaPerConsumer)
        {
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().QNamedWindowCPMulti(
                    exceptionHandlingService.RuntimeURI, deltaPerConsumer, handle, schedulingService.Time);
            }

            handle.StatementAgentInstanceLock.AcquireWriteLock();
            try {
                if (handle.HasVariables) {
                    variableService.SetLocalVersion();
                }

                foreach (KeyValuePair<NamedWindowConsumerView, NamedWindowDeltaData> entryDelta in deltaPerConsumer) {
                    var newData = entryDelta.Value.NewData;
                    var oldData = entryDelta.Value.OldData;
                    entryDelta.Key.Update(newData, oldData);
                }

                // internal join processing, if applicable
                handle.InternalDispatch();
            }
            catch (Exception ex) {
                exceptionHandlingService.HandleException(ex, handle, ExceptionHandlerExceptionType.PROCESS, null);
            }
            finally {
                if (handle.HasTableAccess) {
                    tableManagementService.TableExprEvaluatorContext.ReleaseAcquiredLocks();
                }

                handle.StatementAgentInstanceLock.ReleaseWriteLock();
                if (InstrumentationHelper.ENABLED) {
                    InstrumentationHelper.Get().ANamedWindowCPMulti();
                }
            }
        }

        private void ProcessHandle(
            EPStatementAgentInstanceHandle handle,
            IList<NamedWindowConsumerView> value,
            EventBean[] newData,
            EventBean[] oldData)
        {
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().QNamedWindowCPSingle(
                    exceptionHandlingService.RuntimeURI, value.Count, newData, oldData, handle, schedulingService.Time);
            }

            handle.StatementAgentInstanceLock.AcquireWriteLock();
            try {
                if (handle.HasVariables) {
                    variableService.SetLocalVersion();
                }

                foreach (var consumerView in value) {
                    consumerView.Update(newData, oldData);
                }

                // internal join processing, if applicable
                handle.InternalDispatch();
            }
            catch (Exception ex) {
                exceptionHandlingService.HandleException(ex, handle, ExceptionHandlerExceptionType.PROCESS, null);
            }
            finally {
                if (handle.HasTableAccess) {
                    tableManagementService.TableExprEvaluatorContext.ReleaseAcquiredLocks();
                }

                handle.StatementAgentInstanceLock.ReleaseWriteLock();
                if (InstrumentationHelper.ENABLED) {
                    InstrumentationHelper.Get().ANamedWindowCPSingle();
                }
            }
        }

        public LinkedHashMap<NamedWindowConsumerView, NamedWindowDeltaData> GetDeltaPerConsumer(
            object perStmtObj,
            EPStatementAgentInstanceHandle handle)
        {
            var list = (IList<NamedWindowConsumerLatch>) perStmtObj;
            var deltaPerConsumer = new LinkedHashMap<NamedWindowConsumerView, NamedWindowDeltaData>();
            foreach (var unit in list) {
                // for each unit
                foreach (var consumerView in unit.DispatchTo.Get(handle)) {
                    // each consumer
                    var deltaForConsumer = deltaPerConsumer.Get(consumerView);
                    if (deltaForConsumer == null) {
                        deltaPerConsumer.Put(consumerView, unit.DeltaData);
                    }
                    else {
                        var aggregated = new NamedWindowDeltaData(deltaForConsumer, unit.DeltaData);
                        deltaPerConsumer.Put(consumerView, aggregated);
                    }
                }
            }

            return deltaPerConsumer;
        }

        private class DispatchesTL
        {
            public ArrayDeque<NamedWindowConsumerLatch> Dispatches { get; } = new ArrayDeque<NamedWindowConsumerLatch>();

            public ArrayDeque<NamedWindowConsumerLatch> Current { get; } = new ArrayDeque<NamedWindowConsumerLatch>();

            public ArrayDeque<NamedWindowConsumerLatch> Work { get; } = new ArrayDeque<NamedWindowConsumerLatch>();

            public IDictionary<EPStatementAgentInstanceHandle, object> DispatchesPerStmt { get; } =
                new Dictionary<EPStatementAgentInstanceHandle, object>();
        }
    }
} // end of namespace