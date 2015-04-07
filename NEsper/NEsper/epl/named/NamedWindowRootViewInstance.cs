///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.factory;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.fafquery;
using com.espertech.esper.epl.@join.table;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.virtualdw;
using com.espertech.esper.filter;
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

	    public NamedWindowRootViewInstance(NamedWindowRootView rootView, AgentInstanceContext agentInstanceContext)
        {
	        _rootView = rootView;
	        _agentInstanceContext = agentInstanceContext;

	        _indexRepository = new EventTableIndexRepository();
	        _tablePerMultiLookup = new Dictionary<SubordWMatchExprLookupStrategy, EventTable[]>();
	    }

	    public EventTableIndexRepository IndexRepository
	    {
	        get { return _indexRepository; }
	    }

	    public IndexMultiKey[] Indexes
	    {
	        get { return _indexRepository.IndexDescriptors; }
	    }

	    /// <summary>
	    /// Sets the iterator to use to obtain current named window data window contents.
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
	            foreach (var table in _indexRepository.Tables)
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
	            foreach (var table in _indexRepository.Tables)
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
	            foreach (var table in _indexRepository.Tables)
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
	        return EnumerationHelper<EventBean>.CreateEmptyEnumerator();
	    }

	    /// <summary>
	    /// Dispose and clear resources.
	    /// </summary>
	    public void Dispose()
	    {
	        _indexRepository.Destroy();
	        _tablePerMultiLookup.Clear();
	    }

        /// <summary>
        /// Return a snapshot using index lookup filters.
        /// </summary>
        /// <param name="optionalFilter">to index lookup</param>
        /// <param name="annotations">The annotations.</param>
        /// <returns>
        /// events
        /// </returns>
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
	    /// <throws>com.espertech.esper.epl.expression.core.ExprValidationException if the index fails to be valid</throws>
	    public void AddExplicitIndex(bool unique, string indexName, IList<CreateIndexItem> columns) {
	        lock (this)
	        {
	            _indexRepository.ValidateAddExplicitIndex(
	                unique, indexName, columns, _rootView.EventType, _dataWindowContents);
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
	        var events = new EventBean[1];
	        foreach (var @event in _dataWindowContents) {
	            events[0] = @event;
	            foreach (var table in _indexRepository.Tables) {
	                table.Add(events);
	            }
	        }
	    }

	    public void VisitIndexes(StatementAgentInstancePostLoadIndexVisitor visitor)
        {
	        visitor.Visit(_indexRepository.Tables);
	    }

	    public bool IsQueryPlanLogging
	    {
	        get { return _rootView.IsQueryPlanLogging; }
	    }
	}
} // end of namespace
