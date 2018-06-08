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
using com.espertech.esper.client.context;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.events;
using com.espertech.esper.filter;

namespace com.espertech.esper.core.context.mgr
{
    public class ContextManagerImpl
        : ContextManager
        , ContextControllerLifecycleCallback
        , ContextEnumeratorHandler
        , FilterFaultHandler
    {
        private readonly ILockable _uLock;
        private readonly String _contextName;
        private readonly EPServicesContext _servicesContext;
        private readonly ContextControllerFactory _factory;
        private readonly IDictionary<int, ContextControllerStatementDesc> _statements = new LinkedHashMap<int, ContextControllerStatementDesc>(); // retain order of statement creation
        private readonly ContextDescriptor _contextDescriptor;
        private readonly IDictionary<int, ContextControllerTreeAgentInstanceList> _agentInstances = new LinkedHashMap<int, ContextControllerTreeAgentInstanceList>();

        /// <summary>The single root context. This represents the context declared first. </summary>
        private readonly ContextController _rootContext;
        private readonly ContextPartitionIdManager _contextPartitionIdManager;

        public ContextManagerImpl(
            ContextControllerFactoryServiceContext factoryServiceContext)
        {
            _uLock = factoryServiceContext.ServicesContext.LockManager.CreateLock(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            _contextName = factoryServiceContext.ContextName;
            _servicesContext = factoryServiceContext.ServicesContext;
            _factory = factoryServiceContext.AgentInstanceContextCreate.StatementContext.ContextControllerFactoryService.GetFactory(factoryServiceContext)[0];
            _rootContext = _factory.CreateNoCallback(0, this);   // single instance: created here and activated/deactivated later
            _contextPartitionIdManager = factoryServiceContext.AgentInstanceContextCreate.StatementContext.ContextControllerFactoryService.AllocatePartitionIdMgr(_contextName, factoryServiceContext.AgentInstanceContextCreate.StatementContext.StatementId);

            var resourceRegistryFactory = _factory.StatementAIResourceRegistryFactory;

            var contextProps = _factory.ContextBuiltinProps;
            var contextPropsType = _servicesContext.EventAdapterService.CreateAnonymousMapType(_contextName, contextProps, true);
            var registry = new ContextPropertyRegistryImpl(_factory.ContextDetailPartitionItems, contextPropsType);
            _contextDescriptor = new ContextDescriptor(_contextName, _factory.IsSingleInstanceContext, registry, resourceRegistryFactory, this, _factory.ContextDetail);
        }

        public int NumNestingLevels => 1;

        public IDictionary<int, ContextControllerStatementDesc> Statements => _statements;

        public ContextDescriptor ContextDescriptor => _contextDescriptor;

        public ContextStateCache ContextStateCache => _factory.StateCache;

        public void AddStatement(ContextControllerStatementBase statement, bool isRecoveringResilient)
        {
            // validation down the hierarchy
            var caches = _factory.ValidateStatement(statement);

            // add statement
            var desc = new ContextControllerStatementDesc(statement, new ContextControllerStatementCtxCache[] { caches });
            _statements.Put(statement.StatementContext.StatementId, desc);

            // activate if this is the first statement
            if (_statements.Count == 1)
            {
                Activate();     // this may itself trigger a callback
            }
            // activate statement in respect to existing context partitions
            else
            {
                foreach (var entry in _agentInstances)
                {
                    if (entry.Value.State == ContextPartitionState.STARTED)
                    {
                        var agentInstance = StartStatement(entry.Key, desc, _rootContext, entry.Value.InitPartitionKey, entry.Value.InitContextProperties, isRecoveringResilient);
                        entry.Value.AgentInstances.Add(agentInstance);
                    }
                }
            }
        }

        public void StopStatement(String statementName, int statementId)
        {
            using (_uLock.Acquire())
            {
                DestroyStatement(statementName, statementId);
            }
        }

        public void DestroyStatement(String statementName, int statementId)
        {
            using (_uLock.Acquire())
            {
                if (!_statements.ContainsKey(statementId))
                {
                    return;
                }
                if (_statements.Count == 1)
                {
                    SafeDestroy();
                }
                else
                {
                    RemoveStatement(statementId);
                }
            }
        }

        public void SafeDestroy()
        {
            if (_rootContext != null)
            {
                // deactivate
                _rootContext.Deactivate();
                _factory.FactoryContext.StateCache.RemoveContext(_contextName);

                foreach (var entryCP in _agentInstances)
                {
                    StatementAgentInstanceUtil.StopAgentInstances(entryCP.Value.AgentInstances, null, _servicesContext, true, false);
                }
                _agentInstances.Clear();
                _contextPartitionIdManager.Clear();
                _statements.Clear();
            }
        }

        public ContextControllerInstanceHandle ContextPartitionInstantiate(
            int? optionalContextPartitionId,
            int subPathId,
            int? importSubpathId,
            ContextController originator,
            EventBean optionalTriggeringEvent,
            IDictionary<string, object> optionalTriggeringPattern,
            object partitionKey,
            IDictionary<string, object> contextProperties,
            ContextControllerState states,
            ContextInternalFilterAddendum filterAddendum,
            bool isRecoveringResilient,
            ContextPartitionState state)
        {
            using (_uLock.Acquire())
            {
                // assign context id
                int assignedContextId;
                if (optionalContextPartitionId != null && !states.IsImported)
                {
                    assignedContextId = optionalContextPartitionId.Value;
                    _contextPartitionIdManager.AddExisting(optionalContextPartitionId.Value);
                }
                else
                {
                    assignedContextId = _contextPartitionIdManager.AllocateId();
                    if (states != null && states.PartitionImportCallback != null && optionalContextPartitionId != null)
                    {
                        states.PartitionImportCallback.Allocated(assignedContextId, optionalContextPartitionId.Value);
                    }
                }

                // handle leaf creation
                IList<AgentInstance> newInstances = new List<AgentInstance>();
                if (state == ContextPartitionState.STARTED)
                {
                    foreach (var statementEntry in _statements)
                    {
                        var statementDesc = statementEntry.Value;
                        var instance = StartStatement(assignedContextId, statementDesc, originator, partitionKey, contextProperties, isRecoveringResilient);
                        newInstances.Add(instance);
                    }
                }

                // for all new contexts: evaluate this event for this statement
                if (optionalTriggeringEvent != null || optionalTriggeringPattern != null)
                {
                    StatementAgentInstanceUtil.EvaluateEventForStatement(_servicesContext, optionalTriggeringEvent, optionalTriggeringPattern, newInstances);
                }

                // save leaf
                var filterVersion = _servicesContext.FilterService.FiltersVersion;
                var agentInstanceList = new ContextControllerTreeAgentInstanceList(filterVersion, partitionKey, contextProperties, newInstances, state);
                _agentInstances.Put(assignedContextId, agentInstanceList);

                return new ContextNestedHandleImpl(subPathId, assignedContextId, agentInstanceList);
            }
        }

        public void ContextPartitionTerminate(ContextControllerInstanceHandle contextNestedHandle, IDictionary<String, Object> terminationProperties, bool leaveLocksAcquired, IList<AgentInstance> agentInstancesCollected)
        {
            using (_uLock.Acquire())
            {
                var handle = (ContextNestedHandleImpl)contextNestedHandle;
                var entry = _agentInstances.Delete(handle.ContextPartitionOrPathId);
                if (entry != null)
                {
                    StatementAgentInstanceUtil.StopAgentInstances(entry.AgentInstances, terminationProperties, _servicesContext, false, leaveLocksAcquired);
                    if (agentInstancesCollected != null)
                    {
                        agentInstancesCollected.AddAll(entry.AgentInstances);
                    }
                    entry.AgentInstances.Clear();
                    _contextPartitionIdManager.RemoveId(contextNestedHandle.ContextPartitionOrPathId);
                }
            }
        }

        public void ContextPartitionNavigate(ContextControllerInstanceHandle existingHandle, ContextController originator, ContextControllerState controllerState, int exportedCPOrPathId, ContextInternalFilterAddendum filterAddendum, AgentInstanceSelector agentInstanceSelector, byte[] payload, bool isRecoveringResilient)
        {
            var entry = _agentInstances.Get(existingHandle.ContextPartitionOrPathId);
            if (entry == null)
            {
                return;
            }

            if (entry.State == ContextPartitionState.STOPPED)
            {
                entry.State = ContextPartitionState.STARTED;
                entry.AgentInstances.Clear();
                foreach (var statement in _statements)
                {
                    var instance = StartStatement(existingHandle.ContextPartitionOrPathId, statement.Value, originator, entry.InitPartitionKey, entry.InitContextProperties, false);
                    entry.AgentInstances.Add(instance);
                }
                var key = new ContextStatePathKey(1, 0, existingHandle.SubPathId);
                var value = new ContextStatePathValue(existingHandle.ContextPartitionOrPathId, payload, ContextPartitionState.STARTED);
                _rootContext.Factory.FactoryContext.StateCache.UpdateContextPath(_contextName, key, value);
            }
            else
            {
                IList<AgentInstance> removed = new List<AgentInstance>(2);
                IList<AgentInstance> added = new List<AgentInstance>(2);
                foreach (var agentInstance in entry.AgentInstances)
                {
                    if (!agentInstanceSelector.Select(agentInstance))
                    {
                        continue;
                    }

                    // remove
                    StatementAgentInstanceUtil.StopAgentInstanceRemoveResources(agentInstance, null, _servicesContext, false, false);
                    removed.Add(agentInstance);

                    // start
                    var statementDesc = _statements.Get(agentInstance.AgentInstanceContext.StatementId);
                    var instance = StartStatement(existingHandle.ContextPartitionOrPathId, statementDesc, originator, entry.InitPartitionKey, entry.InitContextProperties, isRecoveringResilient);
                    added.Add(instance);

                    if (controllerState.PartitionImportCallback != null)
                    {
                        controllerState.PartitionImportCallback.Existing(existingHandle.ContextPartitionOrPathId, exportedCPOrPathId);
                    }
                }
                entry.AgentInstances.RemoveAll(removed);
                entry.AgentInstances.AddAll(added);
            }
        }

        public FilterSpecLookupable GetFilterLookupable(EventType eventType)
        {
            return _factory.GetFilterLookupable(eventType);
        }

        public IEnumerator<EventBean> GetEnumerator(int statementId)
        {
            using (_uLock.Acquire())
            {
                var instances = GetAgentInstancesForStmt(statementId);
                return instances.SelectMany(instance => instance.FinalView).GetEnumerator();
            }
        }

        public IEnumerator<EventBean> GetSafeEnumerator(int statementId)
        {
            using (_uLock.Acquire())
            {
                var instances = GetAgentInstancesForStmt(statementId);
                return GetEnumeratorWithInstanceLock(instances);
            }
        }

        private static IEnumerator<EventBean> GetEnumeratorWithInstanceLock(AgentInstance[] instances)
        {
            foreach (var instance in instances)
            {
                var instanceLock = instance.AgentInstanceContext.EpStatementAgentInstanceHandle.StatementAgentInstanceLock;
                using (instanceLock.AcquireWriteLock())
                {
                    foreach (var eventBean in instance.FinalView)
                    {
                        yield return eventBean;
                    }
                }
            }
        }

        public IEnumerator<EventBean> GetEnumerator(int statementId, ContextPartitionSelector selector)
        {
            using (_uLock.Acquire())
            {
                var instances = GetAgentInstancesForStmt(statementId, selector);
                return instances.SelectMany(instance => instance.FinalView).GetEnumerator();
            }
        }

        public IEnumerator<EventBean> GetSafeEnumerator(int statementId, ContextPartitionSelector selector)
        {
            using (_uLock.Acquire())
            {
                var instances = GetAgentInstancesForStmt(statementId, selector);
                return GetEnumeratorWithInstanceLock(instances);
            }
        }

        public ICollection<int> GetAgentInstanceIds(ContextPartitionSelector selector)
        {
            if (selector is ContextPartitionSelectorById)
            {
                var byId = (ContextPartitionSelectorById)selector;
                var ids = byId.ContextPartitionIds;
                if (ids == null || ids.IsEmpty())
                {
                    return Collections.GetEmptyList<int>();
                }
                var agentInstanceIds = new List<int>(ids);
                agentInstanceIds.RetainAll(_agentInstances.Keys);
                return agentInstanceIds;
            }
            else if (selector is ContextPartitionSelectorAll)
            {
                return new List<int>(_agentInstances.Keys);
            }
            else
            {
                var visitor = new ContextPartitionVisitorAgentInstanceId(1);
                _rootContext.VisitSelectedPartitions(selector, visitor);
                return visitor.AgentInstanceIds;
            }
        }

        public ContextStatePathDescriptor ExtractPaths(ContextPartitionSelector selector)
        {
            var visitor = new ContextPartitionVisitorState();
            _rootContext.VisitSelectedPartitions(selector, visitor);
            return new ContextStatePathDescriptor(visitor.States, visitor.ContextPartitionInfo);
        }

        public ContextStatePathDescriptor ExtractStopPaths(ContextPartitionSelector selector)
        {
            var states = ExtractPaths(selector);
            foreach (var entry in states.Paths)
            {
                var agentInstanceId = entry.Value.OptionalContextPartitionId.Value;
                var list = _agentInstances.Get(agentInstanceId);
                list.State = ContextPartitionState.STOPPED;
                StatementAgentInstanceUtil.StopAgentInstances(list.AgentInstances, null, _servicesContext, false, false);
                list.ClearAgentInstances();
                entry.Value.State = ContextPartitionState.STOPPED;
                _rootContext.Factory.FactoryContext.StateCache.UpdateContextPath(_contextName, entry.Key, entry.Value);
            }
            return states;
        }

        public ContextStatePathDescriptor ExtractDestroyPaths(ContextPartitionSelector selector)
        {
            var states = ExtractPaths(selector);
            foreach (var entry in states.Paths)
            {
                var agentInstanceId = entry.Value.OptionalContextPartitionId.Value;
                var descriptor = states.ContextPartitionInformation.Get(agentInstanceId);
                _rootContext.DeletePath(descriptor.Identifier);
                var list = _agentInstances.Delete(agentInstanceId);
                StatementAgentInstanceUtil.StopAgentInstances(list.AgentInstances, null, _servicesContext, false, false);
                list.ClearAgentInstances();
                _rootContext.Factory.FactoryContext.StateCache.RemoveContextPath(_contextName, entry.Key.Level, entry.Key.ParentPath, entry.Key.SubPath);
            }
            return states;
        }

        public IDictionary<int, ContextPartitionDescriptor> StartPaths(ContextPartitionSelector selector)
        {
            var states = ExtractPaths(selector);
            foreach (var entry in states.Paths)
            {
                var agentInstanceId = entry.Value.OptionalContextPartitionId.GetValueOrDefault();
                var list = _agentInstances.Get(agentInstanceId);
                if (list.State == ContextPartitionState.STARTED)
                {
                    continue;
                }
                list.State = ContextPartitionState.STARTED;
                entry.Value.State = ContextPartitionState.STARTED;
                foreach (var statement in _statements)
                {
                    var instance = StartStatement(agentInstanceId, statement.Value, _rootContext, list.InitPartitionKey, list.InitContextProperties, false);
                    list.AgentInstances.Add(instance);
                }
                _rootContext.Factory.FactoryContext.StateCache.UpdateContextPath(_contextName, entry.Key, entry.Value);
            }
            SetState(states.ContextPartitionInformation, ContextPartitionState.STARTED);
            return states.ContextPartitionInformation;
        }

        public void ImportStartPaths(ContextControllerState state, AgentInstanceSelector agentInstanceSelector)
        {
            _rootContext.ImportContextPartitions(state, 0, null, agentInstanceSelector);
        }

        public bool HandleFilterFault(EventBean theEvent, long version)
        {
            using (_uLock.Acquire())
            {
                StatementAgentInstanceUtil.HandleFilterFault(theEvent, version, _servicesContext, _agentInstances);
                return false;
            }
        }

        private void Activate()
        {
            _rootContext.Activate(null, null, null, null, null);
        }

        private AgentInstance[] GetAgentInstancesForStmt(int statementId, ContextPartitionSelector selector)
        {
            var agentInstanceIds = GetAgentInstanceIds(selector);
            if (agentInstanceIds == null || agentInstanceIds.IsEmpty())
            {
                return new AgentInstance[0];
            }

            IList<AgentInstance> instances = new List<AgentInstance>(agentInstanceIds.Count);
            foreach (var agentInstanceId in agentInstanceIds)
            {
                var instancesList = _agentInstances.Get(agentInstanceId);
                if (instancesList != null)
                {
                    foreach (var instance in instancesList.AgentInstances)
                    {
                        if (instance.AgentInstanceContext.StatementContext.StatementId == statementId)
                        {
                            instances.Add(instance);
                        }
                    }
                }
            }
            return instances.ToArray();
        }

        private AgentInstance[] GetAgentInstancesForStmt(int statementId)
        {
            IList<AgentInstance> instances = new List<AgentInstance>();
            foreach (var contextPartitionEntry in _agentInstances)
            {
                foreach (var instance in contextPartitionEntry.Value.AgentInstances)
                {
                    if (instance.AgentInstanceContext.StatementContext.StatementId == statementId)
                    {
                        instances.Add(instance);
                    }
                }
            }
            return instances.ToArray();
        }

        private void RemoveStatement(int statementId)
        {
            var statementDesc = _statements.Get(statementId);
            if (statementDesc == null)
            {
                return;
            }

            foreach (var contextPartitionEntry in _agentInstances)
            {
                var instanceList = contextPartitionEntry.Value.AgentInstances
                    .Where(instance => instance.AgentInstanceContext.StatementContext.StatementId == statementId)
                    .ToList();

                instanceList.ForEach(
                    instance =>
                    {
                        StatementAgentInstanceUtil.Stop(
                            instance.StopCallback, instance.AgentInstanceContext, instance.FinalView, _servicesContext,
                            true, false, true);
                        contextPartitionEntry.Value.AgentInstances.Remove(instance);
                    });
            }

            _statements.Remove(statementId);
        }

        private AgentInstance StartStatement(int contextId, ContextControllerStatementDesc statementDesc, ContextController originator, Object partitionKey, IDictionary<String, Object> contextProperties, bool isRecoveringResilient)
        {
            // build filters
            var filterAddendum = new IdentityDictionary<FilterSpecCompiled, FilterValueSetParam[][]>();
            originator.Factory.PopulateFilterAddendums(filterAddendum, statementDesc, partitionKey, contextId);
            AgentInstanceFilterProxy proxy = new AgentInstanceFilterProxyImpl(filterAddendum);

            // build built-in context properties
            contextProperties.Put(ContextPropertyEventType.PROP_CTX_NAME, _contextName);
            contextProperties.Put(ContextPropertyEventType.PROP_CTX_ID, contextId);
            var contextBean = (MappedEventBean)_servicesContext.EventAdapterService.AdapterForTypedMap(contextProperties, _contextDescriptor.ContextPropertyRegistry.ContextEventType);

            // activate
            var result = StatementAgentInstanceUtil.Start(_servicesContext, statementDesc.Statement, false, contextId, contextBean, proxy, isRecoveringResilient);

            // save only instance data
            return new AgentInstance(result.StopCallback, result.AgentInstanceContext, result.FinalView);
        }

        internal static void SetState(IDictionary<int, ContextPartitionDescriptor> original, ContextPartitionState state)
        {
            foreach (var entry in original)
            {
                entry.Value.State = state;
            }
        }

        public class ContextNestedHandleImpl : ContextControllerInstanceHandle
        {
            public ContextNestedHandleImpl(int subPathId, int contextPartitionId, ContextControllerTreeAgentInstanceList instances)
            {
                SubPathId = subPathId;
                ContextPartitionOrPathId = contextPartitionId;
                Instances = instances;
            }

            public int ContextPartitionOrPathId { get; private set; }

            public ContextControllerTreeAgentInstanceList Instances { get; private set; }

            public int SubPathId { get; private set; }
        }
    }
}
