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
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.type;

namespace com.espertech.esper.core.context.mgr
{
    public class ContextControllerHash
        : ContextController
        , ContextControllerHashedInstanceCallback
    {
        private readonly int _pathId;
        private readonly ContextControllerLifecycleCallback _activationCallback;
        private readonly ContextControllerHashFactoryImpl _factory;

        private readonly IList<ContextControllerHashedFilterCallback> _filterCallbacks = new List<ContextControllerHashedFilterCallback>();
        private readonly IDictionary<int, ContextControllerInstanceHandle> _partitionKeys = new LinkedHashMap<int, ContextControllerInstanceHandle>();

        private ContextInternalFilterAddendum _activationFilterAddendum;
        private int _currentSubpathId;

        public ContextControllerHash(int pathId, ContextControllerLifecycleCallback activationCallback, ContextControllerHashFactoryImpl factory)
        {
            _pathId = pathId;
            _activationCallback = activationCallback;
            _factory = factory;
        }

        public void ImportContextPartitions(ContextControllerState state, int pathIdToUse, ContextInternalFilterAddendum filterAddendum, AgentInstanceSelector agentInstanceSelector)
        {
            InitializeFromState(null, null, state, pathIdToUse, agentInstanceSelector, true);
        }

        public void DeletePath(ContextPartitionIdentifier identifier)
        {
            var hash = (ContextPartitionIdentifierHash)identifier;
            _partitionKeys.Remove(hash.Hash);
        }

        public void VisitSelectedPartitions(ContextPartitionSelector contextPartitionSelector, ContextPartitionVisitor visitor)
        {
            int nestingLevel = _factory.FactoryContext.NestingLevel;

            if (contextPartitionSelector is ContextPartitionSelectorHash)
            {
                var hash = (ContextPartitionSelectorHash)contextPartitionSelector;
                if (hash.Hashes == null || hash.Hashes.IsEmpty())
                {
                    return;
                }
                foreach (int hashCode in hash.Hashes)
                {
                    var handle = _partitionKeys.Get(hashCode);
                    if (handle != null)
                    {
                        visitor.Visit(nestingLevel, _pathId, _factory.Binding, hashCode, this, handle);
                    }
                }
                return;
            }
            if (contextPartitionSelector is ContextPartitionSelectorFiltered)
            {
                var filter = (ContextPartitionSelectorFiltered)contextPartitionSelector;
                var identifierHash = new ContextPartitionIdentifierHash();
                foreach (var entry in _partitionKeys)
                {
                    identifierHash.Hash = entry.Key;
                    identifierHash.ContextPartitionId = entry.Value.ContextPartitionOrPathId;
                    if (filter.Filter(identifierHash))
                    {
                        visitor.Visit(nestingLevel, _pathId, _factory.Binding, entry.Key, this, entry.Value);
                    }
                }
                return;
            }
            if (contextPartitionSelector is ContextPartitionSelectorAll)
            {
                foreach (var entry in _partitionKeys)
                {
                    visitor.Visit(nestingLevel, _pathId, _factory.Binding, entry.Key, this, entry.Value);
                }
                return;
            }
            if (contextPartitionSelector is ContextPartitionSelectorById)
            {
                var byId = (ContextPartitionSelectorById)contextPartitionSelector;
                foreach (var entry in _partitionKeys)
                {
                    int cpid = entry.Value.ContextPartitionOrPathId;
                    if (byId.ContextPartitionIds.Contains(cpid))
                    {
                        visitor.Visit(nestingLevel, _pathId, _factory.Binding, entry.Key, this, entry.Value);
                    }
                }
                return;
            }
            throw ContextControllerSelectorUtil.GetInvalidSelector(new Type[] { typeof(ContextPartitionSelectorHash) }, contextPartitionSelector);
        }

        public void Activate(EventBean optionalTriggeringEvent, IDictionary<String, Object> optionalTriggeringPattern, ContextControllerState controllerState, ContextInternalFilterAddendum activationFilterAddendum, int? importPathId)
        {
            ContextControllerFactoryContext factoryContext = _factory.FactoryContext;
            _activationFilterAddendum = activationFilterAddendum;

            if (factoryContext.NestingLevel == 1)
            {
                controllerState = ContextControllerStateUtil.GetRecoveryStates(_factory.FactoryContext.StateCache, factoryContext.OutermostContextName);
            }
            if (controllerState == null)
            {

                // handle preallocate
                if (_factory.HashedSpec.IsPreallocate)
                {
                    for (int i = 0; i < _factory.HashedSpec.Granularity; i++)
                    {
                        var properties = ContextPropertyEventType.GetHashBean(factoryContext.ContextName, i);
                        _currentSubpathId++;

                        // merge filter addendum, if any
                        var filterAddendumToUse = activationFilterAddendum;
                        if (_factory.HasFiltersSpecsNestedContexts)
                        {
                            filterAddendumToUse = activationFilterAddendum != null ? activationFilterAddendum.DeepCopy() : new ContextInternalFilterAddendum();
                            _factory.PopulateContextInternalFilterAddendums(filterAddendumToUse, i);
                        }

                        ContextControllerInstanceHandle handle = _activationCallback.ContextPartitionInstantiate(null, _currentSubpathId, null, this, optionalTriggeringEvent, null, i, properties, controllerState, filterAddendumToUse, _factory.FactoryContext.IsRecoveringResilient, ContextPartitionState.STARTED);
                        _partitionKeys.Put(i, handle);

                        _factory.FactoryContext.StateCache.AddContextPath(
                            _factory.FactoryContext.OutermostContextName,
                            _factory.FactoryContext.NestingLevel,
                            _pathId, _currentSubpathId, handle.ContextPartitionOrPathId, i,
                            _factory.Binding);
                    }
                    return;
                }

                // start filters if not preallocated
                ActivateFilters(optionalTriggeringEvent);

                return;
            }

            // initialize from existing state
            int pathIdToUse = importPathId ?? _pathId;
            InitializeFromState(optionalTriggeringEvent, optionalTriggeringPattern, controllerState, pathIdToUse, null, false);

            // activate filters
            if (!_factory.HashedSpec.IsPreallocate)
            {
                ActivateFilters(null);
            }
        }

        protected void ActivateFilters(EventBean optionalTriggeringEvent)
        {
            var factoryContext = _factory.FactoryContext;
            foreach (var item in _factory.HashedSpec.Items)
            {
                var callback = new ContextControllerHashedFilterCallback(factoryContext.ServicesContext, factoryContext.AgentInstanceContextCreate, item, this, _activationFilterAddendum);
                _filterCallbacks.Add(callback);

                if (optionalTriggeringEvent != null)
                {
                    var match = StatementAgentInstanceUtil.EvaluateFilterForStatement(factoryContext.ServicesContext, optionalTriggeringEvent, factoryContext.AgentInstanceContextCreate, callback.FilterHandle);
                    if (match)
                    {
                        callback.MatchFound(optionalTriggeringEvent, null);
                    }
                }
            }
        }

        public void Create(int id, EventBean theEvent)
        {
            lock (this)
            {
                ContextControllerFactoryContext factoryContext = _factory.FactoryContext;
                if (_partitionKeys.ContainsKey(id))
                {
                    return;
                }

                IDictionary<String, Object> properties = ContextPropertyEventType.GetHashBean(factoryContext.ContextName, id);
                _currentSubpathId++;

                // merge filter addendum, if any
                ContextInternalFilterAddendum filterAddendumToUse = _activationFilterAddendum;
                if (_factory.HasFiltersSpecsNestedContexts)
                {
                    filterAddendumToUse = _activationFilterAddendum != null ? _activationFilterAddendum.DeepCopy() : new ContextInternalFilterAddendum();
                    _factory.PopulateContextInternalFilterAddendums(filterAddendumToUse, id);
                }

                ContextControllerInstanceHandle handle = _activationCallback.ContextPartitionInstantiate(null, _currentSubpathId, null, this, theEvent, null, id, properties, null, filterAddendumToUse, _factory.FactoryContext.IsRecoveringResilient, ContextPartitionState.STARTED);
                _partitionKeys.Put(id, handle);
                _factory.FactoryContext.StateCache.AddContextPath(factoryContext.OutermostContextName, factoryContext.NestingLevel, _pathId, _currentSubpathId, handle.ContextPartitionOrPathId, id, _factory.Binding);

                // update the filter version for this handle
                long filterVersion = factoryContext.ServicesContext.FilterService.FiltersVersion;
                _factory.FactoryContext.AgentInstanceContextCreate.EpStatementAgentInstanceHandle.StatementFilterVersion.StmtFilterVersion = filterVersion;
            }
        }

        public ContextControllerFactory Factory
        {
            get { return _factory; }
        }

        public int PathId
        {
            get { return _pathId; }
        }

        public void Deactivate()
        {
            var factoryContext = _factory.FactoryContext;
            foreach (var callback in _filterCallbacks)
            {
                callback.Destroy(factoryContext.ServicesContext.FilterService);
            }
            _partitionKeys.Clear();
            _filterCallbacks.Clear();
            _factory.StateCache.RemoveContextParentPath(factoryContext.OutermostContextName, factoryContext.NestingLevel, _pathId);
        }

        private void InitializeFromState(
            EventBean optionalTriggeringEvent,
            IDictionary<String, Object> optionalTriggeringPattern,
            ContextControllerState controllerState,
            int pathIdToUse,
            AgentInstanceSelector agentInstanceSelector,
            bool loadingExistingState)
        {
            var factoryContext = _factory.FactoryContext;
            var states = controllerState.States;
            var childContexts = ContextControllerStateUtil.GetChildContexts(factoryContext, pathIdToUse, states);

            var maxSubpathId = int.MinValue;

            foreach (var entry in childContexts)
            {
                var hashAlgoGeneratedId = (int?)_factory.Binding.ByteArrayToObject(entry.Value.Blob, null);

                // merge filter addendum, if any
                var filterAddendumToUse = _activationFilterAddendum;
                if (_factory.HasFiltersSpecsNestedContexts)
                {
                    filterAddendumToUse = _activationFilterAddendum != null
                        ? _activationFilterAddendum.DeepCopy()
                        : new ContextInternalFilterAddendum();
                    _factory.PopulateContextInternalFilterAddendums(filterAddendumToUse, hashAlgoGeneratedId);
                }

                // check if exists already
                if (controllerState.IsImported)
                {
                    var existingHandle = _partitionKeys.Get(hashAlgoGeneratedId.Value);
                    if (existingHandle != null)
                    {
                        _activationCallback.ContextPartitionNavigate(
                            existingHandle, this, controllerState, entry.Value.OptionalContextPartitionId.Value,
                            filterAddendumToUse, agentInstanceSelector, entry.Value.Blob, loadingExistingState);
                        continue;
                    }
                }

                var properties = ContextPropertyEventType.GetHashBean(factoryContext.ContextName, hashAlgoGeneratedId.Value);

                var assignedSubPathId = !controllerState.IsImported ? entry.Key.SubPath : ++_currentSubpathId;
                var handle = _activationCallback.ContextPartitionInstantiate(
                    entry.Value.OptionalContextPartitionId, assignedSubPathId, entry.Key.SubPath, this,
                    optionalTriggeringEvent, optionalTriggeringPattern, hashAlgoGeneratedId, properties, controllerState,
                    filterAddendumToUse, loadingExistingState || factoryContext.IsRecoveringResilient, entry.Value.State);
                _partitionKeys.Put(hashAlgoGeneratedId.Value, handle);

                if (entry.Key.SubPath > maxSubpathId)
                {
                    maxSubpathId = assignedSubPathId;
                }
            }
            if (!controllerState.IsImported)
            {
                _currentSubpathId = maxSubpathId != int.MinValue ? maxSubpathId : 0;
            }
        }
    }
}
