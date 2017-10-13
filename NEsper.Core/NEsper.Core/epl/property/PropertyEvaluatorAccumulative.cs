///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.property
{
    /// <summary>
    /// A property evaluator that returns a full row of events for each stream, i.e. flattened inner-join results for
    /// property-upon-property.
    /// </summary>
    public class PropertyEvaluatorAccumulative
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    
        private readonly ContainedEventEval[] _containedEventEvals;
        private readonly FragmentEventType[] _fragmentEventType;
        private readonly ExprEvaluator[] _whereClauses;
        private readonly int _lastLevel;
        private readonly int _levels;
        private readonly IList<string> _propertyNames;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="containedEventEvals">property getters or other evaluators</param>
        /// <param name="fragmentEventType">property fragment types</param>
        /// <param name="whereClauses">filters, if any</param>
        /// <param name="propertyNames">the property names that are staggered</param>
        public PropertyEvaluatorAccumulative(
            ContainedEventEval[] containedEventEvals,
            FragmentEventType[] fragmentEventType,
            ExprEvaluator[] whereClauses,
            IList<string> propertyNames)
        {
            _fragmentEventType = fragmentEventType;
            _containedEventEvals = containedEventEvals;
            _whereClauses = whereClauses;
            _lastLevel = fragmentEventType.Length - 1;
            _levels = fragmentEventType.Length + 1;
            _propertyNames = propertyNames;
        }
    
        /// <summary>
        /// Returns the accumulative events for the input event.
        /// </summary>
        /// <param name="theEvent">is the input event</param>
        /// <param name="exprEvaluatorContext">expression evaluation context</param>
        /// <returns>events per stream for each row</returns>
        public ArrayDeque<EventBean[]> GetAccumulative(EventBean theEvent, ExprEvaluatorContext exprEvaluatorContext) {
            var resultEvents = new ArrayDeque<EventBean[]>();
            var eventsPerStream = new EventBean[_levels];
            eventsPerStream[0] = theEvent;
            PopulateEvents(eventsPerStream, theEvent, 0, resultEvents, exprEvaluatorContext);
            if (resultEvents.IsEmpty()) {
                return null;
            }
            return resultEvents;
        }
    
        private void PopulateEvents(EventBean[] eventsPerStream, EventBean branch, int level, ICollection<EventBean[]> events, ExprEvaluatorContext exprEvaluatorContext) {
            try {
                Object result = _containedEventEvals[level].GetFragment(branch, eventsPerStream, exprEvaluatorContext);
    
                if (_fragmentEventType[level].IsIndexed) {
                    EventBean[] fragments = (EventBean[]) result;
                    if (level == _lastLevel) {
                        if (_whereClauses[level] != null) {
                            foreach (EventBean theEvent in fragments) {
                                eventsPerStream[level + 1] = theEvent;
                                if (ExprNodeUtility.ApplyFilterExpression(_whereClauses[level], eventsPerStream, exprEvaluatorContext)) {
                                    var eventsPerRow = new EventBean[_levels];
                                    Array.Copy(eventsPerStream, 0, eventsPerRow, 0, _levels);
                                    events.Add(eventsPerRow);
                                }
                            }
                        } else {
                            foreach (EventBean theEvent in fragments) {
                                eventsPerStream[level + 1] = theEvent;
                                var eventsPerRow = new EventBean[_levels];
                                Array.Copy(eventsPerStream, 0, eventsPerRow, 0, _levels);
                                events.Add(eventsPerRow);
                            }
                        }
                    } else {
                        if (_whereClauses[level] != null) {
                            foreach (EventBean next in fragments) {
                                eventsPerStream[level + 1] = next;
                                if (ExprNodeUtility.ApplyFilterExpression(_whereClauses[level], eventsPerStream, exprEvaluatorContext)) {
                                    PopulateEvents(eventsPerStream, next, level + 1, events, exprEvaluatorContext);
                                }
                            }
                        } else {
                            foreach (EventBean next in fragments) {
                                eventsPerStream[level + 1] = next;
                                PopulateEvents(eventsPerStream, next, level + 1, events, exprEvaluatorContext);
                            }
                        }
                    }
                } else {
                    EventBean fragment = (EventBean) result;
                    if (level == _lastLevel) {
                        if (_whereClauses[level] != null) {
                            eventsPerStream[level + 1] = fragment;
                            if (ExprNodeUtility.ApplyFilterExpression(_whereClauses[level], eventsPerStream, exprEvaluatorContext)) {
                                var eventsPerRow = new EventBean[_levels];
                                Array.Copy(eventsPerStream, 0, eventsPerRow, 0, _levels);
                                events.Add(eventsPerRow);
                            }
                        } else {
                            eventsPerStream[level + 1] = fragment;
                            var eventsPerRow = new EventBean[_levels];
                            Array.Copy(eventsPerStream, 0, eventsPerRow, 0, _levels);
                            events.Add(eventsPerRow);
                        }
                    } else {
                        if (_whereClauses[level] != null) {
                            eventsPerStream[level + 1] = fragment;
                            if (ExprNodeUtility.ApplyFilterExpression(_whereClauses[level], eventsPerStream, exprEvaluatorContext)) {
                                PopulateEvents(eventsPerStream, fragment, level + 1, events, exprEvaluatorContext);
                            }
                        } else {
                            eventsPerStream[level + 1] = fragment;
                            PopulateEvents(eventsPerStream, fragment, level + 1, events, exprEvaluatorContext);
                        }
                    }
                }
            } catch (Exception ex)
            {
                Log.Error(
                    string.Format(
                        "Unexpected error evaluating property expression for event of type '{0}' and property '{1}': {2}",
                        branch.EventType.Name, _propertyNames[level + 1], ex.Message), ex);
            }
        }
    }
} // end of namespace
