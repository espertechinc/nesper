///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.table.onaction;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.table.merge
{
    public class TableOnMergeMatch
    {
        private ExprEvaluator optionalCond;
        private IList<TableOnMergeAction> actions;
    
        public TableOnMergeMatch(ExprNode optionalCond, IList<TableOnMergeAction> actions) {
            this.optionalCond = optionalCond != null ? optionalCond.ExprEvaluator : null;
            this.actions = actions;
        }
    
        public bool IsApplies(EventBean[] eventsPerStream, ExprEvaluatorContext context) {
            if (optionalCond == null) {
                return true;
            }
    
            object result = optionalCond.Evaluate(new EvaluateParams(eventsPerStream, true, context));
            return result != null && true.Equals(result);
        }
    
        public void Apply(EventBean matchingEvent,
                          EventBean[] eventsPerStream,
                          TableStateInstance stateInstance,
                          TableOnMergeViewChangeHandler changeHandlerAdded,
                          TableOnMergeViewChangeHandler changeHandlerRemoved,
                          ExprEvaluatorContext context) {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QInfraMergeWhenThenActions(actions.Count);}
    
            int count = -1;
            foreach (TableOnMergeAction action in actions) {
                count++;
    
                if (InstrumentationHelper.ENABLED) {
                    InstrumentationHelper.Get().QInfraMergeWhenThenActionItem(count, action.Name);
                    bool applies = action.IsApplies(eventsPerStream, context);
                    if (applies) {
                        action.Apply(matchingEvent, eventsPerStream, stateInstance, changeHandlerAdded, changeHandlerRemoved, context);
                    }
                    InstrumentationHelper.Get().AInfraMergeWhenThenActionItem(applies);
                    continue;
                }
    
                if (action.IsApplies(eventsPerStream, context)) {
                    action.Apply(matchingEvent, eventsPerStream, stateInstance, changeHandlerAdded, changeHandlerRemoved, context);
                }
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AInfraMergeWhenThenActions();}
        }
    }
}
