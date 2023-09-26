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
using com.espertech.esper.common.client.context;
using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.context.airegistry;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.common.@internal.context.cpidsvc;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.context.mgr
{
    public class ContextManagerResident : ContextManager,
        ContextIteratorHandler
    {
        protected CopyOnWriteList<ContextPartitionStateListener> _listenersLazy;

        public ContextManagerResident(
            string deploymentId,
            ContextDefinition contextDefinition)
        {
            ContextDefinition = contextDefinition;
            ContextRuntimeDescriptor = new ContextRuntimeDescriptor(contextDefinition.ContextName, deploymentId, this);
        }

        public ContextDefinition ContextDefinition { get; }
        public StatementContext StatementContextCreate { get; private set; }
        public long ContextPartitionCount => ContextPartitionIdService.Count;
        public ContextPartitionIdService ContextPartitionIdService { get; private set; }

        public IEnumerator<EventBean> GetEnumerator(int statementId)
        {
            var instances = GetAgentInstancesForStmt(statementId, new ContextPartitionSelectorAll());
            return new AgentInstanceArraySafeEnumerator(instances);
        }

        public SafeEnumerator<EventBean> GetSafeEnumerator(int statementId)
        {
            var instances = GetAgentInstancesForStmt(statementId, new ContextPartitionSelectorAll());
            return new AgentInstanceArraySafeEnumerator(instances);
        }

        public IEnumerator<EventBean> GetEnumerator(
            int statementId,
            ContextPartitionSelector selector)
        {
            var instances = GetAgentInstancesForStmt(statementId, selector);
            return new AgentInstanceArraySafeEnumerator(instances);
        }

        public SafeEnumerator<EventBean> GetSafeEnumerator(
            int statementId,
            ContextPartitionSelector selector)
        {
            var instances = GetAgentInstancesForStmt(statementId, selector);
            return new AgentInstanceArraySafeEnumerator(instances);
        }

        public void AddStatement(
            ContextControllerStatementDesc statement,
            bool recovery)
        {
            var statementContextOfStatement = statement.Lightweight.StatementContext;
            Statements.Put(statementContextOfStatement.StatementId, statement);
            // dispatch event
            ContextStateEventUtil.DispatchPartition(
                _listenersLazy,
                () => new ContextStateEventContextStatementAdded(
                    StatementContextCreate.RuntimeURI,
                    ContextRuntimeDescriptor.ContextDeploymentId,
                    ContextDefinition.ContextName,
                    statementContextOfStatement.DeploymentId,
                    statementContextOfStatement.StatementName),
                (_, ev) => _.OnContextStatementAdded(ev));
            if (recovery) {
                if (statement.Lightweight.StatementInformationals.StatementType == StatementType.CREATE_VARIABLE) {
                    Realization.ActivateCreateVariableStatement(statement);
                }

                return;
            }

            // activate if this is the first statement
            if (Statements.Count == 1) {
                Realization.StartContext();
                ContextStateEventUtil.DispatchPartition(
                    _listenersLazy,
                    () => new ContextStateEventContextActivated(
                        StatementContextCreate.RuntimeURI,
                        ContextRuntimeDescriptor.ContextDeploymentId,
                        ContextDefinition.ContextName),
                    (_, ev) => _.OnContextActivated(ev));
            }
            else {
                // activate statement in respect to existing context partitions
                Realization.StartLateStatement(statement);
            }
        }

        public void StopStatement(
            int statementId,
            string statementName,
            string statementDeploymentId)
        {
            if (!Statements.ContainsKey(statementId)) {
                return;
            }

            RemoveStatement(statementId);
            ContextStateEventUtil.DispatchPartition(
                _listenersLazy,
                () => new ContextStateEventContextStatementRemoved(
                    StatementContextCreate.RuntimeURI,
                    ContextRuntimeDescriptor.ContextDeploymentId,
                    ContextRuntimeDescriptor.ContextName,
                    statementDeploymentId,
                    statementName),
                (_, ev) => _.OnContextStatementRemoved(ev));
            if (Statements.IsEmpty()) {
                Realization.StopContext();
                ContextPartitionIdService.Clear();
                ContextStateEventUtil.DispatchPartition(
                    _listenersLazy,
                    () => new ContextStateEventContextDeactivated(
                        StatementContextCreate.RuntimeURI,
                        ContextRuntimeDescriptor.ContextDeploymentId,
                        ContextRuntimeDescriptor.ContextName),
                    (_, ev) => _.OnContextDeactivated(ev));
            }
        }

        public int CountStatements(Func<StatementContext, bool> filter)
        {
            var count = 0;
            foreach (var entry in Statements) {
                if (filter.Invoke(entry.Value.Lightweight.StatementContext)) {
                    count++;
                }
            }

            return count;
        }

        public ContextManagerRealization Realization {
            get {
                var statementResourceHolder =
                    StatementContextCreate.StatementCPCacheService.MakeOrGetEntryCanNull(-1, StatementContextCreate);
                return statementResourceHolder.ContextManagerRealization;
            }
        }

        public void DestroyContext()
        {
            if (!Statements.IsEmpty()) {
                throw new IllegalStateException("Cannot invoke destroy with statements still attached");
            }

            if (ContextPartitionIdService == null) {
                return;
            }

            Realization.SafeDestroyContext();
            ContextPartitionIdService.Destroy();
            ContextPartitionIdService = null;
        }

        public ContextManagerRealization AllocateNewRealization(AgentInstanceContext agentInstanceContext)
        {
            return new ContextManagerRealization(this, agentInstanceContext);
        }

        public IDictionary<int, ContextControllerStatementDesc> Statements { get; } =
            new LinkedHashMap<int, ContextControllerStatementDesc>();

        public ContextAgentInstanceInfo GetContextAgentInstanceInfo(
            StatementContext statementContextOfStatement,
            int agentInstanceId)
        {
            var partitionKeys = ContextPartitionIdService.GetPartitionKeys(agentInstanceId);
            if (partitionKeys == null) {
                return null;
            }

            var statement = Statements.Get(statementContextOfStatement.StatementId);
            var props = ContextManagerUtil.BuildContextProperties(
                agentInstanceId,
                partitionKeys,
                ContextDefinition,
                StatementContextCreate);
            var proxy = ComputeFilterAddendum(statement, partitionKeys);
            return new ContextAgentInstanceInfo(props, proxy);
        }

        public ContextRuntimeDescriptor ContextRuntimeDescriptor { get; }

        public IDictionary<string, object> GetContextPartitions(int contextPartitionId)
        {
            foreach (var entry in Statements) {
                var statementContext = entry.Value.Lightweight.StatementContext;
                var resourceService = statementContext.StatementCPCacheService;
                var holder = resourceService.MakeOrGetEntryCanNull(contextPartitionId, statementContext);
                if (holder != null) {
                    return ((MappedEventBean)holder.AgentInstanceContext.ContextProperties).Properties;
                }
            }

            return null;
        }

        public MappedEventBean GetContextPropertiesEvent(int contextPartitionId)
        {
            var props = GetContextPartitions(contextPartitionId);
            return StatementContextCreate.EventBeanTypedEventFactory.AdapterForTypedMap(
                props,
                ContextDefinition.EventTypeContextProperties);
        }

        public ContextPartitionCollection GetContextPartitions(ContextPartitionSelector selector)
        {
            if (selector is ContextPartitionSelectorAll) {
                IDictionary<int, ContextPartitionIdentifier> map = new Dictionary<int, ContextPartitionIdentifier>();
                var ids = ContextPartitionIdService.Ids;
                foreach (var id in ids) {
                    var partitionKeys = ContextPartitionIdService.GetPartitionKeys(id);
                    if (partitionKeys != null) {
                        var identifier = GetContextPartitionIdentifier(partitionKeys);
                        map.Put(id, identifier);
                    }
                }

                return new ContextPartitionCollection(map);
            }

            var idsX = Realization.GetAgentInstanceIds(selector);
            IDictionary<int, ContextPartitionIdentifier>
                identifiers = new Dictionary<int, ContextPartitionIdentifier>();
            foreach (var id in idsX) {
                var partitionKeys = ContextPartitionIdService.GetPartitionKeys(id);
                if (partitionKeys == null) {
                    continue;
                }

                var identifier = GetContextPartitionIdentifier(partitionKeys);
                identifiers.Put(id, identifier);
            }

            return new ContextPartitionCollection(identifiers);
        }

        public ISet<int> GetContextPartitionIds(ContextPartitionSelector selector)
        {
            return new LinkedHashSet<int>(ContextPartitionIdService.Ids);
        }

        public ContextPartitionIdentifier GetContextIdentifier(int agentInstanceId)
        {
            var partitionKeys = ContextPartitionIdService.GetPartitionKeys(agentInstanceId);
            return partitionKeys == null ? null : GetContextPartitionIdentifier(partitionKeys);
        }

        public StatementAIResourceRegistry AllocateAgentInstanceResourceRegistry(
            AIRegistryRequirements registryRequirements)
        {
            if (ContextDefinition.ControllerFactories.Length == 1) {
                return ContextDefinition.ControllerFactories[0]
                    .AllocateAgentInstanceResourceRegistry(registryRequirements);
            }

            return AIRegistryUtil.AllocateRegistries(registryRequirements, AIRegistryFactoryMap.INSTANCE);
        }

        public DataInputOutputSerde[] ContextPartitionKeySerdes { get; private set; }
        public int NumNestingLevels => ContextDefinition.ControllerFactories.Length;

        public void AddListener(ContextPartitionStateListener listener)
        {
            lock (this) {
                if (_listenersLazy == null) {
                    _listenersLazy = new CopyOnWriteList<ContextPartitionStateListener>();
                }

                _listenersLazy.Add(listener);
            }
        }

        public void RemoveListener(ContextPartitionStateListener listener)
        {
            if (_listenersLazy == null) {
                return;
            }

            _listenersLazy.Remove(listener);
        }

        public void RemoveListeners()
        {
            if (_listenersLazy == null) {
                return;
            }

            _listenersLazy.Clear();
        }

        public bool HandleFilterFault(
            EventBean theEvent,
            long version)
        {
            return Realization.HandleFilterFault(theEvent, version);
        }

        private void RemoveStatement(int statementId)
        {
            var statementDesc = Statements.Get(statementId);
            if (statementDesc == null) {
                return;
            }

            Realization.RemoveStatement(statementDesc);
            Statements.Remove(statementId);
        }

        public AgentInstanceFilterProxy ComputeFilterAddendum(
            ContextControllerStatementDesc statement,
            object[] contextPartitionKeys)
        {
            Supplier<IDictionary<FilterSpecActivatable, FilterValueSetParam[][]>> generator = () =>
                ContextManagerUtil.ComputeAddendumForStatement(
                    statement,
                    Statements,
                    ContextDefinition.ControllerFactories,
                    contextPartitionKeys,
                    Realization.AgentInstanceContextCreate);
            return new AgentInstanceFilterProxyImpl(generator);
        }

        public ContextPartitionIdentifier GetContextPartitionIdentifier(object[] partitionKeys)
        {
            if (ContextDefinition.ControllerFactories.Length == 1) {
                return ContextDefinition.ControllerFactories[0].GetContextPartitionIdentifier(partitionKeys[0]);
            }

            var identifiers = new ContextPartitionIdentifier[partitionKeys.Length];
            for (var i = 0; i < partitionKeys.Length; i++) {
                identifiers[i] = ContextDefinition.ControllerFactories[i]
                    .GetContextPartitionIdentifier(partitionKeys[i]);
            }

            return new ContextPartitionIdentifierNested(identifiers);
        }

        public ICollection<int> GetAgentInstanceIds(ContextPartitionSelector selector)
        {
            return Realization.GetAgentInstanceIds(selector);
        }

        public DataInputOutputSerde[] GetContextPartitionKeySerdeSubset(int nestingLevel)
        {
            return GetContextPartitionKeySerdeSubset(nestingLevel, ContextPartitionKeySerdes);
        }

        public static DataInputOutputSerde[] GetContextPartitionKeySerdeSubset(
            int nestingLevel,
            DataInputOutputSerde[] allSerdes)
        {
            var serdes = new DataInputOutputSerde[nestingLevel - 1];
            for (var i = 0; i < nestingLevel - 1; i++) {
                serdes[i] = allSerdes[i];
            }

            return serdes;
        }

        public void ClearCaches()
        {
            if (ContextPartitionIdService != null) {
                ContextPartitionIdService.ClearCaches();
            }
        }

        private AgentInstance[] GetAgentInstancesForStmt(
            int statementId,
            ContextPartitionSelector selector)
        {
            var agentInstanceIds = GetAgentInstanceIds(selector);
            if (agentInstanceIds == null || agentInstanceIds.IsEmpty()) {
                return new AgentInstance[0];
            }

            foreach (var entry in Statements) {
                if (entry.Value.Lightweight.StatementContext.StatementId == statementId) {
                    var agentInstances = ContextManagerUtil.GetAgentInstances(entry.Value, agentInstanceIds);
                    return agentInstances.ToArray();
                }
            }

            return null;
        }

        public StatementContext StatementContext {
            set {
                StatementContextCreate = value;
                ContextPartitionKeySerdes =
                    StatementContextCreate.ContextServiceFactory.GetContextPartitionKeyBindings(ContextDefinition);
                ContextPartitionIdService = StatementContextCreate.ContextServiceFactory.GetContextPartitionIdService(
                    StatementContextCreate,
                    ContextPartitionKeySerdes,
                    ContextDefinition.PartitionIdSvcStateMgmtSettings);
            }
        }

        public CopyOnWriteList<ContextPartitionStateListener> ListenersMayNull => _listenersLazy;

        public IEnumerator<ContextPartitionStateListener> Listeners {
            get {
                if (_listenersLazy == null) {
                    return EnumerationHelper.Empty<ContextPartitionStateListener>();
                }

                return _listenersLazy.GetEnumerator();
            }
        }
    }
} // end of namespace