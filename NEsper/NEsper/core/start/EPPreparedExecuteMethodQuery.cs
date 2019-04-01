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
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.mgr;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.table;
using com.espertech.esper.epl.@join.@base;
using com.espertech.esper.epl.join.hint;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.spec;
using com.espertech.esper.events;
using com.espertech.esper.filter;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.core.start
{
    /// <summary>
    /// Starts and provides the stop method for EPL statements.
    /// </summary>
    public class EPPreparedExecuteMethodQuery : EPPreparedExecuteMethod
    {
        private static readonly ILog QueryPlanLog = LogManager.GetLogger(AuditPath.QUERYPLAN_LOG);
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly StatementSpecCompiled _statementSpec;
        private readonly ResultSetProcessor _resultSetProcessor;
        private readonly FireAndForgetProcessor[] _processors;
        private readonly AgentInstanceContext _agentInstanceContext;
        private readonly EPServicesContext _services;
        private readonly EventBeanReader _eventBeanReader;
        private readonly JoinSetComposerPrototype _joinSetComposerPrototype;
        private readonly QueryGraph _queryGraph;
        private readonly bool _hasTableAccess;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="statementSpec">
        /// is a container for the definition of all statement constructs that may have been used in the
        /// statement, i.e. if defines the select clauses, insert into, outer joins etc.
        /// </param>
        /// <param name="services">is the service instances for dependency injection</param>
        /// <param name="statementContext">is statement-level information and statement services</param>
        /// <throws>ExprValidationException if the preparation failed</throws>
        public EPPreparedExecuteMethodQuery(
            StatementSpecCompiled statementSpec,
            EPServicesContext services,
            StatementContext statementContext)
        {
            var queryPlanLogging = services.ConfigSnapshot.EngineDefaults.Logging.IsEnableQueryPlan;
            if (queryPlanLogging)
            {
                QueryPlanLog.Info("Query plans for Fire-and-forget query '" + statementContext.Expression + "'");
            }

            _hasTableAccess = (statementSpec.TableNodes != null && statementSpec.TableNodes.Length > 0);
            foreach (var streamSpec in statementSpec.StreamSpecs)
            {
                _hasTableAccess |= streamSpec is TableQueryStreamSpec;
            }

            _statementSpec = statementSpec;
            _services = services;

            EPPreparedExecuteMethodHelper.ValidateFAFQuery(statementSpec);

            var numStreams = statementSpec.StreamSpecs.Length;
            var typesPerStream = new EventType[numStreams];
            var namesPerStream = new string[numStreams];
            _processors = new FireAndForgetProcessor[numStreams];
            _agentInstanceContext = new AgentInstanceContext(statementContext, null, -1, null, null, statementContext.DefaultAgentInstanceScriptContext);

            // resolve types and processors
            for (var i = 0; i < numStreams; i++)
            {
                var streamSpec = statementSpec.StreamSpecs[i];
                _processors[i] = FireAndForgetProcessorFactory.ValidateResolveProcessor(streamSpec, services);

                string streamName = _processors[i].NamedWindowOrTableName;
                if (streamSpec.OptionalStreamName != null)
                {
                    streamName = streamSpec.OptionalStreamName;
                }
                namesPerStream[i] = streamName;
                typesPerStream[i] = _processors[i].EventTypeResultSetProcessor;
            }

            // compile filter to optimize access to named window
            var types = new StreamTypeServiceImpl(typesPerStream, namesPerStream, new bool[numStreams], services.EngineURI, false);
            var excludePlanHint = ExcludePlanHint.GetHint(types.StreamNames, statementContext);
            _queryGraph = new QueryGraph(numStreams, excludePlanHint, false);

            if (statementSpec.FilterRootNode != null)
            {
                for (var i = 0; i < numStreams; i++)
                {
                    try
                    {
                        ExprEvaluatorContextStatement evaluatorContextStmt = new ExprEvaluatorContextStatement(statementContext, false);
                        ExprValidationContext validationContext = new ExprValidationContext(
                            statementContext.Container,
                            types, 
                            statementContext.EngineImportService, 
                            statementContext.StatementExtensionServicesContext, null, 
                            statementContext.TimeProvider, 
                            statementContext.VariableService,
                            statementContext.TableService, evaluatorContextStmt,
                            statementContext.EventAdapterService, 
                            statementContext.StatementName, 
                            statementContext.StatementId, 
                            statementContext.Annotations, 
                            statementContext.ContextDescriptor, 
                            statementContext.ScriptingService,
                            false, false, true, false, null, true);
                        ExprNode validated = ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.FILTER, statementSpec.FilterRootNode, validationContext);
                        FilterExprAnalyzer.Analyze(validated, _queryGraph, false);
                    }
                    catch (Exception ex)
                    {
                        Log.Warn("Unexpected exception analyzing filter paths: " + ex.Message, ex);
                    }
                }
            }

            // obtain result set processor
            var isIStreamOnly = new bool[namesPerStream.Length];
            CompatExtensions.Fill(isIStreamOnly, true);
            StreamTypeService typeService = new StreamTypeServiceImpl(typesPerStream, namesPerStream, isIStreamOnly, services.EngineURI, true);
            EPStatementStartMethodHelperValidate.ValidateNodes(statementSpec, statementContext, typeService, null);

            var resultSetProcessorPrototype = ResultSetProcessorFactoryFactory.GetProcessorPrototype(statementSpec, statementContext, typeService, null, new bool[0], true, ContextPropertyRegistryImpl.EMPTY_REGISTRY, null, services.ConfigSnapshot, services.ResultSetProcessorHelperFactory, true, false);
            _resultSetProcessor = EPStatementStartMethodHelperAssignExpr.GetAssignResultSetProcessor(_agentInstanceContext, resultSetProcessorPrototype, false, null, true);

            if (statementSpec.SelectClauseSpec.IsDistinct)
            {
                if (_resultSetProcessor.ResultEventType is EventTypeSPI)
                {
                    _eventBeanReader = ((EventTypeSPI)_resultSetProcessor.ResultEventType).Reader;
                }
                if (_eventBeanReader == null)
                {
                    _eventBeanReader = new EventBeanReaderDefaultImpl(_resultSetProcessor.ResultEventType);
                }
            }

            // check context partition use
            if (statementSpec.OptionalContextName != null)
            {
                if (numStreams > 1)
                {
                    throw new ExprValidationException("Joins in runtime queries for context partitions are not supported");
                }
            }

            // plan joins or simple queries
            if (numStreams > 1)
            {
                var streamJoinAnalysisResult = new StreamJoinAnalysisResult(numStreams);
                CompatExtensions.Fill(streamJoinAnalysisResult.NamedWindow, true);
                for (var i = 0; i < numStreams; i++)
                {
                    var processorInstance = _processors[i].GetProcessorInstance(_agentInstanceContext);
                    if (_processors[i].IsVirtualDataWindow)
                    {
                        streamJoinAnalysisResult.ViewExternal[i] = agentInstanceContext => processorInstance.VirtualDataWindow;
                    }
                    var uniqueIndexes = _processors[i].GetUniqueIndexes(processorInstance);
                    streamJoinAnalysisResult.UniqueKeys[i] = uniqueIndexes;
                }

                var hasAggregations = !resultSetProcessorPrototype.AggregationServiceFactoryDesc.Expressions.IsEmpty();

                _joinSetComposerPrototype = JoinSetComposerPrototypeFactory.MakeComposerPrototype(null, -1,
                    statementSpec.OuterJoinDescList, statementSpec.FilterRootNode, typesPerStream, namesPerStream,
                    streamJoinAnalysisResult, queryPlanLogging, statementContext, new HistoricalViewableDesc(numStreams),
                    _agentInstanceContext, false, hasAggregations, services.TableService, true,
                    services.EventTableIndexService.AllowInitIndex(false));
            }
        }

        /// <summary>
        /// Returns the event type of the prepared statement.
        /// </summary>
        /// <value>event type</value>
        public EventType EventType
        {
            get { return _resultSetProcessor.ResultEventType; }
        }

        /// <summary>
        /// Executes the prepared query.
        /// </summary>
        /// <returns>query results</returns>
        public EPPreparedQueryResult Execute(ContextPartitionSelector[] contextPartitionSelectors)
        {
            try
            {
                var numStreams = _processors.Length;

                if (contextPartitionSelectors != null && contextPartitionSelectors.Length != numStreams)
                {
                    throw new ArgumentException("Number of context partition selectors does not match the number of named windows in the from-clause");
                }

                // handle non-context case
                if (_statementSpec.OptionalContextName == null)
                {

                    ICollection<EventBean>[] snapshots = new ICollection<EventBean>[numStreams];
                    for (var i = 0; i < numStreams; i++)
                    {

                        var selector = contextPartitionSelectors == null ? null : contextPartitionSelectors[i];
                        snapshots[i] = GetStreamFilterSnapshot(i, selector);
                    }

                    _resultSetProcessor.Clear();
                    return Process(snapshots);
                }

                IList<ContextPartitionResult> contextPartitionResults = new List<ContextPartitionResult>();
                var singleSelector = contextPartitionSelectors != null && contextPartitionSelectors.Length > 0 ? contextPartitionSelectors[0] : null;

                // context partition runtime query
                ICollection<int> agentInstanceIds = EPPreparedExecuteMethodHelper.GetAgentInstanceIds(
                    _processors[0], singleSelector, _services.ContextManagementService, _statementSpec.OptionalContextName);

                // collect events and agent instances
                foreach (int agentInstanceId in agentInstanceIds)
                {
                    var processorInstance = _processors[0].GetProcessorInstanceContextById(agentInstanceId);
                    if (processorInstance != null)
                    {
                        EPPreparedExecuteTableHelper.AssignTableAccessStrategies(_services, _statementSpec.TableNodes, processorInstance.AgentInstanceContext);
                        var coll = processorInstance.SnapshotBestEffort(this, _queryGraph, _statementSpec.Annotations);
                        contextPartitionResults.Add(new ContextPartitionResult(coll, processorInstance.AgentInstanceContext));
                    }
                }

                // process context partitions
                var events = new ArrayDeque<EventBean[]>();
                foreach (var contextPartitionResult in contextPartitionResults)
                {
                    var snapshot = contextPartitionResult.Events;
                    if (_statementSpec.FilterRootNode != null)
                    {
                        snapshot = GetFiltered(snapshot, Collections.SingletonList(_statementSpec.FilterRootNode));
                    }

                    EventBean[] rows;

                    var snapshotAsArrayDeque = snapshot as ArrayDeque<EventBean>;
                    if (snapshotAsArrayDeque != null)
                    {
                        rows = snapshotAsArrayDeque.Array;
                    }
                    else
                    {
                        rows = snapshot.ToArray();
                    }

                    _resultSetProcessor.AgentInstanceContext = contextPartitionResult.Context;
                    var results = _resultSetProcessor.ProcessViewResult(rows, null, true);
                    if (results != null && results.First != null && results.First.Length > 0)
                    {
                        events.Add(results.First);
                    }
                }
                return new EPPreparedQueryResult(_resultSetProcessor.ResultEventType, EventBeanUtility.Flatten(events));
            }
            finally
            {
                if (_hasTableAccess)
                {
                    _services.TableService.TableExprEvaluatorContext.ReleaseAcquiredLocks();
                }
            }
        }

        private ICollection<EventBean> GetStreamFilterSnapshot(int streamNum, ContextPartitionSelector contextPartitionSelector)
        {
            var streamSpec = _statementSpec.StreamSpecs[streamNum];
            IList<ExprNode> filterExpressions = Collections.GetEmptyList<ExprNode>();
            if (streamSpec is NamedWindowConsumerStreamSpec)
            {
                var namedSpec = (NamedWindowConsumerStreamSpec)streamSpec;
                filterExpressions = namedSpec.FilterExpressions;
            }
            else
            {
                var tableSpec = (TableQueryStreamSpec)streamSpec;
                filterExpressions = tableSpec.FilterExpressions;
            }

            var fireAndForgetProcessor = _processors[streamNum];

            // handle the case of a single or matching agent instance
            var processorInstance = fireAndForgetProcessor.GetProcessorInstance(_agentInstanceContext);
            if (processorInstance != null)
            {
                EPPreparedExecuteTableHelper.AssignTableAccessStrategies(_services, _statementSpec.TableNodes, _agentInstanceContext);
                return GetStreamSnapshotInstance(streamNum, filterExpressions, processorInstance);
            }

            // context partition runtime query
            var contextPartitions = EPPreparedExecuteMethodHelper.GetAgentInstanceIds(fireAndForgetProcessor, contextPartitionSelector, _services.ContextManagementService, fireAndForgetProcessor.ContextName);

            // collect events
            var events = new ArrayDeque<EventBean>();
            foreach (int agentInstanceId in contextPartitions)
            {
                processorInstance = fireAndForgetProcessor.GetProcessorInstanceContextById(agentInstanceId);
                if (processorInstance != null)
                {
                    var coll = processorInstance.SnapshotBestEffort(this, _queryGraph, _statementSpec.Annotations);
                    events.AddAll(coll);
                }
            }
            return events;
        }

        private ICollection<EventBean> GetStreamSnapshotInstance(
            int streamNum,
            IList<ExprNode> filterExpressions,
            FireAndForgetInstance processorInstance)
        {
            var coll = processorInstance.SnapshotBestEffort(this, _queryGraph, _statementSpec.Annotations);
            if (filterExpressions.Count != 0)
            {
                coll = GetFiltered(coll, filterExpressions);
            }
            return coll;
        }

        private EPPreparedQueryResult Process(ICollection<EventBean>[] snapshots)
        {
            var numStreams = _processors.Length;

            UniformPair<EventBean[]> results;
            if (numStreams == 1)
            {
                if (_statementSpec.FilterRootNode != null)
                {
                    snapshots[0] = GetFiltered(snapshots[0], _statementSpec.FilterRootNode.AsSingleton());
                }
                EventBean[] rows = snapshots[0].ToArray();
                results = _resultSetProcessor.ProcessViewResult(rows, null, true);
            }
            else
            {
                var viewablePerStream = new Viewable[numStreams];
                for (var i = 0; i < numStreams; i++)
                {
                    var instance = _processors[i].GetProcessorInstance(_agentInstanceContext);
                    if (instance == null)
                    {
                        throw new UnsupportedOperationException("Joins against named windows that are under context are not supported");
                    }
                    viewablePerStream[i] = instance.TailViewInstance;
                }

                var joinSetComposerDesc = _joinSetComposerPrototype.Create(viewablePerStream, true, _agentInstanceContext, false);
                var joinComposer = joinSetComposerDesc.JoinSetComposer;
                JoinSetFilter joinFilter;
                if (joinSetComposerDesc.PostJoinFilterEvaluator != null)
                {
                    joinFilter = new JoinSetFilter(joinSetComposerDesc.PostJoinFilterEvaluator);
                }
                else
                {
                    joinFilter = null;
                }

                var oldDataPerStream = new EventBean[numStreams][];
                var newDataPerStream = new EventBean[numStreams][];
                for (var i = 0; i < numStreams; i++)
                {
                    newDataPerStream[i] = snapshots[i].ToArray();
                }
                var result = joinComposer.Join(newDataPerStream, oldDataPerStream, _agentInstanceContext);
                if (joinFilter != null)
                {
                    joinFilter.Process(result.First, null, _agentInstanceContext);
                }
                results = _resultSetProcessor.ProcessJoinResult(result.First, null, true);
            }

            if (_statementSpec.SelectClauseSpec.IsDistinct)
            {
                results.First = EventBeanUtility.GetDistinctByProp(results.First, _eventBeanReader);
            }

            return new EPPreparedQueryResult(_resultSetProcessor.ResultEventType, results.First);
        }

        private ICollection<EventBean> GetFiltered(ICollection<EventBean> snapshot, IList<ExprNode> filterExpressions)
        {
            var deque = new ArrayDeque<EventBean>(Math.Min(snapshot.Count + 1, 16));
            var visitable = snapshot as IVisitable<EventBean>;
            ExprNodeUtility.ApplyFilterExpressionsIterable(snapshot, filterExpressions, _agentInstanceContext, deque);
            return deque;
        }

        public EPServicesContext Services
        {
            get { return _services; }
        }

        public ExprTableAccessNode[] TableNodes
        {
            get { return _statementSpec.TableNodes; }
        }

        public AgentInstanceContext AgentInstanceContext
        {
            get { return _agentInstanceContext; }
        }

        internal class ContextPartitionResult
        {
            private readonly ICollection<EventBean> _events;
            private readonly AgentInstanceContext _context;

            internal ContextPartitionResult(ICollection<EventBean> events, AgentInstanceContext context)
            {
                _events = events;
                _context = context;
            }

            public ICollection<EventBean> Events
            {
                get { return _events; }
            }

            public AgentInstanceContext Context
            {
                get { return _context; }
            }
        }
    }
}
