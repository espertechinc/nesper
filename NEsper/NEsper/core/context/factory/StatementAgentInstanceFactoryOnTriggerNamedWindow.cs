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
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.activator;
using com.espertech.esper.core.context.subselect;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.core.start;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.@join.hint;
using com.espertech.esper.epl.@join.table;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.named;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.view;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.core.context.factory
{
    public class StatementAgentInstanceFactoryOnTriggerNamedWindow : StatementAgentInstanceFactoryOnTriggerBase
    {
        private readonly ResultSetProcessorFactoryDesc _resultSetProcessorPrototype;
        private readonly ResultSetProcessorFactoryDesc _outputResultSetProcessorPrototype;
        private readonly NamedWindowOnExprFactory _onExprFactory;
        private readonly OutputProcessViewFactory _outputProcessViewFactory;
        private readonly NamedWindowProcessor _processor;

        private readonly SubordinateWMatchExprQueryPlanResult _queryPlan;

        public StatementAgentInstanceFactoryOnTriggerNamedWindow(StatementContext statementContext, StatementSpecCompiled statementSpec, EPServicesContext services, ViewableActivator activator, SubSelectStrategyCollection subSelectStrategyCollection, ResultSetProcessorFactoryDesc resultSetProcessorPrototype, ExprNode validatedJoin, ResultSetProcessorFactoryDesc outputResultSetProcessorPrototype, NamedWindowOnExprFactory onExprFactory, OutputProcessViewFactory outputProcessViewFactory, EventType activatorResultEventType, NamedWindowProcessor processor, IList<StopCallback> stopCallbacks)
            : base(statementContext, statementSpec, services, activator, subSelectStrategyCollection)
        {
            _resultSetProcessorPrototype = resultSetProcessorPrototype;
            _outputResultSetProcessorPrototype = outputResultSetProcessorPrototype;
            _onExprFactory = onExprFactory;
            _outputProcessViewFactory = outputProcessViewFactory;
            _processor = processor;

            IndexHintPair pair = GetIndexHintPair(statementContext, statementSpec);
            IndexHint indexHint = pair.IndexHint;
            ExcludePlanHint excludePlanHint = pair.ExcludePlanHint;

            _queryPlan = SubordinateQueryPlanner.PlanOnExpression(
                    validatedJoin, activatorResultEventType, indexHint, processor.IsEnableSubqueryIndexShare, -1, excludePlanHint,
                    processor.IsVirtualDataWindow, processor.EventTableIndexMetadataRepo, processor.NamedWindowType,
                    processor.OptionalUniqueKeyProps, false, statementContext.StatementName, statementContext.StatementId, statementContext.Annotations);
            if (_queryPlan.IndexDescs != null)
            {
                SubordinateQueryPlannerUtil.AddIndexMetaAndRef(_queryPlan.IndexDescs, processor.EventTableIndexMetadataRepo, statementContext.StatementName);
                stopCallbacks.Add(new ProxyStopCallback(() =>
                {
                    for (int i = 0; i < _queryPlan.IndexDescs.Length; i++)
                    {
                        bool last = processor.EventTableIndexMetadataRepo.RemoveIndexReference(_queryPlan.IndexDescs[i].IndexMultiKey, statementContext.StatementName);
                        if (last)
                        {
                            processor.EventTableIndexMetadataRepo.RemoveIndex(_queryPlan.IndexDescs[i].IndexMultiKey);
                            processor.RemoveAllInstanceIndexes(_queryPlan.IndexDescs[i].IndexMultiKey);
                        }
                    }
                }));
            }
            SubordinateQueryPlannerUtil.QueryPlanLogOnExpr(processor.RootView.IsQueryPlanLogging, NamedWindowRootView.QueryPlanLog,
                    _queryPlan, statementContext.Annotations, statementContext.EngineImportService);
        }

        public override OnExprViewResult DetermineOnExprView(AgentInstanceContext agentInstanceContext, IList<StopCallback> stopCallbacks, bool isRecoveringResilient)
        {
            // get result set processor and aggregation services
            Pair<ResultSetProcessor, AggregationService> pair = EPStatementStartMethodHelperUtil.StartResultSetAndAggregation(_resultSetProcessorPrototype, agentInstanceContext, false, null);

            // get named window processor instance
            NamedWindowProcessorInstance processorInstance = _processor.GetProcessorInstance(agentInstanceContext);

            // obtain on-expr view
            EventTable[] indexes = null;
            if (_queryPlan.IndexDescs != null)
            {
                indexes = SubordinateQueryPlannerUtil.RealizeTables(
                    _queryPlan.IndexDescs, 
                    _processor.NamedWindowType, 
                    processorInstance.RootViewInstance.IndexRepository, 
                    processorInstance.RootViewInstance.DataWindowContents, 
                    processorInstance.TailViewInstance.AgentInstanceContext,
                    isRecoveringResilient);
            }
            SubordWMatchExprLookupStrategy strategy = _queryPlan.Factory.Realize(indexes, agentInstanceContext, processorInstance.RootViewInstance.DataWindowContents, processorInstance.RootViewInstance.VirtualDataWindow);
            NamedWindowOnExprBaseView onExprBaseView = _onExprFactory.Make(strategy, processorInstance.RootViewInstance, agentInstanceContext, pair.First);

            return new OnExprViewResult(onExprBaseView, pair.Second);
        }

        public override void AssignExpressions(StatementAgentInstanceFactoryResult result)
        {
        }

        public override void UnassignExpressions()
        {
        }

        public override View DetermineFinalOutputView(AgentInstanceContext agentInstanceContext, View onExprView)
        {
            if ((StatementSpec.OnTriggerDesc.OnTriggerType == OnTriggerType.ON_DELETE) ||
                (StatementSpec.OnTriggerDesc.OnTriggerType == OnTriggerType.ON_UPDATE) ||
                (StatementSpec.OnTriggerDesc.OnTriggerType == OnTriggerType.ON_MERGE))
            {

                ResultSetProcessor outputResultSetProcessor = _outputResultSetProcessorPrototype.ResultSetProcessorFactory.Instantiate(null, null, agentInstanceContext);
                View outputView = _outputProcessViewFactory.MakeView(outputResultSetProcessor, agentInstanceContext);
                onExprView.AddView(outputView);
                return outputView;
            }

            return onExprView;
        }

        internal static IndexHintPair GetIndexHintPair(StatementContext statementContext, StatementSpecCompiled statementSpec)
        {
            IndexHint indexHint = IndexHint.GetIndexHint(statementContext.Annotations);
            ExcludePlanHint excludePlanHint = null;
            if (statementSpec.OnTriggerDesc is OnTriggerWindowDesc)
            {
                OnTriggerWindowDesc onTriggerWindowDesc = (OnTriggerWindowDesc)statementSpec.OnTriggerDesc;
                string[] streamNames = { onTriggerWindowDesc.OptionalAsName, statementSpec.StreamSpecs[0].OptionalStreamName };
                excludePlanHint = ExcludePlanHint.GetHint(streamNames, statementContext);
            }
            return new IndexHintPair(indexHint, excludePlanHint);
        }

        public class IndexHintPair
        {
            public IndexHintPair(IndexHint indexHint, ExcludePlanHint excludePlanHint)
            {
                IndexHint = indexHint;
                ExcludePlanHint = excludePlanHint;
            }

            public IndexHint IndexHint { get; private set; }

            public ExcludePlanHint ExcludePlanHint { get; private set; }
        }
    }
}
