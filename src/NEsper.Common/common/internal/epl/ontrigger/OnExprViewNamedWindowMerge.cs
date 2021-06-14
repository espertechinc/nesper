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
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.diagnostics;

namespace com.espertech.esper.common.@internal.epl.ontrigger
{
    /// <summary>
    ///     View for the on-delete statement that handles removing events from a named window.
    /// </summary>
    public class OnExprViewNamedWindowMerge : OnExprViewNameWindowBase
    {
        private new readonly InfraOnMergeViewFactory parent;

        public OnExprViewNamedWindowMerge(
            SubordWMatchExprLookupStrategy lookupStrategy,
            NamedWindowRootViewInstance rootView,
            AgentInstanceContext agentInstanceContext,
            InfraOnMergeViewFactory parent)
            : base(
                lookupStrategy,
                rootView,
                agentInstanceContext)
        {
            this.parent = parent;
        }

        public override EventType EventType => rootView.EventType;

        public override void HandleMatching(
            EventBean[] triggerEvents,
            EventBean[] matchingEvents)
        {
            var instrumentationCommon = agentInstanceContext.InstrumentationProvider;
            instrumentationCommon.QInfraOnAction(OnTriggerType.ON_MERGE, triggerEvents, matchingEvents);

            var newData = new OneEventCollection();
            OneEventCollection oldData = null;
            var eventsPerStream =
                new EventBean[3]; // first:named window, second: trigger, third:before-update (optional)

            if (matchingEvents == null || matchingEvents.Length == 0) {
                IList<InfraOnMergeMatch> unmatched = parent.OnMergeHelper.Unmatched;

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

                        action.ApplyNamedWindow(null, eventsPerStream, newData, oldData, agentInstanceContext);
                        instrumentationCommon.AInfraMergeWhenThenItem(false, true);
                        break; // apply no other actions
                    }

                    instrumentationCommon.AInfraMergeWhenThens(false);
                }
            }
            else {
                // handle update/
                oldData = new OneEventCollection();

                IList<InfraOnMergeMatch> matched = parent.OnMergeHelper.Matched;

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

                            action.ApplyNamedWindow(
                                matchingEvent,
                                eventsPerStream,
                                newData,
                                oldData,
                                agentInstanceContext);
                            instrumentationCommon.AInfraMergeWhenThenItem(true, true);
                            break; // apply no other actions
                        }
                    }

                    instrumentationCommon.AInfraMergeWhenThens(true);
                }
            }

            ApplyDelta(newData, oldData, parent, rootView, agentInstanceContext, this);

            instrumentationCommon.AInfraOnAction();
        }

        public static void ApplyDelta(
            OneEventCollection newData,
            OneEventCollection oldData,
            InfraOnMergeViewFactory parent,
            NamedWindowRootViewInstance rootView,
            AgentInstanceContext agentInstanceContext,
            ViewSupport viewable)
        {
            if (!newData.IsEmpty() || oldData != null && !oldData.IsEmpty()) {
                var metricHandle = rootView.AgentInstanceContext.StatementContext.EpStatementHandle.MetricsHandle;
                if (metricHandle.IsEnabled && !newData.IsEmpty()) {
                    agentInstanceContext.MetricReportingService.AccountTime(
                        metricHandle,
                        default(PerformanceMetrics),
                        newData.ToArray().Length);
                }

                var statementResultService = agentInstanceContext.StatementResultService;

                // Events to delete are indicated via old data
                // The on-merge listeners receive the events deleted, but only if there is interest
                if (statementResultService.IsMakeNatural) {
                    var eventsPerStreamNaturalNew = newData.IsEmpty() ? null : newData.ToArray();
                    var eventsPerStreamNaturalOld = oldData == null || oldData.IsEmpty() ? null : oldData.ToArray();
                    rootView.Update(
                        EventBeanUtility.Denaturalize(eventsPerStreamNaturalNew),
                        EventBeanUtility.Denaturalize(eventsPerStreamNaturalOld));
                    viewable.Child.Update(eventsPerStreamNaturalNew, eventsPerStreamNaturalOld);
                }
                else {
                    var eventsPerStreamNew = newData.IsEmpty() ? null : newData.ToArray();
                    var eventsPerStreamOld = oldData == null || oldData.IsEmpty() ? null : oldData.ToArray();
                    rootView.Update(eventsPerStreamNew, eventsPerStreamOld);
                    if (statementResultService.IsMakeSynthetic) {
                        viewable.Child.Update(eventsPerStreamNew, eventsPerStreamOld);
                    }
                }
            }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return CollectionUtil.NULL_EVENT_ITERATOR;
        }
    }
} // end of namespace