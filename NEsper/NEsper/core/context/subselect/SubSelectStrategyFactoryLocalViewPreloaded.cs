///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.core.context.factory;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.core.start;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.@join.table;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.subquery;
using com.espertech.esper.filter;
using com.espertech.esper.util;
using com.espertech.esper.view;
using com.espertech.esper.view.internals;

namespace com.espertech.esper.core.context.subselect
{
    /// <summary>
    /// Entry holding lookup resource references for use by <seealso cref="SubSelectActivationCollection"/>.
    /// </summary>
    public class SubSelectStrategyFactoryLocalViewPreloaded : SubSelectStrategyFactory
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly SubordTableLookupStrategyNullRow NULL_ROW_STRATEGY = new SubordTableLookupStrategyNullRow();
    
        private readonly int _subqueryNumber;
        private readonly SubSelectActivationHolder _subSelectHolder;
        private readonly Pair<EventTableFactory, SubordTableLookupStrategyFactory> _pair;
        private readonly ExprNode _filterExprNode;
        private readonly ExprEvaluator _filterExprEval;
        private readonly bool _correlatedSubquery;
        private readonly AggregationServiceFactoryDesc _aggregationServiceFactory;
        private readonly ViewResourceDelegateVerified _viewResourceDelegate;
        private readonly ExprEvaluator[] _groupKeys;
    
        public SubSelectStrategyFactoryLocalViewPreloaded(int subqueryNumber, SubSelectActivationHolder subSelectHolder, Pair<EventTableFactory, SubordTableLookupStrategyFactory> pair, ExprNode filterExprNode, ExprEvaluator filterExprEval, bool correlatedSubquery, AggregationServiceFactoryDesc aggregationServiceFactory, ViewResourceDelegateVerified viewResourceDelegate, ExprEvaluator[] groupKeys)
        {
            _subqueryNumber = subqueryNumber;
            _subSelectHolder = subSelectHolder;
            _pair = pair;
            _filterExprNode = filterExprNode;
            _filterExprEval = filterExprEval;
            _correlatedSubquery = correlatedSubquery;
            _aggregationServiceFactory = aggregationServiceFactory;
            _viewResourceDelegate = viewResourceDelegate;
            _groupKeys = groupKeys;
        }
    
        public SubSelectStrategyRealization Instantiate(EPServicesContext services,
                                                        Viewable viewableRoot,
                                                        AgentInstanceContext agentInstanceContext,
                                                        IList<StopCallback> stopCallbackList)
        {
            var viewFactoryChain = _subSelectHolder.ViewFactoryChain.FactoryChain;
    
            // add "prior" view factory
            var hasPrior = _viewResourceDelegate.PerStream[0].PriorRequests != null && !_viewResourceDelegate.PerStream[0].PriorRequests.IsEmpty();
            if (hasPrior) {
                var priorEventViewFactory = EPStatementStartMethodHelperPrior.GetPriorEventViewFactory(agentInstanceContext.StatementContext, 1024 + _subqueryNumber, viewFactoryChain.Count + 1, viewFactoryChain.IsEmpty());
                viewFactoryChain = new List<ViewFactory>(viewFactoryChain);
                viewFactoryChain.Add(priorEventViewFactory);
            }
    
            // create factory chain context to hold callbacks specific to "prior" and "prev"
            var viewFactoryChainContext = AgentInstanceViewFactoryChainContext.Create(viewFactoryChain, agentInstanceContext, _viewResourceDelegate.PerStream[0]);
    
            var createResult = services.ViewService.CreateViews(viewableRoot, viewFactoryChain, viewFactoryChainContext, false);
            var subselectView = createResult.FinalViewable;
    
            // create index/holder table
            var index = _pair.First.MakeEventTables();
            stopCallbackList.Add(new SubqueryStopCallback(index).Stop);
    
            // create strategy
            var strategy = _pair.Second.MakeStrategy(index, null);
            SubselectAggregationPreprocessorBase subselectAggregationPreprocessor = null;
    
            // handle "prior" nodes and their strategies
            var priorNodeStrategies = EPStatementStartMethodHelperPrior.CompilePriorNodeStrategies(_viewResourceDelegate, new AgentInstanceViewFactoryChainContext[]{viewFactoryChainContext});
    
            // handle "previous" nodes and their strategies
            var previousNodeStrategies = EPStatementStartMethodHelperPrevious.CompilePreviousNodeStrategies(_viewResourceDelegate, new AgentInstanceViewFactoryChainContext[]{viewFactoryChainContext});
    
            AggregationService aggregationService = null;
            if (_aggregationServiceFactory != null) {
                aggregationService = _aggregationServiceFactory.AggregationServiceFactory.MakeService(agentInstanceContext, agentInstanceContext.StatementContext.MethodResolutionService);
    
                if (!_correlatedSubquery) {
                    View aggregatorView;
                    if (_groupKeys == null) {
                        if (_filterExprEval == null) {
                            aggregatorView = new SubselectAggregatorViewUnfilteredUngrouped(aggregationService, _filterExprEval, agentInstanceContext, null);
                        }
                        else {
                            aggregatorView = new SubselectAggregatorViewFilteredUngrouped(aggregationService, _filterExprEval, agentInstanceContext, null, _filterExprNode);
                        }
                    }
                    else {
                        if (_filterExprEval == null) {
                            aggregatorView = new SubselectAggregatorViewUnfilteredGrouped(aggregationService, _filterExprEval, agentInstanceContext, _groupKeys);
                        }
                        else {
                            aggregatorView = new SubselectAggregatorViewFilteredGrouped(aggregationService, _filterExprEval, agentInstanceContext, _groupKeys, _filterExprNode);
                        }
                    }
                    subselectView.AddView(aggregatorView);
    
                    Preload(services, null, aggregatorView, agentInstanceContext);
    
                    return new SubSelectStrategyRealization(NULL_ROW_STRATEGY, null, aggregationService, priorNodeStrategies, previousNodeStrategies, subselectView, null);
                }
                else {
                    if (_groupKeys == null) {
                        if (_filterExprEval == null) {
                            subselectAggregationPreprocessor = new SubselectAggregationPreprocessorUnfilteredUngrouped(aggregationService, _filterExprEval, null);
                        }
                        else {
                            subselectAggregationPreprocessor = new SubselectAggregationPreprocessorFilteredUngrouped(aggregationService, _filterExprEval, null);
                        }
                    }
                    else {
                        if (_filterExprEval == null) {
                            subselectAggregationPreprocessor = new SubselectAggregationPreprocessorUnfilteredGrouped(aggregationService, _filterExprEval, _groupKeys);
                        }
                        else {
                            subselectAggregationPreprocessor = new SubselectAggregationPreprocessorFilteredGrouped(aggregationService, _filterExprEval, _groupKeys);
                        }
                    }
                }
            }
    
            // preload
            Preload(services, index, subselectView, agentInstanceContext);
            StatementAgentInstancePostLoad postLoad = new ProxyStatementAgentInstancePostLoad
            {
                ProcExecutePostLoad = () => Preload(services, index, subselectView, agentInstanceContext),
                ProcAcceptIndexVisitor = visitor =>
                {
                    foreach (var table in index)
                    {
                        visitor.Visit(table);
                    }
                }
            };
    
            var bufferView = new BufferView(_subSelectHolder.StreamNumber);
            bufferView.Observer = new SubselectBufferObserver(index);
            subselectView.AddView(bufferView);
    
            return new SubSelectStrategyRealization(strategy, subselectAggregationPreprocessor, aggregationService, priorNodeStrategies, previousNodeStrategies, subselectView, postLoad);
        }
    
        private void Preload(EPServicesContext services, EventTable[] eventIndex, Viewable subselectView, AgentInstanceContext agentInstanceContext)
        {
            if (_subSelectHolder.StreamSpecCompiled is NamedWindowConsumerStreamSpec)
            {
                var namedSpec = (NamedWindowConsumerStreamSpec) _subSelectHolder.StreamSpecCompiled;
                var processor = services.NamedWindowService.GetProcessor(namedSpec.WindowName);
                if (processor == null) {
                    throw new Exception("Failed to find named window by name '" + namedSpec.WindowName + "'");
                }
    
                var processorInstance = processor.GetProcessorInstance(agentInstanceContext);
                if (processorInstance == null) {
                    throw new EPException("Named window '" + namedSpec.WindowName + "' is associated to context '" + processor.ContextName + "' that is not available for querying");
                }
                var consumerView = processorInstance.TailViewInstance;
    
                // preload view for stream
                ICollection<EventBean> eventsInWindow;
                if (namedSpec.FilterExpressions != null && !namedSpec.FilterExpressions.IsEmpty()) {
    
                    try {
                        var types = new StreamTypeServiceImpl(consumerView.EventType, consumerView.EventType.Name, false, services.EngineURI);
                        var tagged = new LinkedHashMap<String, Pair<EventType, String>>();
                        var filterSpecCompiled = FilterSpecCompiler.MakeFilterSpec(types.EventTypes[0], types.StreamNames[0],
                                namedSpec.FilterExpressions, null, tagged, tagged, types, null, agentInstanceContext.StatementContext, Collections.SingletonList(0));
                        var snapshot = consumerView.SnapshotNoLock(filterSpecCompiled, agentInstanceContext.StatementContext.Annotations);
                        eventsInWindow = new List<EventBean>(snapshot.Count);
                        ExprNodeUtility.ApplyFilterExpressionsIterable(snapshot, namedSpec.FilterExpressions, agentInstanceContext, eventsInWindow);
                    }
                    catch (Exception ex) {
                        Log.Warn("Unexpected exception analyzing filter paths: " + ex.Message, ex);
                        eventsInWindow = new List<EventBean>();
                        ExprNodeUtility.ApplyFilterExpressionsIterable(consumerView, namedSpec.FilterExpressions, agentInstanceContext, eventsInWindow);
                    }
                }
                else {
                    eventsInWindow = new List<EventBean>();
                    eventsInWindow.AddAll(consumerView);
                }
                var newEvents = eventsInWindow.ToArray();
                if (subselectView != null) {
                    ((View)subselectView).Update(newEvents, null);
                }
                if (eventIndex != null) {
                    foreach (EventTable table in eventIndex) {
                        table.Add(newEvents);  // fill index
                    }
                }
            }
            else        // preload from the data window that sit on top
            {
                // Start up event table from the iterator
                var enumerator = subselectView.GetEnumerator();
                if (enumerator.MoveNext())
                {
                    var preloadEvents = new List<EventBean>();

                    do
                    {
                        preloadEvents.Add(enumerator.Current);
                    } while (enumerator.MoveNext());

                    if (eventIndex != null)
                    {
                        foreach (EventTable table in eventIndex)
                        {
                            table.Add(preloadEvents.ToArray());
                        }
                    }
                }
            }
        }
    }
}
