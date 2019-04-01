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
    public class SubselectEvalStrategyRowFilteredSelected : SubselectEvalStrategyRow
    {
        // Filter and Select
        public Object Evaluate(
            EventBean[] eventsPerStream,
            bool newData,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            ExprSubselectRowNode parent)
        {
            var eventsZeroBased = EventBeanUtility.AllocatePerStreamShift(eventsPerStream);
            var subSelectResult = ExprSubselectRowNodeUtility.EvaluateFilterExpectSingleMatch(
                eventsZeroBased, newData, matchingEvents, exprEvaluatorContext, parent);
            if (subSelectResult == null)
            {
                return null;
            }

            eventsZeroBased[0] = subSelectResult;
            Object result;
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
            var result = new List<Object>();
            var selectClauseEvaluator = parent.SelectClauseEvaluator;
            var filterExpr = parent.FilterExpr;
            var events = EventBeanUtility.AllocatePerStreamShift(eventsPerStream);
            var evaluateParamsA = new EvaluateParams(events, true, context);
            var evaluateParamsB = new EvaluateParams(events, isNewData, context);
            foreach (var subselectEvent in matchingEvents)
            {
                events[0] = subselectEvent;
                var pass = filterExpr.Evaluate(evaluateParamsA);
                if ((pass != null) && true.Equals(pass))
                {
                    result.Add(selectClauseEvaluator[0].Evaluate(evaluateParamsB));
                }
            }
            return result;
        }

        // Filter and Select
        public Object[] TypableEvaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            ExprSubselectRowNode parent)
        {
            var events = EventBeanUtility.AllocatePerStreamShift(eventsPerStream);
            var subSelectResult = ExprSubselectRowNodeUtility.EvaluateFilterExpectSingleMatch(
                events, isNewData, matchingEvents, exprEvaluatorContext, parent);
            if (subSelectResult == null)
            {
                return null;
            }

            events[0] = subSelectResult;
            var selectClauseEvaluator = parent.SelectClauseEvaluator;
            var results = new Object[selectClauseEvaluator.Length];
            var evaluateParams = new EvaluateParams(events, isNewData, exprEvaluatorContext);
            for (var i = 0; i < results.Length; i++)
            {
                results[i] = selectClauseEvaluator[i].Evaluate(evaluateParams);
            }
            return results;
        }

        public Object[][] TypableEvaluateMultirow(
            EventBean[] eventsPerStream,
            bool newData,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            ExprSubselectRowNode parent)
        {
            var rows = new Object[matchingEvents.Count][];
            var index = -1;
            var events = EventBeanUtility.AllocatePerStreamShift(eventsPerStream);
            var evaluateParams = new EvaluateParams(events, newData, exprEvaluatorContext);

            foreach (var matchingEvent in matchingEvents)
            {
                events[0] = matchingEvent;

                var pass = parent.FilterExpr.Evaluate(evaluateParams);
                if ((pass != null) && true.Equals(pass))
                {
                    index++;
                    var selectClauseEvaluator = parent.SelectClauseEvaluator;
                    var results = new Object[selectClauseEvaluator.Length];
                    for (var i = 0; i < results.Length; i++)
                    {
                        results[i] = selectClauseEvaluator[i].Evaluate(evaluateParams);
                    }
                    rows[index] = results;
                }
            }
            if (index == rows.Length - 1)
            {
                return rows;
            }
            if (index == -1)
            {
                return new Object[0][];
            }
            var access = index + 1;
            var rowArray = new Object[access][];
            Array.Copy(rows, 0, rowArray, 0, rowArray.Length);
            return rowArray;
        }

        // Filter and Select
        public EventBean EvaluateGetEventBean(
            EventBean[] eventsPerStream,
            bool isNewData,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext context,
            ExprSubselectRowNode parent)
        {
            var events = EventBeanUtility.AllocatePerStreamShift(eventsPerStream);
            var subSelectResult = ExprSubselectRowNodeUtility.EvaluateFilterExpectSingleMatch(
                events, isNewData, matchingEvents, context, parent);
            if (subSelectResult == null)
            {
                return null;
            }
            var row = parent.EvaluateRow(events, true, context);
            return parent.SubselectMultirowType.EventAdapterService.AdapterForTypedMap(
                row, parent.SubselectMultirowType.EventType);
        }
    }
} // end of namespace
