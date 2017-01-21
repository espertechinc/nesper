///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.prev;
using com.espertech.esper.epl.expression.prior;
using com.espertech.esper.epl.@join.hint;
using com.espertech.esper.epl.@join.table;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.named;
using com.espertech.esper.epl.subquery;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.core.context.subselect
{
    /// <summary>
    /// Record holding lookup resource references for use by <seealso cref="SubSelectActivationCollection" />.
    /// </summary>
    public class SubSelectStrategyFactoryIndexShare : SubSelectStrategyFactory
    {
        private readonly NamedWindowProcessor _optionalNamedWindowProcessor;
        private readonly TableMetadata _optionalTableMetadata;
        private readonly ExprEvaluator _filterExprEval;
        private readonly AggregationServiceFactoryDesc _aggregationServiceFactory;
        private readonly ExprEvaluator[] _groupByKeys;
        private readonly TableService _tableService;
        private readonly SubordinateQueryPlanDesc _queryPlan;

        public SubSelectStrategyFactoryIndexShare(
            string statementName,
            int statementId,
            int subqueryNum,
            EventType[] outerEventTypesSelect,
            NamedWindowProcessor optionalNamedWindowProcessor,
            TableMetadata optionalTableMetadata,
            bool fullTableScan,
            IndexHint optionalIndexHint,
            SubordPropPlan joinedPropPlan,
            ExprEvaluator filterExprEval,
            AggregationServiceFactoryDesc aggregationServiceFactory,
            ExprEvaluator[] groupByKeys,
            TableService tableService,
            Attribute[] annotations,
            StatementStopService statementStopService)
        {
            _optionalNamedWindowProcessor = optionalNamedWindowProcessor;
            _optionalTableMetadata = optionalTableMetadata;
            _filterExprEval = filterExprEval;
            _aggregationServiceFactory = aggregationServiceFactory;
            _groupByKeys = groupByKeys;
            _tableService = tableService;
    
            bool isLogging;
            ILog log;
            if (optionalTableMetadata != null) {
                isLogging = optionalTableMetadata.IsQueryPlanLogging;
                log = TableServiceImpl.QueryPlanLog;
                _queryPlan = SubordinateQueryPlanner.PlanSubquery(outerEventTypesSelect, joinedPropPlan, false, fullTableScan, optionalIndexHint, true, subqueryNum,
                        false, optionalTableMetadata.EventTableIndexMetadataRepo, optionalTableMetadata.UniqueKeyProps, true, statementName, statementId, annotations);
                if (_queryPlan != null) {
                    for (int i = 0; i < _queryPlan.IndexDescs.Length; i++) {
                        optionalTableMetadata.AddIndexReference(_queryPlan.IndexDescs[i].IndexName, statementName);
                    }
                }
            }
            else
            {
                isLogging = optionalNamedWindowProcessor.RootView.IsQueryPlanLogging;
                log = NamedWindowRootView.QueryPlanLog;
                _queryPlan = SubordinateQueryPlanner.PlanSubquery(outerEventTypesSelect, joinedPropPlan, false, fullTableScan, optionalIndexHint, true, subqueryNum,
                        optionalNamedWindowProcessor.IsVirtualDataWindow, optionalNamedWindowProcessor.EventTableIndexMetadataRepo, optionalNamedWindowProcessor.OptionalUniqueKeyProps, false, statementName, statementId, annotations);
                if (_queryPlan != null && _queryPlan.IndexDescs != null) {
                    SubordinateQueryPlannerUtil.AddIndexMetaAndRef(_queryPlan.IndexDescs, optionalNamedWindowProcessor.EventTableIndexMetadataRepo, statementName);
                    statementStopService.StatementStopped += () =>
                    {
                        for (int i = 0; i < _queryPlan.IndexDescs.Length; i++) {
                            bool last = optionalNamedWindowProcessor.EventTableIndexMetadataRepo.RemoveIndexReference(_queryPlan.IndexDescs[i].IndexMultiKey, statementName);
                            if (last) {
                                optionalNamedWindowProcessor.EventTableIndexMetadataRepo.RemoveIndex(_queryPlan.IndexDescs[i].IndexMultiKey);
                                optionalNamedWindowProcessor.RemoveAllInstanceIndexes(_queryPlan.IndexDescs[i].IndexMultiKey);
                            }
                        }
                    };
                }
            }
    
            SubordinateQueryPlannerUtil.QueryPlanLogOnSubq(isLogging, log, _queryPlan, subqueryNum, annotations);
        }

        public SubSelectStrategyRealization Instantiate(
            EPServicesContext services,
            Viewable viewableRoot,
            AgentInstanceContext agentInstanceContext,
            IList<StopCallback> stopCallbackList,
            int subqueryNumber,
            bool isRecoveringResilient)
        {
            SubselectAggregationPreprocessorBase subselectAggregationPreprocessor = null;

            AggregationService aggregationService = null;
            if (_aggregationServiceFactory != null)
            {
                aggregationService = _aggregationServiceFactory.AggregationServiceFactory.MakeService(
                    agentInstanceContext, agentInstanceContext.StatementContext.EngineImportService, true, subqueryNumber);
                if (_groupByKeys == null)
                {
                    if (_filterExprEval == null)
                    {
                        subselectAggregationPreprocessor =
                            new SubselectAggregationPreprocessorUnfilteredUngrouped(
                                aggregationService, _filterExprEval, null);
                    }
                    else
                    {
                        subselectAggregationPreprocessor =
                            new SubselectAggregationPreprocessorFilteredUngrouped(aggregationService, _filterExprEval, null);
                    }
                }
                else
                {
                    if (_filterExprEval == null)
                    {
                        subselectAggregationPreprocessor =
                            new SubselectAggregationPreprocessorUnfilteredGrouped(
                                aggregationService, _filterExprEval, _groupByKeys);
                    }
                    else
                    {
                        subselectAggregationPreprocessor =
                            new SubselectAggregationPreprocessorFilteredGrouped(
                                aggregationService, _filterExprEval, _groupByKeys);
                    }
                }
            }

            SubordTableLookupStrategy subqueryLookup;
            if (_optionalNamedWindowProcessor != null)
            {
                NamedWindowProcessorInstance instance =
                    _optionalNamedWindowProcessor.GetProcessorInstance(agentInstanceContext);
                if (_queryPlan == null)
                {
                    if (instance.RootViewInstance.IsQueryPlanLogging && NamedWindowRootView.QueryPlanLog.IsInfoEnabled)
                    {
                        NamedWindowRootView.QueryPlanLog.Info("shared, full table scan");
                    }
                    subqueryLookup =
                        new SubordFullTableScanLookupStrategyLocking(
                            instance.RootViewInstance.DataWindowContents,
                            agentInstanceContext.EpStatementAgentInstanceHandle.StatementAgentInstanceLock);
                }
                else
                {
                    EventTable[] tables = null;
                    if (!_optionalNamedWindowProcessor.IsVirtualDataWindow)
                    {
                        tables = SubordinateQueryPlannerUtil.RealizeTables(
                            _queryPlan.IndexDescs, instance.RootViewInstance.EventType,
                            instance.RootViewInstance.IndexRepository,
                            instance.RootViewInstance.DataWindowContents, agentInstanceContext,
                            isRecoveringResilient);
                    }
                    SubordTableLookupStrategy strategy = _queryPlan.LookupStrategyFactory.MakeStrategy(
                        tables, instance.RootViewInstance.VirtualDataWindow);
                    subqueryLookup = new SubordIndexedTableLookupStrategyLocking(
                        strategy, instance.TailViewInstance.AgentInstanceContext.AgentInstanceLock);
                }
            }
            else
            {
                TableStateInstance state = _tableService.GetState(
                    _optionalTableMetadata.TableName, agentInstanceContext.AgentInstanceId);
                ILockable iLock = agentInstanceContext.StatementContext.IsWritesToTables
                    ? state.TableLevelRWLock.WriteLock
                    : state.TableLevelRWLock.ReadLock;
                if (_queryPlan == null)
                {
                    subqueryLookup = new SubordFullTableScanTableLookupStrategy(iLock, state.IterableTableScan);
                }
                else
                {
                    EventTable[] indexes = new EventTable[_queryPlan.IndexDescs.Length];
                    for (int i = 0; i < indexes.Length; i++)
                    {
                        indexes[i] = state.IndexRepository.GetIndexByDesc(_queryPlan.IndexDescs[i].IndexMultiKey);
                    }
                    subqueryLookup = _queryPlan.LookupStrategyFactory.MakeStrategy(indexes, null);
                    subqueryLookup = new SubordIndexedTableLookupTableStrategy(subqueryLookup, iLock);
                }
            }

            return new SubSelectStrategyRealization(
                subqueryLookup, subselectAggregationPreprocessor, aggregationService,
                Collections.GetEmptyMap<ExprPriorNode, ExprPriorEvalStrategy>(),
                Collections.GetEmptyMap<ExprPreviousNode, ExprPreviousEvalStrategy>(),
                null, null);
        }
    }
}
