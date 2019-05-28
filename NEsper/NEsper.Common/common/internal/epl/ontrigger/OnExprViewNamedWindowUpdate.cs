///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.common.@internal.epl.namedwindow.core;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.epl.ontrigger
{
    /// <summary>
    ///     View for the on-delete statement that handles removing events from a named window.
    /// </summary>
    public class OnExprViewNamedWindowUpdate : OnExprViewNameWindowBase
    {
        private new readonly InfraOnUpdateViewFactory parent;

        public OnExprViewNamedWindowUpdate(
            SubordWMatchExprLookupStrategy lookupStrategy,
            NamedWindowRootViewInstance rootView,
            AgentInstanceContext agentInstanceContext,
            InfraOnUpdateViewFactory parent)
            : base(lookupStrategy, rootView, agentInstanceContext)
        {
            this.parent = parent;
        }

        public override EventType EventType => rootView.EventType;

        public override void HandleMatching(
            EventBean[] triggerEvents,
            EventBean[] matchingEvents)
        {
            agentInstanceContext.InstrumentationProvider.QInfraOnAction(
                OnTriggerType.ON_UPDATE, triggerEvents, matchingEvents);

            if (matchingEvents == null || matchingEvents.Length == 0) {
                agentInstanceContext.InstrumentationProvider.AInfraOnAction();
                return;
            }

            var eventsPerStream = new EventBean[3];

            var newData = new OneEventCollection();
            var oldData = new OneEventCollection();

            foreach (var triggerEvent in triggerEvents) {
                eventsPerStream[1] = triggerEvent;
                foreach (var matchingEvent in matchingEvents) {
                    var copy = parent.UpdateHelperNamedWindow.UpdateWCopy(
                        matchingEvent, eventsPerStream, ExprEvaluatorContext);
                    newData.Add(copy);
                    oldData.Add(matchingEvent);
                }
            }

            if (!newData.IsEmpty()) {
                // Events to delete are indicated via old data
                rootView.Update(newData.ToArray(), oldData.ToArray());

                // The on-delete listeners receive the events deleted, but only if there is interest
                var statementResultService = agentInstanceContext.StatementResultService;
                if (statementResultService.IsMakeNatural || statementResultService.IsMakeSynthetic) {
                    if (Child != null) {
                        Child.Update(newData.ToArray(), oldData.ToArray());
                    }
                }
            }

            // Keep the last delete records
            agentInstanceContext.InstrumentationProvider.AInfraOnAction();
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return CollectionUtil.NULL_EVENT_ITERATOR;
        }
    }
} // end of namespace