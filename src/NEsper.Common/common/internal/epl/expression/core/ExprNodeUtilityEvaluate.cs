///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.logging;

using System;
using System.Collections.Generic;
using System.Reflection;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public class ExprNodeUtilityEvaluate
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static object EvaluateValidationTimeNoStreams(
            ExprEvaluator evaluator,
            ExprEvaluatorContext context,
            string expressionName)
        {
            try {
                return evaluator.Evaluate(null, true, context);
            }
            catch (EPException ex) {
                throw new ExprValidationException("Invalid " + expressionName + " expression: " + ex.Message, ex);
            }
            catch (Exception ex) {
                Log.Warn("Invalid " + expressionName + " expression evaluation: {}", ex.Message, ex);
                throw new ExprValidationException("Invalid " + expressionName + " expression");
            }
        }

        public static void ApplyFilterExpressionIterable(
            IEnumerator<EventBean> enumerator,
            ExprEvaluator filterExpression,
            ExprEvaluatorContext exprEvaluatorContext,
            ICollection<EventBean> eventsInWindow)
        {
            EventBean[] events = new EventBean[1];
            while (enumerator.MoveNext()) {
                events[0] = enumerator.Current;

                try {
                    var result = filterExpression.Evaluate(events, true, exprEvaluatorContext);
                    if ((result == null) || (!((bool) result))) {
                        continue;
                    }

                    eventsInWindow.Add(events[0]);
                }
                catch (InvalidCastException) {
                }
            }
        }

        /// <summary>
        /// Apply a filter expression.
        /// </summary>
        /// <param name="filter">expression</param>
        /// <param name="streamZeroEvent">the event that represents stream zero</param>
        /// <param name="streamOneEvents">all events thate are stream one events</param>
        /// <param name="exprEvaluatorContext">context for expression evaluation</param>
        /// <returns>filtered stream one events</returns>
        public static EventBean[] ApplyFilterExpression(
            ExprEvaluator filter,
            EventBean streamZeroEvent,
            EventBean[] streamOneEvents,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            EventBean[] eventsPerStream = new EventBean[2];
            eventsPerStream[0] = streamZeroEvent;

            EventBean[] filtered = new EventBean[streamOneEvents.Length];
            int countPass = 0;

            foreach (EventBean eventBean in streamOneEvents) {
                eventsPerStream[1] = eventBean;

                var result = filter.Evaluate(eventsPerStream, true, exprEvaluatorContext);
                if ((result != null) && true.Equals(result)) {
                    filtered[countPass] = eventBean;
                    countPass++;
                }
            }

            if (countPass == streamOneEvents.Length) {
                return streamOneEvents;
            }

            return EventBeanUtility.ResizeArray(filtered, countPass);
        }

        /// <summary>
        /// Apply a filter expression returning a pass indicator.
        /// </summary>
        /// <param name="filter">to apply</param>
        /// <param name="eventsPerStream">events per stream</param>
        /// <param name="exprEvaluatorContext">context for expression evaluation</param>
        /// <returns>pass indicator</returns>
        public static bool ApplyFilterExpression(
            ExprEvaluator filter,
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var result = filter.Evaluate(eventsPerStream, true, exprEvaluatorContext);
            return (result != null) && true.Equals(result);
        }

        public static object[] EvaluateExpressions(
            ExprEvaluator[] parameters,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            object[] results = new object[parameters.Length];
            int count = 0;
            foreach (ExprEvaluator expr in parameters) {
                try {
                    results[count] = expr.Evaluate(null, true, exprEvaluatorContext);
                    count++;
                }
                catch (EPException) {
                    throw;
                }
                catch (Exception ex) {
                    string message = "Failed expression evaluation in crontab timer-at for parameter " +
                                     count +
                                     ": " +
                                     ex.Message;
                    Log.Error(message, ex);
                    throw new ArgumentException(message);
                }
            }

            return results;
        }
    }
} // end of namespace