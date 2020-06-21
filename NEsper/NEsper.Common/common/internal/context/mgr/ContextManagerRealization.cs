///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.context;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.context.mgr
{
    public class ContextManagerRealization : ContextControllerLifecycleCallback,
        FilterFaultHandler
    {
        public ContextManagerRealization(
            ContextManagerResident contextManager,
            AgentInstanceContext agentInstanceContextCreate)
        {
            ContextManager = contextManager;
            AgentInstanceContextCreate = agentInstanceContextCreate;

            // create controllers
            var controllerFactories = contextManager.ContextDefinition.ControllerFactories;
            ContextControllers = new ContextController[controllerFactories.Length];
            for (var i = 0; i < controllerFactories.Length; i++) {
                var contextControllerFactory = controllerFactories[i];
                ContextControllers[i] = contextControllerFactory.Create(this);
            }
        }

        public ContextController[] ContextControllers { get; }

        public AgentInstanceContext AgentInstanceContextCreate { get; }

        public ContextManagerResident ContextManager { get; }

        public ContextPartitionInstantiationResult ContextPartitionInstantiate(
            IntSeqKey controllerPathId,
            int subpathId,
            ContextController originator,
            EventBean optionalTriggeringEvent,
            IDictionary<string, object> optionalPatternForInclusiveEval,
            object[] parentPartitionKeys,
            object partitionKey)
        {
            // detect non-leaf
            var controllerEnv = originator.Factory.FactoryEnv;
            if (controllerPathId.Length != controllerEnv.NestingLevel - 1) {
                throw new IllegalStateException("Unexpected controller path");
            }

            if (parentPartitionKeys.Length != controllerEnv.NestingLevel - 1) {
                throw new IllegalStateException("Unexpected partition key size");
            }

            var nestingLevel = controllerEnv.NestingLevel; // starts at 1 for root
            if (nestingLevel < ContextControllers.Length) {
                // next sub-sontext
                var nextContext = ContextControllers[nestingLevel];

                // add a partition key
                var nestedPartitionKeys = AddPartitionKey(nestingLevel, parentPartitionKeys, partitionKey);

                // now post-initialize, this may actually call back
                var childPath = controllerPathId.AddToEnd(subpathId);
                nextContext.Activate(
                    childPath,
                    nestedPartitionKeys,
                    optionalTriggeringEvent,
                    optionalPatternForInclusiveEval);

                return new ContextPartitionInstantiationResult(subpathId, Collections.GetEmptyList<AgentInstance>());
            }

            // assign context id
            var allPartitionKeys = CollectionUtil.AddValue(parentPartitionKeys, partitionKey);
            var assignedContextId = ContextManager.ContextPartitionIdService.AllocateId(allPartitionKeys);

            // build built-in context properties
            var contextBean = ContextManagerUtil.BuildContextProperties(
                assignedContextId,
                allPartitionKeys,
                ContextManager.ContextDefinition,
                AgentInstanceContextCreate.StatementContext);

            // handle leaf creation
            IList<AgentInstance> startedInstances = new List<AgentInstance>(2);
            foreach (var statementEntry in ContextManager.Statements) {
                var statementDesc = statementEntry.Value;

                Supplier<IDictionary<FilterSpecActivatable, FilterValueSetParam[][]>> generator = () =>
                    ContextManagerUtil.ComputeAddendumForStatement(
                        statementDesc,
                        ContextManager.Statements,
                        ContextManager.ContextDefinition.ControllerFactories,
                        allPartitionKeys,
                        AgentInstanceContextCreate);
                
                AgentInstanceFilterProxy proxy = new AgentInstanceFilterProxyImpl(generator);

                var agentInstance = AgentInstanceUtil.StartStatement(
                    ContextManager.StatementContextCreate.StatementContextRuntimeServices,
                    assignedContextId,
                    statementDesc,
                    contextBean,
                    proxy);
                startedInstances.Add(agentInstance);
            }

            // for all new contexts: evaluate this event for this statement
            if (optionalTriggeringEvent != null || optionalPatternForInclusiveEval != null) {
                // comment-in: log.info("Thread " + Thread.currentThread().getId() + " event " + optionalTriggeringEvent.getUnderlying() + " evaluateEventForStatement assignedContextId=" + assignedContextId);
                AgentInstanceUtil.EvaluateEventForStatement(
                    optionalTriggeringEvent,
                    optionalPatternForInclusiveEval,
                    startedInstances,
                    AgentInstanceContextCreate);
            }

            if (ContextManager.ListenersMayNull != null) {
                var identifier = ContextManager.GetContextPartitionIdentifier(allPartitionKeys);
                ContextStateEventUtil.DispatchPartition(
                    ContextManager.ListenersMayNull,
                    () => new ContextStateEventContextPartitionAllocated(
                        AgentInstanceContextCreate.RuntimeURI,
                        ContextManager.ContextRuntimeDescriptor.ContextDeploymentId,
                        ContextManager.ContextDefinition.ContextName,
                        assignedContextId,
                        identifier),
                    (
                        listener,
                        context) => listener.OnContextPartitionAllocated(context));
            }

            return new ContextPartitionInstantiationResult(assignedContextId, startedInstances);
        }

        public void ContextPartitionTerminate(
            IntSeqKey controllerPath,
            int subpathIdOrCPId,
            ContextController originator,
            IDictionary<string, object> terminationProperties,
            bool leaveLocksAcquired,
            IList<AgentInstance> agentInstancesLocksHeld)
        {
            if (controllerPath.Length != originator.Factory.FactoryEnv.NestingLevel - 1) {
                throw new IllegalStateException("Unrecognized controller path");
            }

            // detect non-leaf
            var controllerEnv = originator.Factory.FactoryEnv;
            var nestingLevel = controllerEnv.NestingLevel; // starts at 1 for root
            if (nestingLevel < ContextControllers.Length) {
                var childController = ContextControllers[nestingLevel];
                var path = controllerPath.AddToEnd(subpathIdOrCPId);
                childController.Deactivate(path, true);
                return;
            }

            var agentInstanceId = subpathIdOrCPId;

            // stop - in reverse order of statements, to allow termination to use tables+named-windows
            var contextControllers = ContextControllers;
            var contextControllerStatementDescList =
                new List<ContextControllerStatementDesc>(ContextManager.Statements.Values);
            contextControllerStatementDescList.Reverse();
            foreach (var statementDesc in contextControllerStatementDescList) {
                AgentInstanceUtil.ContextPartitionTerminate(
                    agentInstanceId,
                    statementDesc,
                    contextControllers,
                    terminationProperties,
                    leaveLocksAcquired,
                    agentInstancesLocksHeld);
            }

            // remove all context partition statement resources
            foreach (var statementEntry in ContextManager.Statements) {
                var statementDesc = statementEntry.Value;
                var svc = statementDesc.Lightweight.StatementContext.StatementCPCacheService.StatementResourceService;
                var holder = svc.DeallocatePartitioned(agentInstanceId);
            }

            // remove id
            ContextManager.ContextPartitionIdService.RemoveId(agentInstanceId);
            if (ContextManager.ListenersMayNull != null) {
                ContextStateEventUtil.DispatchPartition(
                    ContextManager.ListenersMayNull,
                    () => new ContextStateEventContextPartitionDeallocated(
                        AgentInstanceContextCreate.RuntimeURI,
                        ContextManager.ContextRuntimeDescriptor.ContextDeploymentId,
                        ContextManager.ContextDefinition.ContextName,
                        agentInstanceId),
                    (
                        listener,
                        context) => listener.OnContextPartitionDeallocated(context));
            }
        }

        public void ContextPartitionRecursiveVisit(
            IntSeqKey controllerPath,
            int subpathOrAgentInstanceId,
            ContextController originator,
            ContextPartitionVisitor visitor,
            ContextPartitionSelector[] selectorPerLevel)
        {
            if (controllerPath.Length != originator.Factory.FactoryEnv.NestingLevel - 1) {
                throw new IllegalStateException("Unrecognized controller path");
            }

            var nestingLevel = originator.Factory.FactoryEnv.NestingLevel; // starts at 1 for root
            if (nestingLevel < ContextControllers.Length) {
                var childController = ContextControllers[nestingLevel];
                var subPath = controllerPath.AddToEnd(subpathOrAgentInstanceId);
                childController.VisitSelectedPartitions(
                    subPath,
                    selectorPerLevel[nestingLevel],
                    visitor,
                    selectorPerLevel);
                return;
            }

            visitor.Add(subpathOrAgentInstanceId, originator.Factory.FactoryEnv.NestingLevel);
        }

        public void StartContext()
        {
            ContextControllers[0].Activate(IntSeqKeyRoot.INSTANCE, new object[0], null, null);
        }

        public void StopContext()
        {
            ContextControllers[0].Deactivate(IntSeqKeyRoot.INSTANCE, ContextControllers.Length > 1);
        }

        public void SafeDestroyContext()
        {
            // destroy context controllers
            foreach (var controllerPage in ContextControllers) {
                controllerPage.Destroy();
            }

            // remove realization
            var statementContext = AgentInstanceContextCreate.StatementContext;
            statementContext.StatementCPCacheService.Clear(statementContext);
        }

        public void StartLateStatement(ContextControllerStatementDesc statement)
        {
            var ids = ContextManager.ContextPartitionIdService.Ids;
            foreach (var cpid in ids) {
                var partitionKeys = ContextManager.ContextPartitionIdService.GetPartitionKeys(cpid);

                // create context properties bean
                var contextBean = ContextManagerUtil.BuildContextProperties(
                    cpid,
                    partitionKeys,
                    ContextManager.ContextDefinition,
                    AgentInstanceContextCreate.StatementContext);

                // create filter proxies
                Supplier<IDictionary<FilterSpecActivatable, FilterValueSetParam[][]>> generator = () =>
                    ContextManagerUtil.ComputeAddendumForStatement(
                        statement,
                        ContextManager.Statements,
                        ContextManager.ContextDefinition.ControllerFactories,
                        partitionKeys,
                        AgentInstanceContextCreate);
                AgentInstanceFilterProxy proxy = new AgentInstanceFilterProxyImpl(generator);

                // start
                AgentInstanceUtil.StartStatement(
                    ContextManager.StatementContextCreate.StatementContextRuntimeServices,
                    cpid,
                    statement,
                    contextBean,
                    proxy);
            }
        }

        public ICollection<int> GetAgentInstanceIds(ContextPartitionSelector selector)
        {
            if (selector is ContextPartitionSelectorById) {
                var byId = (ContextPartitionSelectorById) selector;
                var ids = byId.ContextPartitionIds;
                if (ids == null || ids.IsEmpty()) {
                    return Collections.GetEmptyList<int>();
                }

                var agentInstanceIds = new List<int>(ids);
                agentInstanceIds.RetainAll(ContextManager.ContextPartitionIdService.Ids);
                return agentInstanceIds;
            }

            if (selector is ContextPartitionSelectorAll) {
                return ContextManager.ContextPartitionIdService.Ids;
            }

            if (selector is ContextPartitionSelectorNested) {
                if (ContextControllers.Length == 1) {
                    throw ContextControllerSelectorUtil.GetInvalidSelector(
                        new[] {typeof(ContextPartitionSelectorNested)},
                        selector,
                        true);
                }

                var nested = (ContextPartitionSelectorNested) selector;
                var visitor = new ContextPartitionVisitorAgentInstanceId(ContextControllers.Length);
                foreach (var stack in nested.Selectors) {
                    ContextControllers[0].VisitSelectedPartitions(IntSeqKeyRoot.INSTANCE, stack[0], visitor, stack);
                }

                return visitor.Ids;
            }
            else {
                if (ContextControllers.Length > 1) {
                    throw ContextControllerSelectorUtil.GetInvalidSelector(
                        new[] {
                            typeof(ContextPartitionSelectorAll), typeof(ContextPartitionSelectorById),
                            typeof(ContextPartitionSelectorNested)
                        },
                        selector,
                        true);
                }

                var visitor = new ContextPartitionVisitorAgentInstanceId(ContextControllers.Length);
                ContextControllers[0]
                    .VisitSelectedPartitions(
                        IntSeqKeyRoot.INSTANCE,
                        selector,
                        visitor,
                        new[] {selector});
                return visitor.Ids;
            }
        }

        public void RemoveStatement(ContextControllerStatementDesc statementDesc)
        {
            var contextControllers = ContextControllers;
            foreach (var id in ContextManager.ContextPartitionIdService.Ids) {
                AgentInstanceUtil.ContextPartitionTerminate(id, statementDesc, contextControllers, null, false, null);
            }
        }

        public void ActivateCreateVariableStatement(ContextControllerStatementDesc statement)
        {
            var ids = ContextManager.ContextPartitionIdService.Ids;
            ContextManagerUtil.GetAgentInstances(statement, ids);
        }

        public bool HandleFilterFault(
            EventBean theEvent,
            long version)
        {
            // We handle context-management filter faults always the same way.
            // Statement-partition filter faults are specific to the controller.
            //
            // Hashed-context without preallocate: every time a new bucket shows up the filter version changes, faulting for those that are not aware of the new bucket
            // Example:
            //   T0 sends {key='A', id='E1'}
            //   T1 sends {key='A', id='E1'}
            //   T0 receives create-ctx-ai-lock, processes "matchFound", allocates stmt, which adds filter, sets filter version
            //   T1 encounteres newer filter version, invokes filter fault handler, evaluates event against filters, passes event to statement-partition
            //
            // To avoid duplicate processing into the statement-partition, filter by comparing statement-partition filter version.
            var ids = ContextManager.ContextPartitionIdService.Ids;
            foreach (var stmt in ContextManager.Statements) {
                var agentInstances = ContextManagerUtil.GetAgentInstancesFiltered(
                    stmt.Value,
                    ids,
                    agentInstance => agentInstance.AgentInstanceContext.FilterVersionAfterAllocation >= version);
                AgentInstanceUtil.EvaluateEventForStatement(theEvent, null, agentInstances, AgentInstanceContextCreate);
            }

            return false;
        }

        private object[] AddPartitionKey(
            int nestingLevel,
            object[] parentPartitionKeys,
            object partitionKey)
        {
            var keysPerContext = new object[nestingLevel];
            if (nestingLevel > 1) {
                Array.Copy(parentPartitionKeys, 0, keysPerContext, 0, parentPartitionKeys.Length);
            }

            keysPerContext[nestingLevel - 1] = partitionKey;
            return keysPerContext;
        }

        public void Transfer(AgentInstanceTransferServices xfer)
        {
            ContextControllers[0].Transfer(IntSeqKeyRoot.INSTANCE, ContextControllers.Length > 1, xfer);
        }

        public void TransferRecursive(
            IntSeqKey controllerPath,
            int subpathOrAgentInstanceId,
            ContextController originator,
            AgentInstanceTransferServices xfer)
        {
            if (controllerPath.Length != originator.Factory.FactoryEnv.NestingLevel - 1) {
                throw new IllegalStateException("Unrecognized controller path");
            }

            var nestingLevel = originator.Factory.FactoryEnv.NestingLevel; // starts at 1 for root
            if (nestingLevel >= ContextControllers.Length) {
                return;
            }

            var childController = ContextControllers[nestingLevel];
            var subPath = controllerPath.AddToEnd(subpathOrAgentInstanceId);
            childController.Transfer(subPath, nestingLevel < ContextControllers.Length - 1, xfer);
        }
    }
} // end of namespace