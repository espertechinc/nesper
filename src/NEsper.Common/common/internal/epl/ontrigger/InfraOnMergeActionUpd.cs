///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.epl.table.update;
using com.espertech.esper.common.@internal.epl.updatehelper;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.ontrigger
{
    public class InfraOnMergeActionUpd : InfraOnMergeAction,
        TableUpdateStrategyRedoCallback,
        StatementReadyCallback
    {
        private readonly EventBeanUpdateHelperWCopy namedWindowUpdate;
        private readonly EventBeanUpdateHelperNoCopy tableUpdate;
        private readonly Table table;
        private TableUpdateStrategy tableUpdateStrategy;

        public InfraOnMergeActionUpd(
            ExprEvaluator optionalFilter,
            EventBeanUpdateHelperWCopy namedWindowUpdate)
            : base(
                optionalFilter)
        {
            this.namedWindowUpdate = namedWindowUpdate;
            tableUpdate = null;
        }

        public InfraOnMergeActionUpd(
            ExprEvaluator optionalFilter,
            EventBeanUpdateHelperNoCopy tableUpdate,
            Table table)
            : base(optionalFilter)
        {
            this.tableUpdate = tableUpdate;
            namedWindowUpdate = null;
            this.table = table;
            InitTableUpdateStrategy(table);
        }

        public override string Name => "update";

        public void Ready(
            StatementContext statementContext,
            ModuleIncidentals moduleIncidentals,
            bool recovery)
        {
            table.AddUpdateStrategyCallback(this);
            statementContext.AddFinalizeCallback(
                new ProxyStatementFinalizeCallback {
                    ProcStatementDestroyed = context => table.RemoveUpdateStrategyCallback(this)
                });
        }

        public bool IsMerge => true;

        public string[] TableUpdatedProperties => tableUpdate.UpdatedProperties;

        public void InitTableUpdateStrategy(Table table)
        {
            try {
                tableUpdateStrategy = TableUpdateStrategyFactory.ValidateGetTableUpdateStrategy(
                    table.MetaData,
                    tableUpdate,
                    true);
            }
            catch (ExprValidationException e) {
                throw new EPException(e.Message, e);
            }
        }

        public override void Apply(
            EventBean matchingEvent,
            EventBean[] eventsPerStream,
            OneEventCollection newData,
            OneEventCollection oldData,
            AgentInstanceContext agentInstanceContext)
        {
            var copy = namedWindowUpdate.Invoke(matchingEvent, eventsPerStream, agentInstanceContext);
            newData.Add(copy);
            oldData.Add(matchingEvent);
        }

        public override void Apply(
            EventBean matchingEvent,
            EventBean[] eventsPerStream,
            TableInstance tableStateInstance,
            OnExprViewTableChangeHandler changeHandlerAdded,
            OnExprViewTableChangeHandler changeHandlerRemoved,
            AgentInstanceContext agentInstanceContext)
        {
            changeHandlerRemoved?.Add(matchingEvent, eventsPerStream, false, agentInstanceContext);

            tableUpdateStrategy.UpdateTable(
                Collections.SingletonList(matchingEvent),
                tableStateInstance,
                eventsPerStream,
                agentInstanceContext);
            changeHandlerAdded?.Add(matchingEvent, eventsPerStream, false, agentInstanceContext);
        }
    }
} // end of namespace