///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.ontrigger
{
    public class OnExprViewTableUpdate : OnExprViewTableBase
    {
        private readonly InfraOnUpdateViewFactory parent;

        public OnExprViewTableUpdate(
            SubordWMatchExprLookupStrategy lookupStrategy,
            TableInstance tableInstance,
            AgentInstanceContext agentInstanceContext,
            InfraOnUpdateViewFactory parent)
            : base(
                lookupStrategy,
                tableInstance,
                agentInstanceContext,
                true)
        {
            this.parent = parent;
        }

        public override void HandleMatching(
            EventBean[] triggerEvents,
            EventBean[] matchingEvents)
        {
            agentInstanceContext.InstrumentationProvider.QInfraOnAction(
                OnTriggerType.ON_UPDATE,
                triggerEvents,
                matchingEvents);

            var eventsPerStream = new EventBean[3];

            var statementResultService = agentInstanceContext.StatementResultService;
            var postUpdates = statementResultService.IsMakeNatural || statementResultService.IsMakeSynthetic;
            EventBean[] postedOld = null;
            if (postUpdates) {
                postedOld = OnExprViewTableUtil.ToPublic(
                    matchingEvents,
                    tableInstance.Table,
                    triggerEvents,
                    false,
                    ExprEvaluatorContext);
            }

            var tableUpdateStrategy = parent.TableUpdateStrategy;

            foreach (var triggerEvent in triggerEvents) {
                eventsPerStream[1] = triggerEvent;
                var matching = (IList<EventBean>)matchingEvents ?? EmptyList<EventBean>.Instance;
                tableUpdateStrategy.UpdateTable(
                    matching,
                    tableInstance,
                    eventsPerStream,
                    agentInstanceContext);
            }

            // The on-delete listeners receive the events deleted, but only if there is interest
            if (postUpdates) {
                var postedNew = OnExprViewTableUtil.ToPublic(
                    matchingEvents,
                    tableInstance.Table,
                    triggerEvents,
                    true,
                    ExprEvaluatorContext);
                Child.Update(postedNew, postedOld);
            }

            agentInstanceContext.InstrumentationProvider.AInfraOnAction();
        }
    }
} // end of namespace