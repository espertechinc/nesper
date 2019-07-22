///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
    public class OnExprViewTableMerge : OnExprViewTableBase
    {
        private new readonly InfraOnMergeViewFactory parent;

        public OnExprViewTableMerge(
            SubordWMatchExprLookupStrategy lookupStrategy,
            TableInstance tableInstance,
            AgentInstanceContext agentInstanceContext,
            InfraOnMergeViewFactory parent)
            : base(lookupStrategy, tableInstance, agentInstanceContext, parent.OnMergeHelper.IsRequiresTableWriteLock)
        {
            this.parent = parent;
        }

        public override void HandleMatching(
            EventBean[] triggerEvents,
            EventBean[] matchingEvents)
        {
            var instrumentationCommon = agentInstanceContext.InstrumentationProvider;
            instrumentationCommon.QInfraOnAction(OnTriggerType.ON_MERGE, triggerEvents, matchingEvents);

            var eventsPerStream = new EventBean[3]; // first:table, second: trigger, third:before-update (optional)

            var statementResultService = agentInstanceContext.StatementResultService;
            var postResultsToListeners = statementResultService.IsMakeNatural || statementResultService.IsMakeSynthetic;
            OnExprViewTableChangeHandler changeHandlerRemoved = null;
            OnExprViewTableChangeHandler changeHandlerAdded = null;
            if (postResultsToListeners) {
                changeHandlerRemoved = new OnExprViewTableChangeHandler(tableInstance.Table);
                changeHandlerAdded = new OnExprViewTableChangeHandler(tableInstance.Table);
            }

            if (matchingEvents == null || matchingEvents.Length == 0) {
                var unmatched = parent.OnMergeHelper.Unmatched;

                foreach (var triggerEvent in triggerEvents) {
                    eventsPerStream[1] = triggerEvent;
                    instrumentationCommon.QInfraMergeWhenThens(false, triggerEvent, unmatched.Count);

                    var count = -1;
                    foreach (var action in unmatched) {
                        count++;
                        instrumentationCommon.QInfraMergeWhenThenItem(false, count);

                        if (!action.IsApplies(eventsPerStream, ExprEvaluatorContext)) {
                            instrumentationCommon.AInfraMergeWhenThenItem(false, false);
                            continue;
                        }

                        action.ApplyTable(
                            null,
                            eventsPerStream,
                            tableInstance,
                            changeHandlerAdded,
                            changeHandlerRemoved,
                            agentInstanceContext);
                        instrumentationCommon.AInfraMergeWhenThenItem(false, true);
                        break; // apply no other actions
                    }

                    instrumentationCommon.AInfraMergeWhenThens(false);
                }
            }
            else {
                var matched = parent.OnMergeHelper.Matched;

                foreach (var triggerEvent in triggerEvents) {
                    eventsPerStream[1] = triggerEvent;
                    instrumentationCommon.QInfraMergeWhenThens(true, triggerEvent, matched.Count);

                    foreach (var matchingEvent in matchingEvents) {
                        eventsPerStream[0] = matchingEvent;

                        var count = -1;
                        foreach (var action in matched) {
                            count++;
                            instrumentationCommon.QInfraMergeWhenThenItem(true, count);

                            if (!action.IsApplies(eventsPerStream, ExprEvaluatorContext)) {
                                instrumentationCommon.AInfraMergeWhenThenItem(true, false);
                                continue;
                            }

                            action.ApplyTable(
                                matchingEvent,
                                eventsPerStream,
                                tableInstance,
                                changeHandlerAdded,
                                changeHandlerRemoved,
                                agentInstanceContext);
                            instrumentationCommon.AInfraMergeWhenThenItem(true, true);
                            break; // apply no other actions
                        }
                    }

                    instrumentationCommon.AInfraMergeWhenThens(true);
                }
            }

            // The on-delete listeners receive the events deleted, but only if there is interest
            if (postResultsToListeners) {
                var postedNew = changeHandlerAdded.Events;
                var postedOld = changeHandlerRemoved.Events;
                if (postedNew != null || postedOld != null) {
                    Child.Update(postedNew, postedOld);
                }
            }

            instrumentationCommon.AInfraOnAction();
        }
    }
} // end of namespace