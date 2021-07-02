///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.prior;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.epl.namedwindow.core;
using com.espertech.esper.common.@internal.epl.prior;
using com.espertech.esper.common.@internal.view.access;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.previous;
using com.espertech.esper.common.@internal.view.util;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.subselect
{
    public class SubSelectStrategyFactoryLocalViewPreloaded : SubSelectStrategyFactory
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        private AggregationServiceFactory _aggregationServiceFactory;
        private bool _correlatedSubquery;
        private EventTableFactory _eventTableFactory;
        private EventTableFactoryFactory _eventTableFactoryFactory;
        private EventTableIndexService _eventTableIndexService;
        private ExprEvaluator _filterExprEval;
        private ExprEvaluator _groupKeyEval;
        private SubordTableLookupStrategyFactory _lookupStrategyFactory;
        private NamedWindow _namedWindow;
        private ExprEvaluator _namedWindowFilterExpr;
        private QueryGraph _namedWindowFilterQueryGraph;

        private int _subqueryNumber;
        private ViewFactory[] _viewFactories;
        private ViewResourceDelegateDesc _viewResourceDelegate;

        public int SubqueryNumber {
            set => _subqueryNumber = value;
        }

        public ViewFactory[] ViewFactories {
            set => _viewFactories = value;
        }

        public ViewResourceDelegateDesc ViewResourceDelegate {
            set => _viewResourceDelegate = value;
        }

        public EventTableFactoryFactory EventTableFactoryFactory {
            set => _eventTableFactoryFactory = value;
        }

        public NamedWindow NamedWindow {
            set => _namedWindow = value;
        }

        public SubordTableLookupStrategyFactory LookupStrategyFactory {
            set => _lookupStrategyFactory = value;
        }

        public AggregationServiceFactory AggregationServiceFactory {
            set => _aggregationServiceFactory = value;
        }

        public bool CorrelatedSubquery {
            set => _correlatedSubquery = value;
        }

        public ExprEvaluator GroupKeyEval {
            set => _groupKeyEval = value;
        }

        public ExprEvaluator FilterExprEval {
            set => _filterExprEval = value;
        }

        public ExprEvaluator NamedWindowFilterExpr {
            set => _namedWindowFilterExpr = value;
        }

        public QueryGraph NamedWindowFilterQueryGraph {
            set => _namedWindowFilterQueryGraph = value;
        }

        public SubSelectStrategyRealization Instantiate(
            Viewable viewableRoot,
            ExprEvaluatorContext exprEvaluatorContext,
            IList<AgentInstanceMgmtCallback> stopCallbackList,
            int subqueryNumber,
            bool isRecoveringResilient)
        {
            Viewable subselectView = viewableRoot;
            PriorEvalStrategy priorStrategy = null;
            PreviousGetterStrategy previousGetter = null;
            
            // create factory chain context to hold callbacks specific to "prior" and "prev", when handling subqueries for statements (and not FAF)
            if (_viewFactories.Length > 0 && exprEvaluatorContext is AgentInstanceContext agentInstanceContext) {
                var viewFactoryChainContext = AgentInstanceViewFactoryChainContext.Create(_viewFactories, agentInstanceContext, _viewResourceDelegate);
                var viewables = ViewFactoryUtil.Materialize(_viewFactories, viewableRoot, viewFactoryChainContext, stopCallbackList);
                subselectView = viewables.Last;
                // handle "prior" nodes and their strategies
                priorStrategy = PriorHelper.ToStrategy(viewFactoryChainContext);
                // handle "previous" nodes and their strategies
                previousGetter = viewFactoryChainContext.PreviousNodeGetter;
            }

            // make aggregation service
            AggregationService aggregationService = null;
            if (_aggregationServiceFactory != null) {
                aggregationService = _aggregationServiceFactory.MakeService(exprEvaluatorContext, true, subqueryNumber, null);
                var aggregationServiceStoppable = aggregationService;
                stopCallbackList.Add(
                    new ProxyAgentInstanceMgmtCallback {
                        ProcStop = services => { aggregationServiceStoppable.Stop(); },
                        ProcTransfer = services => {
                            // no action
                        },
                    });
            }

            // handle aggregated and non-correlated queries: there is no strategy or index
            if (_aggregationServiceFactory != null && !_correlatedSubquery) {
                View aggregatorView;
                if (_groupKeyEval == null) {
                    if (_filterExprEval == null) {
                        aggregatorView = new SubselectAggregatorViewUnfilteredUngrouped(
                            aggregationService, _filterExprEval, exprEvaluatorContext, null);
                    }
                    else {
                        aggregatorView = new SubselectAggregatorViewFilteredUngrouped(
                            aggregationService, _filterExprEval, exprEvaluatorContext, null);
                    }
                }
                else {
                    if (_filterExprEval == null) {
                        aggregatorView = new SubselectAggregatorViewUnfilteredGrouped(
                            aggregationService, _filterExprEval, exprEvaluatorContext, _groupKeyEval);
                    }
                    else {
                        aggregatorView = new SubselectAggregatorViewFilteredGrouped(
                            aggregationService, _filterExprEval, exprEvaluatorContext, _groupKeyEval);
                    }
                }

                subselectView.Child = aggregatorView;

                if (_namedWindow != null && _eventTableIndexService.AllowInitIndex(isRecoveringResilient)) {
                    PreloadFromNamedWindow(null, aggregatorView, exprEvaluatorContext);
                }

                return new SubSelectStrategyRealization(
                    SubordTableLookupStrategyNullRow.INSTANCE,
                    null,
                    aggregationService,
                    priorStrategy,
                    previousGetter,
                    subselectView,
                    null);
            }

            // create index/holder table
            var index = _eventTableFactory.MakeEventTables(exprEvaluatorContext, subqueryNumber);
            stopCallbackList.Add(new SubqueryIndexMgmtCallback(index));

            // create strategy
            var strategy = _lookupStrategyFactory.MakeStrategy(index, exprEvaluatorContext, null);

            // handle unaggregated or correlated queries or
            SubselectAggregationPreprocessorBase subselectAggregationPreprocessor = null;
            if (_aggregationServiceFactory != null) {
                if (_groupKeyEval == null) {
                    if (_filterExprEval == null) {
                        subselectAggregationPreprocessor = new SubselectAggregationPreprocessorUnfilteredUngrouped(
                            aggregationService, _filterExprEval, null);
                    }
                    else {
                        subselectAggregationPreprocessor = new SubselectAggregationPreprocessorFilteredUngrouped(
                            aggregationService, _filterExprEval, null);
                    }
                }
                else {
                    if (_filterExprEval == null) {
                        subselectAggregationPreprocessor = new SubselectAggregationPreprocessorUnfilteredGrouped(
                            aggregationService, _filterExprEval, _groupKeyEval);
                    }
                    else {
                        subselectAggregationPreprocessor = new SubselectAggregationPreprocessorFilteredGrouped(
                            aggregationService, _filterExprEval, _groupKeyEval);
                    }
                }
            }

            // preload when allowed
            if (_namedWindow != null && _eventTableIndexService.AllowInitIndex(isRecoveringResilient)) {
                PreloadFromNamedWindow(index, subselectView, exprEvaluatorContext);
            }

            var bufferView = new BufferView(subqueryNumber);
            bufferView.Observer = new SubselectBufferObserver(index, exprEvaluatorContext);
            subselectView.Child = bufferView;

            return new SubSelectStrategyRealization(
                strategy,
                subselectAggregationPreprocessor,
                aggregationService,
                priorStrategy,
                previousGetter,
                subselectView,
                index);
        }

        public void Ready(
            SubSelectStrategyFactoryContext subselectFactoryContext,
            EventType eventType)
        {
            var type = _viewFactories.Length == 0 ? eventType : _viewFactories[_viewFactories.Length - 1].EventType;
            _eventTableFactory = _eventTableFactoryFactory.Create(type, subselectFactoryContext.EventTableFactoryContext);
            _eventTableIndexService = subselectFactoryContext.EventTableIndexService;
        }

        private void PreloadFromNamedWindow(
            EventTable[] eventIndex,
            Viewable subselectView,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var instance = _namedWindow.GetNamedWindowInstance(exprEvaluatorContext);
            if (instance == null) {
                throw new EPException(
                    "Named window '" +
                    _namedWindow.Name +
                    "' is associated to context '" +
                    _namedWindow.StatementContext.ContextName +
                    "' that is not available for querying");
            }

            var consumerView = instance.TailViewInstance;

            // preload view for stream
            ICollection<EventBean> eventsInWindow;
            if (_namedWindowFilterExpr != null) {
                var snapshot = consumerView.SnapshotNoLock(_namedWindowFilterQueryGraph, exprEvaluatorContext.Annotations);
                eventsInWindow = new List<EventBean>(snapshot.Count);
                ExprNodeUtilityEvaluate.ApplyFilterExpressionIterable(
                    snapshot.GetEnumerator(),
                    _namedWindowFilterExpr,
                    exprEvaluatorContext,
                    eventsInWindow);
            }
            else {
                eventsInWindow = consumerView.ToList();
            }

            var newEvents = eventsInWindow.ToArray();
            ((View) subselectView)?.Update(newEvents, null);

            if (eventIndex != null) {
                foreach (var table in eventIndex) {
                    table.Add(newEvents, exprEvaluatorContext); // fill index
                }
            }
        }

        public LookupStrategyDesc LookupStrategyDesc {
            get { return _lookupStrategyFactory.LookupStrategyDesc; }
        }
    }
} // end of namespace