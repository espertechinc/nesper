///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.compat.collections;


namespace com.espertech.esper.common.@internal.epl.ontrigger
{
    public class OnExprViewTableSelect : OnExprViewTableBase
    {
        private readonly InfraOnSelectViewFactory parent;
        private readonly ResultSetProcessor resultSetProcessor;
        private readonly bool audit;
        private readonly bool deleteAndSelect;
        private readonly TableInstance tableInstanceInsertInto;
        private readonly ExprEvaluator eventPrecedence;

        public OnExprViewTableSelect(
            SubordWMatchExprLookupStrategy lookupStrategy,
            TableInstance tableInstance,
            AgentInstanceContext agentInstanceContext,
            ResultSetProcessor resultSetProcessor,
            InfraOnSelectViewFactory parent,
            bool audit,
            bool deleteAndSelect,
            TableInstance tableInstanceInsertInto,
            ExprEvaluator eventPrecedence) : base(lookupStrategy, tableInstance, agentInstanceContext, deleteAndSelect)
        {
            this.parent = parent;
            this.resultSetProcessor = resultSetProcessor;
            this.audit = audit;
            this.deleteAndSelect = deleteAndSelect;
            this.tableInstanceInsertInto = tableInstanceInsertInto;
            this.eventPrecedence = eventPrecedence;
        }

        public override void HandleMatching(
            EventBean[] triggerEvents,
            EventBean[] matchingEvents)
        {
            agentInstanceContext.InstrumentationProvider.QInfraOnAction(
                OnTriggerType.ON_SELECT,
                triggerEvents,
                matchingEvents);

            // clear state from prior results
            resultSetProcessor.Clear();

            // build join result
            // use linked hash set to retain order of join results for last/first/window to work most intuitively
            var newEvents =
                OnExprViewNamedWindowSelect.BuildJoinResult(triggerEvents, matchingEvents);

            // process matches
            var pair = resultSetProcessor.ProcessJoinResult(
                newEvents,
                EmptySet<MultiKeyArrayOfKeys<EventBean>>.Instance,
                false);
            var newData = pair?.First;

            // handle distinct and insert
            newData = InfraOnSelectUtil.HandleDistintAndInsert(
                newData,
                parent,
                agentInstanceContext,
                tableInstanceInsertInto,
                audit,
                eventPrecedence);

            // The on-select listeners receive the events selected
            if (newData != null && newData.Length > 0) {
                // And post only if we have listeners/subscribers that need the data
                var statementResultService = agentInstanceContext.StatementResultService;
                if (statementResultService.IsMakeNatural || statementResultService.IsMakeSynthetic) {
                    child.Update(newData, null);
                }
            }

            // clear state from prior results
            resultSetProcessor.Clear();

            // Events to delete are indicated via old data
            if (deleteAndSelect) {
                foreach (var @event in matchingEvents) {
                    tableInstance.DeleteEvent(@event);
                }
            }

            agentInstanceContext.InstrumentationProvider.AInfraOnAction();
        }

        public override EventType EventType {
            get {
                if (resultSetProcessor != null) {
                    return resultSetProcessor.ResultEventType;
                }
                else {
                    return base.EventType;
                }
            }
        }
    }
} // end of namespace