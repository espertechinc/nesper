///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.contained
{
    /// <summary>
    ///     A property evaluator that returns a full row of events for each stream, i.e. flattened inner-join results for
    ///     property-upon-property.
    /// </summary>
    public class PropertyEvaluatorAccumulative
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private ContainedEventEval[] containedEventEvals;
        private bool[] fragmentEventTypeIsIndexed;
        private string[] propertyNames;
        private ExprEvaluator[] whereClauses;

        public string[] PropertyNames {
            set => propertyNames = value;
        }

        public ContainedEventEval[] ContainedEventEvals {
            set => containedEventEvals = value;
        }

        public bool[] FragmentEventTypeIsIndexed {
            set => fragmentEventTypeIsIndexed = value;
        }

        public ExprEvaluator[] WhereClauses {
            set => whereClauses = value;
        }

        /// <summary>
        ///     Returns the accumulative events for the input event.
        /// </summary>
        /// <param name="theEvent">is the input event</param>
        /// <param name="exprEvaluatorContext">expression evaluation context</param>
        /// <returns>events per stream for each row</returns>
        public ArrayDeque<EventBean[]> GetAccumulative(
            EventBean theEvent,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var resultEvents = new ArrayDeque<EventBean[]>();
            var eventsPerStream = new EventBean[fragmentEventTypeIsIndexed.Length + 1];
            eventsPerStream[0] = theEvent;
            PopulateEvents(eventsPerStream, theEvent, 0, resultEvents, exprEvaluatorContext);
            if (resultEvents.IsEmpty()) {
                return null;
            }

            return resultEvents;
        }

        private void PopulateEvents(
            EventBean[] eventsPerStream,
            EventBean branch,
            int level,
            ICollection<EventBean[]> events,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            try {
                var result = containedEventEvals[level].GetFragment(branch, eventsPerStream, exprEvaluatorContext);
                var lastLevel = fragmentEventTypeIsIndexed.Length - 1;
                var levels = fragmentEventTypeIsIndexed.Length + 1;

                if (fragmentEventTypeIsIndexed[level]) {
                    var fragments = (EventBean[])result;
                    if (level == lastLevel) {
                        if (whereClauses[level] != null) {
                            foreach (var theEvent in fragments) {
                                eventsPerStream[level + 1] = theEvent;
                                if (ExprNodeUtilityEvaluate.ApplyFilterExpression(
                                        whereClauses[level],
                                        eventsPerStream,
                                        exprEvaluatorContext)) {
                                    var eventsPerRow = new EventBean[levels];
                                    Array.Copy(eventsPerStream, 0, eventsPerRow, 0, levels);
                                    events.Add(eventsPerRow);
                                }
                            }
                        }
                        else {
                            foreach (var theEvent in fragments) {
                                eventsPerStream[level + 1] = theEvent;
                                var eventsPerRow = new EventBean[levels];
                                Array.Copy(eventsPerStream, 0, eventsPerRow, 0, levels);
                                events.Add(eventsPerRow);
                            }
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
                                    PopulateEvents(eventsPerStream, next, level + 1, events, exprEvaluatorContext);
                                }
                            }
                        }
                        else {
                            foreach (var next in fragments) {
                                eventsPerStream[level + 1] = next;
                                PopulateEvents(eventsPerStream, next, level + 1, events, exprEvaluatorContext);
                            }
                        }
                    }
                }
                else {
                    var fragment = (EventBean)result;
                    if (level == lastLevel) {
                        if (whereClauses[level] != null) {
                            eventsPerStream[level + 1] = fragment;
                            if (ExprNodeUtilityEvaluate.ApplyFilterExpression(
                                    whereClauses[level],
                                    eventsPerStream,
                                    exprEvaluatorContext)) {
                                var eventsPerRow = new EventBean[levels];
                                Array.Copy(eventsPerStream, 0, eventsPerRow, 0, levels);
                                events.Add(eventsPerRow);
                            }
                        }
                        else {
                            eventsPerStream[level + 1] = fragment;
                            var eventsPerRow = new EventBean[levels];
                            Array.Copy(eventsPerStream, 0, eventsPerRow, 0, levels);
                            events.Add(eventsPerRow);
                        }
                    }
                    else {
                        if (whereClauses[level] != null) {
                            eventsPerStream[level + 1] = fragment;
                            if (ExprNodeUtilityEvaluate.ApplyFilterExpression(
                                    whereClauses[level],
                                    eventsPerStream,
                                    exprEvaluatorContext)) {
                                PopulateEvents(eventsPerStream, fragment, level + 1, events, exprEvaluatorContext);
                            }
                        }
                        else {
                            eventsPerStream[level + 1] = fragment;
                            PopulateEvents(eventsPerStream, fragment, level + 1, events, exprEvaluatorContext);
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
                              propertyNames[level + 1] +
                              "': " +
                              ex.Message;
                Log.Error(message, ex);
                throw new EPException(message, ex);
            }
        }
    }
} // end of namespace