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
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.expression.subquery
{
    /// <summary>Represents a subselect in an expression tree.</summary>
    public class SubselectEvalStrategyRowUnfilteredUnselected : SubselectEvalStrategyRow
    {
        public static readonly SubselectEvalStrategyRowUnfilteredUnselected INSTANCE = new SubselectEvalStrategyRowUnfilteredUnselected();
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        // No filter and no select-clause: return underlying event
        public virtual Object Evaluate(
            EventBean[] eventsPerStream,
            bool newData,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            ExprSubselectRowNode parent)
        {
            if (matchingEvents.Count > 1) {
                Log.Warn(parent.GetMultirowMessage());
                return null;
            }
            return EventBeanUtility.GetNonemptyFirstEventUnderlying(matchingEvents);
        }
    
        // No filter and no select-clause: return matching events
        public ICollection<EventBean> EvaluateGetCollEvents(EventBean[] eventsPerStream, bool newData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext context, ExprSubselectRowNode parent)
        {
            return matchingEvents;
        }
    
        // No filter and no select-clause: no value can be determined
        public ICollection<object> EvaluateGetCollScalar(EventBean[] eventsPerStream, bool isNewData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext context, ExprSubselectRowNode parent)
        {
            return null;
        }
    
        // No filter and no select-clause: no value can be determined
        public Object[] TypableEvaluate(EventBean[] eventsPerStream, bool newData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext exprEvaluatorContext, ExprSubselectRowNode parent)
        {
            return null;
        }

        public Object[][] TypableEvaluateMultirow(EventBean[] eventsPerStream, bool newData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext exprEvaluatorContext, ExprSubselectRowNode parent)
        {
            return null;
        }
    
        public EventBean EvaluateGetEventBean(EventBean[] eventsPerStream, bool newData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext exprEvaluatorContext, ExprSubselectRowNode parent) {
            return null;    // this actually only applies to when there is a select-clause
        }
    }
} // end of namespace
