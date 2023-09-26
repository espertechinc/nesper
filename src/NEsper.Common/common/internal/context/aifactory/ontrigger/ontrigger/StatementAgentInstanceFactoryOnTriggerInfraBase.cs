///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.aifactory.ontrigger.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.common.@internal.epl.namedwindow.core;
using com.espertech.esper.common.@internal.epl.ontrigger;
using com.espertech.esper.common.@internal.epl.output.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.@internal.context.aifactory.ontrigger.ontrigger
{
    public abstract class StatementAgentInstanceFactoryOnTriggerInfraBase : StatementAgentInstanceFactoryOnTriggerBase,
        StatementReadyCallback
    {
        private InfraOnExprBaseViewFactory factory;
        private ResultSetProcessorFactoryProvider nonSelectRSPFactoryProvider;
        private SubordinateWMatchExprQueryPlan queryPlan;

        public NamedWindow NamedWindow { get; set; }

        public Table Table { get; set; }

        protected abstract bool IsSelect { get; }

        public SubordinateWMatchExprQueryPlan QueryPlan {
            set => queryPlan = value;
        }

        public ResultSetProcessorFactoryProvider NonSelectRSPFactoryProvider {
            set => nonSelectRSPFactoryProvider = value;
        }

        public void Ready(
            StatementContext statementContext,
            ModuleIncidentals moduleIncidentals,
            bool recovery)
        {
            var infraEventType =
                NamedWindow != null ? NamedWindow.RootView.EventType : Table.MetaData.InternalEventType;
            factory = SetupFactory(infraEventType, NamedWindow, Table, statementContext);

            if (queryPlan.IndexDescs != null) {
                var indexInfo = NamedWindow != null
                    ? NamedWindow.EventTableIndexMetadata
                    : Table.EventTableIndexMetadata;
                SubordinateQueryPlannerUtil.AddIndexMetaAndRef(
                    queryPlan.IndexDescs,
                    indexInfo,
                    statementContext.DeploymentId,
                    statementContext.StatementName);

                statementContext.AddFinalizeCallback(
                    new ProxyStatementFinalizeCallback {
                        ProcStatementDestroyed = context => {
                            for (var i = 0; i < queryPlan.IndexDescs.Length; i++) {
                                if (NamedWindow != null) {
                                    var last = NamedWindow.EventTableIndexMetadata.RemoveIndexReference(
                                        queryPlan.IndexDescs[i].IndexMultiKey,
                                        statementContext.DeploymentId);
                                    if (last) {
                                        NamedWindow.EventTableIndexMetadata.RemoveIndex(
                                            queryPlan.IndexDescs[i].IndexMultiKey);
                                        NamedWindow.RemoveAllInstanceIndexes(queryPlan.IndexDescs[i].IndexMultiKey);
                                    }
                                }
                                else {
                                    var last = Table.EventTableIndexMetadata.RemoveIndexReference(
                                        queryPlan.IndexDescs[i].IndexMultiKey,
                                        statementContext.DeploymentId);
                                    if (last) {
                                        Table.EventTableIndexMetadata.RemoveIndex(
                                            queryPlan.IndexDescs[i].IndexMultiKey);
                                        Table.RemoveAllInstanceIndexes(queryPlan.IndexDescs[i].IndexMultiKey);
                                    }
                                }
                            }
                        }
                    });
            }
        }

        protected abstract InfraOnExprBaseViewFactory SetupFactory(
            EventType infraEventType,
            NamedWindow namedWindow,
            Table table,
            StatementContext statementContext);

        public override InfraOnExprBaseViewResult DetermineOnExprView(
            AgentInstanceContext agentInstanceContext,
            IList<AgentInstanceMgmtCallback> stopCallbacks,
            bool isRecoveringResilient)
        {
            // get instance
            NamedWindowInstance namedWindowInstance = null;
            TableInstance tableInstance = null;
            if (NamedWindow != null) {
                namedWindowInstance = NamedWindow.GetNamedWindowInstance(agentInstanceContext);
                if (namedWindowInstance == null) {
                    throw new EPRuntimeException(
                        "Failed to obtain named window instance for named window '" + NamedWindow.Name + "'");
                }
            }
            else {
                tableInstance = Table.GetTableInstance(agentInstanceContext.AgentInstanceId);
                if (tableInstance == null) {
                    throw new EPRuntimeException("Failed to obtain table instance for table '" + Table.Name + "'");
                }
            }

            // obtain index
            EventTable[] indexes = null;
            if (queryPlan.IndexDescs != null) {
                if (NamedWindow != null) {
                    indexes = SubordinateQueryPlannerUtil.RealizeTables(
                        queryPlan.IndexDescs,
                        namedWindowInstance.RootViewInstance.EventType,
                        namedWindowInstance.RootViewInstance.IndexRepository,
                        namedWindowInstance.RootViewInstance.DataWindowContents,
                        namedWindowInstance.TailViewInstance.AgentInstanceContext,
                        isRecoveringResilient);

                    stopCallbacks.Add(
                        new ProxyAgentInstanceMgmtCallback {
                            ProcStop = services => {
                                if (services.AgentInstanceContext.ContextName == null) {
                                    for (var i = 0; i < queryPlan.IndexDescs.Length; i++) {
                                        var last = NamedWindow.EventTableIndexMetadata.RemoveIndexReference(
                                            queryPlan.IndexDescs[i].IndexMultiKey,
                                            agentInstanceContext.DeploymentId);
                                        if (last) {
                                            NamedWindow.EventTableIndexMetadata.RemoveIndex(
                                                queryPlan.IndexDescs[i].IndexMultiKey);
                                            NamedWindow.RemoveAllInstanceIndexes(queryPlan.IndexDescs[i].IndexMultiKey);
                                        }
                                    }
                                }
                                else {
                                    for (var i = 0; i < queryPlan.IndexDescs.Length; i++) {
                                        NamedWindow.RemoveIndexInstance(
                                            queryPlan.IndexDescs[i].IndexMultiKey,
                                            services.AgentInstanceContext.AgentInstanceId);
                                    }
                                }
                            }
                        });
                }
                else {
                    indexes = new EventTable[queryPlan.IndexDescs.Length];
                    for (var i = 0; i < indexes.Length; i++) {
                        indexes[i] =
                            tableInstance.IndexRepository.GetIndexByDesc(queryPlan.IndexDescs[i].IndexMultiKey);
                    }
                }
            }

            // realize lookup strategy
            var scanIterable = NamedWindow != null
                ? namedWindowInstance.RootViewInstance.DataWindowContents
                : tableInstance.IterableTableScan;
            var virtualDW =
                NamedWindow != null ? namedWindowInstance.RootViewInstance.VirtualDataWindow : null;
            var lookupStrategy = queryPlan.Factory.Realize(
                indexes,
                agentInstanceContext,
                scanIterable,
                virtualDW);

            // realize view
            if (NamedWindow != null) {
                return factory.MakeNamedWindow(
                    lookupStrategy,
                    namedWindowInstance.RootViewInstance,
                    agentInstanceContext);
            }

            return factory.MakeTable(lookupStrategy, tableInstance, agentInstanceContext);
        }

        public override View DetermineFinalOutputView(
            AgentInstanceContext agentInstanceContext,
            View onExprView)
        {
            if (!IsSelect) {
                var pair =
                    StatementAgentInstanceFactoryUtil.StartResultSetAndAggregation(
                        nonSelectRSPFactoryProvider,
                        agentInstanceContext,
                        false,
                        null);
                var @out = new OutputProcessViewSimpleWProcessor(agentInstanceContext, pair.First);
                @out.Parent = onExprView;
                onExprView.Child = @out;
                return @out;
            }

            return onExprView;
        }
    }
} // end of namespace