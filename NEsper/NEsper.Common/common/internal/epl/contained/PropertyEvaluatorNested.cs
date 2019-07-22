///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.contained
{
    /// <summary>
    ///     A property evaluator that considers nested properties and that considers where-clauses
    ///     but does not consider select-clauses.
    /// </summary>
    public class PropertyEvaluatorNested : PropertyEvaluator
    {
        private ContainedEventEval[] containedEventEvals;
        private EventBean[] eventsPerStream;
        private string[] expressionTexts;
        private bool[] fragmentEventTypeIsIndexed;
        private ExprEvaluator[] whereClauses;

        public EventBean[] GetProperty(
            EventBean theEvent,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var resultEvents = new ArrayDeque<EventBean>();
            eventsPerStream[0] = theEvent;
            PopulateEvents(theEvent, 0, resultEvents, exprEvaluatorContext);
            if (resultEvents.IsEmpty()) {
                return null;
            }

            return resultEvents.ToArray();
        }

        public EventType FragmentEventType { get; private set; }

        public bool CompareTo(PropertyEvaluator otherEval)
        {
            return false;
        }

        public void SetContainedEventEvals(ContainedEventEval[] containedEventEvals)
        {
            this.containedEventEvals = containedEventEvals;
        }

        public void SetFragmentEventTypeIsIndexed(bool[] fragmentEventTypeIsIndexed)
        {
            this.fragmentEventTypeIsIndexed = fragmentEventTypeIsIndexed;
            eventsPerStream = new EventBean[fragmentEventTypeIsIndexed.Length + 1];
        }

        public void SetWhereClauses(ExprEvaluator[] whereClauses)
        {
            this.whereClauses = whereClauses;
        }

        public void SetExpressionTexts(string[] expressionTexts)
        {
            this.expressionTexts = expressionTexts;
        }

        public void SetResultEventType(EventType resultEventType)
        {
            FragmentEventType = resultEventType;
        }

        private void PopulateEvents(
            EventBean branch,
            int level,
            ICollection<EventBean> events,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            try {
                var result = containedEventEvals[level].GetFragment(branch, eventsPerStream, exprEvaluatorContext);
                var lastLevel = fragmentEventTypeIsIndexed.Length - 1;

                if (fragmentEventTypeIsIndexed[level]) {
                    var fragments = (EventBean[]) result;
                    if (level == lastLevel) {
                        if (whereClauses[level] != null) {
                            foreach (var theEvent in fragments) {
                                eventsPerStream[level + 1] = theEvent;
                                if (ExprNodeUtilityEvaluate.ApplyFilterExpression(
                                    whereClauses[level],
                                    eventsPerStream,
                                    exprEvaluatorContext)) {
                                    events.Add(theEvent);
                                }
                            }
                        }
                        else {
                            events.AddAll(fragments);
                        }
                    }
                    else {
                        if (whereClauses[level] != null) {
                            foreach (var next in fragments) {
                                eventsPerStream[level + 1] = next;
                                if (ExprNodeUtilityEvaluate.ApplyFilterExpression(
                                    whereClauses[level],
                                    eventsPerStream,
                                    exprEvaluatorContext)) {
                                    PopulateEvents(next, level + 1, events, exprEvaluatorContext);
                                }
                            }
                        }
                        else {
                            foreach (var next in fragments) {
                                eventsPerStream[level + 1] = next;
                                PopulateEvents(next, level + 1, events, exprEvaluatorContext);
                            }
                        }
                    }
                }
                else {
                    var fragment = (EventBean) result;
                    if (level == lastLevel) {
                        if (whereClauses[level] != null) {
                            eventsPerStream[level + 1] = fragment;
                            if (ExprNodeUtilityEvaluate.ApplyFilterExpression(
                                whereClauses[level],
                                eventsPerStream,
                                exprEvaluatorContext)) {
                                events.Add(fragment);
                            }
                        }
                        else {
                            events.Add(fragment);
                        }
                    }
                    else {
                        if (whereClauses[level] != null) {
                            eventsPerStream[level + 1] = fragment;
                            if (ExprNodeUtilityEvaluate.ApplyFilterExpression(
                                whereClauses[level],
                                eventsPerStream,
                                exprEvaluatorContext)) {
                                PopulateEvents(fragment, level + 1, events, exprEvaluatorContext);
                            }
                        }
                        else {
                            eventsPerStream[level + 1] = fragment;
                            PopulateEvents(fragment, level + 1, events, exprEvaluatorContext);
                        }
                    }
                }
            }
            catch (EPException) {
                throw;
            }
            catch (Exception ex) {
                var message = "Unexpected error evaluating property expression for event of type '" +
                              branch.EventType.Name +
                              "' and property '" +
                              expressionTexts[level + 1] +
                              "': " +
                              ex.Message;
                throw new EPException(message, ex);
            }
        }
    }
} // end of namespace