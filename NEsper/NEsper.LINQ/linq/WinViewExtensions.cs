///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.runtime.client.linq
{
    public static class WinViewExtensions
    {
        /// <summary>
        /// Expands the view to keep the first Count events.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static EsperQuery<T> KeepFirst<T>(this EsperQuery<T> esperQuery, int length)
        {
            return esperQuery.FilterView(() => View.Create("firstlength", new ConstantExpression(length)));
        }

        /// <summary>
        /// Expands the view to keep events that occur within the specified duration.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <param name="duration">The duration.</param>
        /// <returns></returns>
        public static EsperQuery<T> KeepFirst<T>(this EsperQuery<T> esperQuery, TimeSpan duration)
        {
            var timePeriodExpression = duration.ToTimePeriodExpression();
            return esperQuery.FilterView(() => View.Create("firsttime", timePeriodExpression));
        }

        /// <summary>
        /// Expands the view to keep events that satisfy the expression condition.  The expiry expression can
        /// be any expression including expressions on event properties, variables, aggregation functions or
        /// user-defined functions. The view applies this expression to the oldest event(s) currently in the
        /// view.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public static EsperQuery<T> KeepWhile<T>(this EsperQuery<T> esperQuery, System.Linq.Expressions.Expression<Func<T, bool>> expression)
        {
            return esperQuery.FilterView(() => View.Create("expr", LinqToSoda.LinqToSodaExpression(expression)));
        }

        /// <summary>
        /// Expands the view to keep events (tumbling window) until the given expression is satisfied.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public static EsperQuery<T> KeepUntil<T>(this EsperQuery<T> esperQuery, System.Linq.Expressions.Expression<Func<T, bool>> expression)
        {
            return esperQuery.FilterView(() => View.Create("expr_batch", LinqToSoda.LinqToSodaExpression(expression)));
        }

        /// <summary>
        /// Expands the view to keep all events.  The view does not remove events from the
        /// data window, unless used with a named window and the on delete clause.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <returns></returns>
        public static EsperQuery<T> KeepAll<T>(this EsperQuery<T> esperQuery)
        {
            return esperQuery.FilterView(() => View.Create("keepall"));
        }

        /// <summary>
        /// Expands the view to keep a sliding window of events.  This view is a moving (sliding) length
        /// window extending the specified number of elements into the past. The view takes a single
        /// expression as a parameter providing a numeric size value that defines the window size.
        /// <para/>
        /// If batch is specified, then the window buffers events (tumbling window) and releases them
        /// when the given number of events has been collected.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <param name="length">The length.</param>
        /// <param name="batched">if set to <c>true</c> [batched].</param>
        /// <returns></returns>
        public static EsperQuery<T> WithLength<T>(this EsperQuery<T> esperQuery, int length, bool batched = false)
        {
            var windowName = batched
                ? "length_batch"
                : "length";

            return esperQuery.FilterView(() => View.Create(windowName, new ConstantExpression(length)));
        }

        /// <summary>
        /// Expands the view to use a time bound window.  TimeInMillis bound windows are sliding windows that extend the
        /// specified time interval into the past based on the system time.  Provide a time period as parameter.
        /// <para/>
        /// If batch is specified, then the window buffers events (tumbling window) and releases them
        /// after the given time interval has occurred.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <param name="seconds">The seconds.</param>
        /// <param name="batched">if set to <c>true</c> [batched].</param>
        /// <returns></returns>
        public static EsperQuery<T> WithDuration<T>(this EsperQuery<T> esperQuery, int seconds, bool batched = false)
        {
            return WithDuration(esperQuery, TimeSpan.FromSeconds(seconds), batched);
        }

        /// <summary>
        /// Expands the view to use a time bound window.  TimeInMillis bound windows are sliding windows that extend the
        /// specified time interval into the past based on the system time.  Provide a time period as parameter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <param name="timeSpan">The time span.</param>
        /// <param name="batched">if set to <c>true</c> [batched].</param>
        /// <returns></returns>
        public static EsperQuery<T> WithDuration<T>(this EsperQuery<T> esperQuery, TimeSpan timeSpan, bool batched = false)
        {
            var timePeriodExpression = timeSpan.ToTimePeriodExpression();
            var windowName = batched ? "time_batch" : "time";

            return esperQuery.FilterView(() => View.Create(windowName, timePeriodExpression));
        }

        /// <summary>
        /// Expands the view to use a time-accumulating.  This data window view is a specialized moving (sliding)
        /// time window that differs from the regular time window in that it accumulates events until no more events
        /// arrive within a given time interval, and only then releases the accumulated events as a remove stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <param name="timeSpan">The time span.</param>
        /// <returns></returns>
        public static EsperQuery<T> WithAccumlation<T>(this EsperQuery<T> esperQuery, TimeSpan timeSpan)
        {
            var timePeriodExpression = timeSpan.ToTimePeriodExpression();
            return esperQuery.FilterView(() => View.Create("time_accum", timePeriodExpression));
        }

        /// <summary>
        /// Expands the view to use a time and length bound window.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <param name="timeSpan">The time span.</param>
        /// <param name="length">The length.</param>
        /// <param name="flowControlKeywords">The flow control keywords.</param>
        /// <returns></returns>
        public static EsperQuery<T> WithDurationAndLength<T>(this EsperQuery<T> esperQuery, TimeSpan timeSpan, int length, string flowControlKeywords = null)
        {
            var timePeriodExpression = timeSpan.ToTimePeriodExpression();
            var lengthExpression = new ConstantExpression(length);

            if (flowControlKeywords == null)
            {
                return esperQuery.FilterView(() => View.Create("time_length_batch", timePeriodExpression, lengthExpression));
            }

            // we want to take this apart and turn this into something people can use without having to know
            // the keywords.  an enumeration might work well here with the values being accepted as params on
            // function call.
            var flowControlExpression = new ConstantExpression(flowControlKeywords);

            return esperQuery.FilterView(() => View.Create("time_length_batch", timePeriodExpression, lengthExpression, flowControlExpression));
        }
    }
}