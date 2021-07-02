///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.common.@internal.epl.namedwindow.core;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.@internal.epl.subselect
{
    public class SubSelectStrategyFactoryIndexShare : SubSelectStrategyFactory
    {
        private AggregationServiceFactory _aggregationServiceFactory;
        private ExprEvaluator _filterExprEval;
        private ExprEvaluator _groupKeyEval;

        private NamedWindow _namedWindow;
        private SubordinateQueryPlanDesc _queryPlan;
        private Table _table;

        public NamedWindow NamedWindow {
            set => _namedWindow = value;
        }

        public Table Table {
            set => _table = value;
        }

        public SubordinateQueryPlanDesc QueryPlan {
            set => _queryPlan = value;
        }

        public AggregationServiceFactory AggregationServiceFactory {
            set => _aggregationServiceFactory = value;
        }

        public ExprEvaluator GroupKeyEval {
            set => _groupKeyEval = value;
        }

        public ExprEvaluator FilterExprEval {
            set => _filterExprEval = value;
        }

        public void Ready(
            SubSelectStrategyFactoryContext subselectFactoryContext,
            EventType eventType)
        {
            // no action
        }

        public SubSelectStrategyRealization Instantiate(
            Viewable viewableRoot,
            ExprEvaluatorContext exprEvaluatorContext,
            IList<AgentInstanceMgmtCallback> stopCallbackList,
            int subqueryNumber,
            bool isRecoveringResilient)
        {
            SubselectAggregationPreprocessorBase subselectAggregationPreprocessor = null;

            AggregationService aggregationService = null;
            if (_aggregationServiceFactory != null) {
                aggregationService = _aggregationServiceFactory.MakeService(
                    exprEvaluatorContext,
                    true,
                    subqueryNumber,
                    null);

                var aggregationServiceStoppable = aggregationService;
                stopCallbackList.Add(
                    new ProxyAgentInstanceMgmtCallback {
                        ProcStop = services => { aggregationServiceStoppable.Stop(); }
                    });

                if (_groupKeyEval == null) {
                    if (_filterExprEval == null) {
                        subselectAggregationPreprocessor = new SubselectAggregationPreprocessorUnfilteredUngrouped(
                            aggregationService,
                            _filterExprEval,
                            null);
                    }
                    else {
                        subselectAggregationPreprocessor = new SubselectAggregationPreprocessorFilteredUngrouped(
                            aggregationService,
                            _filterExprEval,
                            null);
                    }
                }
                else {
                    if (_filterExprEval == null) {
                        subselectAggregationPreprocessor = new SubselectAggregationPreprocessorUnfilteredGrouped(
                            aggregationService,
                            _filterExprEval,
                            _groupKeyEval);
                    }
                    else {
                        subselectAggregationPreprocessor = new SubselectAggregationPreprocessorFilteredGrouped(
                            aggregationService,
                            _filterExprEval,
                            _groupKeyEval);
                    }
                }
            }

            SubordTableLookupStrategy subqueryLookup;
            if (_namedWindow != null) {
                var instance = _namedWindow.GetNamedWindowInstance(exprEvaluatorContext);
                if (_queryPlan == null) {
                    subqueryLookup = new SubordFullTableScanLookupStrategyLocking(
                        instance.RootViewInstance.DataWindowContents,
                        exprEvaluatorContext.AgentInstanceLock);
                }
                else {
                    var indexes = new EventTable[_queryPlan.IndexDescs.Length];
                    for (var i = 0; i < indexes.Length; i++) {
                        indexes[i] =
                            instance.RootViewInstance.IndexRepository.GetIndexByDesc(
                                _queryPlan.IndexDescs[i].IndexMultiKey);
                    }

                    subqueryLookup = _queryPlan.LookupStrategyFactory.MakeStrategy(
                        indexes,
                        exprEvaluatorContext,
                        instance.RootViewInstance.VirtualDataWindow);
                    subqueryLookup = new SubordIndexedTableLookupStrategyLocking(
                        subqueryLookup,
                        instance.TailViewInstance.AgentInstanceContext.AgentInstanceLock);
                }
            }
            else {
                var instance = _table.GetTableInstance(exprEvaluatorContext.AgentInstanceId);
                var @lock = exprEvaluatorContext.IsWritesToTables
                    ? instance.TableLevelRWLock.WriteLock
                    : instance.TableLevelRWLock.ReadLock;
                if (_queryPlan == null) {
                    subqueryLookup = new SubordFullTableScanTableLookupStrategy(@lock, instance.IterableTableScan);
                }
                else {
                    var indexes = new EventTable[_queryPlan.IndexDescs.Length];
                    for (var i = 0; i < indexes.Length; i++) {
                        indexes[i] = instance.IndexRepository.GetIndexByDesc(_queryPlan.IndexDescs[i].IndexMultiKey);
                    }

                    subqueryLookup = _queryPlan.LookupStrategyFactory.MakeStrategy(indexes, exprEvaluatorContext, null);
                    subqueryLookup = new SubordIndexedTableLookupTableStrategy(subqueryLookup, @lock);
                }
            }

            return new SubSelectStrategyRealization(
                subqueryLookup,
                subselectAggregationPreprocessor,
                aggregationService,
                null,
                null,
                null,
                null);
        }

        public LookupStrategyDesc LookupStrategyDesc => _queryPlan == null
            ? LookupStrategyDesc.SCAN
            : _queryPlan.LookupStrategyFactory.LookupStrategyDesc;
    }
} // end of namespace