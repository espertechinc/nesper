///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.epl.join.@base;
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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.common.@internal.context.aifactory.select
{
	public partial class StatementAgentInstanceFactorySelect : StatementAgentInstanceFactory
	{
		private string[] _streamNames;
		private ViewableActivator[] _viewableActivators;
		private ResultSetProcessorFactoryProvider _resultSetProcessorFactoryProvider;
		private ViewFactory[][] _viewFactories;
		private ExprEvaluator _whereClauseEvaluator;
		private string _whereClauseEvaluatorTextForAudit;
		private OutputProcessViewFactoryProvider _outputProcessViewFactoryProvider;
		private ViewResourceDelegateDesc[] _viewResourceDelegates;
		private JoinSetComposerPrototype _joinSetComposerPrototype;
		private IDictionary<int, SubSelectFactory> _subselects;
		private IDictionary<int, ExprTableEvalStrategyFactory> _tableAccesses;
		private bool _orderByWithoutOutputRateLimit;
		private bool _unidirectionalJoin;

		public StatementAgentInstanceFactorySelect()
		{
		}

		public ViewableActivator[] ViewableActivators {
			set => this._viewableActivators = value;
		}

		public ResultSetProcessorFactoryProvider ResultSetProcessorFactoryProvider {
			set => this._resultSetProcessorFactoryProvider = value;
		}

		public ViewFactory[][] ViewFactories {
			set => this._viewFactories = value;
		}

		public OutputProcessViewFactoryProvider OutputProcessViewFactoryProvider {
			set => this._outputProcessViewFactoryProvider = value;
		}

		public ViewResourceDelegateDesc[] ViewResourceDelegates {
			set => this._viewResourceDelegates = value;
		}

		public ExprEvaluator WhereClauseEvaluator {
			set => this._whereClauseEvaluator = value;
		}

		public string[] StreamNames {
			set => this._streamNames = value;
		}

		public JoinSetComposerPrototype JoinSetComposerPrototype {
			set => this._joinSetComposerPrototype = value;
		}

		public IDictionary<int, SubSelectFactory> Subselects {
			set => this._subselects = value;
		}

		public bool OrderByWithoutOutputRateLimit {
			set => this._orderByWithoutOutputRateLimit = value;
		}

		public bool IsUnidirectionalJoin {
			set => this._unidirectionalJoin = value;
		}

		public IDictionary<int, ExprTableEvalStrategyFactory> TableAccesses {
			set => this._tableAccesses = value;
		}

		public string WhereClauseEvaluatorTextForAudit {
			set => this._whereClauseEvaluatorTextForAudit = value;
		}

		public void StatementCreate(StatementContext statementContext)
		{
		}

		public void StatementDestroy(StatementContext statementContext)
		{
		}

		public IReaderWriterLock ObtainAgentInstanceLock(
			StatementContext statementContext,
			int agentInstanceId)
		{
			return AgentInstanceUtil.NewLock(statementContext);
		}

		//public StatementAgentInstanceFactorySelectResult NewContext(AgentInstanceContext agentInstanceContext, bool isRecoveringResilient) {
		public StatementAgentInstanceFactoryResult NewContext(
			AgentInstanceContext agentInstanceContext,
			bool isRecoveringResilient)
		{
			IList<AgentInstanceMgmtCallback> stopCallbacks = new List<AgentInstanceMgmtCallback>();
			IList<StatementAgentInstancePreload> preloadList = new List<StatementAgentInstancePreload>();
			var numStreams = _viewableActivators.Length;

			// root activations
			var activationResults = new ViewableActivationResult[numStreams];
			var eventStreamParentViewable = new Viewable[numStreams];
			var patternRoots = new EvalRootState[numStreams];
			EvalRootMatchRemover evalRootMatchRemover = null;
			var suppressSameEventMatches = false;
			var discardPartialsOnMatch = false;

			for (var stream = 0; stream < numStreams; stream++) {
				var activationResult = _viewableActivators[stream].Activate(agentInstanceContext, false, isRecoveringResilient);
				stopCallbacks.Add(activationResult.StopCallback);
				activationResults[stream] = activationResult;
				eventStreamParentViewable[stream] = activationResult.Viewable;
				patternRoots[stream] = activationResult.OptionalPatternRoot;
				suppressSameEventMatches = activationResult.IsSuppressSameEventMatches;
				discardPartialsOnMatch = activationResult.IsDiscardPartialsOnMatch;

				if (stream == 0) {
					evalRootMatchRemover = activationResult.OptEvalRootMatchRemover;
				}
			}

			// create view factory chain context: holds stream-specific services
			var viewFactoryChainContexts = new AgentInstanceViewFactoryChainContext[numStreams];
			var priorEvalStrategies = new PriorEvalStrategy[numStreams];
			var previousGetterStrategies = new PreviousGetterStrategy[numStreams];
			RowRecogPreviousStrategy rowRecogPreviousStrategy = null;

			for (var i = 0; i < numStreams; i++) {
				viewFactoryChainContexts[i] = AgentInstanceViewFactoryChainContext.Create(_viewFactories[i], agentInstanceContext, _viewResourceDelegates[i]);
				priorEvalStrategies[i] = PriorHelper.ToStrategy(viewFactoryChainContexts[i]);
				previousGetterStrategies[i] = viewFactoryChainContexts[i].PreviousNodeGetter;
			}

			// materialize views
			var topViews = new Viewable[numStreams];
			var streamViews = new Viewable[numStreams];
			for (var stream = 0; stream < numStreams; stream++) {
				var viewables = ViewFactoryUtil.Materialize(
					_viewFactories[stream],
					eventStreamParentViewable[stream],
					viewFactoryChainContexts[stream],
					stopCallbacks);
				topViews[stream] = viewables.Top;
				streamViews[stream] = viewables.Last;
			}

			// determine match-recognize "previous"-node strategy (none if not present, or one handling and number of nodes)
			var matchRecognize = RowRecogHelper.RecursiveFindRegexService(topViews[0]);
			if (matchRecognize != null) {
				rowRecogPreviousStrategy = matchRecognize.PreviousEvaluationStrategy;
				stopCallbacks.Add(matchRecognize);
			}

			// start subselects
			var subselectActivations = SubSelectHelperStart.StartSubselects(
				_subselects,
				agentInstanceContext,
				stopCallbacks,
				isRecoveringResilient);

			// start table-access
			var tableAccessEvals = ExprTableEvalHelperStart.StartTableAccess(_tableAccesses, agentInstanceContext);

			// result-set-processing
			var processorPair =
				StatementAgentInstanceFactoryUtil.StartResultSetAndAggregation(_resultSetProcessorFactoryProvider, agentInstanceContext, false, null);
			stopCallbacks.Add(new SelectMgmtCallback(processorPair));

			// join versus non-join
			JoinSetComposer joinSetComposer;
			JoinPreloadMethod joinPreloadMethod;
			OutputProcessView outputProcessView;
			if (streamViews.Length == 1) {
				outputProcessView = HandleSimpleSelect(
					streamViews,
					processorPair.First,
					evalRootMatchRemover,
					suppressSameEventMatches,
					discardPartialsOnMatch,
					agentInstanceContext);
				joinSetComposer = null;
				joinPreloadMethod = null;
			}
			else {
				var joinPlanResult = HandleJoin(
					streamViews,
					processorPair.First,
					agentInstanceContext,
					stopCallbacks,
					isRecoveringResilient);
				outputProcessView = joinPlanResult.Viewable;
				joinSetComposer = joinPlanResult.JoinSetComposerDesc.JoinSetComposer;
				joinPreloadMethod = joinPlanResult.PreloadMethod;
			}

			stopCallbacks.Add(outputProcessView);

			// handle preloads
			if (!isRecoveringResilient) {
				var aggregated = _resultSetProcessorFactoryProvider.ResultSetProcessorType.IsAggregated();
				HandlePreloads(preloadList, aggregated, joinPreloadMethod, activationResults, agentInstanceContext, processorPair.First);
			}

			var stopCallback = AgentInstanceUtil.FinalizeSafeStopCallbacks(stopCallbacks);

			// clean up empty holder
			if (CollectionUtil.IsArrayAllNull(priorEvalStrategies)) {
				priorEvalStrategies = PriorEvalStrategyConstants.EMPTY_ARRAY;
			}

			if (CollectionUtil.IsAllNullArray(previousGetterStrategies)) {
				previousGetterStrategies = PreviousGetterStrategyConstants.EMPTY_ARRAY;
			}

			if (CollectionUtil.IsAllNullArray(patternRoots)) {
				patternRoots = EvalRootStateConstants.EMPTY_ARRAY;
			}

			if (CollectionUtil.IsArraySameReferences(topViews, eventStreamParentViewable)) {
				topViews = eventStreamParentViewable;
			}

			// finally process startup events: handle any pattern-match-event that was produced during startup,
			// relevant for "timer:interval(0)" in conjunction with contexts
			Runnable postContextMergeRunnable = () => {
				for (var stream = 0; stream < numStreams; stream++) {
					var activationResult = activationResults[stream];
					activationResult.OptPostContextMergeRunnable?.Invoke();
				}
			};

			return new StatementAgentInstanceFactorySelectResult(
				outputProcessView,
				stopCallback,
				agentInstanceContext,
				processorPair.Second,
				subselectActivations,
				priorEvalStrategies,
				previousGetterStrategies,
				rowRecogPreviousStrategy,
				tableAccessEvals,
				preloadList,
				postContextMergeRunnable,
				patternRoots,
				joinSetComposer,
				topViews,
				eventStreamParentViewable,
				activationResults,
				processorPair.First);
		}

		public EventType StatementEventType => _resultSetProcessorFactoryProvider.ResultEventType;

		public AIRegistryRequirements RegistryRequirements {
			get {
				var hasPrior = false;
				var hasPrevious = false;
				for (var i = 0; i < _viewResourceDelegates.Length; i++) {
					if (_viewResourceDelegates[i].PriorRequests != null && !_viewResourceDelegates[i].PriorRequests.IsEmpty()) {
						hasPrior = true;
					}

					hasPrevious |= _viewResourceDelegates[i].HasPrevious;
				}

				bool[] prior = null;
				if (hasPrior) {
					prior = new bool[_viewResourceDelegates.Length];
					for (var i = 0; i < _viewResourceDelegates.Length; i++) {
						if (_viewResourceDelegates[i].PriorRequests != null && !_viewResourceDelegates[i].PriorRequests.IsEmpty()) {
							prior[i] = true;
						}
					}
				}

				bool[] previous = null;
				if (hasPrevious) {
					previous = new bool[_viewResourceDelegates.Length];
					for (var i = 0; i < _viewResourceDelegates.Length; i++) {
						previous[i] = _viewResourceDelegates[i].HasPrevious;
					}
				}

				var subqueries = AIRegistryRequirements.GetSubqueryRequirements(_subselects);

				var hasRowRecogWithPrevious = false;
				foreach (var viewFactory in _viewFactories[0]) {
					if (viewFactory is RowRecogNFAViewFactory) {
						var recog = (RowRecogNFAViewFactory) viewFactory;
						hasRowRecogWithPrevious = recog.Desc.PreviousRandomAccessIndexes != null;
					}
				}

				return new AIRegistryRequirements(prior, previous, subqueries, _tableAccesses == null ? 0 : _tableAccesses.Count, hasRowRecogWithPrevious);
			}
		}

		private OutputProcessView HandleSimpleSelect(
			Viewable[] streamViews,
			ResultSetProcessor resultSetProcessor,
			EvalRootMatchRemover evalRootMatchRemover,
			bool suppressSameEventMatches,
			bool discardPartialsOnMatch,
			AgentInstanceContext agentInstanceContext)
		{
			Deque<EPStatementDispatch> dispatches = null;
			var finalView = streamViews[0];

			// where-clause
			if (_whereClauseEvaluator != null) {
				var filterView = new FilterExprView(_whereClauseEvaluator, agentInstanceContext, _whereClauseEvaluatorTextForAudit);
				finalView.Child = filterView;
				filterView.Parent = finalView;
				finalView = filterView;
			}

			if (evalRootMatchRemover != null && (suppressSameEventMatches || discardPartialsOnMatch)) {
				var v = new PatternRemoveDispatchView(evalRootMatchRemover, suppressSameEventMatches, discardPartialsOnMatch);
				dispatches = new ArrayDeque<EPStatementDispatch>(2);
				dispatches.Add(v);
				finalView.Child = v;
				v.Parent = finalView;
				finalView = v;
			}

			// for ordered deliver without output limit/buffer
			if (_orderByWithoutOutputRateLimit) {
				var bf = new SingleStreamDispatchView();
				if (dispatches == null) {
					dispatches = new ArrayDeque<EPStatementDispatch>(1);
				}

				dispatches.Add(bf);
				finalView.Child = bf;
				bf.Parent = finalView;
				finalView = bf;
			}

			if (dispatches != null) {
				var handle = agentInstanceContext.EpStatementAgentInstanceHandle;
				if (dispatches.Count == 1) {
					handle.OptionalDispatchable = dispatches.First;
				}
				else {
					var dispatchArray = dispatches.ToArray();
					handle.OptionalDispatchable = new ProxyEPStatementDispatch() {
						ProcExecute = () => {
							foreach (var dispatch in dispatchArray) {
								dispatch.Execute();
							}
						},
					};
				}
			}

			var outputProcessView = _outputProcessViewFactoryProvider.OutputProcessViewFactory.MakeView(resultSetProcessor, agentInstanceContext);
			finalView.Child = outputProcessView;
			outputProcessView.Parent = finalView;

			return outputProcessView;
		}

		private JoinPlanResult HandleJoin(
			Viewable[] streamViews,
			ResultSetProcessor resultSetProcessor,
			AgentInstanceContext agentInstanceContext,
			IList<AgentInstanceMgmtCallback> stopCallbacks,
			bool isRecoveringResilient)
		{
			var joinSetComposerDesc = _joinSetComposerPrototype.Create(streamViews, false, agentInstanceContext, isRecoveringResilient);

			stopCallbacks.Add(
				new ProxyAgentInstanceMgmtCallback() {
					ProcStop = (services) => { joinSetComposerDesc.JoinSetComposer.Destroy(); },
				});

			var outputProcessView = _outputProcessViewFactoryProvider.OutputProcessViewFactory.MakeView(resultSetProcessor, agentInstanceContext);

			// Create strategy for join execution
			JoinExecutionStrategy execution = new JoinExecutionStrategyImpl(
				joinSetComposerDesc.JoinSetComposer,
				joinSetComposerDesc.PostJoinFilterEvaluator,
				outputProcessView,
				agentInstanceContext);

			// The view needs a reference to the join execution to pull iterator values
			outputProcessView.JoinExecutionStrategy = execution;

			// Hook up dispatchable with buffer and execution strategy
			var joinStatementDispatch = new JoinExecStrategyDispatchable(execution, streamViews.Length, agentInstanceContext);
			agentInstanceContext.EpStatementAgentInstanceHandle.OptionalDispatchable = joinStatementDispatch;

			JoinPreloadMethod preloadMethod;
			if (_unidirectionalJoin || !joinSetComposerDesc.JoinSetComposer.AllowsInit()) {
				preloadMethod = new JoinPreloadMethodNull();
			}
			else {
				preloadMethod = new JoinPreloadMethodImpl(_streamNames.Length, joinSetComposerDesc.JoinSetComposer);
			}

			for (var i = 0; i < streamViews.Length; i++) {
				var buffer = new BufferView(i);
				streamViews[i].Child = buffer;
				buffer.Observer = joinStatementDispatch;
				preloadMethod.SetBuffer(buffer, i);
			}

			return new JoinPlanResult(outputProcessView, preloadMethod, joinSetComposerDesc);
		}

		private void HandlePreloads(
			IList<StatementAgentInstancePreload> preloadList,
			bool isAggregated,
			JoinPreloadMethod joinPreloadMethod,
			ViewableActivationResult[] activationResults,
			AgentInstanceContext agentInstanceContext,
			ResultSetProcessor resultSetProcessor)
		{
			var hasNamedWindow = false;

			for (var stream = 0; stream < activationResults.Length; stream++) {
				var activationResult = activationResults[stream];
				if (!(activationResult.Viewable is NamedWindowConsumerView)) {
					continue;
				}

				hasNamedWindow = true;
				var consumer = (NamedWindowConsumerView) activationResult.Viewable;
				if (consumer.ConsumerCallback.IsParentBatchWindow) {
					continue;
				}

				var nwActivator = (ViewableActivatorNamedWindow) _viewableActivators[stream];
				preloadList.Add(new NamedWindowConsumerPreload(nwActivator, consumer, agentInstanceContext, joinPreloadMethod));

				if (_streamNames.Length == 1) {
					preloadList.Add(new NamedWindowConsumerPreloadDispatchNonJoin(agentInstanceContext));
				}
				else {
					preloadList.Add(new NamedWindowConsumerPreloadDispatchJoin(joinPreloadMethod, stream, agentInstanceContext));
				}
			}

			// last, for aggregation we need to send the current join results to the result set processor
			if (hasNamedWindow && joinPreloadMethod != null && isAggregated) {
				preloadList.Add(new NamedWindowConsumerPreloadAggregationJoin(joinPreloadMethod, resultSetProcessor));
			}
		}

		public void StatementDestroyPreconditions(StatementContext statementContext)
		{
		}
	}
} // end of namespace
