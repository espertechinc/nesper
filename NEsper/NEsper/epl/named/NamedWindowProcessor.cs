///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.core.service.resource;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.metric;
using com.espertech.esper.events.vaevent;

namespace com.espertech.esper.epl.named
{
	/// <summary>
	/// An instance of this class is associated with a specific named window. The processor
	/// provides the views to create-window, on-delete statements and statements selecting from a named window.
	/// </summary>
	public class NamedWindowProcessor
	{
	    private readonly string _namedWindowName;
	    private readonly NamedWindowTailView _tailView;
	    private readonly NamedWindowRootView _rootView;
	    private readonly string _contextName;
	    private readonly EventType _eventType;
	    private readonly string _eplExpression;
	    private readonly string _statementName;
	    private readonly bool _isEnableSubqueryIndexShare;
	    private readonly bool _isVirtualDataWindow;
	    private readonly StatementMetricHandle _statementMetricHandle;
	    private readonly ICollection<string> _optionalUniqueKeyProps;
	    private readonly string _eventTypeAsName;
	    private readonly EventTableIndexMetadata _eventTableIndexMetadataRepo = new EventTableIndexMetadata();
        private readonly StatementResourceService _statementResourceService;

	    private readonly ILockable _lock;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="namedWindowName">Name of the named window.</param>
	    /// <param name="namedWindowService">service for dispatching results</param>
	    /// <param name="contextName">Name of the context.</param>
	    /// <param name="singleInstanceContext">if set to <c>true</c> [single instance context].</param>
	    /// <param name="eventType">the type of event held by the named window</param>
	    /// <param name="statementResultService">for coordinating on whether insert and remove stream events should be posted</param>
	    /// <param name="revisionProcessor">for revision processing</param>
	    /// <param name="eplExpression">epl expression</param>
	    /// <param name="statementName">statement name</param>
	    /// <param name="isPrioritized">if the engine is running with prioritized execution</param>
	    /// <param name="isEnableSubqueryIndexShare">if set to <c>true</c> [is enable subquery index share].</param>
	    /// <param name="enableQueryPlanLog">if set to <c>true</c> [enable query plan log].</param>
	    /// <param name="metricReportingService">The metric reporting service.</param>
	    /// <param name="isBatchingDataWindow">if set to <c>true</c> [is batching data window].</param>
	    /// <param name="isVirtualDataWindow">if set to <c>true</c> [is virtual data window].</param>
	    /// <param name="statementMetricHandle">The statement metric handle.</param>
	    /// <param name="optionalUniqueKeyProps">The optional unique key props.</param>
	    /// <param name="eventTypeAsName">Name of the event type as.</param>
	    public NamedWindowProcessor(
	        string namedWindowName,
	        NamedWindowService namedWindowService,
	        string contextName,
	        EventType eventType,
	        StatementResultService statementResultService,
	        ValueAddEventProcessor revisionProcessor,
	        string eplExpression,
	        string statementName,
	        bool isPrioritized,
	        bool isEnableSubqueryIndexShare,
	        bool enableQueryPlanLog,
	        MetricReportingService metricReportingService,
	        bool isBatchingDataWindow,
	        bool isVirtualDataWindow,
	        StatementMetricHandle statementMetricHandle,
            ICollection<string> optionalUniqueKeyProps,
	        string eventTypeAsName,
	        StatementResourceService statementResourceService)
        {
	        _namedWindowName = namedWindowName;
	        _contextName = contextName;
	        _eventType = eventType;
	        _eplExpression = eplExpression;
	        _statementName = statementName;
	        _isEnableSubqueryIndexShare = isEnableSubqueryIndexShare;
	        _isVirtualDataWindow = isVirtualDataWindow;
	        _statementMetricHandle = statementMetricHandle;
	        _optionalUniqueKeyProps = optionalUniqueKeyProps;
	        _eventTypeAsName = eventTypeAsName;
            _statementResourceService = statementResourceService;
	        _lock = LockManager.CreateLock(GetType());

	        _rootView = new NamedWindowRootView(revisionProcessor, enableQueryPlanLog, metricReportingService, eventType, isBatchingDataWindow, isEnableSubqueryIndexShare, optionalUniqueKeyProps);
	        _tailView = new NamedWindowTailView(eventType, namedWindowService, statementResultService, revisionProcessor, isPrioritized, isBatchingDataWindow);
	    }

	    public string GetEventTypeAsName() {
	        return _eventTypeAsName;
	    }

	    public NamedWindowProcessorInstance AddInstance(AgentInstanceContext agentInstanceContext)
        {
	        using (_lock.Acquire())
	        {
	            if (_contextName == null)
	            {
                    return new NamedWindowProcessorInstance(null, this, agentInstanceContext);
                }

	            var instanceId = agentInstanceContext.AgentInstanceId;
                return new NamedWindowProcessorInstance(instanceId, this, agentInstanceContext);
            }
	    }

	    public NamedWindowProcessorInstance ProcessorInstanceNoContext
	    {
	        get
	        {
	            StatementResourceHolder holder = _statementResourceService.Unpartitioned;
	            return holder == null ? null : holder.NamedWindowProcessorInstance;
	        }
	    }

	    public NamedWindowProcessorInstance GetProcessorInstance(int agentInstanceId)
        {
            StatementResourceHolder holder = _statementResourceService.GetPartitioned(agentInstanceId);
            return holder == null ? null : holder.NamedWindowProcessorInstance;
        }

        public ICollection<int> GetProcessorInstancesAll()
        {
            using (_lock.Acquire())
            {
                var keyset = _statementResourceService.ResourcesPartitioned.Keys;
                return new ArrayDeque<int>(keyset);
            }
        }

	    public NamedWindowProcessorInstance GetProcessorInstance(AgentInstanceContext agentInstanceContext)
        {
	        if (_contextName == null) {
                StatementResourceHolder holder = _statementResourceService.Unpartitioned;
                return holder == null ? null : holder.NamedWindowProcessorInstance;
	        }

	        if (agentInstanceContext.StatementContext.ContextDescriptor == null) {
	            return null;
	        }

	        if (_contextName.Equals(agentInstanceContext.StatementContext.ContextDescriptor.ContextName)) {
                StatementResourceHolder holder = _statementResourceService.GetPartitioned(agentInstanceContext.AgentInstanceId);
                return holder == null ? null : holder.NamedWindowProcessorInstance;
            }
	        return null;
	    }

	    public string ContextName
	    {
	        get { return _contextName; }
	    }

	    public NamedWindowConsumerView AddConsumer(NamedWindowConsumerDesc consumerDesc, bool isSubselect)
        {
	        // handle same-context consumer
	        if (_contextName != null) {
	            var contextDescriptor = consumerDesc.AgentInstanceContext.StatementContext.ContextDescriptor;
	            if (contextDescriptor != null && _contextName.Equals(contextDescriptor.ContextName)) {
                    var holder = _statementResourceService.GetPartitioned(consumerDesc.AgentInstanceContext.AgentInstanceId);
                    return holder.NamedWindowProcessorInstance.TailViewInstance.AddConsumer(consumerDesc, isSubselect);
                }
	            else {
	                // consumer is out-of-context
	                return _tailView.AddConsumer(consumerDesc);  // non-context consumers
	            }
	        }

	        // handle no context associated
            return _statementResourceService.ResourcesUnpartitioned.NamedWindowProcessorInstance.TailViewInstance.AddConsumer(consumerDesc, isSubselect);
	    }

	    public bool IsVirtualDataWindow
	    {
	        get { return _isVirtualDataWindow; }
	    }

	    /// <summary>
	    /// Returns the tail view of the named window, hooked into the view chain after the named window's data window views,
	    /// as the last view.
	    /// </summary>
	    /// <value>tail view</value>
	    public NamedWindowTailView TailView
	    {
	        get { return _tailView; } // hooked as the tail sview before any data windows
	    }

	    /// <summary>
	    /// Returns the root view of the named window, hooked into the view chain before the named window's data window views,
	    /// right after the filter stream that filters for insert-into events.
	    /// </summary>
	    /// <value>tail view</value>
	    public NamedWindowRootView RootView
	    {
	        get { return _rootView; }  // hooked as the top view before any data windows
	    }

	    /// <summary>
	    /// Returns the event type of the named window.
	    /// </summary>
	    /// <value>event type</value>
	    public EventType NamedWindowType
	    {
	        get { return _eventType; }
	    }

	    /// <summary>
	    /// Returns the EPL expression.
	    /// </summary>
	    /// <value>epl</value>
	    public string EplExpression
	    {
	        get { return _eplExpression; }
	    }

	    /// <summary>
	    /// Returns the statement name.
	    /// </summary>
	    /// <value>name</value>
	    public string StatementName
	    {
	        get { return _statementName; }
	    }

	    /// <summary>
	    /// Deletes a named window and removes any associated resources.
	    /// </summary>
	    public void Dispose()
	    {
	    }

	    public bool IsEnableSubqueryIndexShare
	    {
	        get { return _isEnableSubqueryIndexShare; }
	    }

	    public StatementMetricHandle CreateNamedWindowMetricsHandle
	    {
	        get { return _statementMetricHandle; }
	    }

	    public string NamedWindowName
	    {
	        get { return _namedWindowName; }
	    }

	    public string[][] GetUniqueIndexes(NamedWindowProcessorInstance processorInstance)
        {
	        IList<string[]> unique = null;
	        if (processorInstance != null) {
	            var indexDescriptors = processorInstance.IndexDescriptors;
	            foreach (var index in indexDescriptors) {
	                if (!index.IsUnique) {
	                    continue;
	                }
	                var uniqueKeys = IndexedPropDesc.GetIndexProperties(index.HashIndexedProps);
	                if (unique == null) {
	                    unique = new List<string[]>();
	                }
	                unique.Add(uniqueKeys);
	            }
	        }
	        if (_optionalUniqueKeyProps != null) {
	            if (unique == null) {
	                unique = new List<string[]>();
	            }
	            unique.Add(_optionalUniqueKeyProps.ToArray());
	        }
	        if (unique == null) {
	            return null;
	        }
	        return unique.ToArray();
	    }

	    public ICollection<string> OptionalUniqueKeyProps
	    {
	        get { return _optionalUniqueKeyProps; }
	    }

	    public EventTableIndexMetadata EventTableIndexMetadataRepo
	    {
	        get { return _eventTableIndexMetadataRepo; }
	    }

	    public void RemoveAllInstanceIndexes(IndexMultiKey index) {
            if (_contextName == null) {
                StatementResourceHolder holder = _statementResourceService.Unpartitioned;
                if (holder != null && holder.NamedWindowProcessorInstance != null) {
                    holder.NamedWindowProcessorInstance.RemoveIndex(index);
                }
            }
            else {
                foreach (var entry in _statementResourceService.ResourcesPartitioned) {
                    if (entry.Value.NamedWindowProcessorInstance != null) {
                        entry.Value.NamedWindowProcessorInstance.RemoveIndex(index);
                    }
                }
            }
        }

	    public void ValidateAddIndex(string statementName, string indexName, IndexMultiKey imk) {
	        _eventTableIndexMetadataRepo.AddIndex(false, imk, indexName, statementName, true);
	    }

	    public void RemoveIndexReferencesStmtMayRemoveIndex(IndexMultiKey imk, string finalStatementName) {
	        var last = _eventTableIndexMetadataRepo.RemoveIndexReference(imk, finalStatementName);
	        if (last) {
	            _eventTableIndexMetadataRepo.RemoveIndex(imk);
	            RemoveAllInstanceIndexes(imk);
	        }
	    }

        private void CheckAlreadyAllocated(StatementResourceHolder holder) {
            if (holder.NamedWindowProcessorInstance != null)
            {
                throw new IllegalStateException("Failed to allocated processor instance: already allocated and not released");
            }
        }
	}
} // end of namespace
