///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.context.factory;
using com.espertech.esper.core.context.mgr;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.named;
using com.espertech.esper.epl.script;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.view;
using com.espertech.esper.events;
using com.espertech.esper.filter;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.core.context.util
{
    public class StatementAgentInstanceUtil
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void HandleFilterFault(EventBean theEvent, long version, EPServicesContext servicesContext, IDictionary<int, ContextControllerTreeAgentInstanceList> agentInstanceListMap)
        {
            foreach (var agentInstanceEntry in agentInstanceListMap)
            {
                if (agentInstanceEntry.Value.FilterVersionAfterAllocation > version)
                {
                    StatementAgentInstanceUtil.EvaluateEventForStatement(
                        servicesContext, theEvent, null, agentInstanceEntry.Value.AgentInstances);
                }
            }
        }

        public static void StopAgentInstances(IList<AgentInstance> agentInstances, IDictionary<string, object> terminationProperties, EPServicesContext servicesContext, bool isStatementStop, bool leaveLocksAcquired)
        {
            if (agentInstances == null)
            {
                return;
            }
            foreach (var instance in agentInstances)
            {
                StopAgentInstanceRemoveResources(instance, terminationProperties, servicesContext, isStatementStop, leaveLocksAcquired);
            }
        }

        public static void StopAgentInstanceRemoveResources(AgentInstance agentInstance, IDictionary<string, object> terminationProperties, EPServicesContext servicesContext, bool isStatementStop, bool leaveLocksAcquired)
        {
            if (terminationProperties != null)
            {
                var contextProperties = (MappedEventBean)agentInstance.AgentInstanceContext.ContextProperties;
                contextProperties.Properties.PutAll(terminationProperties);
            }
            StatementAgentInstanceUtil.Stop(agentInstance.StopCallback, agentInstance.AgentInstanceContext, agentInstance.FinalView, servicesContext, isStatementStop, leaveLocksAcquired, true);
        }

        public static void StopSafe(ICollection<StopCallback> terminationCallbacks, StopCallback[] stopCallbacks, StatementContext statementContext)
        {
            var terminationArr = terminationCallbacks.ToArray();
            StopSafe(terminationArr, statementContext);
            StopSafe(stopCallbacks, statementContext);
        }

        public static void StopSafe(StopCallback[] stopMethods, StatementContext statementContext)
        {
            foreach (var stopCallback in stopMethods)
            {
                StopSafe(stopCallback, statementContext);
            }
        }

        public static void StopSafe(StopCallback stopMethod, StatementContext statementContext)
        {
            try
            {
                stopMethod.Stop();
            }
            catch (Exception e)
            {
                statementContext.ExceptionHandlingService.HandleException(e, statementContext.StatementName, statementContext.Expression, ExceptionHandlerExceptionType.STOP, null);
            }
        }

        public static void Stop(StopCallback stopCallback, AgentInstanceContext agentInstanceContext, Viewable finalView, EPServicesContext servicesContext, bool isStatementStop, bool leaveLocksAcquired, bool removedStatementResources)
        {
            using (Instrument.With(
                i => i.QContextPartitionDestroy(agentInstanceContext),
                i => i.AContextPartitionDestroy()))
            {
                // obtain statement lock
                var iLock = agentInstanceContext.EpStatementAgentInstanceHandle.StatementAgentInstanceLock;
                using (var iLockEnd = iLock.WriteLock.Acquire(!leaveLocksAcquired))
                {
                    try
                    {
                        if (finalView is OutputProcessViewTerminable && !isStatementStop)
                        {
                            var terminable = (OutputProcessViewTerminable)finalView;
                            terminable.Terminated();
                        }

                        StopSafe(stopCallback, agentInstanceContext.StatementContext);

                        // release resource
                        agentInstanceContext.StatementContext.StatementAgentInstanceRegistry.Deassign(
                            agentInstanceContext.AgentInstanceId);

                        // cause any remaining schedules, that may concide with the caller's schedule, to be ignored
                        agentInstanceContext.EpStatementAgentInstanceHandle.IsDestroyed = true;

                        // cause any filters, that may concide with the caller's filters, to be ignored
                        agentInstanceContext.EpStatementAgentInstanceHandle.StatementFilterVersion.StmtFilterVersion = Int64.MaxValue;

                        if (removedStatementResources &&
                            agentInstanceContext.StatementContext.StatementExtensionServicesContext != null &&
                            agentInstanceContext.StatementContext.StatementExtensionServicesContext.StmtResources != null)
                        {
                            agentInstanceContext.StatementContext.StatementExtensionServicesContext.StmtResources
                                .DeallocatePartitioned(agentInstanceContext.AgentInstanceId);
                        }
                    }
                    finally
                    {
                        if (!leaveLocksAcquired)
                        {
                            if (agentInstanceContext.StatementContext.EpStatementHandle.HasTableAccess)
                            {
                                agentInstanceContext.TableExprEvaluatorContext.ReleaseAcquiredLocks();
                            }
                        }
                    }
                }
            }
        }

        public static StatementAgentInstanceFactoryResult Start(EPServicesContext servicesContext, ContextControllerStatementBase statement, bool isSingleInstanceContext, int agentInstanceId, MappedEventBean agentInstanceProperties, AgentInstanceFilterProxy agentInstanceFilterProxy, bool isRecoveringResilient)
        {
            var statementContext = statement.StatementContext;

            // for on-trigger statements against named windows we must use the named window lock
            OnTriggerDesc optOnTriggerDesc = statement.StatementSpec.OnTriggerDesc;
            String namedWindowName = null;
            if ((optOnTriggerDesc != null) && (optOnTriggerDesc is OnTriggerWindowDesc))
            {
                String windowName = ((OnTriggerWindowDesc)optOnTriggerDesc).WindowName;
                if (servicesContext.TableService.GetTableMetadata(windowName) == null)
                {
                    namedWindowName = windowName;
                }
            }

            // determine lock to use
            IReaderWriterLock agentInstanceLock;
            if (namedWindowName != null)
            {
                NamedWindowProcessor processor = servicesContext.NamedWindowMgmtService.GetProcessor(namedWindowName);
                NamedWindowProcessorInstance instance = processor.GetProcessorInstance(agentInstanceId);
                agentInstanceLock = instance.RootViewInstance.AgentInstanceContext.EpStatementAgentInstanceHandle.StatementAgentInstanceLock;
            }
            else
            {
                if (isSingleInstanceContext)
                {
                    agentInstanceLock = statementContext.DefaultAgentInstanceLock;
                }
                else
                {
                    agentInstanceLock = servicesContext.StatementLockFactory.GetStatementLock(
                        statementContext.StatementName, statementContext.Annotations, statementContext.IsStatelessSelect);
                }
            }

            // share the filter version between agent instance handle (callbacks) and agent instance context
            var filterVersion = new StatementAgentInstanceFilterVersion();

            // create handle that comtains lock for use in scheduling and filter callbacks
            var agentInstanceHandle = new EPStatementAgentInstanceHandle(statementContext.EpStatementHandle, agentInstanceLock, agentInstanceId, filterVersion, statementContext.FilterFaultHandlerFactory);

            // create agent instance context
            AgentInstanceScriptContext agentInstanceScriptContext = null;
            if (statementContext.DefaultAgentInstanceScriptContext != null)
            {
                agentInstanceScriptContext = AgentInstanceScriptContext.From(statementContext.EventAdapterService);
            }
            var agentInstanceContext = new AgentInstanceContext(statementContext, agentInstanceHandle, agentInstanceId, agentInstanceFilterProxy, agentInstanceProperties, agentInstanceScriptContext);
            var statementAgentInstanceLock = agentInstanceContext.EpStatementAgentInstanceHandle.StatementAgentInstanceLock;

            using (Instrument.With(
                i => i.QContextPartitionAllocate(agentInstanceContext),
                i => i.AContextPartitionAllocate()))
            {
                using (statementAgentInstanceLock.AcquireWriteLock())
                {
                    try
                    {
                        // start
                        var startResult = statement.Factory.NewContext(agentInstanceContext, isRecoveringResilient);

                        // hook up with listeners+subscribers
                        startResult.FinalView.AddView(statement.MergeView); // hook output to merge view

                        // assign agents for expression-node based strategies
                        var aiExprSvc = statementContext.StatementAgentInstanceRegistry.AgentInstanceExprService;
                        var aiAggregationSvc =
                            statementContext.StatementAgentInstanceRegistry.AgentInstanceAggregationService;

                        // allocate aggregation service
                        if (startResult.OptionalAggegationService != null)
                        {
                            aiAggregationSvc.AssignService(agentInstanceId, startResult.OptionalAggegationService);
                        }

                        // allocate subquery
                        foreach (var item in startResult.SubselectStrategies)
                        {
                            var node = item.Key;
                            var strategyHolder = item.Value;

                            aiExprSvc.GetSubselectService(node).AssignService(agentInstanceId, strategyHolder.Stategy);
                            aiExprSvc.GetSubselectAggregationService(node)
                                .AssignService(agentInstanceId, strategyHolder.SubselectAggregationService);

                            // allocate prior within subquery
                            foreach (var priorEntry in strategyHolder.PriorStrategies)
                            {
                                aiExprSvc.GetPriorServices(priorEntry.Key).AssignService(agentInstanceId, priorEntry.Value);
                            }

                            // allocate previous within subquery
                            foreach (var prevEntry in strategyHolder.PreviousNodeStrategies)
                            {
                                aiExprSvc.GetPreviousServices(prevEntry.Key)
                                    .AssignService(agentInstanceId, prevEntry.Value);
                            }
                        }

                        // allocate prior-expressions
                        foreach (var item in startResult.PriorNodeStrategies)
                        {
                            aiExprSvc.GetPriorServices(item.Key).AssignService(agentInstanceId, item.Value);
                        }

                        // allocate previous-expressions
                        foreach (var item in startResult.PreviousNodeStrategies)
                        {
                            aiExprSvc.GetPreviousServices(item.Key).AssignService(agentInstanceId, item.Value);
                        }

                        // allocate match-recognize previous expressions
                        var regexExprPreviousEvalStrategy = startResult.RegexExprPreviousEvalStrategy;
                        aiExprSvc.GetMatchRecognizePrevious().AssignService(agentInstanceId, regexExprPreviousEvalStrategy);

                        // allocate table-access-expressions
                        foreach (var item in startResult.TableAccessEvalStrategies)
                        {
                            aiExprSvc.GetTableAccessServices(item.Key).AssignService(agentInstanceId, item.Value);
                        }

                        // execute preloads, if any
                        foreach (var preload in startResult.PreloadList)
                        {
                            preload.ExecutePreload(agentInstanceContext);
                        }

                        if (statementContext.StatementExtensionServicesContext != null &&
                            statementContext.StatementExtensionServicesContext.StmtResources != null)
                        {
                            var holder = statementContext.StatementExtensionServicesContext.ExtractStatementResourceHolder(startResult);
                            statementContext.StatementExtensionServicesContext.StmtResources.SetPartitioned(agentInstanceId, holder);
                        }

                        // instantiate
                        return startResult;
                    }
                    finally
                    {
                        if (agentInstanceContext.StatementContext.EpStatementHandle.HasTableAccess)
                        {
                            agentInstanceContext.TableExprEvaluatorContext.ReleaseAcquiredLocks();
                        }
                    }
                }
            }
        }

        public static void EvaluateEventForStatement(EPServicesContext servicesContext, EventBean theEvent, IDictionary<string, object> optionalTriggeringPattern, IList<AgentInstance> agentInstances)
        {
            if (theEvent != null)
            {
                EvaluateEventForStatementInternal(servicesContext, theEvent, agentInstances);
            }
            if (optionalTriggeringPattern != null)
            {
                // evaluation order definition is up to the originator of the triggering pattern
                foreach (var entry in optionalTriggeringPattern)
                {
                    if (entry.Value is EventBean)
                    {
                        EvaluateEventForStatementInternal(servicesContext, (EventBean)entry.Value, agentInstances);
                    }
                    else if (entry.Value is EventBean[])
                    {
                        var eventsArray = (EventBean[])entry.Value;
                        for (var ii = 0; ii < eventsArray.Length; ii++)
                        {
                            EvaluateEventForStatementInternal(servicesContext, eventsArray[ii], agentInstances);
                        }
                    }
                }
            }
        }

        private static void EvaluateEventForStatementInternal(EPServicesContext servicesContext, EventBean theEvent, IList<AgentInstance> agentInstances)
        {
            // context was created - reevaluate for the given event
            var callbacks = new ArrayDeque<FilterHandle>(2);
            servicesContext.FilterService.Evaluate(theEvent, callbacks);   // evaluates for ALL statements
            if (callbacks.IsEmpty())
            {
                return;
            }

            // there is a single callback and a single context, if they match we are done
            if (agentInstances.Count == 1 && callbacks.Count == 1)
            {
                var agentInstance = agentInstances[0];
                if (agentInstance.AgentInstanceContext.StatementId == callbacks.First.StatementId)
                {
                    Process(agentInstance, servicesContext, callbacks, theEvent);
                }
                return;
            }

            // use the right sorted/unsorted Map keyed by AgentInstance to sort
            var isPrioritized = servicesContext.ConfigSnapshot.EngineDefaults.Execution.IsPrioritized;
            IDictionary<AgentInstance, object> stmtCallbacks;
            if (!isPrioritized)
            {
                stmtCallbacks = new Dictionary<AgentInstance, object>();
            }
            else
            {
                stmtCallbacks = new SortedDictionary<AgentInstance, object>(AgentInstanceComparator.INSTANCE);
            }

            // process all callbacks
            foreach (var filterHandle in callbacks)
            {
                // determine if this filter entry applies to any of the affected agent instances
                var statementId = filterHandle.StatementId;
                AgentInstance agentInstanceFound = null;
                foreach (var agentInstance in agentInstances)
                {
                    if (agentInstance.AgentInstanceContext.StatementId == statementId)
                    {
                        agentInstanceFound = agentInstance;
                        break;
                    }
                }
                if (agentInstanceFound == null)
                {   // when the callback is for some other stmt
                    continue;
                }

                var handleCallback = (EPStatementHandleCallback)filterHandle;
                var handle = handleCallback.AgentInstanceHandle;

                // Self-joins require that the internal dispatch happens after all streams are evaluated.
                // Priority or preemptive settings also require special ordering.
                if (handle.CanSelfJoin || isPrioritized)
                {
                    var stmtCallback = stmtCallbacks.Get(agentInstanceFound);
                    if (stmtCallback == null)
                    {
                        stmtCallbacks.Put(agentInstanceFound, handleCallback);
                    }
                    else if (stmtCallback is ICollection<FilterHandle>)
                    {
                        var collection = (ICollection<FilterHandle>) stmtCallback;
                        if (!collection.Contains(handleCallback)) // De-duplicate for Filter OR expression paths
                        { 
                            collection.Add(handleCallback);
                        }
                    }
                    else
                    {
                        var deque = new ArrayDeque<FilterHandle>(4);
                        deque.Add((EPStatementHandleCallback)stmtCallback);
                        if (stmtCallback != handleCallback) // De-duplicate for Filter OR expression paths
                        {
                            deque.Add(handleCallback);
                        }
                        stmtCallbacks.Put(agentInstanceFound, deque);
                    }
                    continue;
                }

                // no need to be sorted, process
                Process(agentInstanceFound, servicesContext, Collections.SingletonList<FilterHandle>(handleCallback), theEvent);
            }

            if (stmtCallbacks.IsEmpty())
            {
                return;
            }

            // Process self-join or sorted prioritized callbacks
            foreach (var entry in stmtCallbacks)
            {
                var agentInstance = entry.Key;
                var callbackList = entry.Value;
                if (callbackList is ICollection<FilterHandle>)
                {
                    Process(agentInstance, servicesContext, (ICollection<FilterHandle>)callbackList, theEvent);
                }
                else
                {
                    Process(agentInstance, servicesContext, Collections.SingletonList<FilterHandle>((FilterHandle)callbackList), theEvent);
                }
                if (agentInstance.AgentInstanceContext.EpStatementAgentInstanceHandle.IsPreemptive)
                {
                    return;
                }
            }
        }

        public static bool EvaluateFilterForStatement(EPServicesContext servicesContext, EventBean theEvent, AgentInstanceContext agentInstanceContext, FilterHandle filterHandle)
        {
            // context was created - reevaluate for the given event
            var callbacks = new ArrayDeque<FilterHandle>();
            servicesContext.FilterService.Evaluate(theEvent, callbacks, agentInstanceContext.StatementContext.StatementId);

            try
            {
                servicesContext.VariableService.SetLocalVersion();

                // sub-selects always go first
                if (callbacks.Any(handle => handle == filterHandle))
                {
                    return true;
                }

                agentInstanceContext.EpStatementAgentInstanceHandle.InternalDispatch();

            }
            catch (Exception ex)
            {
                servicesContext.ExceptionHandlingService.HandleException(
                    ex, agentInstanceContext.EpStatementAgentInstanceHandle, ExceptionHandlerExceptionType.PROCESS,
                    theEvent);
            }

            return false;
        }

        public static StopCallback GetStopCallback(IList<StopCallback> stopCallbacks, AgentInstanceContext agentInstanceContext)
        {
            var stopCallbackArr = stopCallbacks.ToArray();
            return new ProxyStopCallback(() => StopSafe(
                agentInstanceContext.TerminationCallbackRO, stopCallbackArr,
                agentInstanceContext.StatementContext));
        }

        private static void Process(
            AgentInstance agentInstance,
            EPServicesContext servicesContext,
            IEnumerable<FilterHandle> callbacks,
            EventBean theEvent)
        {
            var agentInstanceContext = agentInstance.AgentInstanceContext;
            using (agentInstanceContext.AgentInstanceLock.AcquireWriteLock())
            {
                try
                {
                    servicesContext.VariableService.SetLocalVersion();

                    // sub-selects always go first
                    foreach (var handle in callbacks)
                    {
                        var callback = (EPStatementHandleCallback)handle;
                        if (callback.AgentInstanceHandle != agentInstanceContext.EpStatementAgentInstanceHandle)
                        {
                            continue;
                        }
                        callback.FilterCallback.MatchFound(theEvent, null);
                    }

                    agentInstanceContext.EpStatementAgentInstanceHandle.InternalDispatch();
                }
               catch (Exception ex)
                {
                    servicesContext.ExceptionHandlingService.HandleException(
                        ex, agentInstanceContext.EpStatementAgentInstanceHandle, ExceptionHandlerExceptionType.PROCESS,
                        theEvent);
                }
                finally
                {
                    if (agentInstanceContext.StatementContext.EpStatementHandle.HasTableAccess)
                    {
                        agentInstanceContext.TableExprEvaluatorContext.ReleaseAcquiredLocks();
                    }
                }
            }
        }
    }
} // end of namespace
