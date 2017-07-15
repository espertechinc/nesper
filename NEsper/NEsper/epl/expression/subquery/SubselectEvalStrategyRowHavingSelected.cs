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
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.expression.subquery
{
    public class SubselectEvalStrategyRowHavingSelected : SubselectEvalStrategyRow
    {
        public Object Evaluate(
            EventBean[] eventsPerStream,
            bool newData,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            ExprSubselectRowNode parent)
        {
            EventBean[] eventsZeroBased = EventBeanUtility.AllocatePerStreamShift(eventsPerStream);
            var pass = parent.HavingExpr.Evaluate(new EvaluateParams(eventsZeroBased, newData, exprEvaluatorContext));
            if ((pass == null) || (false.Equals(pass))) {
                return null;
            }
    
            Object result;
            if (parent.SelectClauseEvaluator.Length == 1) {
                result = parent.SelectClauseEvaluator[0].Evaluate(new EvaluateParams(eventsZeroBased, true, exprEvaluatorContext));
            } else {
                // we are returning a Map here, not object-array, preferring the self-describing structure
                result = parent.EvaluateRow(eventsZeroBased, true, exprEvaluatorContext);
            }
    
            return result;
        }
    
        // Filter and Select
        public ICollection<EventBean> EvaluateGetCollEvents(EventBean[] eventsPerStream, bool newData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext context, ExprSubselectRowNode parent)
        {
            return null;
        }
    
        // Filter and Select
        public ICollection<object> EvaluateGetCollScalar(EventBean[] eventsPerStream, bool isNewData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext context, ExprSubselectRowNode parent)
        {
            return null;
        }
    
        // Filter and Select
        public Object[] TypableEvaluate(EventBean[] eventsPerStream, bool isNewData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext exprEvaluatorContext, ExprSubselectRowNode parent)
        {
            return null;
        }

        public Object[][] TypableEvaluateMultirow(EventBean[] eventsPerStream, bool newData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext exprEvaluatorContext, ExprSubselectRowNode parent)
        {
            return null;
        }
    
        // Filter and Select
        public EventBean EvaluateGetEventBean(EventBean[] eventsPerStream, bool isNewData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext context, ExprSubselectRowNode parent)
        {
            return null;
        }
    }
} // end of namespace
