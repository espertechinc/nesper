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
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.fafquery.processor;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.join.lookup;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.epl.join.queryplan;
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.common.@internal.epl.virtualdw;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.namedwindow.core
{
    /// <summary>
    /// The root window in a named window plays multiple roles: It holds the indexes for deleting rows, if any on-delete statement
    /// requires such indexes. Such indexes are updated when events arrive, or remove from when a data window
    /// or on-delete statement expires events. The view keeps track of on-delete statements their indexes used.
    /// </summary>
    public class NamedWindowRootViewInstance : ViewSupport
    {
        private readonly NamedWindowRootView rootView;
        private readonly AgentInstanceContext agentInstanceContext;

        private readonly EventTableIndexRepository indexRepository;

        private IEnumerable<EventBean> dataWindowContents;

        public NamedWindowRootViewInstance(
            NamedWindowRootView rootView,
            AgentInstanceContext agentInstanceContext,
            EventTableIndexMetadata eventTableIndexMetadata)
        {
            this.rootView = rootView;
            this.agentInstanceContext = agentInstanceContext;

            this.indexRepository = new EventTableIndexRepository(eventTableIndexMetadata);
            foreach (KeyValuePair<IndexMultiKey, EventTableIndexMetadataEntry> entry in eventTableIndexMetadata.Indexes
            ) {
                if (entry.Value.OptionalQueryPlanIndexItem != null) {
                    EventTable index = EventTableUtil.BuildIndex(
                        agentInstanceContext, 0, entry.Value.OptionalQueryPlanIndexItem, rootView.EventType, true,
                        entry.Key.IsUnique, entry.Value.OptionalIndexName, null, false);
                    indexRepository.AddIndex(
                        entry.Key,
                        new EventTableIndexRepositoryEntry(
                            entry.Value.OptionalIndexName, entry.Value.OptionalIndexModuleName, index));
                }
            }
        }

        public AgentInstanceContext AgentInstanceContext {
            get => agentInstanceContext;
        }

        public IndexMultiKey[] Indexes {
            get => indexRepository.IndexDescriptors;
        }

        public IEnumerable<EventBean> DataWindowContents {
            get => dataWindowContents;
        }

        /// <summary>
        /// Sets the iterator to use to obtain current named window data window contents.
        /// </summary>
        /// <param name="dataWindowContents">iterator over events help by named window</param>
        public void SetDataWindowContents(IEnumerable<EventBean> dataWindowContents)
        {
            this.dataWindowContents = dataWindowContents;
        }

        /// <summary>
        /// Called by tail view to indicate that the data window view exired events that must be removed from index tables.
        /// </summary>
        /// <param name="oldData">removed stream of the data window</param>
        public void RemoveOldData(EventBean[] oldData)
        {
            foreach (EventTable table in indexRepository.Tables) {
                table.Remove(oldData, agentInstanceContext);
            }
        }

        /// <summary>
        /// Called by tail view to indicate that the data window view has new events that must be added to index tables.
        /// </summary>
        /// <param name="newData">new event</param>
        public void AddNewData(EventBean[] newData)
        {
            foreach (EventTable table in indexRepository.Tables) {
                table.Add(newData, agentInstanceContext);
            }
        }

        // Called by deletion strategy and also the insert-into for new events only
        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            // Update indexes for fast deletion, if there are any
            if (rootView.IsChildBatching) {
                foreach (EventTable table in indexRepository.Tables) {
                    table.Add(newData, agentInstanceContext);
                }
            }

            // Update child views
            Child.Update(newData, oldData);
        }

        //public override Viewable Parent {
        //    set { base.Parent = value; }
        //}

        public override EventType EventType {
            get => rootView.EventType;
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return null;
        }

        /// <summary>
        /// Destroy and clear resources.
        /// </summary>
        public void Destroy()
        {
            indexRepository.Destroy();
            if (IsVirtualDataWindow) {
                VirtualDataWindow.HandleDestroy(agentInstanceContext.AgentInstanceId);
            }
        }

        /// <summary>
        /// Return a snapshot using index lookup filters.
        /// </summary>
        /// <param name="annotations">annotations</param>
        /// <param name="queryGraph">query graph</param>
        /// <returns>events</returns>
        public ICollection<EventBean> Snapshot(
            QueryGraph queryGraph,
            Attribute[] annotations)
        {
            VirtualDWView virtualDataWindow = null;
            if (IsVirtualDataWindow) {
                virtualDataWindow = VirtualDataWindow;
            }

            return FireAndForgetQueryExec.Snapshot(
                queryGraph, annotations, virtualDataWindow,
                indexRepository, rootView.EventType.Name, agentInstanceContext);
        }

        /// <summary>
        /// Add an explicit index.
        /// </summary>
        /// <param name="explicitIndexDesc">index descriptor</param>
        /// <param name="explicitIndexModuleName">module name</param>
        /// <param name="isRecoveringResilient">indicator for recovering</param>
        /// <param name="explicitIndexName">index name</param>
        /// <throws>ExprValidationException if the index fails to be valid</throws>
        public void AddExplicitIndex(
            string explicitIndexName,
            string explicitIndexModuleName,
            QueryPlanIndexItem explicitIndexDesc,
            bool isRecoveringResilient)
        {
            lock (this) {
                bool initIndex =
                    agentInstanceContext.StatementContext.EventTableIndexService.AllowInitIndex(isRecoveringResilient);
                IEnumerable<EventBean> initializeFrom =
                    initIndex ? this.dataWindowContents : CollectionUtil.NULL_EVENT_ITERABLE;
                indexRepository.ValidateAddExplicitIndex(
                    explicitIndexName, explicitIndexModuleName, explicitIndexDesc, rootView.EventType, initializeFrom,
                    agentInstanceContext, isRecoveringResilient, null);
            }
        }

        public void VisitIndexes(EventTableVisitor visitor)
        {
            visitor.Visit(indexRepository.Tables);
        }

        public bool IsParentBatchWindow {
            get => rootView.IsChildBatching;
        }

        public EventTableIndexRepository IndexRepository {
            get => indexRepository;
        }

        public bool IsVirtualDataWindow {
            get => Child is VirtualDWView;
        }

        public VirtualDWView VirtualDataWindow {
            get {
                if (!IsVirtualDataWindow) {
                    return null;
                }

                return (VirtualDWView) Child;
            }
        }
    }
} // end of namespace