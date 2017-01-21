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
using com.espertech.esper.core.context.factory;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.fafquery;
using com.espertech.esper.epl.@join.table;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.virtualdw;
using com.espertech.esper.filter;
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

	    public NamedWindowRootViewInstance(NamedWindowRootView rootView, AgentInstanceContext agentInstanceContext, EventTableIndexMetadata eventTableIndexMetadata)
        {
	        _rootView = rootView;
	        _agentInstanceContext = agentInstanceContext;

	        _indexRepository = new EventTableIndexRepository();
	        foreach (KeyValuePair<IndexMultiKey, EventTableIndexMetadataEntry> entry in eventTableIndexMetadata.Indexes)
            {
	            if (entry.Value.QueryPlanIndexItem != null)
                {
	                EventTable index = EventTableUtil.BuildIndex(agentInstanceContext, 0, entry.Value.QueryPlanIndexItem, rootView.EventType, true, entry.Key.IsUnique, entry.Value.OptionalIndexName, null, false);
	                _indexRepository.AddIndex(entry.Key, new EventTableIndexRepositoryEntry(entry.Value.OptionalIndexName, index));
	            }
	        }

	        _tablePerMultiLookup = new Dictionary<SubordWMatchExprLookupStrategy, EventTable[]>();
	    }

	    public AgentInstanceContext AgentInstanceContext
	    {
	        get { return _agentInstanceContext; }
	    }

	    public EventTableIndexRepository IndexRepository
	    {
	        get { return _indexRepository; }
	    }

	    public IndexMultiKey[] Indexes
	    {
	        get { return _indexRepository.GetIndexDescriptors(); }
	    }

	    /// <summary>
	    /// Gets or sets the enumeratable to use to obtain current named window data window contents.
	    /// </summary>
	    /// <value>iterator over events help by named window</value>
	    public IEnumerable<EventBean> DataWindowContents
	    {
	        get { return _dataWindowContents; }
	        set { _dataWindowContents = value; }
	    }

	    /// <summary>
	    /// Called by tail view to indicate that the data window view exired events that must be removed from index tables.
	    /// </summary>
	    /// <param name="oldData">removed stream of the data window</param>
	    public void RemoveOldData(EventBean[] oldData)
	    {
	        if (_rootView.RevisionProcessor != null)
	        {
	            _rootView.RevisionProcessor.RemoveOldData(oldData, _indexRepository);
	        }
	        else
	        {
	            foreach (EventTable table in _indexRepository.GetTables())
	            {
	                table.Remove(oldData);
	            }
	        }
	    }

	    /// <summary>
	    /// Called by tail view to indicate that the data window view has new events that must be added to index tables.
	    /// </summary>
	    /// <param name="newData">new event</param>
	    public void AddNewData(EventBean[] newData)
	    {
	        if (_rootView.RevisionProcessor == null) {
	            // Update indexes for fast deletion, if there are any
	            foreach (EventTable table in _indexRepository.GetTables())
	            {
	                table.Add(newData);
	            }
	        }
	    }

	    // Called by deletion strategy and also the insert-into for new events only
	    public override void Update(EventBean[] newData, EventBean[] oldData)
	    {
	        if (_rootView.RevisionProcessor != null)
	        {
	            _rootView.RevisionProcessor.OnUpdate(newData, oldData, this, _indexRepository);
	        }
	        else
	        {
	            // Update indexes for fast deletion, if there are any
	            foreach (EventTable table in _indexRepository.GetTables())
	            {
	                if (_rootView.IsChildBatching) {
	                    table.Add(newData);
	                }
	            }

	            // Update child views
	            UpdateChildren(newData, oldData);
	        }
	    }

	    public override EventType EventType
	    {
	        get { return _rootView.EventType; }
	    }

	    public override IEnumerator<EventBean> GetEnumerator()
	    {
	        return null;
	    }

	    /// <summary>
	    /// Dispose and clear resources.
	    /// </summary>
	    public void Dispose()
	    {
	        _indexRepository.Destroy();
	        _tablePerMultiLookup.Clear();
	        if (IsVirtualDataWindow) {
	            VirtualDataWindow.HandleStopWindow();
	        }
	    }

	    /// <summary>
	    /// Return a snapshot using index lookup filters.
	    /// </summary>
	    /// <param name="optionalFilter">to index lookup</param>
	    /// <returns>events</returns>
	    public ICollection<EventBean> Snapshot(FilterSpecCompiled optionalFilter, Attribute[] annotations)
        {
	        VirtualDWView virtualDataWindow = null;
	        if (IsVirtualDataWindow) {
	            virtualDataWindow = VirtualDataWindow;
	        }
	        return FireAndForgetQueryExec.Snapshot(optionalFilter, annotations, virtualDataWindow,
	                _indexRepository, _rootView.IsQueryPlanLogging, NamedWindowRootView.QueryPlanLog,
	                _rootView.EventType.Name, _agentInstanceContext);
	    }

        /// <summary>
        /// Add an explicit index.
        /// </summary>
        /// <param name="unique">indicator whether unique</param>
        /// <param name="indexName">indexname</param>
        /// <param name="columns">properties indexed</param>
        /// <param name="isRecoveringResilient">if set to <c>true</c> [is recovering resilient].</param>
        /// <throws>com.espertech.esper.epl.expression.core.ExprValidationException if the index fails to be valid</throws>
	    public void AddExplicitIndex(bool unique, string indexName, IList<CreateIndexItem> columns, bool isRecoveringResilient)
        {
	        lock (this)
	        {
	            var initIndex = _agentInstanceContext.StatementContext.EventTableIndexService.AllowInitIndex(isRecoveringResilient);
	            var initializeFrom = initIndex ? _dataWindowContents : CollectionUtil.NULL_EVENT_ITERABLE;
	            _indexRepository.ValidateAddExplicitIndex(
	                unique, indexName, columns, _rootView.EventType, initializeFrom, _agentInstanceContext,
	                isRecoveringResilient, null);
	        }
	    }

	    public bool IsVirtualDataWindow
	    {
	        get { return Views[0] is VirtualDWView; }
	    }

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
	        EventBean[] events = new EventBean[1];
	        foreach (EventBean @event in _dataWindowContents)
            {
	            events[0] = @event;
	            foreach (EventTable table in _indexRepository.GetTables()) {
	                table.Add(events);
	            }
	        }
	    }

	    public void VisitIndexes(StatementAgentInstancePostLoadIndexVisitor visitor)
        {
            visitor.Visit(_indexRepository.GetTables());
	    }

	    public bool IsQueryPlanLogging
	    {
	        get { return _rootView.IsQueryPlanLogging; }
	    }

	    public void Stop() {
	        if (IsVirtualDataWindow) {
	            VirtualDataWindow.HandleStopWindow();
	        }
	    }
	}
} // end of namespace
