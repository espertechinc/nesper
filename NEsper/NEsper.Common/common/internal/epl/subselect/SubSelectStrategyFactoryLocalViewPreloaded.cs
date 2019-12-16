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
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.epl.namedwindow.core;
using com.espertech.esper.common.@internal.epl.prior;
using com.espertech.esper.common.@internal.view.access;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.util;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.subselect
{
    public class SubSelectStrategyFactoryLocalViewPreloaded : SubSelectStrategyFactory
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SubSelectStrategyFactoryLocalViewPreloaded));

        private AggregationServiceFactory aggregationServiceFactory;
        private bool correlatedSubquery;
        private EventTableFactory eventTableFactory;
        private EventTableFactoryFactory eventTableFactoryFactory;
        private EventTableIndexService eventTableIndexService;
        private ExprEvaluator filterExprEval;
        private ExprEvaluator groupKeyEval;
        private SubordTableLookupStrategyFactory lookupStrategyFactory;
        private NamedWindow namedWindow;
        private ExprEvaluator namedWindowFilterExpr;

        private QueryGraph namedWindowFilterQueryGraph;
        // private final static SubordTableLookupStrategyNullRow NULL_ROW_STRATEGY = new SubordTableLookupStrategyNullRow();

        private int subqueryNumber;
        private ViewFactory[] viewFactories;
        private ViewResourceDelegateDesc viewResourceDelegate;

        public int SubqueryNumber {
            set => subqueryNumber = value;
        }

        public ViewFactory[] ViewFactories {
            set => viewFactories = value;
        }

        public ViewResourceDelegateDesc ViewResourceDelegate {
            set => viewResourceDelegate = value;
        }

        public EventTableFactoryFactory EventTableFactoryFactory {
            set => eventTableFactoryFactory = value;
        }

        public NamedWindow NamedWindow {
            set => namedWindow = value;
        }

        public SubordTableLookupStrategyFactory LookupStrategyFactory {
            set => lookupStrategyFactory = value;
        }

        public AggregationServiceFactory AggregationServiceFactory {
            set => aggregationServiceFactory = value;
        }

        public bool CorrelatedSubquery {
            set => correlatedSubquery = value;
        }

        public ExprEvaluator GroupKeyEval {
            set => groupKeyEval = value;
        }

        public ExprEvaluator FilterExprEval {
            set => filterExprEval = value;
        }

        public ExprEvaluator NamedWindowFilterExpr {
            set => namedWindowFilterExpr = value;
        }

        public QueryGraph NamedWindowFilterQueryGraph {
            set => namedWindowFilterQueryGraph = value;
        }

        public void Ready(
            StatementContext statementContext,
            EventType eventType)
        {
            var type = viewFactories.Length == 0 ? eventType : viewFactories[viewFactories.Length - 1].EventType;
            eventTableFactory = eventTableFactoryFactory.Create(type, statementContext);
            eventTableIndexService = statementContext.EventTableIndexService;
        }

        public SubSelectStrategyRealization Instantiate(
            Viewable viewableRoot,
            AgentInstanceContext agentInstanceContext,
            IList<AgentInstanceStopCallback> stopCallbackList,
            int subqueryNumber,
            bool isRecoveringResilient)
        {
            // create factory chain context to hold callbacks specific to "prior" and "prev"
            var viewFactoryChainContext = AgentInstanceViewFactoryChainContext.Create(
                viewFactories,
                agentInstanceContext,
                viewResourceDelegate);
            var viewables = ViewFactoryUtil.Materialize(
                viewFactories,
                viewableRoot,
                viewFactoryChainContext,
                stopCallbackList);
            var subselectView = viewables.Last;

            // make aggregation service
            AggregationService aggregationService = null;
            if (aggregationServiceFactory != null) {
                aggregationService = aggregationServiceFactory.MakeService(
                    agentInstanceContext,
                    agentInstanceContext.ImportServiceRuntime,
                    true,
                    subqueryNumber,
                    null);

                var aggregationServiceStoppable = aggregationService;
                stopCallbackList.Add(
                    new ProxyAgentInstanceStopCallback {
                        ProcStop = services => { aggregationServiceStoppable.Stop(); }
                    });
            }

            // handle "prior" nodes and their strategies
            var priorStrategy = PriorHelper.ToStrategy(viewFactoryChainContext);

            // handle "previous" nodes and their strategies
            var previousGetter = viewFactoryChainContext.PreviousNodeGetter;

            // handle aggregated and non-correlated queries: there is no strategy or index
            if (aggregationServiceFactory != null && !correlatedSubquery) {
                View aggregatorView;
                if (groupKeyEval == null) {
                    if (filterExprEval == null) {
                        aggregatorView = new SubselectAggregatorViewUnfilteredUngrouped(
                            aggregationService,
                            filterExprEval,
                            agentInstanceContext,
                            null);
                    }
                    else {
                        aggregatorView = new SubselectAggregatorViewFilteredUngrouped(
                            aggregationService,
                            filterExprEval,
                            agentInstanceContext,
                            null);
                    }
                }
                else {
                    if (filterExprEval == null) {
                        aggregatorView = new SubselectAggregatorViewUnfilteredGrouped(
                            aggregationService,
                            filterExprEval,
                            agentInstanceContext,
                            groupKeyEval);
                    }
                    else {
                        aggregatorView = new SubselectAggregatorViewFilteredGrouped(
                            aggregationService,
                            filterExprEval,
                            agentInstanceContext,
                            groupKeyEval);
                    }
                }

                subselectView.Child = aggregatorView;

                if (namedWindow != null && eventTableIndexService.AllowInitIndex(isRecoveringResilient)) {
                    PreloadFromNamedWindow(null, aggregatorView, agentInstanceContext);
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
            var index = eventTableFactory.MakeEventTables(agentInstanceContext, subqueryNumber);
            stopCallbackList.Add(new SubqueryIndexStopCallback(index));

            // create strategy
            var strategy = lookupStrategyFactory.MakeStrategy(index, agentInstanceContext, null);

            // handle unaggregated or correlated queries or
            SubselectAggregationPreprocessorBase subselectAggregationPreprocessor = null;
            if (aggregationServiceFactory != null) {
                if (groupKeyEval == null) {
                    if (filterExprEval == null) {
                        subselectAggregationPreprocessor = new SubselectAggregationPreprocessorUnfilteredUngrouped(
                            aggregationService,
                            filterExprEval,
                            null);
                    }
                    else {
                        subselectAggregationPreprocessor = new SubselectAggregationPreprocessorFilteredUngrouped(
                            aggregationService,
                            filterExprEval,
                            null);
                    }
                }
                else {
                    if (filterExprEval == null) {
                        subselectAggregationPreprocessor = new SubselectAggregationPreprocessorUnfilteredGrouped(
                            aggregationService,
                            filterExprEval,
                            groupKeyEval);
                    }
                    else {
                        subselectAggregationPreprocessor = new SubselectAggregationPreprocessorFilteredGrouped(
                            aggregationService,
                            filterExprEval,
                            groupKeyEval);
                    }
                }
            }

            // preload when allowed
            if (namedWindow != null && eventTableIndexService.AllowInitIndex(isRecoveringResilient)) {
                PreloadFromNamedWindow(index, subselectView, agentInstanceContext);
            }

            var bufferView = new BufferView(subqueryNumber);
            bufferView.Observer = new SubselectBufferObserver(index, agentInstanceContext);
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

        public LookupStrategyDesc LookupStrategyDesc => lookupStrategyFactory.LookupStrategyDesc;

        private void PreloadFromNamedWindow(
            EventTable[] eventIndex,
            Viewable subselectView,
            AgentInstanceContext agentInstanceContext)
        {
            var instance = namedWindow.GetNamedWindowInstance(agentInstanceContext);
            if (instance == null) {
                throw new EPException(
                    "Named window '" +
                    namedWindow.Name +
                    "' is associated to context '" +
                    namedWindow.StatementContext.ContextName +
                    "' that is not available for querying");
            }

            var consumerView = instance.TailViewInstance;

            // preload view for stream
            ICollection<EventBean> eventsInWindow;
            if (namedWindowFilterExpr != null) {
                ICollection<EventBean> snapshot = consumerView.SnapshotNoLock(
                    namedWindowFilterQueryGraph,
                    agentInstanceContext.Annotations);
                eventsInWindow = new List<EventBean>(snapshot.Count);
                ExprNodeUtilityEvaluate.ApplyFilterExpressionIterable(
                    snapshot.GetEnumerator(),
                    namedWindowFilterExpr,
                    agentInstanceContext,
                    eventsInWindow);
            }
            else {
                eventsInWindow = new List<EventBean>();
                using (var enumerator = consumerView.GetEnumerator()) {
                    while (enumerator.MoveNext()) {
                        eventsInWindow.Add(enumerator.Current);
                    }
                }
            }

            var newEvents = eventsInWindow.ToArray();
            if (subselectView != null) {
                ((View) subselectView).Update(newEvents, null);
            }

            if (eventIndex != null) {
                foreach (var table in eventIndex) {
                    table.Add(newEvents, agentInstanceContext); // fill index
                }
            }
        }
    }
} // end of namespace