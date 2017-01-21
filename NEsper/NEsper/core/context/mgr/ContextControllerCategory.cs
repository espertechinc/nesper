///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.context;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.core.context.mgr
{
    public class ContextControllerCategory : ContextController
    {
        private readonly ContextControllerLifecycleCallback _activationCallback;

        private readonly IDictionary<int, ContextControllerInstanceHandle> _handleCategories =
            new LinkedHashMap<int, ContextControllerInstanceHandle>();

        private int _currentSubpathId;
        private readonly ContextControllerCategoryFactoryImpl _factory;

        public ContextControllerCategory(
            int pathId,
            ContextControllerLifecycleCallback activationCallback,
            ContextControllerCategoryFactoryImpl factory)
        {
            PathId = pathId;
            _activationCallback = activationCallback;
            _factory = factory;
        }

        public void ImportContextPartitions(
            ContextControllerState state,
            int pathIdToUse,
            ContextInternalFilterAddendum filterAddendum,
            AgentInstanceSelector agentInstanceSelector)
        {
            InitializeFromState(null, null, filterAddendum, state, pathIdToUse, agentInstanceSelector, true);
        }

        public void DeletePath(ContextPartitionIdentifier identifier)
        {
            var category = (ContextPartitionIdentifierCategory) identifier;
            var count = 0;
            foreach (var cat in _factory.CategorySpec.Items)
            {
                if (cat.Name.Equals(category.Label))
                {
                    _handleCategories.Remove(count);
                    break;
                }
                count++;
            }
        }

        public void VisitSelectedPartitions(
            ContextPartitionSelector contextPartitionSelector,
            ContextPartitionVisitor visitor)
        {
            var nestingLevel = Factory.FactoryContext.NestingLevel;

            if (contextPartitionSelector is ContextPartitionSelectorFiltered)
            {
                var filter = (ContextPartitionSelectorFiltered) contextPartitionSelector;
                var identifier = new ContextPartitionIdentifierCategory();
                foreach (var entry in _handleCategories)
                {
                    identifier.ContextPartitionId = entry.Value.ContextPartitionOrPathId;
                    var categoryName = _factory.CategorySpec.Items[entry.Key].Name;
                    identifier.Label = categoryName;
                    if (filter.Filter(identifier))
                    {
                        visitor.Visit(nestingLevel, PathId, _factory.Binding, entry.Key, this, entry.Value);
                    }
                }
                return;
            }
            if (contextPartitionSelector is ContextPartitionSelectorCategory)
            {
                var category = (ContextPartitionSelectorCategory) contextPartitionSelector;
                if (category.Labels == null || category.Labels.IsEmpty())
                {
                    return;
                }
                foreach (var entry in _handleCategories)
                {
                    String categoryName = _factory.CategorySpec.Items[entry.Key].Name;
                    if (category.Labels.Contains(categoryName))
                    {
                        visitor.Visit(nestingLevel, PathId, _factory.Binding, entry.Key, this, entry.Value);
                    }
                }
                return;
            }
            if (contextPartitionSelector is ContextPartitionSelectorById)
            {
                var byId = (ContextPartitionSelectorById) contextPartitionSelector;
                foreach (var entry in _handleCategories)
                {
                    var cpid = entry.Value.ContextPartitionOrPathId;
                    if (byId.ContextPartitionIds.Contains(cpid))
                    {
                        visitor.Visit(nestingLevel, PathId, _factory.Binding, entry.Key, this, entry.Value);
                    }
                }
                return;
            }
            if (contextPartitionSelector is ContextPartitionSelectorAll)
            {
                foreach (var entry in _handleCategories)
                {
                    var categoryName = _factory.CategorySpec.Items[entry.Key].Name;
                    visitor.Visit(nestingLevel, PathId, _factory.Binding, entry.Key, this, entry.Value);
                }
                return;
            }
            throw ContextControllerSelectorUtil.GetInvalidSelector(
                new Type[]
                {
                    typeof (ContextPartitionSelectorCategory)
                }, contextPartitionSelector);
        }

        public void Activate(
            EventBean optionalTriggeringEvent,
            IDictionary<String, Object> optionalTriggeringPattern,
            ContextControllerState controllerState,
            ContextInternalFilterAddendum activationFilterAddendum,
            int? importPathId)
        {
            if (Factory.FactoryContext.NestingLevel == 1)
            {
                controllerState = ContextControllerStateUtil.GetRecoveryStates(
                    Factory.FactoryContext.StateCache, Factory.FactoryContext.OutermostContextName);
            }

            if (controllerState == null)
            {
                var count = 0;
                foreach (var category in _factory.CategorySpec.Items)
                {
                    var context =
                        ContextPropertyEventType.GetCategorizedBean(
                            Factory.FactoryContext.ContextName, 0, category.Name);
                    _currentSubpathId++;

                    // merge filter addendum, if any
                    var filterAddendumToUse = activationFilterAddendum;
                    if (_factory.HasFiltersSpecsNestedContexts)
                    {
                        filterAddendumToUse = activationFilterAddendum != null
                            ? activationFilterAddendum.DeepCopy()
                            : new ContextInternalFilterAddendum();
                        _factory.PopulateContextInternalFilterAddendums(filterAddendumToUse, count);
                    }

                    var handle = _activationCallback.ContextPartitionInstantiate(
                        null, _currentSubpathId, null, this, optionalTriggeringEvent, optionalTriggeringPattern, count,
                        context, controllerState, filterAddendumToUse, Factory.FactoryContext.IsRecoveringResilient,
                        ContextPartitionState.STARTED);
                    _handleCategories.Put(count, handle);

                    Factory.FactoryContext.StateCache.AddContextPath(
                        Factory.FactoryContext.OutermostContextName, Factory.FactoryContext.NestingLevel, PathId,
                        _currentSubpathId, handle.ContextPartitionOrPathId, count, _factory.Binding);
                    count++;
                }
                return;
            }

            var pathIdToUse = importPathId != null ? importPathId.Value : PathId;
            InitializeFromState(
                optionalTriggeringEvent, optionalTriggeringPattern, activationFilterAddendum, controllerState,
                pathIdToUse, null, false);
        }

        public ContextControllerFactory Factory
        {
            get { return _factory; }
        }

        public int PathId { get; private set; }

        public void Deactivate()
        {
            _handleCategories.Clear();
        }

        private void InitializeFromState(
            EventBean optionalTriggeringEvent,
            IDictionary<String, Object> optionalTriggeringPattern,
            ContextInternalFilterAddendum activationFilterAddendum,
            ContextControllerState controllerState,
            int pathIdToUse,
            AgentInstanceSelector agentInstanceSelector,
            bool loadingExistingState)
        {
            var states = controllerState.States;
            var childContexts = ContextControllerStateUtil.GetChildContexts(
                Factory.FactoryContext, pathIdToUse, states);

            int maxSubpathId = int.MinValue;
            foreach (var entry in childContexts)
            {
                var categoryNumber = (int)_factory.Binding.ByteArrayToObject(entry.Value.Blob, null);
                ContextDetailCategoryItem category = _factory.CategorySpec.Items[categoryNumber];

                // merge filter addendum, if any
                var filterAddendumToUse = activationFilterAddendum;
                if (_factory.HasFiltersSpecsNestedContexts)
                {
                    filterAddendumToUse = activationFilterAddendum != null
                        ? activationFilterAddendum.DeepCopy()
                        : new ContextInternalFilterAddendum();
                    _factory.PopulateContextInternalFilterAddendums(filterAddendumToUse, categoryNumber);
                }

                // check if exists already
                if (controllerState.IsImported)
                {
                    var existingHandle = _handleCategories.Get(categoryNumber);
                    if (existingHandle != null)
                    {
                        _activationCallback.ContextPartitionNavigate(
                            existingHandle, this, controllerState, entry.Value.OptionalContextPartitionId.Value,
                            filterAddendumToUse, agentInstanceSelector, entry.Value.Blob, loadingExistingState);
                        continue;
                    }
                }

                var context =
                    ContextPropertyEventType.GetCategorizedBean(Factory.FactoryContext.ContextName, 0, category.Name);

                var contextPartitionId = entry.Value.OptionalContextPartitionId.Value;
                var assignedSubPathId = !controllerState.IsImported ? entry.Key.SubPath : ++_currentSubpathId;
                var handle =
                    _activationCallback.ContextPartitionInstantiate(
                        contextPartitionId, assignedSubPathId, entry.Key.SubPath, this, null, null, categoryNumber,
                        context, controllerState, filterAddendumToUse, loadingExistingState || Factory.FactoryContext.IsRecoveringResilient,
                        entry.Value.State);
                _handleCategories.Put(categoryNumber, handle);

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