///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.aifactory.ontrigger.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.ontrigger;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.common.@internal.context.aifactory.ontrigger.onsplit
{
    public class StatementAgentInstanceFactoryOnTriggerSplitStream : StatementAgentInstanceFactoryOnTriggerBase,
        StatementReadyCallback
    {
        private bool isFirst;
        private OnSplitItemEval[] items;
        private StatementContext statementContext;

        public OnSplitItemEval[] Items {
            set => items = value;
        }

        public bool IsFirst {
            set => isFirst = value;
        }

        public void Ready(
            StatementContext statementContext,
            ModuleIncidentals moduleIncidentals,
            bool recovery)
        {
            this.statementContext = statementContext;
        }

        public override InfraOnExprBaseViewResult DetermineOnExprView(
            AgentInstanceContext agentInstanceContext,
            IList<AgentInstanceMgmtCallback> stopCallbacks,
            bool isRecoveringReslient)
        {
            var processors = new ResultSetProcessor[items.Length];
            for (var i = 0; i < processors.Length; i++) {
                var factory = items[i].RspFactoryProvider;
                var processor = factory.ResultSetProcessorFactory.Instantiate(null, null, agentInstanceContext);
                processors[i] = processor;
            }

            var tableStateInstances = new TableInstance[processors.Length];
            for (var i = 0; i < items.Length; i++) {
                var table = items[i].InsertIntoTable;
                if (table != null) {
                    tableStateInstances[i] = table.GetTableInstance(agentInstanceContext.AgentInstanceId);
                }
            }

            View view = new RouteResultView(
                isFirst,
                StatementEventType,
                statementContext.EpStatementHandle,
                statementContext.InternalEventRouter,
                tableStateInstances,
                items,
                processors,
                agentInstanceContext);
            return new InfraOnExprBaseViewResult(view, null);
        }

        public override View DetermineFinalOutputView(
            AgentInstanceContext agentInstanceContext,
            View onExprView)
        {
            return onExprView;
        }

        public override IReaderWriterLock ObtainAgentInstanceLock(
            StatementContext statementContext,
            int agentInstanceId)
        {
            return AgentInstanceUtil.NewLock(statementContext, agentInstanceId);
        }
    }
} // end of namespace