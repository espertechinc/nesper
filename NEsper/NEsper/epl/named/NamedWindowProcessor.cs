///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.core.service.resource;
using com.espertech.esper.core.start;
using com.espertech.esper.epl.join.plan;
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
        private readonly ICollection<string> _optionalUniqueKeyProps;
        private readonly string _eventTypeAsName;
        private readonly EventTableIndexMetadata _eventTableIndexMetadataRepo = new EventTableIndexMetadata();
        private readonly StatementContext _statementContextCreateWindow;

        private readonly ILockable _lock;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="namedWindowName">Name of the named window.</param>
        /// <param name="namedWindowMgmtService">service for dispatching results</param>
        /// <param name="namedWindowDispatchService">The named window dispatch service.</param>
        /// <param name="contextName">Name of the context.</param>
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
        /// <param name="optionalUniqueKeyProps">The optional unique key props.</param>
        /// <param name="eventTypeAsName">Name of the event type as.</param>
        /// <param name="statementContextCreateWindow">The statement context create window.</param>
        /// <param name="lockManager">The lock manager.</param>
        public NamedWindowProcessor(
            string namedWindowName,
            NamedWindowMgmtService namedWindowMgmtService,
            NamedWindowDispatchService namedWindowDispatchService,
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
            ICollection<string> optionalUniqueKeyProps,
            string eventTypeAsName,
            StatementContext statementContextCreateWindow, ILockManager lockManager)
        {
            _namedWindowName = namedWindowName;
            _contextName = contextName;
            _eventType = eventType;
            _eplExpression = eplExpression;
            _statementName = statementName;
            _isEnableSubqueryIndexShare = isEnableSubqueryIndexShare;
            _isVirtualDataWindow = isVirtualDataWindow;
            _optionalUniqueKeyProps = optionalUniqueKeyProps;
            _eventTypeAsName = eventTypeAsName;
            _statementContextCreateWindow = statementContextCreateWindow;

            _rootView = new NamedWindowRootView(revisionProcessor, enableQueryPlanLog, metricReportingService, eventType, isBatchingDataWindow, isEnableSubqueryIndexShare, optionalUniqueKeyProps);
            _tailView = namedWindowDispatchService.CreateTailView(
                eventType, namedWindowMgmtService, namedWindowDispatchService, statementResultService, revisionProcessor,
                isPrioritized, isBatchingDataWindow, contextName, statementContextCreateWindow.TimeSourceService,
                statementContextCreateWindow.ConfigSnapshot.EngineDefaults.Threading);
            _lock = lockManager.CreateLock(MethodBase.GetCurrentMethod().DeclaringType);
        }

        public string EventTypeAsName => _eventTypeAsName;

        public NamedWindowProcessorInstance AddInstance(AgentInstanceContext agentInstanceContext)
        {
            using (_lock.Acquire())
            {
                if (_contextName == null)
                {
                    return new NamedWindowProcessorInstance(null, this, agentInstanceContext);
                }

                int instanceId = agentInstanceContext.AgentInstanceId;
                return new NamedWindowProcessorInstance(instanceId, this, agentInstanceContext);
            }
        }

        public NamedWindowProcessorInstance ProcessorInstanceNoContext
        {
            get
            {
                StatementResourceHolder holder =
                    _statementContextCreateWindow.StatementExtensionServicesContext.StmtResources.Unpartitioned;
                return holder == null ? null : holder.NamedWindowProcessorInstance;
            }
        }

        public NamedWindowProcessorInstance GetProcessorInstance(int agentInstanceId)
        {
            StatementResourceHolder holder = _statementContextCreateWindow.StatementExtensionServicesContext.StmtResources.GetPartitioned(agentInstanceId);
            return holder == null ? null : holder.NamedWindowProcessorInstance;
        }

        public NamedWindowProcessorInstance GetProcessorInstanceAllowUnpartitioned(int agentInstanceId)
        {
            if (agentInstanceId == EPStatementStartMethodConst.DEFAULT_AGENT_INSTANCE_ID)
            {
                return ProcessorInstanceNoContext;
            }
            StatementResourceHolder holder = _statementContextCreateWindow.StatementExtensionServicesContext.StmtResources.GetPartitioned(agentInstanceId);
            return holder == null ? null : holder.NamedWindowProcessorInstance;
        }

        public ICollection<int> ProcessorInstancesAll
        {
            get
            {
                using (_lock.Acquire())
                {
                    var keyset =
                        _statementContextCreateWindow.StatementExtensionServicesContext.StmtResources.ResourcesPartitioned.Keys;
                    return new ArrayDeque<int>(keyset);
                }
            }
        }

        public NamedWindowProcessorInstance GetProcessorInstance(AgentInstanceContext agentInstanceContext)
        {
            if (_contextName == null)
            {
                return ProcessorInstanceNoContext;
            }

            if (agentInstanceContext.StatementContext.ContextDescriptor == null)
            {
                return null;
            }

            if (_contextName.Equals(agentInstanceContext.StatementContext.ContextDescriptor.ContextName))
            {
                return GetProcessorInstance(agentInstanceContext.AgentInstanceId);
            }

            return null;
        }

        public string ContextName => _contextName;

        public NamedWindowConsumerView AddConsumer(NamedWindowConsumerDesc consumerDesc, bool isSubselect)
        {
            StatementResourceService statementResourceService = _statementContextCreateWindow.StatementExtensionServicesContext.StmtResources;

            // handle same-context consumer
            if (_contextName != null)
            {
                ContextDescriptor contextDescriptor = consumerDesc.AgentInstanceContext.StatementContext.ContextDescriptor;
                if (contextDescriptor != null && _contextName.Equals(contextDescriptor.ContextName))
                {
                    StatementResourceHolder holder = statementResourceService.GetPartitioned(consumerDesc.AgentInstanceContext.AgentInstanceId);
                    return holder.NamedWindowProcessorInstance.TailViewInstance.AddConsumer(consumerDesc, isSubselect);
                }
                else
                {
                    // consumer is out-of-context
                    return _tailView.AddConsumer(consumerDesc);  // non-context consumers
                }
            }

            // handle no context associated
            return statementResourceService.ResourcesUnpartitioned.NamedWindowProcessorInstance.TailViewInstance.AddConsumer(consumerDesc, isSubselect);
        }

        public bool IsVirtualDataWindow => _isVirtualDataWindow;

        /// <summary>
        /// Returns the tail view of the named window, hooked into the view chain after the named window's data window views,
        /// as the last view.
        /// </summary>
        /// <value>tail view</value>
        public NamedWindowTailView TailView => _tailView;

        /// <summary>
        /// Returns the root view of the named window, hooked into the view chain before the named window's data window views,
        /// right after the filter stream that filters for insert-into events.
        /// </summary>
        /// <value>tail view</value>
        public NamedWindowRootView RootView => _rootView;

        /// <summary>
        /// Returns the event type of the named window.
        /// </summary>
        /// <value>event type</value>
        public EventType NamedWindowType => _eventType;

        /// <summary>
        /// Returns the EPL expression.
        /// </summary>
        /// <value>epl</value>
        public string EplExpression => _eplExpression;

        /// <summary>
        /// Returns the statement name.
        /// </summary>
        /// <value>name</value>
        public string StatementName => _statementName;

        /// <summary>
        /// Deletes a named window and removes any associated resources.
        /// </summary>
        public void Dispose()
        {
        }

        public bool IsEnableSubqueryIndexShare => _isEnableSubqueryIndexShare;

        public StatementMetricHandle CreateNamedWindowMetricsHandle => _statementContextCreateWindow.EpStatementHandle.MetricsHandle;

        public string NamedWindowName => _namedWindowName;

        public string[][] UniqueIndexes
        {
            get
            {
                IList<string[]> unique = null;

                var indexDescriptors = EventTableIndexMetadataRepo.Indexes.Keys;
                foreach (var index in indexDescriptors)
                {
                    if (!index.IsUnique)
                    {
                        continue;
                    }
                    string[] uniqueKeys = IndexedPropDesc.GetIndexProperties(index.HashIndexedProps);
                    if (unique == null)
                    {
                        unique = new List<string[]>();
                    }
                    unique.Add(uniqueKeys);
                }
                if (_optionalUniqueKeyProps != null)
                {
                    if (unique == null)
                    {
                        unique = new List<string[]>();
                    }
                    unique.Add(_optionalUniqueKeyProps.ToArray());
                }
                if (unique == null)
                {
                    return null;
                }
                return unique.ToArray();
            }
        }

        public ICollection<string> OptionalUniqueKeyProps => _optionalUniqueKeyProps;

        public EventTableIndexMetadata EventTableIndexMetadataRepo => _eventTableIndexMetadataRepo;

        public StatementContext StatementContextCreateWindow => _statementContextCreateWindow;

        public void RemoveAllInstanceIndexes(IndexMultiKey index)
        {
            StatementResourceService statementResourceService = _statementContextCreateWindow.StatementExtensionServicesContext.StmtResources;

            if (_contextName == null)
            {
                StatementResourceHolder holder = statementResourceService.Unpartitioned;
                if (holder != null && holder.NamedWindowProcessorInstance != null)
                {
                    holder.NamedWindowProcessorInstance.RemoveIndex(index);
                }
            }
            else
            {
                foreach (var entry in statementResourceService.ResourcesPartitioned)
                {
                    if (entry.Value.NamedWindowProcessorInstance != null)
                    {
                        entry.Value.NamedWindowProcessorInstance.RemoveIndex(index);
                    }
                }
            }
        }

        public void ValidateAddIndex(string statementName, string explicitIndexName, QueryPlanIndexItem explicitIndexDesc, IndexMultiKey imk)
        {
            _eventTableIndexMetadataRepo.AddIndexExplicit(false, imk, explicitIndexName, explicitIndexDesc, statementName);
        }

        public void RemoveIndexReferencesStmtMayRemoveIndex(IndexMultiKey imk, string finalStatementName)
        {
            bool last = _eventTableIndexMetadataRepo.RemoveIndexReference(imk, finalStatementName);
            if (last)
            {
                _eventTableIndexMetadataRepo.RemoveIndex(imk);
                RemoveAllInstanceIndexes(imk);
            }
        }

        public void ClearProcessorInstances()
        {
            if (_contextName == null)
            {
                NamedWindowProcessorInstance instance = ProcessorInstanceNoContext;
                if (instance != null)
                {
                    instance.Dispose();
                }
                return;
            }
            var cpids = ProcessorInstancesAll;
            foreach (int cpid in cpids)
            {
                NamedWindowProcessorInstance instance = GetProcessorInstance(cpid);
                if (instance != null)
                {
                    instance.Dispose();
                }
                return;
            }
        }
    }
} // end of namespace
