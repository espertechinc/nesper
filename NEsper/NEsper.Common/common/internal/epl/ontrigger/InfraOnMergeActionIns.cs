///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.ontrigger
{
    public class InfraOnMergeActionIns : InfraOnMergeAction
    {
        private readonly SelectExprProcessor insertHelper;
        private readonly Table insertIntoTable;
        private readonly bool audit;
        private readonly bool route;

        public InfraOnMergeActionIns(
            ExprEvaluator optionalFilter,
            SelectExprProcessor insertHelper,
            Table insertIntoTable,
            bool audit,
            bool route)
            : base(optionalFilter)

        {
            this.insertHelper = insertHelper;
            this.insertIntoTable = insertIntoTable;
            this.audit = audit;
            this.route = route;
        }

        public override void Apply(
            EventBean matchingEvent,
            EventBean[] eventsPerStream,
            OneEventCollection newData,
            OneEventCollection oldData,
            AgentInstanceContext agentInstanceContext)
        {
            EventBean theEvent = insertHelper.Process(eventsPerStream, true, true, agentInstanceContext);

            if (insertIntoTable != null) {
                TableInstance tableInstance = insertIntoTable.GetTableInstance(agentInstanceContext.AgentInstanceId);
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

            agentInstanceContext.InternalEventRouter.Route(theEvent, agentInstanceContext, false);
        }

        public override void Apply(
            EventBean matchingEvent,
            EventBean[] eventsPerStream,
            TableInstance tableStateInstance,
            OnExprViewTableChangeHandler changeHandlerAdded,
            OnExprViewTableChangeHandler changeHandlerRemoved,
            AgentInstanceContext agentInstanceContext)
        {
            EventBean theEvent = insertHelper.Process(eventsPerStream, true, true, agentInstanceContext);
            if (!route) {
                AggregationRow aggs = tableStateInstance.Table.AggregationRowFactory.Make();
                ((object[]) theEvent.Underlying)[0] = aggs;
                tableStateInstance.AddEvent(theEvent);
                if (changeHandlerAdded != null) {
                    changeHandlerAdded.Add(theEvent, eventsPerStream, true, agentInstanceContext);
                }

                return;
            }

            if (audit) {
                agentInstanceContext.AuditProvider.Insert(theEvent, agentInstanceContext);
            }

            agentInstanceContext.InternalEventRouter.Route(theEvent, agentInstanceContext, false);
        }

        public override string Name {
            get { return route ? "insert-into" : "select"; }
        }
    }
} // end of namespace