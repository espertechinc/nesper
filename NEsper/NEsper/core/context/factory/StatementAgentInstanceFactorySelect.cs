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
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.activator;
using com.espertech.esper.core.context.subselect;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.core.start;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.prev;
using com.espertech.esper.epl.expression.prior;
using com.espertech.esper.epl.expression.subquery;
using com.espertech.esper.epl.expression.table;
using com.espertech.esper.epl.@join.@base;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.named;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.view;
using com.espertech.esper.filter;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.pattern;
using com.espertech.esper.rowregex;
using com.espertech.esper.util;
using com.espertech.esper.view;
using com.espertech.esper.view.internals;

namespace com.espertech.esper.core.context.factory
{
    /// <summary>
    /// Defines the <see cref="StatementAgentInstanceFactorySelect" />
    /// </summary>
    public class StatementAgentInstanceFactorySelect : StatementAgentInstanceFactoryBase
    {
        /// <summary>
        /// Defines the Log
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Defines the _numStreams
        /// </summary>
        private readonly int _numStreams;

        /// <summary>
        /// Defines the _eventStreamParentViewableActivators
        /// </summary>
        private readonly ViewableActivator[] _eventStreamParentViewableActivators;

        /// <summary>
        /// Defines the _statementContext
        /// </summary>
        private readonly StatementContext _statementContext;

        /// <summary>
        /// Defines the _statementSpec
        /// </summary>
        private readonly StatementSpecCompiled _statementSpec;

        /// <summary>
        /// Defines the _services
        /// </summary>
        private readonly EPServicesContext _services;

        /// <summary>
        /// Defines the _typeService
        /// </summary>
        private readonly StreamTypeService _typeService;

        /// <summary>
        /// Defines the _unmaterializedViewChain
        /// </summary>
        private readonly ViewFactoryChain[] _unmaterializedViewChain;

        /// <summary>
        /// Defines the _resultSetProcessorFactoryDesc
        /// </summary>
        private readonly ResultSetProcessorFactoryDesc _resultSetProcessorFactoryDesc;

        /// <summary>
        /// Defines the _joinAnalysisResult
        /// </summary>
        private readonly StreamJoinAnalysisResult _joinAnalysisResult;

        /// <summary>
        /// Defines the _joinSetComposerPrototype
        /// </summary>
        private readonly JoinSetComposerPrototype _joinSetComposerPrototype;

        /// <summary>
        /// Defines the _subSelectStrategyCollection
        /// </summary>
        private readonly SubSelectStrategyCollection _subSelectStrategyCollection;

        /// <summary>
        /// Defines the _viewResourceDelegate
        /// </summary>
        private readonly ViewResourceDelegateVerified _viewResourceDelegate;

        /// <summary>
        /// Defines the _outputProcessViewFactory
        /// </summary>
        private readonly OutputProcessViewFactory _outputProcessViewFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="StatementAgentInstanceFactorySelect"/> class.
        /// </summary>
        /// <param name="numStreams">The <see cref="int"/></param>
        /// <param name="eventStreamParentViewableActivators">The <see cref="ViewableActivator"/> array</param>
        /// <param name="statementContext">The <see cref="StatementContext"/></param>
        /// <param name="statementSpec">The <see cref="StatementSpecCompiled"/></param>
        /// <param name="services">The <see cref="EPServicesContext"/></param>
        /// <param name="typeService">The <see cref="StreamTypeService"/></param>
        /// <param name="unmaterializedViewChain">The <see cref="ViewFactoryChain"/> array</param>
        /// <param name="resultSetProcessorFactoryDesc">The <see cref="ResultSetProcessorFactoryDesc"/></param>
        /// <param name="joinAnalysisResult">The <see cref="StreamJoinAnalysisResult"/></param>
        /// <param name="recoveringResilient">The <see cref="bool"/></param>
        /// <param name="joinSetComposerPrototype">The <see cref="JoinSetComposerPrototype"/></param>
        /// <param name="subSelectStrategyCollection">The <see cref="SubSelectStrategyCollection"/></param>
        /// <param name="viewResourceDelegate">The <see cref="ViewResourceDelegateVerified"/></param>
        /// <param name="outputProcessViewFactory">The <see cref="OutputProcessViewFactory"/></param>
        public StatementAgentInstanceFactorySelect(
            int numStreams,
            ViewableActivator[] eventStreamParentViewableActivators,
            StatementContext statementContext,
            StatementSpecCompiled statementSpec,
            EPServicesContext services,
            StreamTypeService typeService,
            ViewFactoryChain[] unmaterializedViewChain,
            ResultSetProcessorFactoryDesc resultSetProcessorFactoryDesc,
            StreamJoinAnalysisResult joinAnalysisResult,
            bool recoveringResilient,
            JoinSetComposerPrototype joinSetComposerPrototype,
            SubSelectStrategyCollection subSelectStrategyCollection,
            ViewResourceDelegateVerified viewResourceDelegate,
            OutputProcessViewFactory outputProcessViewFactory) :
            base(statementSpec.Annotations)
        {
            _numStreams = numStreams;
            _eventStreamParentViewableActivators = eventStreamParentViewableActivators;
            _statementContext = statementContext;
            _statementSpec = statementSpec;
            _services = services;
            _typeService = typeService;
            _unmaterializedViewChain = unmaterializedViewChain;
            _resultSetProcessorFactoryDesc = resultSetProcessorFactoryDesc;
            _joinAnalysisResult = joinAnalysisResult;
            _joinSetComposerPrototype = joinSetComposerPrototype;
            _subSelectStrategyCollection = subSelectStrategyCollection;
            _viewResourceDelegate = viewResourceDelegate;
            _outputProcessViewFactory = outputProcessViewFactory;
        }

        /// <summary>
        /// Gets the ViewResourceDelegate
        /// </summary>
        public ViewResourceDelegateVerified ViewResourceDelegate
        {
            get { return _viewResourceDelegate; }
        }

        /// <summary>
        /// The NewContextInternal
        /// </summary>
        /// <param name="agentInstanceContext">The <see cref="AgentInstanceContext"/></param>
        /// <param name="isRecoveringResilient">The <see cref="bool"/></param>
        /// <returns>The <see cref="StatementAgentInstanceFactoryResult"/></returns>
        protected override StatementAgentInstanceFactoryResult NewContextInternal(AgentInstanceContext agentInstanceContext, bool isRecoveringResilient)
        {
            IList<StopCallback> stopCallbacks = new List<StopCallback>(2);

            Viewable finalView;
            var viewableActivationResult = new ViewableActivationResult[_eventStreamParentViewableActivators.Length];
            IDictionary<ExprSubselectNode, SubSelectStrategyHolder> subselectStrategies;
            AggregationService aggregationService;
            Viewable[] streamViews;
            Viewable[] eventStreamParentViewable;
            Viewable[] topViews;
            IDictionary<ExprPriorNode, ExprPriorEvalStrategy> priorNodeStrategies;
            IDictionary<ExprPreviousNode, ExprPreviousEvalStrategy> previousNodeStrategies;
            IDictionary<ExprTableAccessNode, ExprTableAccessEvalStrategy> tableAccessStrategies;
            RegexExprPreviousEvalStrategy regexExprPreviousEvalStrategy = null;
            IList<StatementAgentInstancePreload> preloadList = new List<StatementAgentInstancePreload>();
            EvalRootState[] patternRoots;
            StatementAgentInstancePostLoad postLoadJoin = null;
            var suppressSameEventMatches = false;
            var discardPartialsOnMatch = false;
            EvalRootMatchRemover evalRootMatchRemover = null;

            try
            {
                // create root viewables
                eventStreamParentViewable = new Viewable[_numStreams];
                patternRoots = new EvalRootState[_numStreams];

                for (var stream = 0; stream < _eventStreamParentViewableActivators.Length; stream++)
                {
                    var activationResult = _eventStreamParentViewableActivators[stream].Activate(agentInstanceContext, false, isRecoveringResilient);
                    viewableActivationResult[stream] = activationResult;
                    stopCallbacks.Add(activationResult.StopCallback);
                    suppressSameEventMatches = activationResult.IsSuppressSameEventMatches;
                    discardPartialsOnMatch = activationResult.IsDiscardPartialsOnMatch;

                    // add stop callback for any stream-level viewable when applicable
                    if (activationResult.Viewable is StopCallback)
                    {
                        stopCallbacks.Add((StopCallback)activationResult.Viewable);
                    }

                    eventStreamParentViewable[stream] = activationResult.Viewable;
                    patternRoots[stream] = activationResult.OptionalPatternRoot;
                    if (stream == 0)
                    {
                        evalRootMatchRemover = activationResult.OptEvalRootMatchRemover;
                    }

                    if (activationResult.OptionalLock != null)
                    {
                        agentInstanceContext.EpStatementAgentInstanceHandle.StatementAgentInstanceLock = activationResult.OptionalLock;
                        _statementContext.DefaultAgentInstanceLock = activationResult.OptionalLock;
                    }
                }

                // compile view factories adding "prior" as necessary
                var viewFactoryChains = new IList<ViewFactory>[_numStreams];
                for (var i = 0; i < _numStreams; i++)
                {
                    var viewFactoryChain = _unmaterializedViewChain[i].FactoryChain;

                    // add "prior" view factory
                    var hasPrior = _viewResourceDelegate.PerStream[i].PriorRequests != null && !_viewResourceDelegate.PerStream[i].PriorRequests.IsEmpty();
                    if (hasPrior)
                    {
                        var priorEventViewFactory = EPStatementStartMethodHelperPrior.GetPriorEventViewFactory(
                            agentInstanceContext.StatementContext, i, viewFactoryChain.IsEmpty(), false, -1);
                        viewFactoryChain = new List<ViewFactory>(viewFactoryChain);
                        viewFactoryChain.Add(priorEventViewFactory);
                    }
                    viewFactoryChains[i] = viewFactoryChain;
                }

                // create view factory chain context: holds stream-specific services
                var viewFactoryChainContexts = new AgentInstanceViewFactoryChainContext[_numStreams];
                for (var i = 0; i < _numStreams; i++)
                {
                    viewFactoryChainContexts[i] = AgentInstanceViewFactoryChainContext.Create(viewFactoryChains[i], agentInstanceContext, _viewResourceDelegate.PerStream[i]);
                }

                // handle "prior" nodes and their strategies
                priorNodeStrategies = EPStatementStartMethodHelperPrior.CompilePriorNodeStrategies(_viewResourceDelegate, viewFactoryChainContexts);

                // handle "previous" nodes and their strategies
                previousNodeStrategies = EPStatementStartMethodHelperPrevious.CompilePreviousNodeStrategies(_viewResourceDelegate, viewFactoryChainContexts);

                // materialize views
                streamViews = new Viewable[_numStreams];
                topViews = new Viewable[_numStreams];
                for (var i = 0; i < _numStreams; i++)
                {
                    var hasPreviousNode = _viewResourceDelegate.PerStream[i].PreviousRequests != null && !_viewResourceDelegate.PerStream[i].PreviousRequests.IsEmpty();

                    var createResult = _services.ViewService.CreateViews(eventStreamParentViewable[i], viewFactoryChains[i], viewFactoryChainContexts[i], hasPreviousNode);
                    topViews[i] = createResult.TopViewable;
                    streamViews[i] = createResult.FinalViewable;

                    var isReuseableView = _eventStreamParentViewableActivators[i] is ViewableActivatorStreamReuseView;
                    if (isReuseableView)
                    {
                        var viewsCreated = createResult.NewViews;
                        var stopCallback = new ProxyStopCallback(() =>
                        {
                            ViewServiceHelper.RemoveFirstUnsharedView(viewsCreated);
                        });
                        stopCallbacks.Add(stopCallback);
                    }

                    // add views to stop callback if applicable
                    AddViewStopCallback(stopCallbacks, createResult.NewViews);
                }

                // determine match-recognize "previous"-node strategy (none if not present, or one handling and number of nodes)
                var matchRecognize = EventRowRegexHelper.RecursiveFindRegexService(topViews[0]);
                if (matchRecognize != null)
                {
                    regexExprPreviousEvalStrategy = matchRecognize.PreviousEvaluationStrategy;
                    stopCallbacks.Add(new ProxyStopCallback(matchRecognize.Stop));
                }

                // start subselects
                subselectStrategies = EPStatementStartMethodHelperSubselect.StartSubselects(_services, _subSelectStrategyCollection, agentInstanceContext, stopCallbacks, isRecoveringResilient);

                // plan table access
                tableAccessStrategies = EPStatementStartMethodHelperTableAccess.AttachTableAccess(_services, agentInstanceContext, _statementSpec.TableNodes);

                // obtain result set processor and aggregation services
                var processorPair = EPStatementStartMethodHelperUtil.StartResultSetAndAggregation(_resultSetProcessorFactoryDesc, agentInstanceContext, false, null);
                var resultSetProcessor = processorPair.First;
                aggregationService = processorPair.Second;
                stopCallbacks.Add(aggregationService);
                stopCallbacks.Add(resultSetProcessor);

                // for just 1 event stream without joins, handle the one-table process separately.
                JoinPreloadMethod joinPreloadMethod;
                JoinSetComposerDesc joinSetComposer = null;
                if (streamViews.Length == 1)
                {
                    finalView = HandleSimpleSelect(streamViews[0], resultSetProcessor, agentInstanceContext, evalRootMatchRemover, suppressSameEventMatches, discardPartialsOnMatch);
                    joinPreloadMethod = null;
                }
                else
                {
                    var joinPlanResult = HandleJoin(_typeService.StreamNames, streamViews, resultSetProcessor,
                            agentInstanceContext, stopCallbacks, _joinAnalysisResult, isRecoveringResilient);
                    finalView = joinPlanResult.Viewable;
                    joinPreloadMethod = joinPlanResult.PreloadMethod;
                    joinSetComposer = joinPlanResult.JoinSetComposerDesc;
                }

                // for stoppable final views, add callback
                if (finalView is StopCallback)
                {
                    stopCallbacks.Add((StopCallback)finalView);
                }

                // Replay any named window data, for later consumers of named data windows
                if (_services.EventTableIndexService.AllowInitIndex(isRecoveringResilient))
                {
                    var hasNamedWindow = false;
                    var namedWindowPostloadFilters = new QueryGraph[_statementSpec.StreamSpecs.Length];
                    var namedWindowTailViews = new NamedWindowTailViewInstance[_statementSpec.StreamSpecs.Length];
                    var namedWindowFilters = new IList<ExprNode>[_statementSpec.StreamSpecs.Length];

                    for (var i = 0; i < _statementSpec.StreamSpecs.Length; i++)
                    {
                        var streamNum = i;
                        var streamSpec = _statementSpec.StreamSpecs[i];

                        if (streamSpec is NamedWindowConsumerStreamSpec)
                        {
                            hasNamedWindow = true;
                            var namedSpec = (NamedWindowConsumerStreamSpec)streamSpec;
                            var processor = _services.NamedWindowMgmtService.GetProcessor(namedSpec.WindowName);
                            var processorInstance = processor.GetProcessorInstance(agentInstanceContext);
                            if (processorInstance != null)
                            {
                                var consumerView = processorInstance.TailViewInstance;
                                namedWindowTailViews[i] = consumerView;
                                var view = (NamedWindowConsumerView)viewableActivationResult[i].Viewable;

                                // determine preload/postload filter for index access
                                if (!namedSpec.FilterExpressions.IsEmpty())
                                {
                                    namedWindowFilters[streamNum] = namedSpec.FilterExpressions;
                                    var streamName = streamSpec.OptionalStreamName != null ? streamSpec.OptionalStreamName : consumerView.EventType.Name;
                                    var types = new StreamTypeServiceImpl(consumerView.EventType, streamName, false, _services.EngineURI);
                                    namedWindowPostloadFilters[i] = ExprNodeUtility.ValidateFilterGetQueryGraphSafe(
                                        ExprNodeUtility.ConnectExpressionsByLogicalAndWhenNeeded(namedSpec.FilterExpressions), 
                                        _statementContext, types);
                                }

                                // preload view for stream unless the expiry policy is batch window
                                var preload = !consumerView.TailView.IsParentBatchWindow && consumerView.HasFirst();
                                if (preload)
                                {
                                    if (isRecoveringResilient && _numStreams < 2)
                                    {
                                        preload = false;
                                    }
                                }
                                if (preload)
                                {
                                    var yesRecoveringResilient = isRecoveringResilient;
                                    var preloadFilterSpec = namedWindowPostloadFilters[i];
                                    preloadList.Add(new ProxyStatementAgentInstancePreload()
                                    {
                                        ProcExecutePreload = exprEvaluatorContext =>
                                        {
                                            var snapshot = consumerView.Snapshot(preloadFilterSpec, _statementContext.Annotations);
                                            var eventsInWindow = new List<EventBean>(snapshot.Count);
                                            ExprNodeUtility.ApplyFilterExpressionsIterable(snapshot, namedSpec.FilterExpressions, agentInstanceContext, eventsInWindow);
                                            EventBean[] newEvents = eventsInWindow.ToArray();
                                            view.Update(newEvents, null);
                                            if (!yesRecoveringResilient && joinPreloadMethod != null && !joinPreloadMethod.IsPreloading && agentInstanceContext.EpStatementAgentInstanceHandle.OptionalDispatchable != null)
                                            {
                                                agentInstanceContext.EpStatementAgentInstanceHandle.OptionalDispatchable.Execute();
                                            }
                                        },
                                    });
                                }
                            }
                            else
                            {
                                Log.Info("Named window access is out-of-context, the named window '" + namedSpec.WindowName + "' has been declared for a different context then the current statement, the aggregation and join state will not be initialized for statement expression [" + _statementContext.Expression + "]");
                            }

                            preloadList.Add(new ProxyStatementAgentInstancePreload()
                            {
                                ProcExecutePreload = exprEvaluatorContext =>
                                {
                                    // in a join, preload indexes, if any
                                    if (joinPreloadMethod != null)
                                    {
                                        joinPreloadMethod.PreloadFromBuffer(streamNum, exprEvaluatorContext);
                                    }
                                    else
                                    {
                                        if (agentInstanceContext.EpStatementAgentInstanceHandle.OptionalDispatchable != null)
                                        {
                                            agentInstanceContext.EpStatementAgentInstanceHandle.OptionalDispatchable.Execute();
                                        }
                                    }
                                },
                            });
                        }
                    }

                    // last, for aggregation we need to send the current join results to the result set processor
                    if ((hasNamedWindow) && (joinPreloadMethod != null) && (!isRecoveringResilient) && _resultSetProcessorFactoryDesc.ResultSetProcessorFactory.HasAggregation)
                    {
                        preloadList.Add(new ProxyStatementAgentInstancePreload()
                        {
                            ProcExecutePreload = exprEvaluatorContext =>
                            {
                                joinPreloadMethod.PreloadAggregation(resultSetProcessor);
                            },
                        });
                    }

                    if (isRecoveringResilient)
                    {
                        postLoadJoin = new StatementAgentInstancePostLoadSelect(streamViews, joinSetComposer, namedWindowTailViews, namedWindowPostloadFilters, namedWindowFilters, _statementContext.Annotations, agentInstanceContext);
                    }
                    else if (joinSetComposer != null)
                    {
                        postLoadJoin = new StatementAgentInstancePostLoadIndexVisiting(joinSetComposer.JoinSetComposer);
                    }
                }
            }
            catch (Exception)
            {
                var stopCallback = StatementAgentInstanceUtil.GetStopCallback(stopCallbacks, agentInstanceContext);
                StatementAgentInstanceUtil.StopSafe(stopCallback, _statementContext);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AContextPartitionAllocate(); }
                throw;
            }

            var selectResult = new StatementAgentInstanceFactorySelectResult(finalView, null, agentInstanceContext, aggregationService, subselectStrategies, priorNodeStrategies, previousNodeStrategies, regexExprPreviousEvalStrategy, tableAccessStrategies, preloadList, patternRoots, postLoadJoin, topViews, eventStreamParentViewable, viewableActivationResult);

            if (_statementContext.StatementExtensionServicesContext != null)
            {
                _statementContext.StatementExtensionServicesContext.ContributeStopCallback(selectResult, stopCallbacks);
            }
            var stopCallbackX = StatementAgentInstanceUtil.GetStopCallback(stopCallbacks, agentInstanceContext);
            selectResult.StopCallback = stopCallbackX;

            return selectResult;
        }

        /// <summary>
        /// The AddViewStopCallback
        /// </summary>
        /// <param name="stopCallbacks">The <see cref="IList{StopCallback}"/></param>
        /// <param name="topViews">The <see cref="IList{View}"/></param>
        internal static void AddViewStopCallback(IList<StopCallback> stopCallbacks, IList<View> topViews)
        {
            foreach (var view in topViews)
            {
                if (view is StoppableView)
                {
                    stopCallbacks.Add((StoppableView)view);
                }
            }
        }

        /// <summary>
        /// The AssignExpressions
        /// </summary>
        /// <param name="result">The <see cref="StatementAgentInstanceFactoryResult"/></param>
        public override void AssignExpressions(StatementAgentInstanceFactoryResult result)
        {
            var selectResult = (StatementAgentInstanceFactorySelectResult)result;
            EPStatementStartMethodHelperAssignExpr.AssignAggregations(selectResult.OptionalAggegationService, _resultSetProcessorFactoryDesc.AggregationServiceFactoryDesc.Expressions);
            EPStatementStartMethodHelperAssignExpr.AssignSubqueryStrategies(_subSelectStrategyCollection, result.SubselectStrategies);
            EPStatementStartMethodHelperAssignExpr.AssignPriorStrategies(result.PriorNodeStrategies);
            EPStatementStartMethodHelperAssignExpr.AssignPreviousStrategies(result.PreviousNodeStrategies);
            var matchRecognizeNodes = _viewResourceDelegate.PerStream[0].MatchRecognizePreviousRequests;
            EPStatementStartMethodHelperAssignExpr.AssignMatchRecognizePreviousStrategies(matchRecognizeNodes, result.RegexExprPreviousEvalStrategy);
        }

        /// <summary>
        /// The UnassignExpressions
        /// </summary>
        public override void UnassignExpressions()
        {
            EPStatementStartMethodHelperAssignExpr.UnassignAggregations(_resultSetProcessorFactoryDesc.AggregationServiceFactoryDesc.Expressions);
            EPStatementStartMethodHelperAssignExpr.UnassignSubqueryStrategies(_subSelectStrategyCollection.Subqueries.Keys);
            for (var streamNum = 0; streamNum < _viewResourceDelegate.PerStream.Length; streamNum++)
            {
                var viewResourceStream = _viewResourceDelegate.PerStream[streamNum];
                var callbacksPerIndex = viewResourceStream.PriorRequests;
                foreach (var priorItem in callbacksPerIndex)
                {
                    EPStatementStartMethodHelperAssignExpr.UnassignPriorStrategies(priorItem.Value);
                }
                EPStatementStartMethodHelperAssignExpr.UnassignPreviousStrategies(viewResourceStream.PreviousRequests);
            }
            var matchRecognizeNodes = _viewResourceDelegate.PerStream[0].MatchRecognizePreviousRequests;
            EPStatementStartMethodHelperAssignExpr.UnassignMatchRecognizePreviousStrategies(matchRecognizeNodes);
        }

        /// <summary>
        /// Gets the EventStreamParentViewableActivators
        /// </summary>
        public ViewableActivator[] EventStreamParentViewableActivators
        {
            get { return _eventStreamParentViewableActivators; }
        }

        /// <summary>
        /// Gets the UnmaterializedViewChain
        /// </summary>
        public ViewFactoryChain[] UnmaterializedViewChain
        {
            get { return _unmaterializedViewChain; }
        }

        /// <summary>
        /// Gets the NumStreams
        /// </summary>
        public int NumStreams
        {
            get { return _numStreams; }
        }

        /// <summary>
        /// Gets the StatementContext
        /// </summary>
        public StatementContext StatementContext
        {
            get { return _statementContext; }
        }

        /// <summary>
        /// Gets the StatementSpec
        /// </summary>
        public StatementSpecCompiled StatementSpec
        {
            get { return _statementSpec; }
        }

        /// <summary>
        /// Gets the Services
        /// </summary>
        public EPServicesContext Services
        {
            get { return _services; }
        }

        /// <summary>
        /// Gets the TypeService
        /// </summary>
        public StreamTypeService TypeService
        {
            get { return _typeService; }
        }

        /// <summary>
        /// Gets the ResultSetProcessorFactoryDesc
        /// </summary>
        public ResultSetProcessorFactoryDesc ResultSetProcessorFactoryDesc
        {
            get { return _resultSetProcessorFactoryDesc; }
        }

        /// <summary>
        /// Gets the JoinAnalysisResult
        /// </summary>
        public StreamJoinAnalysisResult JoinAnalysisResult
        {
            get { return _joinAnalysisResult; }
        }

        /// <summary>
        /// Gets the JoinSetComposerPrototype
        /// </summary>
        public JoinSetComposerPrototype JoinSetComposerPrototype
        {
            get { return _joinSetComposerPrototype; }
        }

        /// <summary>
        /// Gets the SubSelectStrategyCollection
        /// </summary>
        public SubSelectStrategyCollection SubSelectStrategyCollection
        {
            get { return _subSelectStrategyCollection; }
        }

        /// <summary>
        /// Gets the OutputProcessViewFactory
        /// </summary>
        public OutputProcessViewFactory OutputProcessViewFactory
        {
            get { return _outputProcessViewFactory; }
        }

        /// <summary>
        /// The HandleSimpleSelect
        /// </summary>
        /// <param name="view">The <see cref="Viewable"/></param>
        /// <param name="resultSetProcessor">The <see cref="ResultSetProcessor"/></param>
        /// <param name="agentInstanceContext">The <see cref="AgentInstanceContext"/></param>
        /// <param name="evalRootMatchRemover">The <see cref="EvalRootMatchRemover"/></param>
        /// <param name="suppressSameEventMatches">The <see cref="bool"/></param>
        /// <param name="discardPartialsOnMatch">The <see cref="bool"/></param>
        /// <returns>The <see cref="Viewable"/></returns>
        private Viewable HandleSimpleSelect(
            Viewable view,
            ResultSetProcessor resultSetProcessor,
            AgentInstanceContext agentInstanceContext,
            EvalRootMatchRemover evalRootMatchRemover,
            bool suppressSameEventMatches,
            bool discardPartialsOnMatch)
        {
            var finalView = view;

            // Add filter view that evaluates the filter expression
            if (_statementSpec.FilterRootNode != null)
            {
                var filterView = new FilterExprView(_statementSpec.FilterRootNode, _statementSpec.FilterRootNode.ExprEvaluator, agentInstanceContext);
                finalView.AddView(filterView);
                finalView = filterView;
            }

            Deque<EPStatementDispatch> dispatches = null;

            if (evalRootMatchRemover != null && (suppressSameEventMatches || discardPartialsOnMatch))
            {
                var v = new PatternRemoveDispatchView(evalRootMatchRemover, suppressSameEventMatches, discardPartialsOnMatch);
                dispatches = new ArrayDeque<EPStatementDispatch>(2);
                dispatches.Add(v);
                finalView.AddView(v);
                finalView = v;
            }

            // for ordered deliver without output limit/buffer
            if (_statementSpec.OrderByList.Length > 0 && (_statementSpec.OutputLimitSpec == null))
            {
                var bf = new SingleStreamDispatchView();
                if (dispatches == null)
                {
                    dispatches = new ArrayDeque<EPStatementDispatch>(1);
                }
                dispatches.Add(bf);
                finalView.AddView(bf);
                finalView = bf;
            }

            if (dispatches != null)
            {
                var handle = agentInstanceContext.EpStatementAgentInstanceHandle;
                if (dispatches.Count == 1)
                {
                    handle.OptionalDispatchable = dispatches.First;
                }
                else
                {
                    EPStatementDispatch[] dispatchArray = dispatches.ToArray();
                    handle.OptionalDispatchable = new ProxyEPStatementDispatch()
                    {
                        ProcExecute = () =>
                        {
                            foreach (var dispatch in dispatchArray)
                            {
                                dispatch.Execute();
                            }
                        },
                    };
                }
            }

            View selectView = _outputProcessViewFactory.MakeView(resultSetProcessor, agentInstanceContext);

            finalView.AddView(selectView);
            finalView = selectView;

            return finalView;
        }

        /// <summary>
        /// The HandleJoin
        /// </summary>
        /// <param name="streamNames">The stream names.</param>
        /// <param name="streamViews">The stream views.</param>
        /// <param name="resultSetProcessor">The result set processor.</param>
        /// <param name="agentInstanceContext">The agent instance context.</param>
        /// <param name="stopCallbacks">The stop callbacks.</param>
        /// <param name="joinAnalysisResult">The join analysis result.</param>
        /// <param name="isRecoveringResilient">if set to <c>true</c> [is recovering resilient].</param>
        /// <returns></returns>
        private JoinPlanResult HandleJoin(
            string[] streamNames,
            Viewable[] streamViews,
            ResultSetProcessor resultSetProcessor,
            AgentInstanceContext agentInstanceContext,
            IList<StopCallback> stopCallbacks,
            StreamJoinAnalysisResult joinAnalysisResult,
            bool isRecoveringResilient)
        {
            var joinSetComposerDesc = _joinSetComposerPrototype.Create(streamViews, false, agentInstanceContext, isRecoveringResilient);

            stopCallbacks.Add(new ProxyStopCallback(() => joinSetComposerDesc.JoinSetComposer.Destroy()));

            var filter = new JoinSetFilter(joinSetComposerDesc.PostJoinFilterEvaluator);
            var indicatorView = _outputProcessViewFactory.MakeView(resultSetProcessor, agentInstanceContext);

            // Create strategy for join execution
            JoinExecutionStrategy execution = new JoinExecutionStrategyImpl(joinSetComposerDesc.JoinSetComposer, filter, indicatorView, agentInstanceContext);

            // The view needs a reference to the join execution to pull iterator values
            indicatorView.JoinExecutionStrategy = execution;

            // Hook up dispatchable with buffer and execution strategy
            var joinStatementDispatch = new JoinExecStrategyDispatchable(execution, _statementSpec.StreamSpecs.Length);
            agentInstanceContext.EpStatementAgentInstanceHandle.OptionalDispatchable = joinStatementDispatch;

            JoinPreloadMethod preloadMethod;
            if (joinAnalysisResult.IsUnidirectional)
            {
                preloadMethod = new JoinPreloadMethodNull();
            }
            else
            {
                if (!joinSetComposerDesc.JoinSetComposer.AllowsInit)
                {
                    preloadMethod = new JoinPreloadMethodNull();
                }
                else
                {
                    preloadMethod = new JoinPreloadMethodImpl(streamNames.Length, joinSetComposerDesc.JoinSetComposer);
                }
            }

            // Create buffer for each view. Point buffer to dispatchable for join.
            for (var i = 0; i < _statementSpec.StreamSpecs.Length; i++)
            {
                var buffer = new BufferView(i);
                streamViews[i].AddView(buffer);
                buffer.Observer = joinStatementDispatch;
                preloadMethod.SetBuffer(buffer, i);
            }

            return new JoinPlanResult(indicatorView, preloadMethod, joinSetComposerDesc);
        }

        /// <summary>
        /// Defines the <see cref="JoinPlanResult" />
        /// </summary>
        internal class JoinPlanResult
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="JoinPlanResult"/> class.
            /// </summary>
            /// <param name="viewable">The <see cref="Viewable"/></param>
            /// <param name="preloadMethod">The <see cref="JoinPreloadMethod"/></param>
            /// <param name="joinSetComposerDesc">The <see cref="JoinSetComposerDesc"/></param>
            internal JoinPlanResult(Viewable viewable, JoinPreloadMethod preloadMethod, JoinSetComposerDesc joinSetComposerDesc)
            {
                Viewable = viewable;
                PreloadMethod = preloadMethod;
                JoinSetComposerDesc = joinSetComposerDesc;
            }

            /// <summary>
            /// Gets or sets the Viewable
            /// </summary>
            public Viewable Viewable { get; private set; }

            /// <summary>
            /// Gets or sets the PreloadMethod
            /// </summary>
            public JoinPreloadMethod PreloadMethod { get; private set; }

            /// <summary>
            /// Gets or sets the JoinSetComposerDesc
            /// </summary>
            public JoinSetComposerDesc JoinSetComposerDesc { get; private set; }
        }
    }
}
