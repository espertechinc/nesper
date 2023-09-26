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
using com.espertech.esper.common.@internal.epl.fafquery.processor;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.join.lookup;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.epl.join.queryplan;
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.common.@internal.epl.virtualdw;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.namedwindow.core
{
    /// <summary>
    ///     The root window in a named window plays multiple roles: It holds the indexes for deleting rows, if any on-delete
    ///     statement
    ///     requires such indexes. Such indexes are updated when events arrive, or remove from when a data window
    ///     or on-delete statement expires events. The view keeps track of on-delete statements their indexes used.
    /// </summary>
    public class NamedWindowRootViewInstance : ViewSupport
    {
        private readonly NamedWindowRootView rootView;

        public NamedWindowRootViewInstance(
            NamedWindowRootView rootView,
            AgentInstanceContext agentInstanceContext,
            EventTableIndexMetadata eventTableIndexMetadata)
        {
            this.rootView = rootView;
            AgentInstanceContext = agentInstanceContext;

            IndexRepository = new EventTableIndexRepository(eventTableIndexMetadata);
            foreach (var entry in eventTableIndexMetadata.Indexes) {
                if (entry.Value.OptionalQueryPlanIndexItem != null) {
                    var index = EventTableUtil.BuildIndex(
                        agentInstanceContext,
                        0,
                        entry.Value.OptionalQueryPlanIndexItem,
                        rootView.EventType,
                        entry.Key.IsUnique,
                        entry.Value.OptionalIndexName,
                        null,
                        false);
                    IndexRepository.AddIndex(
                        entry.Key,
                        new EventTableIndexRepositoryEntry(
                            entry.Value.OptionalIndexName,
                            entry.Value.OptionalIndexModuleName,
                            index));
                }
            }
        }

        public AgentInstanceContext AgentInstanceContext { get; }

        public IndexMultiKey[] Indexes => IndexRepository.IndexDescriptors;

        /// <summary>
        ///     Sets the iterator to use to obtain current named window data window contents.
        /// </summary>
        /// <value>iterator over events help by named window</value>
        public IEnumerable<EventBean> DataWindowContents { get; set; }

        //public override Viewable Parent {
        //    set { base.Parent = value; }
        //}

        public override EventType EventType => rootView.EventType;

        public bool IsParentBatchWindow => rootView.IsChildBatching;

        public EventTableIndexRepository IndexRepository { get; }

        public bool IsVirtualDataWindow => Child is VirtualDWView;

        public VirtualDWView VirtualDataWindow {
            get {
                if (!IsVirtualDataWindow) {
                    return null;
                }

                return (VirtualDWView)Child;
            }
        }

        /// <summary>
        ///     Called by tail view to indicate that the data window view exired events that must be removed from index tables.
        /// </summary>
        /// <param name="oldData">removed stream of the data window</param>
        public void RemoveOldData(EventBean[] oldData)
        {
            foreach (var table in IndexRepository.Tables) {
                table.Remove(oldData, AgentInstanceContext);
            }
        }

        /// <summary>
        ///     Called by tail view to indicate that the data window view has new events that must be added to index tables.
        /// </summary>
        /// <param name="newData">new event</param>
        public void AddNewData(EventBean[] newData)
        {
            foreach (var table in IndexRepository.Tables) {
                table.Add(newData, AgentInstanceContext);
            }
        }

        // Called by deletion strategy and also the insert-into for new events only
        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            // Update indexes for fast deletion, if there are any
            if (rootView.IsChildBatching) {
                foreach (var table in IndexRepository.Tables) {
                    table.Add(newData, AgentInstanceContext);
                }
            }

            // Update child views
            Child.Update(newData, oldData);
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return EnumerationHelper.Empty<EventBean>();
        }

        /// <summary>
        ///     Destroy and clear resources.
        /// </summary>
        public void Destroy()
        {
            IndexRepository.Destroy();
            if (IsVirtualDataWindow) {
                VirtualDataWindow.HandleDestroy(AgentInstanceContext.AgentInstanceId);
            }
        }

        /// <summary>
        ///     Return a snapshot using index lookup filters.
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
                queryGraph,
                annotations,
                virtualDataWindow,
                IndexRepository,
                rootView.EventType.Name,
                AgentInstanceContext);
        }

        /// <summary>
        ///     Add an explicit index.
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
                var initIndex =
                    AgentInstanceContext.StatementContext.EventTableIndexService.AllowInitIndex(isRecoveringResilient);
                var initializeFrom =
                    initIndex ? DataWindowContents : CollectionUtil.NULL_EVENT_ITERABLE;
                IndexRepository.ValidateAddExplicitIndex(
                    explicitIndexName,
                    explicitIndexModuleName,
                    explicitIndexDesc,
                    rootView.EventType,
                    initializeFrom,
                    AgentInstanceContext,
                    isRecoveringResilient,
                    null);
            }
        }

        public void VisitIndexes(EventTableVisitor visitor)
        {
            visitor.Visit(IndexRepository.Tables);
        }


        public void ClearDeliveriesRemoveStream(EventBean[] removedEvents)
        {
            AgentInstanceContext.StatementResultService.ClearDeliveriesRemoveStream(removedEvents);
        }
    }
} // end of namespace