///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.expression.subquery
{
    public class ExprSubselectRowEvalStrategyFilteredUnselected : ExprSubselectRowEvalStrategy
    {
        // Filter and no-select
        public virtual object Evaluate(
            EventBean[] eventsPerStream,
            bool newData,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            ExprSubselectRowNode parent)
        {
            EventBean[] eventsZeroBased = EventBeanUtility.AllocatePerStreamShift(eventsPerStream);
            EventBean subSelectResult = ExprSubselectRowNodeUtility.EvaluateFilterExpectSingleMatch(eventsZeroBased, newData, matchingEvents, exprEvaluatorContext, parent);
            if (subSelectResult == null) {
                return null;
            }
            return subSelectResult.Underlying;
        }
    
        // Filter and no-select
        public virtual ICollection<EventBean> EvaluateGetCollEvents(
            EventBean[] eventsPerStream,
            bool newData,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext context,
            ExprSubselectRowNode parent)
        {
            var events = EventBeanUtility.AllocatePerStreamShift(eventsPerStream);
            var evaluateParams = new EvaluateParams(events, true, context);
    
            ArrayDeque<EventBean> filtered = null;
            foreach (EventBean subselectEvent in matchingEvents) {
                events[0] = subselectEvent;
                var pass = parent.FilterExpr.Evaluate(evaluateParams);
                if ((pass != null) && (true.Equals(pass))) {
                    if (filtered == null) {
                        filtered = new ArrayDeque<EventBean>();
                    }
                    filtered.Add(subselectEvent);
                }
            }
            return filtered;
        }
    
        // Filter and no-select
        public virtual ICollection<object> EvaluateGetCollScalar(
            EventBean[] eventsPerStream,
            bool isNewData,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext context,
            ExprSubselectRowNode parent)
        {
            return null;
        }
    
        // Filter and no-select
        public virtual object[] TypableEvaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            ExprSubselectRowNode parent)
        {
            return null;
        }
    
        // Filer and no-select
        public virtual object[][] TypableEvaluateMultirow(
            EventBean[] eventsPerStream,
            bool newData,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            ExprSubselectRowNode parent)
        {
            return null;
        }
    
        // Filter and no-select
        public virtual EventBean EvaluateGetEventBean(
            EventBean[] eventsPerStream,
            bool newData,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext context,
            ExprSubselectRowNode parent)

    {
        return null;
    }
    }
}
