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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.factory;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.fafquery;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.virtualdw;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.named
{
    /// <summary>
    /// The root window in a named window plays multiple roles: It holds the indexes for deleting rows, if any on-delete statement
    /// requires such indexes. Such indexes are updated when events arrive, or remove from when a data window
    /// or on-delete statement expires events. The view keeps track of on-delete statements their indexes used.
    /// </summary>
    public class NamedWindowRootViewInstance : ViewSupport
    {
        private readonly NamedWindowRootView _rootView;
        private readonly AgentInstanceContext _agentInstanceContext;
    
        private readonly EventTableIndexRepository _indexRepository;
        private readonly IDictionary<SubordWMatchExprLookupStrategy, EventTable[]> _tablePerMultiLookup;
    
        private IEnumerable<EventBean> _dataWindowContents;
    
        public NamedWindowRootViewInstance(NamedWindowRootView rootView, AgentInstanceContext agentInstanceContext, EventTableIndexMetadata eventTableIndexMetadata) {
            _rootView = rootView;
            _agentInstanceContext = agentInstanceContext;
    
            _indexRepository = new EventTableIndexRepository(eventTableIndexMetadata);
            foreach (var entry in eventTableIndexMetadata.Indexes) {
                if (entry.Value.QueryPlanIndexItem != null) {
                    EventTable index = EventTableUtil.BuildIndex(agentInstanceContext, 0, entry.Value.QueryPlanIndexItem, rootView.EventType, true, entry.Key.IsUnique, entry.Value.OptionalIndexName, null, false);
                    _indexRepository.AddIndex(entry.Key, new EventTableIndexRepositoryEntry(entry.Value.OptionalIndexName, index));
                }
            }
    
            _tablePerMultiLookup = new Dictionary<SubordWMatchExprLookupStrategy, EventTable[]>();
        }

        public AgentInstanceContext AgentInstanceContext => _agentInstanceContext;

        public EventTableIndexRepository IndexRepository => _indexRepository;

        public IndexMultiKey[] Indexes => _indexRepository.IndexDescriptors;

        public IEnumerable<EventBean> DataWindowContents
        {
            get => _dataWindowContents;
            set => _dataWindowContents = value;
        }

        /// <summary>
        /// Called by tail view to indicate that the data window view exired events that must be removed from index tables.
        /// </summary>
        /// <param name="oldData">removed stream of the data window</param>
        public void RemoveOldData(EventBean[] oldData) {
            if (_rootView.RevisionProcessor != null) {
                _rootView.RevisionProcessor.RemoveOldData(oldData, _indexRepository, _agentInstanceContext);
            } else {
                foreach (EventTable table in _indexRepository.Tables) {
                    table.Remove(oldData, _agentInstanceContext);
                }
            }
        }
    
        /// <summary>
        /// Called by tail view to indicate that the data window view has new events that must be added to index tables.
        /// </summary>
        /// <param name="newData">new event</param>
        public void AddNewData(EventBean[] newData) {
            if (_rootView.RevisionProcessor == null) {
                // Update indexes for fast deletion, if there are any
                foreach (EventTable table in _indexRepository.Tables) {
                    table.Add(newData, _agentInstanceContext);
                }
            }
        }
    
        // Called by deletion strategy and also the insert-into for new events only
        public override void Update(EventBean[] newData, EventBean[] oldData) {
            if (_rootView.RevisionProcessor != null) {
                _rootView.RevisionProcessor.OnUpdate(newData, oldData, this, _indexRepository);
            } else {
                // Update indexes for fast deletion, if there are any
                foreach (EventTable table in _indexRepository.Tables) {
                    if (_rootView.IsChildBatching) {
                        table.Add(newData, _agentInstanceContext);
                    }
                }
    
                // Update child views
                UpdateChildren(newData, oldData);
            }
        }
    
        public override Viewable Parent
        {
            set => base.Parent = value;
        }

        public override EventType EventType => _rootView.EventType;

        public override IEnumerator<EventBean> GetEnumerator() {
            return null;
        }
    
        /// <summary>Destroy and clear resources.</summary>
        public void Destroy() {
            _indexRepository.Destroy();
            _tablePerMultiLookup.Clear();
            if (IsVirtualDataWindow) {
                VirtualDataWindow.HandleStopWindow();
            }
        }
    
        /// <summary>
        /// Return a snapshot using index lookup filters.
        /// </summary>
        /// <param name="annotations">annotations</param>
        /// <param name="queryGraph">query graph</param>
        /// <returns>events</returns>
        public ICollection<EventBean> Snapshot(QueryGraph queryGraph, Attribute[] annotations) {
            VirtualDWView virtualDataWindow = null;
            if (IsVirtualDataWindow) {
                virtualDataWindow = VirtualDataWindow;
            }
            return FireAndForgetQueryExec.Snapshot(queryGraph, annotations, virtualDataWindow,
                    _indexRepository, _rootView.IsQueryPlanLogging, NamedWindowRootView.QueryPlanLog,
                    _rootView.EventType.Name, _agentInstanceContext);
        }
    
        /// <summary>
        /// Add an explicit index.
        /// </summary>
        /// <param name="explicitIndexDesc">index descriptor</param>
        /// <param name="isRecoveringResilient">indicator for recovering</param>
        /// <param name="explicitIndexName">index name</param>
        /// <exception cref="com.espertech.esper.epl.expression.core.ExprValidationException">if the index fails to be valid</exception>
        public void AddExplicitIndex(string explicitIndexName, QueryPlanIndexItem explicitIndexDesc, bool isRecoveringResilient)
        {
            lock (this)
            {
                var initIndex = _agentInstanceContext.StatementContext.EventTableIndexService.AllowInitIndex(isRecoveringResilient);
                var initializeFrom = initIndex ? _dataWindowContents : CollectionUtil.NULL_EVENT_ITERABLE;
                _indexRepository.ValidateAddExplicitIndex(explicitIndexName, explicitIndexDesc, _rootView.EventType, initializeFrom, _agentInstanceContext, isRecoveringResilient, null);
            }
        }

        public bool IsVirtualDataWindow => Views[0] is VirtualDWView;

        public VirtualDWView VirtualDataWindow
        {
            get
            {
                if (!IsVirtualDataWindow)
                {
                    return null;
                }
                return (VirtualDWView) Views[0];
            }
        }

        public void PostLoad()
        {
            var events = new EventBean[1];
            foreach (EventBean @event in _dataWindowContents) {
                events[0] = @event;
                foreach (EventTable table in _indexRepository.Tables) {
                    table.Add(events, _agentInstanceContext);
                }
            }
        }
    
        public void VisitIndexes(StatementAgentInstancePostLoadIndexVisitor visitor) {
            visitor.Visit(_indexRepository.Tables);
        }

        public bool IsQueryPlanLogging => _rootView.IsQueryPlanLogging;

        public void Stop() {
            if (IsVirtualDataWindow) {
                VirtualDataWindow.HandleStopWindow();
            }
        }
    }
} // end of namespace
