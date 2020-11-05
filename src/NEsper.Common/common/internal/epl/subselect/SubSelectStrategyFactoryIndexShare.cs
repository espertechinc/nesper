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
        private AggregationServiceFactory aggregationServiceFactory;
        private ExprEvaluator filterExprEval;
        private ExprEvaluator groupKeyEval;

        private NamedWindow namedWindow;
        private SubordinateQueryPlanDesc queryPlan;
        private Table table;

        public NamedWindow NamedWindow {
            set => namedWindow = value;
        }

        public Table Table {
            set => table = value;
        }

        public SubordinateQueryPlanDesc QueryPlan {
            set => queryPlan = value;
        }

        public AggregationServiceFactory AggregationServiceFactory {
            set => aggregationServiceFactory = value;
        }

        public ExprEvaluator GroupKeyEval {
            set => groupKeyEval = value;
        }

        public ExprEvaluator FilterExprEval {
            set => filterExprEval = value;
        }

        public void Ready(
            SubSelectStrategyFactoryContext subselectFactoryContext,
            EventType eventType)
        {
            // no action
        }

        public SubSelectStrategyRealization Instantiate(
            Viewable viewableRoot,
            AgentInstanceContext agentInstanceContext,
            IList<AgentInstanceMgmtCallback> stopCallbackList,
            int subqueryNumber,
            bool isRecoveringResilient)
        {
            SubselectAggregationPreprocessorBase subselectAggregationPreprocessor = null;

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
                    new ProxyAgentInstanceMgmtCallback {
                        ProcStop = services => { aggregationServiceStoppable.Stop(); }
                    });

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

            SubordTableLookupStrategy subqueryLookup;
            if (namedWindow != null) {
                var instance = namedWindow.GetNamedWindowInstance(agentInstanceContext);
                if (queryPlan == null) {
                    subqueryLookup = new SubordFullTableScanLookupStrategyLocking(
                        instance.RootViewInstance.DataWindowContents,
                        agentInstanceContext.EpStatementAgentInstanceHandle.StatementAgentInstanceLock);
                }
                else {
                    var indexes = new EventTable[queryPlan.IndexDescs.Length];
                    for (var i = 0; i < indexes.Length; i++) {
                        indexes[i] =
                            instance.RootViewInstance.IndexRepository.GetIndexByDesc(
                                queryPlan.IndexDescs[i].IndexMultiKey);
                    }

                    subqueryLookup = queryPlan.LookupStrategyFactory.MakeStrategy(
                        indexes,
                        agentInstanceContext,
                        instance.RootViewInstance.VirtualDataWindow);
                    subqueryLookup = new SubordIndexedTableLookupStrategyLocking(
                        subqueryLookup,
                        instance.TailViewInstance.AgentInstanceContext.AgentInstanceLock);
                }
            }
            else {
                var instance = table.GetTableInstance(agentInstanceContext.AgentInstanceId);
                var @lock = agentInstanceContext.StatementContext.StatementInformationals.IsWritesToTables
                    ? instance.TableLevelRWLock.WriteLock
                    : instance.TableLevelRWLock.ReadLock;
                if (queryPlan == null) {
                    subqueryLookup = new SubordFullTableScanTableLookupStrategy(@lock, instance.IterableTableScan);
                }
                else {
                    var indexes = new EventTable[queryPlan.IndexDescs.Length];
                    for (var i = 0; i < indexes.Length; i++) {
                        indexes[i] = instance.IndexRepository.GetIndexByDesc(queryPlan.IndexDescs[i].IndexMultiKey);
                    }

                    subqueryLookup = queryPlan.LookupStrategyFactory.MakeStrategy(indexes, agentInstanceContext, null);
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

        public LookupStrategyDesc LookupStrategyDesc => queryPlan == null
            ? LookupStrategyDesc.SCAN
            : queryPlan.LookupStrategyFactory.LookupStrategyDesc;
    }
} // end of namespace