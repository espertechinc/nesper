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

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.metric;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.variable;
using com.espertech.esper.events.vaevent;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.schedule;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.named
{
	/// <summary>
	/// This service hold for each named window a dedicated processor and a lock to the named window.
	/// This lock is shrared between the named window and on-delete statements.
	/// </summary>
	public class NamedWindowServiceImpl : NamedWindowService
	{
	    private readonly SchedulingService _schedulingService;
	    private readonly IDictionary<string, NamedWindowProcessor> _processors;
	    private readonly IDictionary<string, NamedWindowLockPair> _windowStatementLocks;
	    private readonly VariableService _variableService;
	    private readonly TableService _tableService;
	    private readonly ISet<NamedWindowLifecycleObserver> _observers;
	    private readonly ExceptionHandlingService _exceptionHandlingService;
	    private readonly bool _isPrioritized;
	    private readonly IReaderWriterLock _eventProcessingRwLock;
	    private readonly bool _enableQueryPlanLog;
	    private readonly MetricReportingService _metricReportingService;

        private readonly IThreadLocal<List<NamedWindowConsumerDispatchUnit>> _threadLocal = 
            ThreadLocalManager.Create(() => new List<NamedWindowConsumerDispatchUnit>());

        private readonly IThreadLocal<Dictionary<EPStatementAgentInstanceHandle, Object>> _dispatchesPerStmtTL = 
            ThreadLocalManager.Create(() => new Dictionary<EPStatementAgentInstanceHandle, Object>());

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="schedulingService">The scheduling service.</param>
        /// <param name="variableService">is for variable access</param>
        /// <param name="tableService">The table service.</param>
        /// <param name="isPrioritized">if the engine is running with prioritized execution</param>
        /// <param name="eventProcessingRWLock">The event processing rw lock.</param>
        /// <param name="exceptionHandlingService">The exception handling service.</param>
        /// <param name="enableQueryPlanLog">if set to <c>true</c> [enable query plan log].</param>
        /// <param name="metricReportingService">The metric reporting service.</param>
	    public NamedWindowServiceImpl(
	        SchedulingService schedulingService,
	        VariableService variableService,
	        TableService tableService,
	        bool isPrioritized,
	        IReaderWriterLock eventProcessingRWLock,
	        ExceptionHandlingService exceptionHandlingService,
	        bool enableQueryPlanLog,
	        MetricReportingService metricReportingService)
	    {
	        _schedulingService = schedulingService;
	        _processors = new Dictionary<string, NamedWindowProcessor>();
	        _windowStatementLocks = new Dictionary<string, NamedWindowLockPair>();
	        _variableService = variableService;
	        _tableService = tableService;
	        _observers = new HashSet<NamedWindowLifecycleObserver>();
	        _isPrioritized = isPrioritized;
	        _eventProcessingRwLock = eventProcessingRWLock;
	        _exceptionHandlingService = exceptionHandlingService;
	        _enableQueryPlanLog = enableQueryPlanLog;
	        _metricReportingService = metricReportingService;
	    }

	    public void Dispose()
	    {
	        _processors.Clear();
	        _threadLocal.Dispose();
	        _dispatchesPerStmtTL.Dispose();
	    }

	    public string[] NamedWindows
	    {
	        get { return _processors.Keys.ToArray(); }
	    }

	    public IReaderWriterLock GetNamedWindowLock(string windowName)
	    {
	        var pair = _windowStatementLocks.Get(windowName);
	        if (pair == null) {
	            return null;
	        }
	        return pair.Lock;
	    }

	    public void AddNamedWindowLock(string windowName, IReaderWriterLock statementResourceLock, string statementName)
	    {
	        _windowStatementLocks.Put(windowName, new NamedWindowLockPair(statementName, statementResourceLock));
	    }

	    public void RemoveNamedWindowLock(string statementName)
        {
	        foreach (var entry in _windowStatementLocks)
            {
	            if (entry.Value.StatementName == statementName)
                {
	                _windowStatementLocks.Remove(entry.Key);
	                return;
	            }
	        }
	    }

	    public bool IsNamedWindow(string name)
	    {
	        return _processors.ContainsKey(name);
	    }

	    public NamedWindowProcessor GetProcessor(string name)
	    {
	        return _processors.Get(name);
	    }

	    public IndexMultiKey[] GetNamedWindowIndexes(string windowName)
        {
	        var processor = _processors.Get(windowName);
	        if (processor == null)
	        {
	            return null;
	        }
	        return processor.GetProcessorInstance(null).IndexDescriptors;
	    }

	    public NamedWindowProcessor AddProcessor(
	        string name,
	        string contextName,
	        bool singleInstanceContext,
	        EventType eventType,
	        StatementResultService statementResultService,
	        ValueAddEventProcessor revisionProcessor,
	        string eplExpression,
	        string statementName,
	        bool isPrioritized,
	        bool isEnableSubqueryIndexShare,
	        bool isBatchingDataWindow,
	        bool isVirtualDataWindow,
	        StatementMetricHandle statementMetricHandle,
	        ICollection<string> optionalUniqueKeyProps,
	        string eventTypeAsName)
	    {
	        if (_processors.ContainsKey(name))
	        {
	            throw new ViewProcessingException("A named window by name '" + name + "' has already been created");
	        }

	        var processor = new NamedWindowProcessor(
	            name, this, contextName, singleInstanceContext, eventType, statementResultService, revisionProcessor,
	            eplExpression, statementName, isPrioritized, isEnableSubqueryIndexShare, _enableQueryPlanLog,
	            _metricReportingService, isBatchingDataWindow, isVirtualDataWindow, statementMetricHandle,
	            optionalUniqueKeyProps, eventTypeAsName);
	        _processors.Put(name, processor);

	        if (!_observers.IsEmpty())
	        {
	            var theEvent = new NamedWindowLifecycleEvent(name, processor, NamedWindowLifecycleEvent.LifecycleEventType.CREATE);
	            foreach (var observer in _observers)
	            {
	                observer.Observe(theEvent);
	            }
	        }

	        return processor;
	    }

	    public void RemoveProcessor(string name)
	    {
	        var processor = _processors.Get(name);
	        if (processor != null)
	        {
	            processor.Dispose();
	            _processors.Remove(name);

	            if (!_observers.IsEmpty())
	            {
	                var theEvent = new NamedWindowLifecycleEvent(name, processor, NamedWindowLifecycleEvent.LifecycleEventType.DESTROY);
	                foreach (var observer in _observers)
	                {
	                    observer.Observe(theEvent);
	                }
	            }
	        }
	    }

	    public void AddDispatch(NamedWindowDeltaData delta, IDictionary<EPStatementAgentInstanceHandle, IList<NamedWindowConsumerView>> consumers)
	    {
	        if (!consumers.IsEmpty()) {
	            var unit = new NamedWindowConsumerDispatchUnit(delta, consumers);
	            _threadLocal.GetOrCreate().Add(unit);
	        }
	    }

	    public bool Dispatch()
	    {
	        var dispatches = _threadLocal.GetOrCreate();
	        if (dispatches.Count == 0)
	        {
	            return false;
	        }

	        while (dispatches.Count != 0)
            {
	            // Acquire main processing lock which locks out statement management

                using (Instrument.With(
	                i => i.QNamedWindowDispatch(_exceptionHandlingService.EngineURI),
	                i => i.ANamedWindowDispatch()))
	            {
	                using (_eventProcessingRwLock.ReadLock.Acquire())
	                {
	                    try
	                    {
	                        var units = dispatches.ToArray();
	                        dispatches.Clear();
	                        ProcessDispatches(units);
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
	            }
	        }

	        return true;
	    }

	    private void ProcessDispatches(NamedWindowConsumerDispatchUnit[] dispatches)
        {
	        if (dispatches.Length == 1)
	        {
	            var unit = dispatches[0];
	            var newData = unit.DeltaData.NewData;
	            var oldData = unit.DeltaData.OldData;

	            if (MetricReportingPath.IsMetricsEnabled)
	            {
	                foreach (var entry in unit.DispatchTo)
	                {
	                    var handle = entry.Key;
	                    if (handle.StatementHandle.MetricsHandle.IsEnabled)
	                    {
	                        handle.StatementHandle.MetricsHandle.Call(
	                            _metricReportingService.PerformanceCollector,
	                            () =>
	                            {
	                                ProcessHandle(handle, entry.Value, newData, oldData);
	                            });
	                    }
	                    else
	                    {
	                        ProcessHandle(handle, entry.Value, newData, oldData);
	                    }

	                    if ((_isPrioritized) && (handle.IsPreemptive))
	                    {
	                        break;
	                    }
	                }
	            }
	            else
	            {
	                foreach (var entry in unit.DispatchTo)
	                {
	                    var handle = entry.Key;
	                    ProcessHandle(handle, entry.Value, newData, oldData);

	                    if ((_isPrioritized) && (handle.IsPreemptive))
	                    {
	                        break;
	                    }
	                }
	            }

	            return;
	        }

	        // Multiple different-result dispatches to same or different statements are needed in two situations:
	        // a) an event comes in, triggers two insert-into statements inserting into the same named window and the window produces 2 results
	        // b) a time batch is grouped in the named window, and a timer fires for both groups at the same time producing more then one result
	        // c) two on-merge/update/delete statements fire for the same arriving event each updating the named window

	        // Most likely all dispatches go to different statements since most statements are not joins of
	        // named windows that produce results at the same time. Therefore sort by statement handle.
	        IDictionary<EPStatementAgentInstanceHandle, object> dispatchesPerStmt = _dispatchesPerStmtTL.GetOrCreate();
	        foreach (var unit in dispatches)
	        {
	            foreach (var entry in unit.DispatchTo)
	            {
	                var handle = entry.Key;
	                var perStmtObj = dispatchesPerStmt.Get(handle);
	                if (perStmtObj == null)
	                {
	                    dispatchesPerStmt.Put(handle, unit);
	                }
	                else if (perStmtObj is IList<NamedWindowConsumerDispatchUnit>)
	                {
	                    var list = (IList<NamedWindowConsumerDispatchUnit>) perStmtObj;
	                    list.Add(unit);
	                }
	                else    // convert from object to list
	                {
	                    var unitObj = (NamedWindowConsumerDispatchUnit) perStmtObj;
	                    IList<NamedWindowConsumerDispatchUnit> list = new List<NamedWindowConsumerDispatchUnit>();
	                    list.Add(unitObj);
	                    list.Add(unit);
	                    dispatchesPerStmt.Put(handle, list);
	                }
	            }
	        }

	        // Dispatch - with or without metrics reporting
	        if (MetricReportingPath.IsMetricsEnabled)
	        {
	            foreach (var entry in dispatchesPerStmt)
	            {
	                var handle = entry.Key;
	                var perStmtObj = entry.Value;

	                // dispatch of a single result to the statement
	                if (perStmtObj is NamedWindowConsumerDispatchUnit)
	                {
	                    var unit = (NamedWindowConsumerDispatchUnit) perStmtObj;
	                    var newData = unit.DeltaData.NewData;
	                    var oldData = unit.DeltaData.OldData;

	                    if (handle.StatementHandle.MetricsHandle.IsEnabled) {
                            handle.StatementHandle.MetricsHandle.Call(
                                _metricReportingService.PerformanceCollector,
                                () => ProcessHandle(handle, unit.DispatchTo.Get(handle), newData, oldData));
	                    }
	                    else {
	                        var entries = unit.DispatchTo;
	                    	var items = entries.Get(handle);
	                    	if (items != null) {
								ProcessHandle(handle, items, newData, oldData);
							}
	                    }

	                    if ((_isPrioritized) && (handle.IsPreemptive))
	                    {
	                        break;
	                    }

	                    continue;
	                }

	                // dispatch of multiple results to a the same statement, need to aggregate per consumer view
	                var deltaPerConsumer = GetDeltaPerConsumer(perStmtObj, handle);
	                if (handle.StatementHandle.MetricsHandle.IsEnabled) {
                        handle.StatementHandle.MetricsHandle.Call(
                                                    _metricReportingService.PerformanceCollector,
                                                    () => ProcessHandleMultiple(handle, deltaPerConsumer));
	                }
	                else {
	                    ProcessHandleMultiple(handle, deltaPerConsumer);
	                }

	                if ((_isPrioritized) && (handle.IsPreemptive))
	                {
	                    break;
	                }
	            }
	        }
	        else {

	            foreach (var entry in dispatchesPerStmt)
	            {
	                var handle = entry.Key;
	                var perStmtObj = entry.Value;

	                // dispatch of a single result to the statement
	                if (perStmtObj is NamedWindowConsumerDispatchUnit)
	                {
	                    var unit = (NamedWindowConsumerDispatchUnit) perStmtObj;
	                    var newData = unit.DeltaData.NewData;
	                    var oldData = unit.DeltaData.OldData;

	                    ProcessHandle(handle, unit.DispatchTo.Get(handle), newData, oldData);

	                    if ((_isPrioritized) && (handle.IsPreemptive))
	                    {
	                        break;
	                    }

	                    continue;
	                }

	                // dispatch of multiple results to a the same statement, need to aggregate per consumer view
	                var deltaPerConsumer = GetDeltaPerConsumer(perStmtObj, handle);
	                ProcessHandleMultiple(handle, deltaPerConsumer);

	                if ((_isPrioritized) && (handle.IsPreemptive))
	                {
	                    break;
	                }
	            }
	        }

	        dispatchesPerStmt.Clear();
	    }

	    private void ProcessHandleMultiple(EPStatementAgentInstanceHandle handle, IDictionary<NamedWindowConsumerView, NamedWindowDeltaData> deltaPerConsumer)
        {
	        using (Instrument.With(
	            i => i.QNamedWindowCPMulti(_exceptionHandlingService.EngineURI, deltaPerConsumer, handle, _schedulingService.Time),
	            i => i.ANamedWindowCPMulti()))
	        {
	            using (handle.StatementAgentInstanceLock.WriteLock.Acquire())
	            {
	                try
	                {
	                    if (handle.HasVariables)
	                    {
	                        _variableService.SetLocalVersion();
	                    }
	                    foreach (var entryDelta in deltaPerConsumer)
	                    {
	                        var newData = entryDelta.Value.NewData;
	                        var oldData = entryDelta.Value.OldData;
	                        entryDelta.Key.Update(newData, oldData);
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
	                    _exceptionHandlingService.HandleException(ex, handle);
	                }
	                finally
	                {
	                    if (handle.HasTableAccess)
	                    {
	                        _tableService.TableExprEvaluatorContext.ReleaseAcquiredLocks();
	                    }
	                }
	            }
	        }
	    }

	    private void ProcessHandle(EPStatementAgentInstanceHandle handle, IList<NamedWindowConsumerView> value, EventBean[] newData, EventBean[] oldData)
        {
	        using (Instrument.With(
	            i =>
	                i.QNamedWindowCPSingle(
	                    _exceptionHandlingService.EngineURI, value, newData, oldData, handle, _schedulingService.Time),
	            i => i.ANamedWindowCPSingle()))
	        {
	            using (handle.StatementAgentInstanceLock.WriteLock.Acquire())
	            {
	                try
	                {
	                    if (handle.HasVariables)
	                    {
	                        _variableService.SetLocalVersion();
	                    }

	                    foreach (var consumerView in value)
	                    {
	                        consumerView.Update(newData, oldData);
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
	                    _exceptionHandlingService.HandleException(ex, handle);
	                }
	                finally
	                {
	                    if (handle.HasTableAccess)
	                    {
	                        _tableService.TableExprEvaluatorContext.ReleaseAcquiredLocks();
	                    }
	                }
	            }
	        }
        }

	    public void AddObserver(NamedWindowLifecycleObserver observer)
	    {
	        _observers.Add(observer);
	    }

	    public void RemoveObserver(NamedWindowLifecycleObserver observer)
	    {
	        _observers.Remove(observer);
	    }

	    public LinkedHashMap<NamedWindowConsumerView, NamedWindowDeltaData> GetDeltaPerConsumer(object perStmtObj, EPStatementAgentInstanceHandle handle) {
	        var list = (IList<NamedWindowConsumerDispatchUnit>) perStmtObj;
	        var deltaPerConsumer = new LinkedHashMap<NamedWindowConsumerView, NamedWindowDeltaData>();
	        foreach (var unit in list)   // for each unit
	        {
	            foreach (var consumerView in unit.DispatchTo.Get(handle))   // each consumer
	            {
	                var deltaForConsumer = deltaPerConsumer.Get(consumerView);
	                if (deltaForConsumer == null)
	                {
	                    deltaPerConsumer.Put(consumerView, unit.DeltaData);
	                }
	                else
	                {
	                    var aggregated = new NamedWindowDeltaData(deltaForConsumer, unit.DeltaData);
	                    deltaPerConsumer.Put(consumerView, aggregated);
	                }
	            }
	        }
	        return deltaPerConsumer;
	    }

	    private class NamedWindowLockPair
        {
	        public NamedWindowLockPair(string statementName, IReaderWriterLock @lock)
            {
	            StatementName = statementName;
	            Lock = @lock;
	        }

	        public string StatementName { get; private set; }

	        public IReaderWriterLock Lock { get; private set; }
        }
	}
} // end of namespace
