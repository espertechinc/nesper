///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.exception;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.airegistry;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.epl.output.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.common.@internal.metrics.audit;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.common.@internal.statement.resource;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.common.@internal.context.util
{
    public class AgentInstanceUtil
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void EvaluateEventForStatement(
            EventBean theEvent,
            IDictionary<string, object> optionalTriggeringPattern,
            IList<AgentInstance> agentInstances,
            AgentInstanceContext agentInstanceContextCreate)
        {
            var evaluator =
                agentInstanceContextCreate.ContextServiceFactory.ContextStatementEventEvaluator;
            if (theEvent != null) {
                evaluator.EvaluateEventForStatement(theEvent, agentInstances, agentInstanceContextCreate);
            }

            if (optionalTriggeringPattern != null) {
                // evaluation order definition is up to the originator of the triggering pattern
                foreach (var entry in optionalTriggeringPattern) {
                    if (entry.Value is EventBean) {
                        evaluator.EvaluateEventForStatement(
                            (EventBean) entry.Value,
                            agentInstances,
                            agentInstanceContextCreate);
                    }
                    else if (entry.Value is EventBean[]) {
                        var eventsArray = (EventBean[]) entry.Value;
                        foreach (var eventElement in eventsArray) {
                            evaluator.EvaluateEventForStatement(
                                eventElement,
                                agentInstances,
                                agentInstanceContextCreate);
                        }
                    }
                }
            }
        }

        public static void ContextPartitionTerminate(
            int agentInstanceId,
            ContextControllerStatementDesc statementDesc,
            ContextController[] contextControllers,
            IDictionary<string, object> terminationProperties,
            bool leaveLocksAcquired,
            IList<AgentInstance> agentInstancesLocksHeld)
        {
            var statementContext = statementDesc.Lightweight.StatementContext;
            var holder =
                statementContext.StatementCPCacheService.MakeOrGetEntryCanNull(agentInstanceId, statementContext);

            if (terminationProperties != null) {
                var mappedEventBean = (MappedEventBean) holder.AgentInstanceContext.ContextProperties;
                if (contextControllers.Length == 1) {
                    mappedEventBean.Properties.PutAll(terminationProperties);
                }
                else {
                    var lastController = contextControllers[contextControllers.Length - 1];
                    var lastContextName = lastController.Factory.FactoryEnv.ContextName;
                    var inner = (IDictionary<string, object>) mappedEventBean.Properties.Get(lastContextName);
                    if (inner != null) {
                        inner.PutAll(terminationProperties);
                    }
                }
            }

            // we are not removing statement resources from memory as they may still be used for the same event
            Stop(
                holder.AgentInstanceStopCallback,
                holder.AgentInstanceContext,
                holder.FinalView,
                false,
                leaveLocksAcquired);
            if (leaveLocksAcquired) {
                agentInstancesLocksHeld.Add(
                    new AgentInstance(holder.AgentInstanceStopCallback, holder.AgentInstanceContext, holder.FinalView));
            }
        }

        public static void Stop(
            AgentInstanceMgmtCallback stopCallback,
            AgentInstanceContext agentInstanceContext,
            Viewable finalView,
            bool isStatementStop,
            bool leaveLocksAcquired)
        {
            agentInstanceContext.InstrumentationProvider.QContextPartitionDestroy(agentInstanceContext);

            // obtain statement lock
            var @lock = agentInstanceContext.EpStatementAgentInstanceHandle.StatementAgentInstanceLock;
            @lock.AcquireWriteLock();
            try {
                if (finalView is OutputProcessViewTerminable terminable && !isStatementStop) {
                    terminable.Terminated();
                }

                StopSafe(stopCallback, agentInstanceContext);

                // release resource
                agentInstanceContext.StatementContext.StatementAIResourceRegistry?.Deassign(
                    agentInstanceContext.AgentInstanceId);

                // cause any remaining schedules, that may concide with the caller's schedule, to be ignored
                agentInstanceContext.EpStatementAgentInstanceHandle.IsDestroyed = true;

                // cause any filters, that may concide with the caller's filters, to be ignored
                agentInstanceContext.EpStatementAgentInstanceHandle.StatementFilterVersion.StmtFilterVersion =
                    Int64.MaxValue;

                if (agentInstanceContext.AgentInstanceId != -1) {
                    agentInstanceContext.AuditProvider.ContextPartition(false, agentInstanceContext);
                }
            }
            finally {
                if (!leaveLocksAcquired) {
                    if (agentInstanceContext.StatementContext.EpStatementHandle.HasTableAccess) {
                        agentInstanceContext.TableExprEvaluatorContext.ReleaseAcquiredLocks();
                    }

                    @lock.ReleaseWriteLock();
                }

                agentInstanceContext.InstrumentationProvider.AContextPartitionDestroy();
            }
        }

        public static void StopSafe(
            AgentInstanceMgmtCallback stopMethod,
            AgentInstanceContext agentInstanceContext)
        {
            var stopServices = new AgentInstanceStopServices(agentInstanceContext);

            var additionalTerminations = agentInstanceContext.TerminationCallbackRO;
            foreach (var stop in additionalTerminations) {
                try {
                    stop.Stop(stopServices);
                }
                catch (EPException) {
                    throw;
                }
                catch (Exception e) {
                    HandleStopException(e, agentInstanceContext);
                }
            }

            try {
                stopMethod.Stop(stopServices);
            }
            catch (EPException) {
                throw;
            }
            catch (Exception e) {
                HandleStopException(e, agentInstanceContext);
            }
        }

        public static AgentInstanceMgmtCallback FinalizeSafeStopCallbacks(
            IList<AgentInstanceMgmtCallback> stopCallbacks)
        {
            var stopCallbackArray = stopCallbacks.ToArray();
            return new AgentInstanceFinalizedMgmtCallback(stopCallbackArray);
        }

        private static void HandleStopException(
            Exception e,
            AgentInstanceContext agentInstanceContext)
        {
            agentInstanceContext.ExceptionHandlingService.HandleException(
                e,
                agentInstanceContext.EpStatementAgentInstanceHandle,
                ExceptionHandlerExceptionType.UNDEPLOY,
                null);
        }

        public static AgentInstance StartStatement(
            StatementContextRuntimeServices services,
            int assignedContextId,
            ContextControllerStatementDesc statementDesc,
            MappedEventBean contextBean,
            AgentInstanceFilterProxy proxy)
        {
            var result = AgentInstanceUtil.Start(
                services,
                statementDesc,
                assignedContextId,
                contextBean,
                proxy,
                false);
            return new AgentInstance(result.StopCallback, result.AgentInstanceContext, result.FinalView);
        }

        public static StatementAgentInstanceFactoryResult Start(
            StatementContextRuntimeServices services,
            ContextControllerStatementDesc statement,
            int agentInstanceId,
            MappedEventBean contextProperties,
            AgentInstanceFilterProxy agentInstanceFilterProxy,
            bool isRecoveringResilient)
        {
            var statementContext = statement.Lightweight.StatementContext;

            // create handle that comtains lock for use in scheduling and filter callbacks
            var @lock =
                statementContext.StatementAIFactoryProvider.Factory.ObtainAgentInstanceLock(
                    statementContext,
                    agentInstanceId);
            var agentInstanceHandle =
                new EPStatementAgentInstanceHandle(statementContext.EpStatementHandle, agentInstanceId, @lock);

            var auditProvider = statementContext.StatementInformationals.AuditProvider;
            var instrumentationProvider =
                statementContext.StatementInformationals.InstrumentationProvider;
            var agentInstanceContext = new AgentInstanceContext(
                statementContext,
                agentInstanceHandle,
                agentInstanceFilterProxy,
                contextProperties,
                auditProvider,
                instrumentationProvider);
            if (agentInstanceId != -1) {
                agentInstanceContext.AuditProvider.ContextPartition(true, agentInstanceContext);
            }

            var statementAgentInstanceLock =
                agentInstanceContext.EpStatementAgentInstanceHandle.StatementAgentInstanceLock;

            agentInstanceContext.InstrumentationProvider.QContextPartitionAllocate(agentInstanceContext);

            using (statementAgentInstanceLock.AcquireWriteLock()) {
                try {
                    // start
                    var startResult =
                        statement.Lightweight.StatementProvider.StatementAIFactoryProvider.Factory.NewContext(
                            agentInstanceContext,
                            isRecoveringResilient);

                    // hook up with listeners+subscribers
                    startResult.FinalView.Child = statement.ContextMergeView; // hook output to merge view

                    // assign agents for expression-node based strategies
                    var aiResourceRegistry = statementContext.StatementAIResourceRegistry;
                    AIRegistryUtil.AssignFutures(
                        aiResourceRegistry,
                        agentInstanceId,
                        startResult.OptionalAggegationService,
                        startResult.PriorStrategies,
                        startResult.PreviousGetterStrategies,
                        startResult.SubselectStrategies,
                        startResult.TableAccessStrategies,
                        startResult.RowRecogPreviousStrategy);

                    // execute preloads, if any
                    if (startResult.PreloadList != null) {
                        foreach (var preload in startResult.PreloadList) {
                            preload.ExecutePreload();
                        }
                    }
                    
                    // handle any pattern-match-event that was produced during startup, relevant for "timer:interval(0)" in conjunction with contexts
                    startResult.PostContextMergeRunnable?.Invoke();

                    var holder =
                        services.StatementResourceHolderBuilder.Build(agentInstanceContext, startResult);
                    statementContext.StatementCPCacheService.StatementResourceService.SetPartitioned(
                        agentInstanceId,
                        holder);

                    // instantiated
                    return startResult;
                }
                finally {
                    if (agentInstanceContext.StatementContext.EpStatementHandle.HasTableAccess) {
                        agentInstanceContext.TableExprEvaluatorContext.ReleaseAcquiredLocks();
                    }

                    agentInstanceContext.InstrumentationProvider.AContextPartitionAllocate();
                }
            }
        }

        public static bool EvaluateFilterForStatement(
            EventBean theEvent,
            AgentInstanceContext agentInstanceContext,
            FilterHandle filterHandle)
        {
            // context was created - reevaluate for the given event
            var callbacks = new ArrayDeque<FilterHandle>();
            agentInstanceContext.FilterService.Evaluate(
                theEvent,
                callbacks,
                agentInstanceContext.StatementContext.StatementId,
                agentInstanceContext);

            try {
                agentInstanceContext.VariableManagementService.SetLocalVersion();

                // sub-selects always go first
                foreach (var handle in callbacks) {
                    if (handle.Equals(filterHandle)) {
                        return true;
                    }
                }

                agentInstanceContext.EpStatementAgentInstanceHandle.InternalDispatch();
            }
            catch (EPException) {
                throw;
            }
            catch (Exception ex) {
                agentInstanceContext.ExceptionHandlingService.HandleException(
                    ex,
                    agentInstanceContext.EpStatementAgentInstanceHandle,
                    ExceptionHandlerExceptionType.PROCESS,
                    theEvent);
            }

            return false;
        }

        public static IReaderWriterLock NewLock(StatementContext statementContext)
        {
            return statementContext.StatementAgentInstanceLockFactory.GetStatementLock(
                statementContext.StatementName,
                statementContext.Annotations,
                statementContext.IsStatelessSelect,
                statementContext.StatementType);
        }

        internal class AgentInstanceFinalizedMgmtCallback : AgentInstanceMgmtCallback
        {
            private readonly AgentInstanceMgmtCallback[] _mgmtCallbackArray;

            internal AgentInstanceFinalizedMgmtCallback(AgentInstanceMgmtCallback[] mgmtCallbackArray)
            {
                this._mgmtCallbackArray = mgmtCallbackArray;
            }

            public void Stop(AgentInstanceStopServices services)
            {
                foreach (AgentInstanceMgmtCallback callback in _mgmtCallbackArray) {
                    try {
                        callback.Stop(services);
                    }
                    catch (EPException) {
                        throw;
                    }
                    catch (Exception e) {
                        HandleStopException(e, services.AgentInstanceContext);
                    }
                }
            }

            public void Transfer(AgentInstanceTransferServices services)
            {
                foreach (AgentInstanceMgmtCallback callback in _mgmtCallbackArray) {
                    try {
                        callback.Transfer(services);
                    }
                    catch (EPException) {
                        throw;
                    }
                    catch (Exception e) {
                        services
                            .AgentInstanceContext
                            .ExceptionHandlingService
                            .HandleException(
                                e,
                                services.AgentInstanceContext.EpStatementAgentInstanceHandle,
                                ExceptionHandlerExceptionType.STAGE,
                                null);
                    }
                }
            }
        }
    }
} // end of namespace