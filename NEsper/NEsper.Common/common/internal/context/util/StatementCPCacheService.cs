///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Linq;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.airegistry;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.statement.resource;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.common.@internal.context.util
{
    public class StatementCPCacheService
    {
        public const int DEFAULT_AGENT_INSTANCE_ID = -1;

        private readonly ILockable @lock = new MonitorSpinLock(60000);

        /* for expression resources under context partitioning */
        private readonly StatementAIResourceRegistry statementAgentInstanceRegistry;

        public StatementCPCacheService(
            bool contextPartitioned,
            StatementResourceService statementResourceService,
            StatementAIResourceRegistry statementAgentInstanceRegistry)
        {
            IsContextPartitioned = contextPartitioned;
            StatementResourceService = statementResourceService;
            this.statementAgentInstanceRegistry = statementAgentInstanceRegistry;
        }

        public StatementResourceService StatementResourceService { get; }

        public bool IsContextPartitioned { get; }

        public StatementResourceHolder MakeOrGetEntryCanNull(
            int agentInstanceId, StatementContext statementContext)
        {
            if (!IsContextPartitioned) {
                return MakeOrGetEntryUnpartitioned(statementContext);
            }

            return MakeOrGetEntryPartitioned(agentInstanceId, statementContext);
        }

        /// <summary>
        ///     Thread-safe and efficient make-or-get
        /// </summary>
        /// <param name="statementContext">statement context</param>
        /// <returns>page resources</returns>
        private StatementResourceHolder MakeOrGetEntryUnpartitioned(
            StatementContext statementContext)
        {
            var resources = StatementResourceService.ResourcesUnpartitioned;
            if (resources == null) {
                using (@lock.Acquire()) {
                    resources = StatementResourceService.ResourcesUnpartitioned;
                    if (resources != null) {
                        return resources;
                    }

                    var agentInstanceContext = MakeNewAgentInstanceContextCanNull(
                        DEFAULT_AGENT_INSTANCE_ID, statementContext, false);
                    var result =
                        statementContext.StatementAIFactoryProvider.Factory.NewContext(agentInstanceContext, true);
                    HookUpNewRealization(result, statementContext);
                    resources = statementContext.StatementContextRuntimeServices.StatementResourceHolderBuilder.Build(
                        agentInstanceContext, result);
                    // for consistency with context partition behavior we are holding on to resources for now
                    StatementResourceService.Unpartitioned = resources;
                }
            }

            return resources;
        }

        private StatementResourceHolder MakeOrGetEntryPartitioned(
            int agentInstanceId, StatementContext statementContext)
        {
            var statementResourceService = statementContext.StatementCPCacheService.StatementResourceService;
            var resources = statementResourceService.ResourcesPartitioned.Get(agentInstanceId);
            if (resources != null) {
                return resources;
            }

            using (@lock.Acquire()) {
                resources = statementResourceService.ResourcesPartitioned.Get(agentInstanceId);
                if (resources != null) {
                    return resources;
                }

                var agentInstanceContext =
                    MakeNewAgentInstanceContextCanNull(agentInstanceId, statementContext, true);

                // we may receive a null if the context partition has already been deleted
                if (agentInstanceContext == null) {
                    return null;
                }

                var result = statementContext.StatementAIFactoryProvider.Factory
                    .NewContext(agentInstanceContext, true);
                HookUpNewRealization(result, statementContext);
                resources = statementContext.StatementContextRuntimeServices.StatementResourceHolderBuilder.Build(
                    agentInstanceContext, result);

                // we need to hold onto the handle for now even if it gets removed in order to correctly handle filter faults
                // i.e. for example context partitioned and context partition gets destroyed the statement should not fire for same event
                statementResourceService.SetPartitioned(agentInstanceId, resources);

                // assign the strategies
                AssignAIResourcesForExpressionContextPartitions(agentInstanceId, resources);
            }

            return resources;
        }

        private static AgentInstanceContext MakeNewAgentInstanceContextCanNull(
            int agentInstanceId, StatementContext statementContext, bool partitioned)
        {
            // re-allocate lock: for unpartitoned cases we use the same lock associated to the statement (no need to produce more locks)
            var @lock = statementContext.StatementAIFactoryProvider.Factory
                .ObtainAgentInstanceLock(statementContext, agentInstanceId);
            var epStatementAgentInstanceHandle = new EPStatementAgentInstanceHandle(
                statementContext.EpStatementHandle, agentInstanceId, @lock);

            // filter fault handler for create-context statements
            var contextName = statementContext.ContextName;
            MappedEventBean contextProperties = null;
            AgentInstanceFilterProxy agentInstanceFilterProxy = null;
            if (contextName != null) {
                var contextDeploymentId = statementContext.ContextRuntimeDescriptor.ContextDeploymentId;
                var contextManager =
                    statementContext.ContextManagementService.GetContextManager(contextDeploymentId, contextName);
                epStatementAgentInstanceHandle.FilterFaultHandler = contextManager;

                // the context partition may have been deleted
                var info = contextManager.GetContextAgentInstanceInfo(
                    statementContext, agentInstanceId);
                if (info == null) {
                    return null;
                }

                agentInstanceFilterProxy = info.FilterProxy;
                contextProperties = info.ContextProperties;
            }

            // re-allocate context
            var auditProvider = statementContext.StatementInformationals.AuditProvider;
            var instrumentationProvider =
                statementContext.StatementInformationals.InstrumentationProvider;
            return new AgentInstanceContext(
                statementContext, agentInstanceId, epStatementAgentInstanceHandle, agentInstanceFilterProxy,
                contextProperties, auditProvider, instrumentationProvider);
        }

        private void HookUpNewRealization(
            StatementAgentInstanceFactoryResult result, 
            StatementContext statementContext)
        {
            View dispatchChildView = statementContext.UpdateDispatchView;
            if (dispatchChildView != null) {
                result.FinalView.Child = dispatchChildView;
            }

            if (statementContext.ContextName == null) {
                StatementAIFactoryAssignments assignments = new StatementAIFactoryAssignmentsImpl(
                    result.OptionalAggegationService,
                    result.PriorStrategies, result.PreviousGetterStrategies, result.SubselectStrategies,
                    result.TableAccessStrategies,
                    result.RowRecogPreviousStrategy);
                statementContext.StatementAIFactoryProvider.Assign(assignments);
            }
        }

        private void AssignAIResourcesForExpressionContextPartitions(
            int agentInstanceId, StatementResourceHolder holder)
        {
            AIRegistryUtil.AssignFutures(
                statementAgentInstanceRegistry, agentInstanceId,
                holder.AggregationService,
                holder.PriorEvalStrategies,
                holder.PreviousGetterStrategies,
                holder.SubselectStrategies,
                holder.TableAccessStrategies,
                holder.RowRecogPreviousStrategy);
        }

        public int Clear(StatementContext statementContext)
        {
            var numCleared = 0;

            // un-assign any assigned expressions
            if (statementContext.ContextName == null) {
                statementContext.StatementAIFactoryProvider.Unassign();
            }

            var statementResourceService = statementContext.StatementCPCacheService.StatementResourceService;

            if (!IsContextPartitioned) {
                if (statementResourceService.ResourcesUnpartitioned != null) {
                    statementResourceService.DeallocateUnpartitioned();
                    numCleared++;
                }

                return numCleared;
            }

            var agentInstanceIds = statementResourceService.ResourcesPartitioned.Keys.ToArray();
            foreach (var agentInstanceId in agentInstanceIds) {
                statementAgentInstanceRegistry.Deassign(agentInstanceId);
            }

            foreach (var agentInstanceId in agentInstanceIds) {
                statementResourceService.DeallocatePartitioned(agentInstanceId);
                numCleared++;
            }

            return numCleared;
        }
    }
} // end of namespace