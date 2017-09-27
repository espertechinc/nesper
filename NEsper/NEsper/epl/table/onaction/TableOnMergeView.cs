///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.merge;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.table.onaction
{
    public class TableOnMergeView : TableOnViewBase
    {
        private readonly TableOnMergeViewFactory parent;

        public TableOnMergeView(
            SubordWMatchExprLookupStrategy lookupStrategy,
            TableStateInstance rootView,
            ExprEvaluatorContext exprEvaluatorContext,
            TableMetadata metadata,
            TableOnMergeViewFactory parent)
            : base(lookupStrategy, rootView, exprEvaluatorContext, metadata, parent.OnMergeHelper.IsRequiresWriteLock)
        {
            this.parent = parent;
        }
    
        public override void HandleMatching(EventBean[] triggerEvents, EventBean[] matchingEvents)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QInfraOnAction(OnTriggerType.ON_MERGE, triggerEvents, matchingEvents);}
    
            EventBean[] eventsPerStream = new EventBean[3]; // first:table, second: trigger, third:before-update (optional)
    
            bool postResultsToListeners = parent.StatementResultService.IsMakeNatural || parent.StatementResultService.IsMakeSynthetic;
            TableOnMergeViewChangeHandler changeHandlerRemoved = null;
            TableOnMergeViewChangeHandler changeHandlerAdded = null;
            if (postResultsToListeners) {
                changeHandlerRemoved = new TableOnMergeViewChangeHandler(parent.TableMetadata);
                changeHandlerAdded = new TableOnMergeViewChangeHandler(parent.TableMetadata);
            }
    
            if ((matchingEvents == null) || (matchingEvents.Length == 0)){
    
                IList<TableOnMergeMatch> unmatched = parent.OnMergeHelper.Unmatched;
    
                foreach (EventBean triggerEvent in triggerEvents) {
                    eventsPerStream[1] = triggerEvent;
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QInfraMergeWhenThens(false, triggerEvent, unmatched.Count);}
    
                    int count = -1;
                    foreach (TableOnMergeMatch action in unmatched) {
                        count++;
    
                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QInfraMergeWhenThenItem(false, count);}
                        if (!action.IsApplies(eventsPerStream, base.ExprEvaluatorContext)) {
                            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AInfraMergeWhenThenItem(false, false);}
                            continue;
                        }
                        action.Apply(null, eventsPerStream, TableStateInstance, changeHandlerAdded, changeHandlerRemoved, base.ExprEvaluatorContext);
                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AInfraMergeWhenThenItem(false, true);}
                        break;  // apply no other actions
                    }
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AInfraMergeWhenThens(false);}
                }
            }
            else
            {
                IList<TableOnMergeMatch> matched = parent.OnMergeHelper.Matched;
    
                foreach (EventBean triggerEvent in triggerEvents) {
                    eventsPerStream[1] = triggerEvent;
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QInfraMergeWhenThens(true, triggerEvent, matched.Count);}
    
                    foreach (EventBean matchingEvent in matchingEvents) {
                        eventsPerStream[0] = matchingEvent;
    
                        int count = -1;
                        foreach (TableOnMergeMatch action in matched) {
                            count++;
    
                            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QInfraMergeWhenThenItem(true, count);}
                            if (!action.IsApplies(eventsPerStream, base.ExprEvaluatorContext)) {
                                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AInfraMergeWhenThenItem(true, false);}
                                continue;
                            }
                            action.Apply(matchingEvent, eventsPerStream, TableStateInstance, changeHandlerAdded, changeHandlerRemoved, base.ExprEvaluatorContext);
                            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AInfraMergeWhenThenItem(true, true);}
                            break;  // apply no other actions
                        }
                    }
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AInfraMergeWhenThens(true);}
                }
            }
    
            // The on-delete listeners receive the events deleted, but only if there is interest
            if (postResultsToListeners) {
                EventBean[] postedNew = changeHandlerAdded.Events;
                EventBean[] postedOld = changeHandlerRemoved.Events;
                if (postedNew != null || postedOld != null) {
                    UpdateChildren(postedNew, postedOld);
                }
            }
    
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AInfraOnAction();}
        }
    }
}
