///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.common.@internal.epl.namedwindow.core;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.epl.ontrigger
{
    /// <summary>
    ///     View for the on-delete statement that handles removing events from a named window.
    /// </summary>
    public class OnExprViewNamedWindowDelete : OnExprViewNameWindowBase
    {
        public bool IsSilentDelete { get; }
        public OnExprViewNamedWindowDelete(
            SubordWMatchExprLookupStrategy lookupStrategy,
            NamedWindowRootViewInstance rootView,
            AgentInstanceContext agentInstanceContext)
            : base(lookupStrategy, rootView, agentInstanceContext)
        {
            IsSilentDelete = HintEnum.SILENT_DELETE.HasHint(agentInstanceContext.Annotations);
        }

        public override EventType EventType => rootView.EventType;

        public override void HandleMatching(
            EventBean[] triggerEvents,
            EventBean[] matchingEvents)
        {
            agentInstanceContext.InstrumentationProvider.QInfraOnAction(
                OnTriggerType.ON_DELETE,
                triggerEvents,
                matchingEvents);

            if (matchingEvents != null && matchingEvents.Length > 0) {
                // Events to delete are indicated via old data
                rootView.Update(null, matchingEvents);

                if (IsSilentDelete) {
                    rootView.ClearDeliveriesRemoveStream(matchingEvents);
                }
                
                var statementResultService = agentInstanceContext.StatementResultService;
                // The on-delete listeners receive the events deleted, but only if there is interest
                if (statementResultService.IsMakeNatural || statementResultService.IsMakeSynthetic) {
                    Child.Update(matchingEvents, null);
                }
            }

            agentInstanceContext.InstrumentationProvider.AInfraOnAction();
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return CollectionUtil.NULL_EVENT_ITERATOR;
        }
    }
} // end of namespace