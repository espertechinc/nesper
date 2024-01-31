///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.common.@internal.epl.table.core;

namespace com.espertech.esper.common.@internal.epl.ontrigger
{
    /// <summary>
    ///     View for the on-delete statement that handles removing events from a named window.
    /// </summary>
    public class OnExprViewTableDelete : OnExprViewTableBase
    {
        public OnExprViewTableDelete(
            SubordWMatchExprLookupStrategy lookupStrategy,
            TableInstance rootView,
            AgentInstanceContext agentInstanceContext)
            : base(lookupStrategy, rootView, agentInstanceContext, true)
        {
        }

        public override EventType EventType => tableInstance.Table.MetaData.PublicEventType;

        public override void HandleMatching(
            EventBean[] triggerEvents,
            EventBean[] matchingEvents)
        {
            agentInstanceContext.InstrumentationProvider.QInfraOnAction(
                OnTriggerType.ON_DELETE,
                triggerEvents,
                matchingEvents);

            if (matchingEvents != null && matchingEvents.Length > 0) {
                foreach (var @event in matchingEvents) {
                    tableInstance.DeleteEvent(@event);
                }

                // The on-delete listeners receive the events deleted, but only if there is interest
                var statementResultService = agentInstanceContext.StatementResultService;
                if (statementResultService.IsMakeNatural || statementResultService.IsMakeSynthetic) {
                    var posted = OnExprViewTableUtil.ToPublic(
                        matchingEvents,
                        tableInstance.Table,
                        triggerEvents,
                        true,
                        ExprEvaluatorContext);
                    Child.Update(posted, null);
                }
            }

            agentInstanceContext.InstrumentationProvider.AInfraOnAction();
        }
    }
} // end of namespace