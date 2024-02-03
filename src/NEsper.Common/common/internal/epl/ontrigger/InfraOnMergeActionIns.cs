///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.compat;


namespace com.espertech.esper.common.@internal.epl.ontrigger
{
    public class InfraOnMergeActionIns : InfraOnMergeAction
    {
        private readonly SelectExprProcessor insertHelper;
        private readonly Table insertIntoTable;
        private readonly bool audit;
        private readonly bool route;
        private readonly ExprEvaluator eventPrecedence;

        public InfraOnMergeActionIns(
            ExprEvaluator optionalFilter,
            SelectExprProcessor insertHelper,
            Table insertIntoTable,
            bool audit,
            bool route,
            ExprEvaluator eventPrecedence) : base(optionalFilter)
        {
            this.insertHelper = insertHelper;
            this.insertIntoTable = insertIntoTable;
            this.audit = audit;
            this.route = route;
            this.eventPrecedence = eventPrecedence;
        }

        public override void Apply(
            EventBean matchingEvent,
            EventBean[] eventsPerStream,
            OneEventCollection newData,
            OneEventCollection oldData,
            AgentInstanceContext agentInstanceContext)
        {
            var theEvent = insertHelper.Process(eventsPerStream, true, true, agentInstanceContext);
            if (insertIntoTable != null) {
                var tableInstance = insertIntoTable.GetTableInstance(agentInstanceContext.AgentInstanceId);
                tableInstance.AddEventUnadorned(theEvent);
                return;
            }

            if (!route) {
                newData.Add(theEvent);
                return;
            }

            if (audit) {
                agentInstanceContext.AuditProvider.Insert(theEvent, agentInstanceContext);
            }

            var precedence = 0;
            if (eventPrecedence != null) {
                var result = eventPrecedence
                    .Evaluate(new EventBean[] { theEvent }, true, agentInstanceContext)
                    .AsBoxedInt32();
                if (result != null) {
                    precedence = result.Value;
                }
            }

            agentInstanceContext.InternalEventRouter.Route(theEvent, agentInstanceContext, false, precedence);
        }

        public override void Apply(
            EventBean matchingEvent,
            EventBean[] eventsPerStream,
            TableInstance tableStateInstance,
            OnExprViewTableChangeHandler changeHandlerAdded,
            OnExprViewTableChangeHandler changeHandlerRemoved,
            AgentInstanceContext agentInstanceContext)
        {
            var theEvent = insertHelper.Process(eventsPerStream, true, true, agentInstanceContext);
            if (!route) {
                var aggs = tableStateInstance.Table.AggregationRowFactory.Make();
                ((object[])theEvent.Underlying)[0] = aggs;
                tableStateInstance.AddEvent(theEvent);
                if (changeHandlerAdded != null) {
                    changeHandlerAdded.Add(theEvent, eventsPerStream, true, agentInstanceContext);
                }

                return;
            }

            if (insertIntoTable != null) {
                var tableInstance = insertIntoTable.GetTableInstance(agentInstanceContext.AgentInstanceId);
                tableInstance.AddEventUnadorned(theEvent);
                return;
            }

            if (audit) {
                agentInstanceContext.AuditProvider.Insert(theEvent, agentInstanceContext);
            }

            // Evaluate event precedence
            var precedence = ExprNodeUtilityEvaluate.EvaluateIntOptional(
                eventPrecedence,
                theEvent,
                0,
                agentInstanceContext);
            agentInstanceContext.InternalEventRouter.Route(theEvent, agentInstanceContext, false, precedence);
        }

        public override string Name => route ? "insert-into" : "select";
    }
} // end of namespace