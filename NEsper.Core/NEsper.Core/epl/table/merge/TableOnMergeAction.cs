///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.table.onaction;

namespace com.espertech.esper.epl.table.merge
{
    public abstract class TableOnMergeAction {
    
        private readonly ExprEvaluator optionalFilter;
    
        protected TableOnMergeAction(ExprEvaluator optionalFilter) {
            this.optionalFilter = optionalFilter;
        }
    
        public bool IsApplies(EventBean[] eventsPerStream, ExprEvaluatorContext context) {
            if (optionalFilter == null) {
                return true;
            }
            object result = optionalFilter.Evaluate(new EvaluateParams(eventsPerStream, true, context));
            return result != null && (Boolean) result;
        }
    
        public abstract void Apply(EventBean matchingEvent,
                                   EventBean[] eventsPerStream,
                                   TableStateInstance tableStateInstance,
                                   TableOnMergeViewChangeHandler changeHandlerAdded,
                                   TableOnMergeViewChangeHandler changeHandlerRemoved,
                                   ExprEvaluatorContext exprEvaluatorContext);

        public abstract string Name { get; }
    }
}
