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
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.client.context;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.context.stmt;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.spec;
using com.espertech.esper.events;
using com.espertech.esper.filter;

namespace com.espertech.esper.core.context.mgr
{
    public class ContextManagerNested
        : ContextManager
        , ContextControllerLifecycleCallback
        , ContextEnumeratorHandler
        , FilterFaultHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ILockable _iLock;

        private readonly String _contextName;
        private readonly EPServicesContext _servicesContext;

        private readonly ContextControllerFactory[] _nestedContextFactories;

        private readonly IDictionary<int, ContextControllerStatementDesc> _statements =
            new LinkedHashMap<int, ContextControllerStatementDesc>(); // retain order of statement creation

        private readonly ContextDescriptor _contextDescriptor;

        /// <summary>
        /// The single root context. This represents the context declared first.
        /// </summary>
        private ContextController _rootContext;

        /// <summary>
        /// Double-linked tree of sub-contexts. An entry exists for all branches including the root. For example 
        /// with 2 contexts declared this map has entries representing the root and all second-level sub-contexts.
        /// For example with 3 contexts declared this map has entries for the root, second and third-level contexts.
        /// </summary>
        private readonly IDictionary<ContextController, ContextControllerTreeEntry> _subcontexts =
            new Dictionary<ContextController, ContextControllerTreeEntry>().WithNullSupport();

        private readonly ContextPartitionIdManager _contextPartitionIdManager;

        public ContextManagerNested(
            ILockManager lockManager,
            ContextControllerFactoryServiceContext factoryServiceContext)
        {
            _iLock = lockManager.CreateLock(GetType());
            _contextName = factoryServiceContext.ContextName;
            _servicesContext = factoryServiceContext.ServicesContext;
            _contextPartitionIdManager = factoryServiceContext.AgentInstanceContextCreate.StatementContext.ContextControllerFactoryService.AllocatePartitionIdMgr(
                _contextName, factoryServiceContext.AgentInstanceContextCreate.StatementContext.StatementId);
            _nestedContextFactories = factoryServiceContext.AgentInstanceContextCreate.StatementContext.ContextControllerFactoryService.GetFactory(
                factoryServiceContext);

            StatementAIResourceRegistryFactory resourceRegistryFactory =
                () => new StatementAIResourceRegistry(new AIRegistryAggregationMap(), new AIRegistryExprMap());

            var contextProps = ContextPropertyEventType.GetNestedTypeBase();
            foreach (var factory in _nestedContextFactories)
            {
                contextProps.Put(factory.FactoryContext.ContextName, factory.ContextBuiltinProps);
            }
            var contextPropsType = _servicesContext.EventAdapterService.CreateAnonymousMapType(
                _contextName, contextProps, true);
            var registry = new ContextPropertyRegistryImpl(
                Collections.GetEmptyList<ContextDetailPartitionItem>(), contextPropsType);
            _contextDescriptor = new ContextDescriptor(
                _contextName, false, registry, resourceRegistryFactory, this, factoryServiceContext.Detail);
        }

        public IDictionary<int, ContextControllerStatementDesc> Statements
        {
            get { return _statements; }
        }

        public ContextDescriptor ContextDescriptor
        {
            get { return _contextDescriptor; }
        }

        public int NumNestingLevels
        {
            get { return _nestedContextFactories.Length; }
        }

        public IEnumerator<EventBean> GetEnumerator(int statementId, ContextPartitionSelector selector)
        {
            using (_iLock.Acquire())
            {
                var instances = GetAgentInstancesForStmt(statementId, selector);
                return instances.SelectMany(instance => instance.FinalView).GetEnumerator();
            }
        }

        public IEnumerator<EventBean> GetSafeEnumerator(int statementId, ContextPartitionSelector selector)
        {
            using (_iLock.Acquire())
            {
                var instances = GetAgentInstancesForStmt(statementId, selector);
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

        public ICollection<int> GetAgentInstanceIds(ContextPartitionSelector contextPartitionSelector)
        {
            return GetAgentInstancesForSelector(contextPartitionSelector);
        }

        public void ImportStartPaths(ContextControllerState state, AgentInstanceSelector agentInstanceSelector)
        {
            _rootContext.ImportContextPartitions(state, 0, null, agentInstanceSelector);
        }

        internal static ContextPartitionIdentifier[] GetTreeCompositeKey(
            ContextControllerFactory[] nestedContextFactories,
            Object initPartitionKey,
            ContextControllerTreeEntry treeEntry,
            IDictionary<ContextController, ContextControllerTreeEntry> subcontexts)
        {
            var length = nestedContextFactories.Length;
            var keys = new ContextPartitionIdentifier[length];
            keys[length - 1] = nestedContextFactories[length - 1].KeyPayloadToIdentifier(initPartitionKey);
            keys[length - 2] = nestedContextFactories[length - 2].KeyPayloadToIdentifier(treeEntry.InitPartitionKey);

            // get parent's parent
            if (length > 2)
            {
                var parent = treeEntry.Parent;
                var parentEntry = subcontexts.Get(parent);
                for (var i = 0; i < length - 2; i++)
                {
                    keys[length - 2 - i] =
                        nestedContextFactories[length - 2 - i].KeyPayloadToIdentifier(parentEntry.InitPartitionKey);
                    parent = parentEntry.Parent;
                    parentEntry = subcontexts.Get(parent);
                }
            }

            return keys;
        }

        public ContextStatePathDescriptor ExtractPaths(ContextPartitionSelector selector)
        {
            var visitor = GetContextPartitionPathsInternal(selector);
            return new ContextStatePathDescriptor(visitor.States, visitor.AgentInstanceInfo);
        }

        public ContextStatePathDescriptor ExtractStopPaths(ContextPartitionSelector selector)
        {
            var visitor = GetContextPartitionPathsInternal(selector);
            foreach (var entry in visitor.ControllerAgentInstances)
            {
                var treeEntry = _subcontexts.Get(entry.Key);
                foreach (var leaf in entry.Value)
                {
                    var agentInstanceId = leaf.Value.OptionalContextPartitionId.GetValueOrDefault();
                    var list = treeEntry.AgentInstances.Get(agentInstanceId);
                    list.State = ContextPartitionState.STOPPED;
                    StatementAgentInstanceUtil.StopAgentInstances(
                        list.AgentInstances, null, _servicesContext, false, false);
                    list.ClearAgentInstances();
                    leaf.Value.State = ContextPartitionState.STOPPED;
                    _rootContext.Factory.FactoryContext.StateCache.UpdateContextPath(_contextName, leaf.Key, leaf.Value);
                }
            }
            return new ContextStatePathDescriptor(visitor.States, visitor.AgentInstanceInfo);
        }

        public ContextStatePathDescriptor ExtractDestroyPaths(ContextPartitionSelector selector)
        {
            var visitor = GetContextPartitionPathsInternal(selector);
            foreach (var entry in visitor.ControllerAgentInstances)
            {
                var treeEntry = _subcontexts.Get(entry.Key);
                foreach (var leaf in entry.Value)
                {
                    var agentInstanceId = leaf.Value.OptionalContextPartitionId.GetValueOrDefault();
                    var list = treeEntry.AgentInstances.Get(agentInstanceId);
                    StatementAgentInstanceUtil.StopAgentInstances(
                        list.AgentInstances, null, _servicesContext, false, false);
                    _rootContext.Factory.FactoryContext.StateCache.RemoveContextPath(
                        _contextName, leaf.Key.Level, leaf.Key.ParentPath, leaf.Key.SubPath);
                    var descriptor = visitor.AgentInstanceInfo.Get(agentInstanceId);
                    var nestedIdent = (ContextPartitionIdentifierNested)descriptor.Identifier;
                    entry.Key.DeletePath(nestedIdent.Identifiers[_nestedContextFactories.Length - 1]);
                }
            }
            return new ContextStatePathDescriptor(visitor.States, visitor.AgentInstanceInfo);
        }

        public IDictionary<int, ContextPartitionDescriptor> StartPaths(ContextPartitionSelector selector)
        {
            var visitor = GetContextPartitionPathsInternal(selector);
            foreach (var entry in visitor.ControllerAgentInstances)
            {
                var treeEntry = _subcontexts.Get(entry.Key);

                foreach (var leaf in entry.Value)
                {
                    int agentInstanceId = leaf.Value.OptionalContextPartitionId.GetValueOrDefault();
                    var list = treeEntry.AgentInstances.Get(agentInstanceId);
                    if (list.State == ContextPartitionState.STARTED)
                    {
                        continue;
                    }
                    foreach (var statement in _statements)
                    {
                        var instance = StartStatement(
                            agentInstanceId, statement.Value, _rootContext, list.InitPartitionKey,
                            list.InitContextProperties, false);
                        list.AgentInstances.Add(instance);
                    }
                    list.State = ContextPartitionState.STARTED;
                    leaf.Value.State = ContextPartitionState.STARTED;
                    _rootContext.Factory.FactoryContext.StateCache.UpdateContextPath(_contextName, leaf.Key, leaf.Value);
                }
            }
            ContextManagerImpl.SetState(visitor.AgentInstanceInfo, ContextPartitionState.STARTED);
            return visitor.AgentInstanceInfo;
        }

        public ContextPartitionVisitorStateWithPath GetContextPartitionPathsInternal(ContextPartitionSelector selector)
        {
            var visitor = new ContextPartitionVisitorStateWithPath(_nestedContextFactories, _subcontexts);
            IList<ContextPartitionSelector[]> selectors;
            if (selector is ContextPartitionSelectorNested)
            {
                var nested = (ContextPartitionSelectorNested)selector;
                selectors = nested.Selectors;
            }
            else if (selector is ContextPartitionSelectorAll)
            {
                var all = new ContextPartitionSelector[NumNestingLevels];
                all.Fill(selector);
                selectors = Collections.SingletonList(all);
            }
            else
            {
                throw new ArgumentException("Invalid selector for nested context");
            }
            foreach (var item in selectors)
            {
                RecursivePopulateSelector(_rootContext, 1, item, visitor);
            }
            return visitor;
        }

        public void AddStatement(ContextControllerStatementBase statement, bool isRecoveringResilient)
        {
            // validation down the hierarchy
            var caches = new ContextControllerStatementCtxCache[_nestedContextFactories.Length];
            for (var i = 0; i < _nestedContextFactories.Length; i++)
            {
                var nested = _nestedContextFactories[i];
                caches[i] = nested.ValidateStatement(statement);
            }

            // save statement
            var desc = new ContextControllerStatementDesc(statement, caches);
            _statements.Put(statement.StatementContext.StatementId, desc);

            // activate if this is the first statement
            if (_statements.Count == 1)
            {
                Activate(); // this may itself trigger a callback
            }
            // activate statement in respect to existing context partitions
            else
            {
                foreach (var subcontext in _subcontexts)
                {
                    if (subcontext.Key.Factory.FactoryContext.NestingLevel != _nestedContextFactories.Length)
                    {
                        continue;
                    }
                    if (subcontext.Value.AgentInstances == null || subcontext.Value.AgentInstances.IsEmpty())
                    {
                        continue;
                    }

                    foreach (var entry in subcontext.Value.AgentInstances)
                    {
                        if (entry.Value.State == ContextPartitionState.STARTED)
                        {
                            var agentInstance = StartStatement(
                                entry.Key, desc, subcontext.Key, entry.Value.InitPartitionKey,
                                entry.Value.InitContextProperties, isRecoveringResilient);
                            entry.Value.AgentInstances.Add(agentInstance);
                        }
                    }
                }
            }
        }

        public void StopStatement(String statementName, int statementId)
        {
            using (_iLock.Acquire())
            {
                DestroyStatement(statementName, statementId);
            }
        }

        public void DestroyStatement(String statementName, int statementId)
        {
            using (_iLock.Acquire())
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
                RecursiveDeactivateStop(_rootContext, false, null);
                _nestedContextFactories[0].FactoryContext.StateCache.RemoveContext(_contextName);
                _rootContext = null;
                _statements.Clear();
                _subcontexts.Clear();
                _contextPartitionIdManager.Clear();
            }
        }

        public FilterSpecLookupable GetFilterLookupable(EventType eventType)
        {
            throw new UnsupportedOperationException();
        }

        public void ContextPartitionNavigate(
            ContextControllerInstanceHandle existingHandle,
            ContextController originator,
            ContextControllerState controllerState,
            int exportedCPOrPathId,
            ContextInternalFilterAddendum filterAddendum,
            AgentInstanceSelector agentInstanceSelector,
            byte[] payload,
            bool isRecoveringResilient)
        {
            var nestedHandle = (ContextManagerNestedInstanceHandle)existingHandle;

            // detect non-leaf
            var nestingLevel = originator.Factory.FactoryContext.NestingLevel; // starts at 1 for root
            if (nestingLevel < _nestedContextFactories.Length)
            {
                nestedHandle.Controller.ImportContextPartitions(
                    controllerState, exportedCPOrPathId, filterAddendum, agentInstanceSelector);
                return;
            }

            var entry = _subcontexts.Get(originator);
            if (entry == null)
            {
                return;
            }
            foreach (var cpEntry in entry.AgentInstances.ToArray())
            {
                if (cpEntry.Value.State == ContextPartitionState.STOPPED)
                {
                    cpEntry.Value.State = ContextPartitionState.STARTED;
                    entry.AgentInstances.Clear();
                    foreach (var statement in _statements)
                    {
                        var instance = StartStatement(
                            existingHandle.ContextPartitionOrPathId, statement.Value, originator,
                            cpEntry.Value.InitPartitionKey, entry.InitContextProperties, false);
                        cpEntry.Value.AgentInstances.Add(instance);
                    }
                    var key = new ContextStatePathKey(
                        _nestedContextFactories.Length, originator.PathId, existingHandle.SubPathId);
                    var value = new ContextStatePathValue(
                        existingHandle.ContextPartitionOrPathId, payload, ContextPartitionState.STARTED);
                    originator.Factory.FactoryContext.StateCache.UpdateContextPath(_contextName, key, value);
                }
                else
                {
                    IList<AgentInstance> removed = new List<AgentInstance>(2);
                    IList<AgentInstance> added = new List<AgentInstance>(2);
                    var current = cpEntry.Value.AgentInstances;

                    foreach (var agentInstance in current)
                    {
                        if (!agentInstanceSelector.Select(agentInstance))
                        {
                            continue;
                        }

                        // remove
                        StatementAgentInstanceUtil.StopAgentInstanceRemoveResources(
                            agentInstance, null, _servicesContext, false, false);
                        removed.Add(agentInstance);

                        // start
                        var statementDesc = _statements.Get(agentInstance.AgentInstanceContext.StatementId);
                        var instance = StartStatement(
                            cpEntry.Key, statementDesc, originator, cpEntry.Value.InitPartitionKey,
                            entry.InitContextProperties, false);
                        added.Add(instance);

                        if (controllerState.PartitionImportCallback != null)
                        {
                            controllerState.PartitionImportCallback.Existing(
                                existingHandle.ContextPartitionOrPathId, exportedCPOrPathId);
                        }
                    }
                    current.RemoveAll(removed);
                    current.AddAll(added);
                }
            }
        }

        public ContextControllerInstanceHandle ContextPartitionInstantiate(
            int? optionalContextPartitionId,
            int subPathId,
            int? importSubpathId,
            ContextController originator,
            EventBean optionalTriggeringEvent,
            IDictionary<String, Object> optionalTriggeringPattern,
            Object partitionKey,
            IDictionary<String, Object> contextProperties,
            ContextControllerState states,
            ContextInternalFilterAddendum filterAddendum,
            bool isRecoveringResilient,
            ContextPartitionState state)
        {
            using (_iLock.Acquire())
            {
                ContextControllerTreeEntry entry;

                // detect non-leaf
                var nestingLevel = originator.Factory.FactoryContext.NestingLevel; // starts at 1 for root
                if (nestingLevel < _nestedContextFactories.Length)
                {
                    // next sub-sontext
                    var nextFactory = _nestedContextFactories[originator.Factory.FactoryContext.NestingLevel];
                    var nextContext = nextFactory.CreateNoCallback(subPathId, this);

                    // link current context to sub-context
                    var branch = _subcontexts.Get(originator);
                    if (branch.ChildContexts == null)
                    {
                        branch.ChildContexts = new Dictionary<int, ContextController>();
                    }
                    branch.ChildContexts.Put(subPathId, nextContext);

                    // save child branch, linking sub-context to its parent
                    entry = new ContextControllerTreeEntry(originator, null, partitionKey, contextProperties);
                    _subcontexts.Put(nextContext, entry);

                    // now post-initialize, this may actually call back
                    nextContext.Activate(
                        optionalTriggeringEvent, optionalTriggeringPattern, states, filterAddendum, importSubpathId);

                    if (Log.IsDebugEnabled)
                    {
                        Log.Debug(
                            "Instantiating branch context path for " + _contextName +
                            " from level " + originator.Factory.FactoryContext.NestingLevel +
                            "(" + originator.Factory.FactoryContext.ContextName + ")" +
                            " parentPath " + originator.PathId +
                            " for level " + nextContext.Factory.FactoryContext.NestingLevel +
                            "(" + nextContext.Factory.FactoryContext.ContextName + ")" +
                            " childPath " + subPathId
                            );
                    }

                    return new ContextManagerNestedInstanceHandle(subPathId, nextContext, subPathId, true, null);
                }

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

                if (Log.IsDebugEnabled)
                {
                    Log.Debug(
                        "Instantiating agent instance for " + _contextName +
                        " from level " + originator.Factory.FactoryContext.NestingLevel +
                        "(" + originator.Factory.FactoryContext.ContextName + ")" +
                        " parentPath " + originator.PathId +
                        " contextPartId " + assignedContextId);
                }

                // handle leaf creation
                IList<AgentInstance> newInstances = new List<AgentInstance>();
                if (state == ContextPartitionState.STARTED)
                {
                    foreach (var statementEntry in _statements)
                    {
                        var statementDesc = statementEntry.Value;
                        var instance = StartStatement(
                            assignedContextId, statementDesc, originator, partitionKey, contextProperties,
                            isRecoveringResilient);
                        newInstances.Add(instance);
                    }
                }

                // for all new contexts: evaluate this event for this statement
                if (optionalTriggeringEvent != null)
                {
                    StatementAgentInstanceUtil.EvaluateEventForStatement(
                        _servicesContext, optionalTriggeringEvent, optionalTriggeringPattern, newInstances);
                }

                // save leaf
                entry = _subcontexts.Get(originator);
                if (entry.AgentInstances == null)
                {
                    entry.AgentInstances = new LinkedHashMap<int, ContextControllerTreeAgentInstanceList>();
                }

                var filterVersion = _servicesContext.FilterService.FiltersVersion;
                var agentInstanceList = new ContextControllerTreeAgentInstanceList(
                    filterVersion, partitionKey, contextProperties, newInstances, state);
                entry.AgentInstances.Put(assignedContextId, agentInstanceList);

                return new ContextManagerNestedInstanceHandle(
                    subPathId, originator, assignedContextId, false, agentInstanceList);
            }
        }

        public virtual bool HandleFilterFault(EventBean theEvent, long version)
        {
            using (_iLock.Acquire())
            {
                foreach (var entry in _subcontexts)
                {
                    if (entry.Value.AgentInstances != null)
                    {
                        StatementAgentInstanceUtil.HandleFilterFault(
                            theEvent, version, _servicesContext, entry.Value.AgentInstances);
                    }
                }

                return false;
            }
        }

        public ContextStateCache ContextStateCache
        {
            get { return _rootContext.Factory.StateCache; }
        }

        /// <summary>
        /// Provides the sub-context that ends.
        /// </summary>
        /// <param name="contextNestedHandle">The context nested handle.</param>
        /// <param name="terminationProperties">The termination properties.</param>
        /// <param name="leaveLocksAcquired">if set to <c>true</c> [leave locks acquired].</param>
        /// <param name="agentInstances">The agent instances.</param>
        public void ContextPartitionTerminate(
            ContextControllerInstanceHandle contextNestedHandle,
            IDictionary<String, Object> terminationProperties,
            bool leaveLocksAcquired,
            IList<AgentInstance> agentInstances)
        {
            var handle = (ContextManagerNestedInstanceHandle)contextNestedHandle;
            if (handle.IsBranch)
            {
                var branchHandle = handle;
                var branch = branchHandle.Controller;
                RecursiveDeactivateStop(branch, leaveLocksAcquired, agentInstances);

                if (Log.IsDebugEnabled)
                {
                    Log.Debug(
                        "Terminated context branch for " + _contextName +
                        " from level " + branch.Factory.FactoryContext.NestingLevel +
                        "(" + branch.Factory.FactoryContext.ContextName + ")" +
                        " parentPath " + branch.PathId);
                }
            }
            else
            {
                var leafHandle = handle;
                var leaf = leafHandle.Controller;
                var leafEntry = _subcontexts.Get(leaf);
                if (leafEntry != null)
                {
                    // could be terminated earlier
                    var ailist = leafEntry.AgentInstances.Get(leafHandle.ContextPartitionOrPathId);
                    if (ailist != null)
                    {
                        StatementAgentInstanceUtil.StopAgentInstances(
                            ailist.AgentInstances, null, _servicesContext, false, false);
                        _contextPartitionIdManager.RemoveId(leafHandle.ContextPartitionOrPathId);
                        ailist.AgentInstances.Clear();
                    }

                    if (Log.IsDebugEnabled)
                    {
                        Log.Debug(
                            "Terminated context leaf for " + _contextName +
                            " from level " + leaf.Factory.FactoryContext.NestingLevel +
                            "(" + leaf.Factory.FactoryContext.ContextName + ")" +
                            " parentPath " + leaf.PathId +
                            " contextPartId " + leafHandle.ContextPartitionOrPathId);
                    }
                }
            }
        }

        public IEnumerator<EventBean> GetEnumerator(int statementId)
        {
            using (_iLock.Acquire())
            {
                var instances = GetAgentInstancesForStmt(statementId);
                foreach (var instance in instances)
                {
                    foreach (var eventBean in instance.FinalView)
                    {
                        yield return eventBean;
                    }
                }
            }
        }

        public IEnumerator<EventBean> GetSafeEnumerator(int statementId)
        {
            using (_iLock.Acquire())
            {
                var instances = GetAgentInstancesForStmt(statementId);

                foreach (var instance in instances)
                {
                    using (instance.AgentInstanceContext.EpStatementAgentInstanceHandle.StatementAgentInstanceLock.AcquireWriteLock())
                    {
                        foreach (var eventBean in instance.FinalView)
                        {
                            yield return eventBean;
                        }
                    }
                }
            }
        }

        private AgentInstance StartStatement(
            int contextId,
            ContextControllerStatementDesc statementDesc,
            ContextController originator,
            Object partitionKey,
            IDictionary<String, Object> contextProperties,
            bool isRecoveringResilient)
        {
            // build filters
            var proxy = GetMergedFilterAddendums(statementDesc, originator, partitionKey, contextId);

            // build built-in context properties
            var properties = ContextPropertyEventType.GetNestedBeanBase(_contextName, contextId);
            properties.Put(
                _nestedContextFactories[_nestedContextFactories.Length - 1].FactoryContext.ContextName,
                contextProperties);
            RecursivePopulateBuiltinProps(originator, properties);
            properties.Put(ContextPropertyEventType.PROP_CTX_NAME, _contextName);
            properties.Put(ContextPropertyEventType.PROP_CTX_ID, contextId);
            var contextBean =
                (MappedEventBean)
                    _servicesContext.EventAdapterService.AdapterForTypedMap(
                        properties, _contextDescriptor.ContextPropertyRegistry.ContextEventType);

            // activate
            var result = StatementAgentInstanceUtil.Start(
                _servicesContext, statementDesc.Statement, false, contextId, contextBean, proxy, isRecoveringResilient);
            return new AgentInstance(result.StopCallback, result.AgentInstanceContext, result.FinalView);
        }

        private void RecursivePopulateBuiltinProps(ContextController originator, IDictionary<String, Object> properties)
        {
            var entry = _subcontexts.Get(originator);
            if (entry != null)
            {
                if (entry.InitContextProperties != null)
                {
                    properties.Put(entry.Parent.Factory.FactoryContext.ContextName, entry.InitContextProperties);
                }
                if (entry.Parent != null && entry.Parent.Factory.FactoryContext.NestingLevel > 1)
                {
                    RecursivePopulateBuiltinProps(entry.Parent, properties);
                }
            }
        }

        private AgentInstanceFilterProxy GetMergedFilterAddendums(
            ContextControllerStatementDesc statement,
            ContextController originator,
            Object partitionKey,
            int contextId)
        {
            var result = new IdentityDictionary<FilterSpecCompiled, FilterValueSetParam[][]>();
            originator.Factory.PopulateFilterAddendums(result, statement, partitionKey, contextId);
            var originatorEntry = _subcontexts.Get(originator);
            if (originatorEntry != null)
            {
                RecursivePopulateFilterAddendum(statement, originatorEntry, contextId, result);
            }
            return new AgentInstanceFilterProxyImpl(result);
        }

        private void RecursivePopulateFilterAddendum(
            ContextControllerStatementDesc statement,
            ContextControllerTreeEntry originatorEntry,
            int contextId,
            IdentityDictionary<FilterSpecCompiled, FilterValueSetParam[][]> result)
        {
            if (originatorEntry.Parent == null)
            {
                return;
            }
            originatorEntry.Parent.Factory.PopulateFilterAddendums(
                result, statement, originatorEntry.InitPartitionKey, contextId);

            var parentEntry = _subcontexts.Get(originatorEntry.Parent);
            if (parentEntry != null)
            {
                RecursivePopulateFilterAddendum(statement, parentEntry, contextId, result);
            }
        }

        private void Activate()
        {
            _rootContext = _nestedContextFactories[0].CreateNoCallback(0, this);
            _subcontexts.Put(_rootContext, new ContextControllerTreeEntry(null, null, null, null));
            _rootContext.Activate(null, null, null, null, null);
        }

        private void RemoveStatement(int statementId)
        {
            var statementDesc = _statements.Get(statementId);
            if (statementDesc == null)
            {
                return;
            }

            foreach (var entry in _subcontexts)
            {
                // ignore branches
                if (entry.Key.Factory.FactoryContext.NestingLevel < _nestedContextFactories.Length)
                {
                    continue;
                }
                if (entry.Value.AgentInstances == null || entry.Value.AgentInstances.IsEmpty())
                {
                    continue;
                }

                foreach (var contextPartitionEntry in entry.Value.AgentInstances)
                {
                    var agentInstances = contextPartitionEntry.Value.AgentInstances;

                    for (int ii = agentInstances.Count - 1; ii >= 0; ii--)
                    {
                        var instance = agentInstances[ii];
                        if (instance.AgentInstanceContext.StatementContext.StatementId != statementId)
                        {
                            continue;
                        }
                        StatementAgentInstanceUtil.Stop(
                            instance.StopCallback, instance.AgentInstanceContext, instance.FinalView, _servicesContext,
                            true, false, true);
                        agentInstances.RemoveAt(ii);
                    }
                }
            }

            _statements.Remove(statementId);
        }

        private void RecursiveDeactivateStop(
            ContextController currentContext,
            bool leaveLocksAcquired,
            IList<AgentInstance> agentInstancesCollected)
        {
            // deactivate
            currentContext.Deactivate();

            // remove state
            var entry = _subcontexts.Delete(currentContext);
            if (entry == null)
            {
                return;
            }

            // remove from parent
            var parent = _subcontexts.Get(entry.Parent);
            if (parent != null)
            {
                parent.ChildContexts.Remove(currentContext.PathId);
            }

            // stop instances
            if (entry.AgentInstances != null)
            {
                foreach (var entryCP in entry.AgentInstances)
                {
                    StatementAgentInstanceUtil.StopAgentInstances(
                        entryCP.Value.AgentInstances, null, _servicesContext, false, leaveLocksAcquired);
                    if (agentInstancesCollected != null)
                    {
                        agentInstancesCollected.AddAll(entryCP.Value.AgentInstances);
                    }
                    _contextPartitionIdManager.RemoveId(entryCP.Key);
                }
            }

            // deactivate child contexts
            if (entry.ChildContexts == null || entry.ChildContexts.IsEmpty())
            {
                return;
            }
            foreach (ContextController inner in entry.ChildContexts.Values)
            {
                RecursiveDeactivateStop(inner, leaveLocksAcquired, agentInstancesCollected);
            }
        }

        private AgentInstance[] GetAgentInstancesForStmt(int statementId)
        {
            var instances = new List<AgentInstance>();
            foreach (var subcontext in _subcontexts)
            {
                if (subcontext.Key.Factory.FactoryContext.NestingLevel != _nestedContextFactories.Length)
                {
                    continue;
                }
                if (subcontext.Value.AgentInstances == null || subcontext.Value.AgentInstances.IsEmpty())
                {
                    continue;
                }

                foreach (var entry in subcontext.Value.AgentInstances)
                {
                    foreach (var ai in entry.Value.AgentInstances)
                    {
                        if (ai.AgentInstanceContext.StatementContext.StatementId == statementId)
                        {
                            instances.Add(ai);
                        }
                    }
                }
            }
            return instances.ToArray();
        }

        private AgentInstance[] GetAgentInstancesForStmt(int statementId, ContextPartitionSelector selector)
        {
            var agentInstanceIds = GetAgentInstancesForSelector(selector);
            if (agentInstanceIds == null || agentInstanceIds.IsEmpty())
            {
                return new AgentInstance[0];
            }

            IList<AgentInstance> instances = new List<AgentInstance>(agentInstanceIds.Count);
            foreach (var subcontext in _subcontexts)
            {
                if (subcontext.Key.Factory.FactoryContext.NestingLevel != _nestedContextFactories.Length)
                {
                    continue;
                }
                if (subcontext.Value.AgentInstances == null || subcontext.Value.AgentInstances.IsEmpty())
                {
                    continue;
                }

                foreach (int agentInstanceId in agentInstanceIds)
                {
                    var instancesList = subcontext.Value.AgentInstances.Get(agentInstanceId);
                    if (instancesList != null)
                    {
                        var instanceIt = instancesList.AgentInstances.GetEnumerator();
                        while (instanceIt.MoveNext())
                        {
                            var instance = instanceIt.Current;
                            if (instance.AgentInstanceContext.StatementContext.StatementId == statementId)
                            {
                                instances.Add(instance);
                            }
                        }
                    }
                }
            }
            return instances.ToArray();
        }

        private ICollection<int> GetAgentInstancesForSelector(ContextPartitionSelector selector)
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
                agentInstanceIds.RetainAll(_contextPartitionIdManager.Ids);
                return agentInstanceIds;
            }
            else if (selector is ContextPartitionSelectorAll)
            {
                return new List<int>(_contextPartitionIdManager.Ids);
            }
            else if (selector is ContextPartitionSelectorNested)
            {
                var nested = (ContextPartitionSelectorNested)selector;
                var visitor = new ContextPartitionVisitorAgentInstanceIdWPath(_nestedContextFactories.Length);
                foreach (var item in nested.Selectors)
                {
                    RecursivePopulateSelector(_rootContext, 1, item, visitor);
                }
                return visitor.AgentInstanceIds;
            }
            throw ContextControllerSelectorUtil.GetInvalidSelector(
                new Type[]
                {
                    typeof (ContextPartitionSelectorNested)
                }, selector, true);
        }

        private void RecursivePopulateSelector(
            ContextController currentContext,
            int level,
            ContextPartitionSelector[] selectorStack,
            ContextPartitionVisitorWithPath visitor)
        {
            var entry = _subcontexts.Get(currentContext);
            if (entry == null)
            {
                return;
            }
            var selector = selectorStack[level - 1];

            // handle branch
            if (level < _nestedContextFactories.Length)
            {
                visitor.ResetSubPaths();
                currentContext.VisitSelectedPartitions(selector, visitor);
                var selectedPaths = new List<int>(visitor.Subpaths);
                foreach (int path in selectedPaths)
                {
                    var controller = entry.ChildContexts.Get(path);
                    if (controller != null)
                    {
                        RecursivePopulateSelector(controller, level + 1, selectorStack, visitor);
                    }
                }
            }
            // handle leaf
            else
            {
                currentContext.VisitSelectedPartitions(selector, visitor);
            }
        }
    }
}