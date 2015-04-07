///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.core.context.activator;
using com.espertech.esper.core.context.subselect;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.core.start;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.@join.table;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.table.onaction;
using com.espertech.esper.epl.view;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.core.context.factory
{
    public class StatementAgentInstanceFactoryOnTriggerTable : StatementAgentInstanceFactoryOnTriggerBase
    {
        private readonly ResultSetProcessorFactoryDesc _resultSetProcessorPrototype;
        private readonly ResultSetProcessorFactoryDesc _outputResultSetProcessorPrototype;
        private readonly OutputProcessViewFactory _outputProcessViewFactory;
        private readonly TableOnViewFactory _onExprFactory;
        private readonly SubordinateWMatchExprQueryPlanResult _queryPlanResult;
    
        public StatementAgentInstanceFactoryOnTriggerTable(StatementContext statementContext, StatementSpecCompiled statementSpec, EPServicesContext services, ViewableActivator activator, SubSelectStrategyCollection subSelectStrategyCollection, ResultSetProcessorFactoryDesc resultSetProcessorPrototype, ExprNode validatedJoin, TableOnViewFactory onExprFactory, EventType activatorResultEventType, TableMetadata tableMetadata, ResultSetProcessorFactoryDesc outputResultSetProcessorPrototype, OutputProcessViewFactory outputProcessViewFactory)
            : base(statementContext, statementSpec, services, activator, subSelectStrategyCollection)
        {
            _resultSetProcessorPrototype = resultSetProcessorPrototype;
            _onExprFactory = onExprFactory;
            _outputResultSetProcessorPrototype = outputResultSetProcessorPrototype;
            _outputProcessViewFactory = outputProcessViewFactory;
    
            var pair = StatementAgentInstanceFactoryOnTriggerNamedWindow.GetIndexHintPair(statementContext, statementSpec);
            var indexHint = pair.IndexHint;
            var excludePlanHint = pair.ExcludePlanHint;
    
            _queryPlanResult = SubordinateQueryPlanner.PlanOnExpression(
                    validatedJoin, activatorResultEventType, indexHint, true, -1, excludePlanHint,
                    false, tableMetadata.EventTableIndexMetadataRepo, tableMetadata.InternalEventType,
                    tableMetadata.UniqueKeyProps, true, statementContext.StatementName, statementContext.StatementId, statementContext.Annotations);
            if (_queryPlanResult.IndexDescs != null) {
                for (var i = 0; i < _queryPlanResult.IndexDescs.Length; i++) {
                    tableMetadata.AddIndexReference(_queryPlanResult.IndexDescs[i].IndexName, statementContext.StatementName);
                }
            }
            SubordinateQueryPlannerUtil.QueryPlanLogOnExpr(tableMetadata.IsQueryPlanLogging, TableServiceImpl.QueryPlanLog,
                    _queryPlanResult, statementContext.Annotations);
        }
    
        public override OnExprViewResult DetermineOnExprView(AgentInstanceContext agentInstanceContext, IList<StopCallback> stopCallbacks)
        {
            var onTriggerWindowDesc = (OnTriggerWindowDesc) StatementSpec.OnTriggerDesc;
    
            // get result set processor and aggregation services
            var pair = EPStatementStartMethodHelperUtil.StartResultSetAndAggregation(_resultSetProcessorPrototype, agentInstanceContext);
    
            var state = Services.TableService.GetState(onTriggerWindowDesc.WindowName, agentInstanceContext.AgentInstanceId);
            EventTable[] indexes;
            if (_queryPlanResult.IndexDescs == null) {
                indexes = null;
            }
            else {
                indexes = new EventTable[_queryPlanResult.IndexDescs.Length];
                for (var i = 0; i < indexes.Length; i++) {
                    indexes[i] = state.IndexRepository.GetIndexByDesc(_queryPlanResult.IndexDescs[i].IndexMultiKey);
                }
            }
            var strategy = _queryPlanResult.Factory.Realize(indexes, agentInstanceContext, state.IterableTableScan, null);
            var onExprBaseView = _onExprFactory.Make(strategy, state, agentInstanceContext, pair.First);
    
            return new OnExprViewResult(onExprBaseView, pair.Second);
        }
    
        public override View DetermineFinalOutputView(AgentInstanceContext agentInstanceContext, View onExprView)
        {
            if ((StatementSpec.OnTriggerDesc.OnTriggerType == OnTriggerType.ON_DELETE) ||
                    (StatementSpec.OnTriggerDesc.OnTriggerType == OnTriggerType.ON_UPDATE) ||
                    (StatementSpec.OnTriggerDesc.OnTriggerType == OnTriggerType.ON_MERGE)) {
    
                var outputResultSetProcessor = _outputResultSetProcessorPrototype.ResultSetProcessorFactory.Instantiate(null, null, agentInstanceContext);
                View outputView = _outputProcessViewFactory.MakeView(outputResultSetProcessor, agentInstanceContext);
                onExprView.AddView(outputView);
                return outputView;
            }
    
            return onExprView;
        }
    }
}
