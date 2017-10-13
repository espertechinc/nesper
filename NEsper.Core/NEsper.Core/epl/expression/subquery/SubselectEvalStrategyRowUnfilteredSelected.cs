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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.expression.subquery
{
    public class SubselectEvalStrategyRowUnfilteredSelected : SubselectEvalStrategyRow
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        // No filter and with select clause
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
    
            var events = EventBeanUtility.AllocatePerStreamShift(eventsPerStream);
            events[0] = EventBeanUtility.GetNonemptyFirstEvent(matchingEvents);
    
            Object result;
            if (parent.SelectClauseEvaluator.Length == 1) {
                result = parent.SelectClauseEvaluator[0].Evaluate(new EvaluateParams(events, true, exprEvaluatorContext));
            } else {
                // we are returning a Map here, not object-array, preferring the self-describing structure
                result = parent.EvaluateRow(events, true, exprEvaluatorContext);
            }
            return result;
        }
    
        // No filter and with select clause
        public virtual ICollection<EventBean> EvaluateGetCollEvents(EventBean[] eventsPerStream, bool newData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext context, ExprSubselectRowNode parent)
        {
            if (matchingEvents.Count == 0) {
                return Collections.GetEmptyList<EventBean>();
            }
    
            // when selecting a single property in the select clause that provides a fragment
            if (parent.SubselectMultirowType == null) {
                var eventsX = new ArrayDeque<EventBean>(matchingEvents.Count);
                var eval = (ExprIdentNodeEvaluator) parent.SelectClauseEvaluator[0];
                var getter = eval.Getter;
                foreach (var subselectEvent in matchingEvents) {
                    var fragment = getter.GetFragment(subselectEvent);
                    if (fragment == null) {
                        continue;
                    }
                    eventsX.Add((EventBean) fragment);
                }
                return eventsX;
            }
    
            // when selecting a combined output row that contains multiple fields
            var events = new ArrayDeque<EventBean>(matchingEvents.Count);
            var eventsPerStreamEval = EventBeanUtility.AllocatePerStreamShift(eventsPerStream);
            foreach (var subselectEvent in matchingEvents) {
                eventsPerStreamEval[0] = subselectEvent;
                var row = parent.EvaluateRow(eventsPerStreamEval, true, context);
                var @event = parent.SubselectMultirowType.EventAdapterService.AdapterForTypedMap(row, parent.SubselectMultirowType.EventType);
                events.Add(@event);
            }
            return events;
        }
    
        // No filter and with select clause
        public virtual ICollection<object> EvaluateGetCollScalar(EventBean[] eventsPerStream, bool isNewData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext context, ExprSubselectRowNode parent)
        {
            var result = new List<Object>();
            var events = EventBeanUtility.AllocatePerStreamShift(eventsPerStream);
            var evaluateParams = new EvaluateParams(events, isNewData, context);
            foreach (var subselectEvent in matchingEvents)
            {
                events[0] = subselectEvent;
                result.Add(parent.SelectClauseEvaluator[0].Evaluate(evaluateParams));
            }
            return result;
        }
    
        // No filter and with select clause
        public Object[] TypableEvaluate(EventBean[] eventsPerStream, bool isNewData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext exprEvaluatorContext, ExprSubselectRowNode parent)
        {
            // take the first match only
            var events = EventBeanUtility.AllocatePerStreamShift(eventsPerStream);
            var evaluateParams = new EvaluateParams(events, isNewData, exprEvaluatorContext);
            events[0] = EventBeanUtility.GetNonemptyFirstEvent(matchingEvents);
            var results = new Object[parent.SelectClauseEvaluator.Length];
            for (var i = 0; i < results.Length; i++) {
                results[i] = parent.SelectClauseEvaluator[i].Evaluate(evaluateParams);
            }
            return results;
        }
    
        // No filter and with select clause
        public Object[][] TypableEvaluateMultirow(EventBean[] eventsPerStream, bool isNewData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext exprEvaluatorContext, ExprSubselectRowNode parent)
        {
            var rows = new Object[matchingEvents.Count][];
            var index = -1;
            var events = EventBeanUtility.AllocatePerStreamShift(eventsPerStream);
            var evaluateParams = new EvaluateParams(events, isNewData, exprEvaluatorContext);
            foreach (var matchingEvent in matchingEvents)
            {
                index++;
                events[0] = matchingEvent;
                var results = new Object[parent.SelectClauseEvaluator.Length];
                for (var i = 0; i < results.Length; i++) {
                    results[i] = parent.SelectClauseEvaluator[i].Evaluate(evaluateParams);
                }
                rows[index] = results;
            }
            return rows;
        }
    
        // No filter and with select clause
        public virtual EventBean EvaluateGetEventBean(EventBean[] eventsPerStream, bool newData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext context, ExprSubselectRowNode parent)
        {
            var events = EventBeanUtility.AllocatePerStreamShift(eventsPerStream);
            events[0] = EventBeanUtility.GetNonemptyFirstEvent(matchingEvents);
            var row = parent.EvaluateRow(events, true, context);
            return parent.SubselectMultirowType.EventAdapterService.AdapterForTypedMap(row, parent.SubselectMultirowType.EventType);
        }
    }
} // end of namespace
