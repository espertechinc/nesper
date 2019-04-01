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
            ContextStatementEventEvaluator evaluator =
                agentInstanceContextCreate.ContextServiceFactory.ContextStatementEventEvaluator;
            if (theEvent != null) {
                evaluator.EvaluateEventForStatement(theEvent, agentInstances, agentInstanceContextCreate);
            }

            if (optionalTriggeringPattern != null) {
                // evaluation order definition is up to the originator of the triggering pattern
                foreach (KeyValuePair<string, object> entry in optionalTriggeringPattern) {
                    if (entry.Value is EventBean) {
                        evaluator.EvaluateEventForStatement(
                            (EventBean) entry.Value, agentInstances, agentInstanceContextCreate);
                    }
                    else if (entry.Value is EventBean[]) {
                        EventBean[] eventsArray = (EventBean[]) entry.Value;
                        foreach (EventBean eventElement in eventsArray) {
                            evaluator.EvaluateEventForStatement(
                                eventElement, agentInstances, agentInstanceContextCreate);
                        }
                    }
                }
            }
        }

        public static void ContextPartitionTerminate(
            int agentInstanceId, ContextControllerStatementDesc statementDesc,
            IDictionary<string, object> terminationProperties, bool leaveLocksAcquired,
            IList<AgentInstance> agentInstancesLocksHeld)
        {
            StatementContext statementContext = statementDesc.Lightweight.StatementContext;
            StatementResourceHolder holder =
                statementContext.StatementCPCacheService.MakeOrGetEntryCanNull(agentInstanceId, statementContext);

            if (terminationProperties != null) {
                MappedEventBean mappedEventBean = (MappedEventBean) holder.AgentInstanceContext.ContextProperties;
                mappedEventBean.Properties.PutAll(terminationProperties);
            }

            // we are not removing statement resources from memory as they may still be used for the same event
            Stop(
                holder.AgentInstanceStopCallback, holder.AgentInstanceContext, holder.FinalView, false,
                leaveLocksAcquired);
            if (leaveLocksAcquired) {
                agentInstancesLocksHeld.Add(
                    new AgentInstance(holder.AgentInstanceStopCallback, holder.AgentInstanceContext, holder.FinalView));
            }
        }

        public static void Stop(
            AgentInstanceStopCallback stopCallback, AgentInstanceContext agentInstanceContext, Viewable finalView,
            bool isStatementStop, bool leaveLocksAcquired)
        {

            agentInstanceContext.InstrumentationProvider.QContextPartitionDestroy(agentInstanceContext);

            // obtain statement lock
            StatementAgentInstanceLock @lock = agentInstanceContext.EpStatementAgentInstanceHandle
                .StatementAgentInstanceLock;
            @lock.AcquireWriteLock();
            try {
                if (finalView is OutputProcessViewTerminable && !isStatementStop) {
                    OutputProcessViewTerminable terminable = (OutputProcessViewTerminable) finalView;
                    terminable.Terminated();
                }

                StopSafe(stopCallback, agentInstanceContext);

                // release resource
                if (agentInstanceContext.StatementContext.StatementAIResourceRegistry != null) {
                    agentInstanceContext.StatementContext.StatementAIResourceRegistry.Deassign(
                        agentInstanceContext.AgentInstanceId);
                }

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

        public static void StopSafe(AgentInstanceStopCallback stopMethod, AgentInstanceContext agentInstanceContext)
        {
            AgentInstanceStopServices stopServices = new AgentInstanceStopServices(agentInstanceContext);

            ICollection<AgentInstanceStopCallback> additionalTerminations = agentInstanceContext.TerminationCallbackRO;
            foreach (AgentInstanceStopCallback stop in additionalTerminations) {
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

        public static AgentInstanceStopCallback FinalizeSafeStopCallbacks(
            IList<AgentInstanceStopCallback> stopCallbacks)
        {
            AgentInstanceStopCallback[] stopCallbackArray = stopCallbacks.ToArray();
            return new ProxyAgentInstanceStopCallback() {
                ProcStop = (services) => {
                    foreach (AgentInstanceStopCallback callback in stopCallbackArray) {
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
                },
            };
        }

        private static void HandleStopException(Exception e, AgentInstanceContext agentInstanceContext)
        {
            agentInstanceContext.ExceptionHandlingService.HandleException(
                e, agentInstanceContext.EpStatementAgentInstanceHandle, ExceptionHandlerExceptionType.UNDEPLOY, null);
        }

        public static AgentInstance StartStatement(
            StatementContextRuntimeServices services, int assignedContextId,
            ContextControllerStatementDesc statementDesc, MappedEventBean contextBean, AgentInstanceFilterProxy proxy)
        {
            StatementAgentInstanceFactoryResult result = AgentInstanceUtil.Start(
                services, statementDesc, assignedContextId, contextBean, proxy, false);
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
            StatementContext statementContext = statement.Lightweight.StatementContext;

            // create handle that comtains lock for use in scheduling and filter callbacks
            StatementAgentInstanceLock @lock =
                statementContext.StatementAIFactoryProvider.Factory.ObtainAgentInstanceLock(
                    statementContext, agentInstanceId);
            EPStatementAgentInstanceHandle agentInstanceHandle =
                new EPStatementAgentInstanceHandle(statementContext.EpStatementHandle, agentInstanceId,  @lock);

            AuditProvider auditProvider = statementContext.StatementInformationals.AuditProvider;
            InstrumentationCommon instrumentationProvider =
                statementContext.StatementInformationals.InstrumentationProvider;
            AgentInstanceContext agentInstanceContext = new AgentInstanceContext(
                statementContext, agentInstanceId, agentInstanceHandle, agentInstanceFilterProxy, contextProperties,
                auditProvider, instrumentationProvider);
            if (agentInstanceId != -1) {
                agentInstanceContext.AuditProvider.ContextPartition(true, agentInstanceContext);
            }

            StatementAgentInstanceLock statementAgentInstanceLock =
                agentInstanceContext.EpStatementAgentInstanceHandle.StatementAgentInstanceLock;

            agentInstanceContext.InstrumentationProvider.QContextPartitionAllocate(agentInstanceContext);

            statementAgentInstanceLock.AcquireWriteLock();

            try {
                // start
                StatementAgentInstanceFactoryResult startResult =
                    statement.Lightweight.StatementProvider.StatementAIFactoryProvider.Factory.NewContext(
                        agentInstanceContext, isRecoveringResilient);

                // hook up with listeners+subscribers
                startResult.FinalView.Child = statement.ContextMergeView; // hook output to merge view

                // assign agents for expression-node based strategies
                StatementAIResourceRegistry aiResourceRegistry = statementContext.StatementAIResourceRegistry;
                AIRegistryUtil.AssignFutures(
                    aiResourceRegistry, agentInstanceId,
                    startResult.OptionalAggegationService,
                    startResult.PriorStrategies,
                    startResult.PreviousGetterStrategies,
                    startResult.SubselectStrategies,
                    startResult.TableAccessStrategies,
                    startResult.RowRecogPreviousStrategy);

                // execute preloads, if any
                if (startResult.PreloadList != null) {
                    foreach (StatementAgentInstancePreload preload in startResult.PreloadList) {
                        preload.ExecutePreload();
                    }
                }

                StatementResourceHolder holder =
                    services.StatementResourceHolderBuilder.Build(agentInstanceContext, startResult);
                statementContext.StatementCPCacheService.StatementResourceService.SetPartitioned(
                    agentInstanceId, holder);

                // instantiate
                return startResult;
            }
            finally {
                if (agentInstanceContext.StatementContext.EpStatementHandle.HasTableAccess) {
                    agentInstanceContext.TableExprEvaluatorContext.ReleaseAcquiredLocks();
                }

                statementAgentInstanceLock.ReleaseWriteLock();
                agentInstanceContext.InstrumentationProvider.AContextPartitionAllocate();
            }
        }

        public static bool EvaluateFilterForStatement(
            EventBean theEvent, AgentInstanceContext agentInstanceContext, FilterHandle filterHandle)
        {
            // context was created - reevaluate for the given event
            ArrayDeque<FilterHandle> callbacks = new ArrayDeque<FilterHandle>();
            agentInstanceContext.FilterService.Evaluate(
                theEvent, callbacks, agentInstanceContext.StatementContext.StatementId);

            try {
                agentInstanceContext.VariableManagementService.SetLocalVersion();

                // sub-selects always go first
                foreach (FilterHandle handle in callbacks) {
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
                    ex, agentInstanceContext.EpStatementAgentInstanceHandle, ExceptionHandlerExceptionType.PROCESS,
                    theEvent);
            }

            return false;
        }

        public static StatementAgentInstanceLock NewLock(StatementContext statementContext)
        {
            return statementContext.StatementAgentInstanceLockFactory.GetStatementLock(
                statementContext.StatementName, statementContext.Annotations, statementContext.IsStatelessSelect,
                statementContext.StatementType);
        }
    }
} // end of namespace