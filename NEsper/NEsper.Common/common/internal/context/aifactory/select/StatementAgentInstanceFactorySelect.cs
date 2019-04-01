///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.activator;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.airegistry;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.prior;
using com.espertech.esper.common.@internal.epl.@join.@base;
using com.espertech.esper.common.@internal.epl.namedwindow.consume;
using com.espertech.esper.common.@internal.epl.output.core;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.epl.prior;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.rowrecog.core;
using com.espertech.esper.common.@internal.epl.subselect;
using com.espertech.esper.common.@internal.epl.table.strategy;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.access;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.filter;
using com.espertech.esper.common.@internal.view.previous;
using com.espertech.esper.common.@internal.view.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.aifactory.select
{
    public class StatementAgentInstanceFactorySelect : StatementAgentInstanceFactory
    {
        private JoinSetComposerPrototype joinSetComposerPrototype;
        private bool orderByWithoutOutputRateLimit;
        private OutputProcessViewFactoryProvider outputProcessViewFactoryProvider;
        private ResultSetProcessorFactoryProvider resultSetProcessorFactoryProvider;

        private string[] streamNames;
        private IDictionary<int, SubSelectFactory> subselects;
        private IDictionary<int, ExprTableEvalStrategyFactory> tableAccesses;
        private bool unidirectionalJoin;
        private ViewableActivator[] viewableActivators;
        private ViewFactory[][] viewFactories;
        private ViewResourceDelegateDesc[] viewResourceDelegates;
        private ExprEvaluator whereClauseEvaluator;
        private string whereClauseEvaluatorTextForAudit;

        public ViewableActivator[] ViewableActivators
        {
            set => viewableActivators = value;
        }

        public ResultSetProcessorFactoryProvider ResultSetProcessorFactoryProvider
        {
            set => resultSetProcessorFactoryProvider = value;
        }

        public ViewFactory[][] ViewFactories
        {
            set => viewFactories = value;
        }

        public OutputProcessViewFactoryProvider OutputProcessViewFactoryProvider
        {
            set => outputProcessViewFactoryProvider = value;
        }

        public ViewResourceDelegateDesc[] ViewResourceDelegates
        {
            set => viewResourceDelegates = value;
        }

        public ExprEvaluator WhereClauseEvaluator
        {
            set => whereClauseEvaluator = value;
        }

        public string[] StreamNames
        {
            set => streamNames = value;
        }

        public JoinSetComposerPrototype JoinSetComposerPrototype
        {
            set => joinSetComposerPrototype = value;
        }

        public IDictionary<int, SubSelectFactory> Subselects
        {
            set => subselects = value;
        }

        public bool OrderByWithoutOutputRateLimit
        {
            set => orderByWithoutOutputRateLimit = value;
        }

        public bool IsUnidirectionalJoin
        {
            set => unidirectionalJoin = value;
        }

        public IDictionary<int, ExprTableEvalStrategyFactory> TableAccesses
        {
            set => tableAccesses = value;
        }

        public string WhereClauseEvaluatorTextForAudit
        {
            set => whereClauseEvaluatorTextForAudit = value;
        }

        public void StatementCreate(StatementContext statementContext)
        {
        }

        public void StatementDestroy(StatementContext statementContext)
        {
        }

        public void StatementDestroyPreconditions(StatementContext statementContext)
        {
        }

        public StatementAgentInstanceLock ObtainAgentInstanceLock(
            StatementContext statementContext, int agentInstanceId)
        {
            return AgentInstanceUtil.NewLock(statementContext);
        }

        public StatementAgentInstanceFactoryResult NewContext(
            AgentInstanceContext agentInstanceContext, bool isRecoveringResilient)
        {
            IList<AgentInstanceStopCallback> stopCallbacks = new List<AgentInstanceStopCallback>();
            IList<StatementAgentInstancePreload> preloadList = new List<StatementAgentInstancePreload>();
            var numStreams = viewableActivators.Length;

            // root activations
            var activationResults = new ViewableActivationResult[numStreams];
            var eventStreamParentViewable = new Viewable[numStreams];
            EvalRootMatchRemover evalRootMatchRemover = null;
            var suppressSameEventMatches = false;
            var discardPartialsOnMatch = false;

            for (var stream = 0; stream < numStreams; stream++)
            {
                var activationResult = viewableActivators[stream].Activate(
                    agentInstanceContext, false, isRecoveringResilient);
                stopCallbacks.Add(activationResult.StopCallback);
                activationResults[stream] = activationResult;
                eventStreamParentViewable[stream] = activationResult.Viewable;
                suppressSameEventMatches = activationResult.IsSuppressSameEventMatches;
                discardPartialsOnMatch = activationResult.IsDiscardPartialsOnMatch;

                if (stream == 0)
                {
                    evalRootMatchRemover = activationResult.OptEvalRootMatchRemover;
                }
            }

            // create view factory chain context: holds stream-specific services
            var viewFactoryChainContexts = new AgentInstanceViewFactoryChainContext[numStreams];
            var priorEvalStrategies = new PriorEvalStrategy[numStreams];
            var previousGetterStrategies = new PreviousGetterStrategy[numStreams];
            RowRecogPreviousStrategy rowRecogPreviousStrategy = null;

            for (var i = 0; i < numStreams; i++)
            {
                viewFactoryChainContexts[i] = AgentInstanceViewFactoryChainContext.Create(
                    viewFactories[i], agentInstanceContext, viewResourceDelegates[i]);
                priorEvalStrategies[i] = PriorHelper.ToStrategy(viewFactoryChainContexts[i]);
                previousGetterStrategies[i] = viewFactoryChainContexts[i].PreviousNodeGetter;
            }

            // materialize views
            var topViews = new Viewable[numStreams];
            var streamViews = new Viewable[numStreams];
            for (var stream = 0; stream < numStreams; stream++)
            {
                var viewables = ViewFactoryUtil.Materialize(
                    viewFactories[stream], eventStreamParentViewable[stream], viewFactoryChainContexts[stream],
                    stopCallbacks);
                topViews[stream] = viewables.Top;
                streamViews[stream] = viewables.Last;
            }

            // determine match-recognize "previous"-node strategy (none if not present, or one handling and number of nodes)
            var matchRecognize = RowRecogHelper.RecursiveFindRegexService(topViews[0]);
            if (matchRecognize != null)
            {
                rowRecogPreviousStrategy = matchRecognize.PreviousEvaluationStrategy;
                stopCallbacks.Add(matchRecognize);
            }

            // start subselects
            var subselectActivations = SubSelectHelperStart.StartSubselects(
                subselects, agentInstanceContext, stopCallbacks, isRecoveringResilient);

            // start table-access
            var tableAccessEvals = ExprTableEvalHelperStart.StartTableAccess(tableAccesses, agentInstanceContext);

            // result-set-processing
            var processorPair = StatementAgentInstanceFactoryUtil.StartResultSetAndAggregation(
                resultSetProcessorFactoryProvider, agentInstanceContext, false, null);
            stopCallbacks.Add(
                new ProxyAgentInstanceStopCallback
                {
                    ProcStop = services =>
                    {
                        processorPair.First.Stop();
                        processorPair.Second.Stop();
                    }
                });

            // join versus non-join
            JoinSetComposer joinSetComposer;
            JoinPreloadMethod joinPreloadMethod;
            OutputProcessView outputProcessView;
            if (streamViews.Length == 1)
            {
                outputProcessView = HandleSimpleSelect(
                    streamViews, processorPair.First, evalRootMatchRemover, suppressSameEventMatches,
                    discardPartialsOnMatch, agentInstanceContext);
                joinSetComposer = null;
                joinPreloadMethod = null;
            }
            else
            {
                var joinPlanResult = HandleJoin(
                    streamViews, processorPair.First,
                    agentInstanceContext, stopCallbacks, isRecoveringResilient);
                outputProcessView = joinPlanResult.Viewable;
                joinSetComposer = joinPlanResult.JoinSetComposerDesc.JoinSetComposer;
                joinPreloadMethod = joinPlanResult.PreloadMethod;
            }

            stopCallbacks.Add(outputProcessView);

            // handle preloads
            if (!isRecoveringResilient)
            {
                bool aggregated = resultSetProcessorFactoryProvider.ResultSetProcessorType.IsAggregated;
                HandlePreloads(
                    preloadList, aggregated, joinPreloadMethod, activationResults, agentInstanceContext,
                    processorPair.First);
            }

            var stopCallback = AgentInstanceUtil.FinalizeSafeStopCallbacks(stopCallbacks);

            return new StatementAgentInstanceFactorySelectResult(
                outputProcessView, stopCallback, agentInstanceContext, processorPair.Second,
                subselectActivations, priorEvalStrategies, previousGetterStrategies, rowRecogPreviousStrategy,
                tableAccessEvals, preloadList, null,
                joinSetComposer, topViews, null, activationResults);
        }

        public EventType StatementEventType => resultSetProcessorFactoryProvider.ResultEventType;

        public AIRegistryRequirements RegistryRequirements
        {
            get
            {
                var hasPrior = false;
                var hasPrevious = false;
                for (var i = 0; i < viewResourceDelegates.Length; i++)
                {
                    if (viewResourceDelegates[i].PriorRequests != null &&
                        !viewResourceDelegates[i].PriorRequests.IsEmpty())
                    {
                        hasPrior = true;
                    }

                    hasPrevious |= viewResourceDelegates[i].HasPrevious;
                }

                bool[] prior = null;
                if (hasPrior)
                {
                    prior = new bool[viewResourceDelegates.Length];
                    for (var i = 0; i < viewResourceDelegates.Length; i++)
                    {
                        if (viewResourceDelegates[i].PriorRequests != null &&
                            !viewResourceDelegates[i].PriorRequests.IsEmpty())
                        {
                            prior[i] = true;
                        }
                    }
                }

                bool[] previous = null;
                if (hasPrevious)
                {
                    previous = new bool[viewResourceDelegates.Length];
                    for (var i = 0; i < viewResourceDelegates.Length; i++)
                    {
                        previous[i] = viewResourceDelegates[i].HasPrevious;
                    }
                }

                var subqueries = AIRegistryRequirements.GetSubqueryRequirements(subselects);

                var hasRowRecogWithPrevious = false;
                foreach (var viewFactory in viewFactories[0])
                {
                    if (viewFactory is RowRecogNFAViewFactory)
                    {
                        var recog = (RowRecogNFAViewFactory)viewFactory;
                        hasRowRecogWithPrevious = recog.Desc.PreviousRandomAccessIndexes != null;
                    }
                }

                return new AIRegistryRequirements(
                    prior, previous, subqueries, tableAccesses == null ? 0 : tableAccesses.Count,
                    hasRowRecogWithPrevious);
            }
        }

        private OutputProcessView HandleSimpleSelect(
            Viewable[] streamViews, ResultSetProcessor resultSetProcessor, EvalRootMatchRemover evalRootMatchRemover,
            bool suppressSameEventMatches, bool discardPartialsOnMatch, AgentInstanceContext agentInstanceContext)
        {
            Deque<EPStatementDispatch> dispatches = null;
            var finalView = streamViews[0];

            // where-clause
            if (whereClauseEvaluator != null)
            {
                var filterView = new FilterExprView(
                    whereClauseEvaluator, agentInstanceContext, whereClauseEvaluatorTextForAudit);
                finalView.Child = filterView;
                filterView.Parent = finalView;
                finalView = filterView;
            }

            if (evalRootMatchRemover != null && (suppressSameEventMatches || discardPartialsOnMatch))
            {
                var v = new PatternRemoveDispatchView(
                    evalRootMatchRemover, suppressSameEventMatches, discardPartialsOnMatch);
                dispatches = new ArrayDeque<EPStatementDispatch>(2);
                dispatches.Add(v);
                finalView.Child = v;
                v.Parent = finalView;
                finalView = v;
            }

            // for ordered deliver without output limit/buffer
            if (orderByWithoutOutputRateLimit)
            {
                var bf = new SingleStreamDispatchView();
                if (dispatches == null)
                {
                    dispatches = new ArrayDeque<EPStatementDispatch>(1);
                }

                dispatches.Add(bf);
                finalView.Child = bf;
                bf.Parent = finalView;
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
                    var dispatchArray = dispatches.ToArray();
                    handle.OptionalDispatchable = new ProxyEPStatementDispatch
                    {
                        ProcExecute = () =>
                        {
                            foreach (var dispatch in dispatchArray)
                            {
                                dispatch.Execute();
                            }
                        }
                    };
                }
            }

            var outputProcessView =
                outputProcessViewFactoryProvider.OutputProcessViewFactory.MakeView(
                    resultSetProcessor, agentInstanceContext);
            finalView.Child = outputProcessView;
            outputProcessView.Parent = finalView;

            return outputProcessView;
        }

        private JoinPlanResult HandleJoin(
            Viewable[] streamViews,
            ResultSetProcessor resultSetProcessor,
            AgentInstanceContext agentInstanceContext,
            IList<AgentInstanceStopCallback> stopCallbacks,
            bool isRecoveringResilient)
        {
            var joinSetComposerDesc = joinSetComposerPrototype.Create(
                streamViews, false, agentInstanceContext, isRecoveringResilient);

            stopCallbacks.Add(
                new ProxyAgentInstanceStopCallback
                {
                    ProcStop = services => { joinSetComposerDesc.JoinSetComposer.Destroy(); }
                });

            var outputProcessView =
                outputProcessViewFactoryProvider.OutputProcessViewFactory.MakeView(
                    resultSetProcessor, agentInstanceContext);

            // Create strategy for join execution
            JoinExecutionStrategy execution = new JoinExecutionStrategyImpl(
                joinSetComposerDesc.JoinSetComposer, joinSetComposerDesc.PostJoinFilterEvaluator, outputProcessView,
                agentInstanceContext);

            // The view needs a reference to the join execution to pull iterator values
            outputProcessView.JoinExecutionStrategy = execution;

            // Hook up dispatchable with buffer and execution strategy
            var joinStatementDispatch = new JoinExecStrategyDispatchable(
                execution, streamViews.Length, agentInstanceContext);
            agentInstanceContext.EpStatementAgentInstanceHandle.OptionalDispatchable = joinStatementDispatch;

            JoinPreloadMethod preloadMethod;
            if (unidirectionalJoin || !joinSetComposerDesc.JoinSetComposer.AllowsInit())
            {
                preloadMethod = new JoinPreloadMethodNull();
            }
            else
            {
                preloadMethod = new JoinPreloadMethodImpl(streamNames.Length, joinSetComposerDesc.JoinSetComposer);
            }

            for (var i = 0; i < streamViews.Length; i++)
            {
                var buffer = new BufferView(i);
                streamViews[i].Child = buffer;
                buffer.Observer = joinStatementDispatch;
                preloadMethod.SetBuffer(buffer, i);
            }

            return new JoinPlanResult(outputProcessView, preloadMethod, joinSetComposerDesc);
        }

        private void HandlePreloads(
            IList<StatementAgentInstancePreload> preloadList, bool isAggregated, JoinPreloadMethod joinPreloadMethod,
            ViewableActivationResult[] activationResults, AgentInstanceContext agentInstanceContext,
            ResultSetProcessor resultSetProcessor)
        {
            var hasNamedWindow = false;

            for (var stream = 0; stream < activationResults.Length; stream++)
            {
                var activationResult = activationResults[stream];
                if (!(activationResult.Viewable is NamedWindowConsumerView))
                {
                    continue;
                }

                hasNamedWindow = true;
                var consumer = (NamedWindowConsumerView)activationResult.Viewable;
                if (consumer.ConsumerCallback.IsParentBatchWindow)
                {
                    continue;
                }

                var nwActivator = (ViewableActivatorNamedWindow)viewableActivators[stream];
                preloadList.Add(
                    new NamedWindowConsumerPreload(nwActivator, consumer, agentInstanceContext, joinPreloadMethod));

                if (streamNames.Length == 1)
                {
                    preloadList.Add(new NamedWindowConsumerPreloadDispatchNonJoin(agentInstanceContext));
                }
                else
                {
                    preloadList.Add(
                        new NamedWindowConsumerPreloadDispatchJoin(joinPreloadMethod, stream, agentInstanceContext));
                }
            }

            // last, for aggregation we need to send the current join results to the result set processor
            if (hasNamedWindow && joinPreloadMethod != null && isAggregated)
            {
                preloadList.Add(new NamedWindowConsumerPreloadAggregationJoin(joinPreloadMethod, resultSetProcessor));
            }
        }

        private class JoinPlanResult
        {
            private JoinPlanResult(
                OutputProcessView viewable, JoinPreloadMethod preloadMethod, JoinSetComposerDesc joinSetComposerDesc)
            {
                Viewable = viewable;
                PreloadMethod = preloadMethod;
                JoinSetComposerDesc = joinSetComposerDesc;
            }

            public OutputProcessView Viewable { get; }

            public JoinPreloadMethod PreloadMethod { get; }

            public JoinSetComposerDesc JoinSetComposerDesc { get; }
        }

        private class NamedWindowConsumerPreload : StatementAgentInstancePreload
        {
            private readonly AgentInstanceContext agentInstanceContext;
            private readonly NamedWindowConsumerView consumer;
            private readonly JoinPreloadMethod joinPreloadMethod;
            private readonly ViewableActivatorNamedWindow nwActivator;

            public NamedWindowConsumerPreload(
                ViewableActivatorNamedWindow nwActivator, NamedWindowConsumerView consumer,
                AgentInstanceContext agentInstanceContext, JoinPreloadMethod joinPreloadMethod)
            {
                this.nwActivator = nwActivator;
                this.consumer = consumer;
                this.agentInstanceContext = agentInstanceContext;
                this.joinPreloadMethod = joinPreloadMethod;
            }

            public void ExecutePreload()
            {
                if (nwActivator.NamedWindowContextName != null &&
                    !nwActivator.NamedWindowContextName.Equals(agentInstanceContext.StatementContext.ContextName))
                {
                    return;
                }

                var snapshot = consumer.ConsumerCallback.Snapshot(
                    nwActivator.FilterQueryGraph, agentInstanceContext.Annotations);

                EventBean[] events;
                if (consumer.Filter == null)
                {
                    events = CollectionUtil.ToArrayEvents(snapshot);
                }
                else
                {
                    IList<EventBean> eventsInWindow = new List<EventBean>(snapshot.Count);
                    ExprNodeUtilityEvaluate.ApplyFilterExpressionIterable(
                        snapshot.GetEnumerator(), consumer.Filter, agentInstanceContext, eventsInWindow);
                    events = eventsInWindow.ToArray();
                }

                if (events.Length == 0)
                {
                    return;
                }

                consumer.Update(events, null);

                if (joinPreloadMethod != null && !joinPreloadMethod.IsPreloading &&
                    agentInstanceContext.EpStatementAgentInstanceHandle.OptionalDispatchable != null)
                {
                    agentInstanceContext.EpStatementAgentInstanceHandle.OptionalDispatchable.Execute();
                }
            }
        }

        private class NamedWindowConsumerPreloadDispatchNonJoin : StatementAgentInstancePreload
        {
            private readonly AgentInstanceContext agentInstanceContext;

            public NamedWindowConsumerPreloadDispatchNonJoin(AgentInstanceContext agentInstanceContext)
            {
                this.agentInstanceContext = agentInstanceContext;
            }

            public void ExecutePreload()
            {
                if (agentInstanceContext.EpStatementAgentInstanceHandle.OptionalDispatchable != null)
                {
                    agentInstanceContext.EpStatementAgentInstanceHandle.OptionalDispatchable.Execute();
                }
            }
        }

        private class NamedWindowConsumerPreloadDispatchJoin : StatementAgentInstancePreload
        {
            private readonly AgentInstanceContext agentInstanceContext;
            private readonly JoinPreloadMethod joinPreloadMethod;
            private readonly int stream;

            public NamedWindowConsumerPreloadDispatchJoin(
                JoinPreloadMethod joinPreloadMethod, int stream, AgentInstanceContext agentInstanceContext)
            {
                this.joinPreloadMethod = joinPreloadMethod;
                this.stream = stream;
                this.agentInstanceContext = agentInstanceContext;
            }

            public void ExecutePreload()
            {
                joinPreloadMethod.PreloadFromBuffer(stream, agentInstanceContext);
            }
        }

        private class NamedWindowConsumerPreloadAggregationJoin : StatementAgentInstancePreload
        {
            private readonly JoinPreloadMethod joinPreloadMethod;
            private readonly ResultSetProcessor resultSetProcessor;

            public NamedWindowConsumerPreloadAggregationJoin(
                JoinPreloadMethod joinPreloadMethod, ResultSetProcessor resultSetProcessor)
            {
                this.joinPreloadMethod = joinPreloadMethod;
                this.resultSetProcessor = resultSetProcessor;
            }

            public void ExecutePreload()
            {
                joinPreloadMethod.PreloadAggregation(resultSetProcessor);
            }
        }
    }
} // end of namespace