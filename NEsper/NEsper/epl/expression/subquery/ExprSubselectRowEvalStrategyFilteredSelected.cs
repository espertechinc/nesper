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
    public class ExprSubselectRowEvalStrategyFilteredSelected : ExprSubselectRowEvalStrategy
    {
        // Filter and Select
        public object Evaluate(
            EventBean[] eventsPerStream,
            bool newData,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            ExprSubselectRowNode parent)
        {
            var eventsZeroBased = EventBeanUtility.AllocatePerStreamShift(eventsPerStream);
            var subSelectResult = ExprSubselectRowNodeUtility.EvaluateFilterExpectSingleMatch(eventsZeroBased, newData, matchingEvents, exprEvaluatorContext, parent);
            if (subSelectResult == null) {
                return null;
            }
    
            eventsZeroBased[0] = subSelectResult;
            object result;
            if (parent.SelectClauseEvaluator.Length == 1)
            {
                result = parent.SelectClauseEvaluator[0].Evaluate(new EvaluateParams(eventsZeroBased, true, exprEvaluatorContext));
            }
            else
            {
                // we are returning a Map here, not object-array, preferring the self-describing structure
                result = parent.EvaluateRow(eventsZeroBased, true, exprEvaluatorContext);
            }
    
            return result;
        }
    
        // Filter and Select
        public ICollection<EventBean> EvaluateGetCollEvents(
            EventBean[] eventsPerStream,
            bool newData,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext context,
            ExprSubselectRowNode parent)
        {
            return null;
        }
    
        // Filter and Select
        public ICollection<object> EvaluateGetCollScalar(
            EventBean[] eventsPerStream,
            bool isNewData,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext context,
            ExprSubselectRowNode parent)
        {
            IList<object> result = new List<object>();
            var events = EventBeanUtility.AllocatePerStreamShift(eventsPerStream);
            var evaluateParamsA = new EvaluateParams(events, true, context);
            var evaluateParamsB = new EvaluateParams(events, isNewData, context);
            foreach (var subselectEvent in matchingEvents)
            {
                events[0] = subselectEvent;
                var pass = parent.FilterExpr.Evaluate(evaluateParamsA);
                if ((pass != null) && (true.Equals(pass)))
                {
                    result.Add(parent.SelectClauseEvaluator[0].Evaluate(evaluateParamsB));
                }
            }
            return result;
        }
    
        // Filter and Select
        public object[] TypableEvaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            ExprSubselectRowNode parent)
        {
            var events = EventBeanUtility.AllocatePerStreamShift(eventsPerStream);
            var subSelectResult = ExprSubselectRowNodeUtility.EvaluateFilterExpectSingleMatch(events, isNewData, matchingEvents, exprEvaluatorContext, parent);
            if (subSelectResult == null) {
                return null;
            }
    
            events[0] = subSelectResult;
            var results = new object[parent.SelectClauseEvaluator.Length];
            var evaluateParams = new EvaluateParams(events, isNewData, exprEvaluatorContext);
            for (var i = 0; i < results.Length; i++)
            {
                results[i] = parent.SelectClauseEvaluator[i].Evaluate(evaluateParams);
            }
            return results;
        }

        public object[][] TypableEvaluateMultirow(
            EventBean[] eventsPerStream,
            bool newData,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            ExprSubselectRowNode parent)
        {
            var rows = new object[matchingEvents.Count][];
            var index = -1;
            var events = EventBeanUtility.AllocatePerStreamShift(eventsPerStream);
            var evaluateParams = new EvaluateParams(events, newData, exprEvaluatorContext);

            foreach (var matchingEvent in matchingEvents)
            {
                events[0] = matchingEvent;
    
                var pass = parent.FilterExpr.Evaluate(evaluateParams);
                if ((pass != null) && (true.Equals(pass))) {
                    index++;
                    var results = new object[parent.SelectClauseEvaluator.Length];
                    for (var i = 0; i < results.Length; i++)
                    {
                        results[i] = parent.SelectClauseEvaluator[i].Evaluate(evaluateParams);
                    }
                    rows[index] = results;
                }
            }
            if (index == rows.Length - 1) {
                return rows;
            }
            if (index == -1) {
                return new object[0][];
            }
            var rowArray = new object[index + 1][];
            Array.Copy(rows, 0, rowArray, 0, rowArray.Length);
            return rowArray;
        }
    
        // Filter and Select
        public EventBean EvaluateGetEventBean(EventBean[] eventsPerStream, bool isNewData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext context, ExprSubselectRowNode parent)
        {
            var events = EventBeanUtility.AllocatePerStreamShift(eventsPerStream);
            var subSelectResult = ExprSubselectRowNodeUtility.EvaluateFilterExpectSingleMatch(events, isNewData, matchingEvents, context, parent);
            if (subSelectResult == null) {
                return null;
            }
            var row = parent.EvaluateRow(events, true, context);
            return parent.subselectMultirowType.EventAdapterService.AdapterForTypedMap(row, parent.subselectMultirowType.EventType);
        }
    }
}
