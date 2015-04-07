///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.epl.datetime.eval;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.datetime.calop
{
    public class CalendarOpPlusMinus : CalendarOp
    {
        private readonly ExprEvaluator _param;
        private readonly int _factor;

        public CalendarOpPlusMinus(ExprEvaluator param, int factor)
        {
            _param = param;
            _factor = factor;
        }

        public void Evaluate(ref DateTime dateTime, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            var value = _param.Evaluate(new EvaluateParams(eventsPerStream, isNewData, context));
            if (value.IsNumber())
            {
                dateTime = Action(dateTime, _factor, value.AsLong());
            }
            else
            {
                dateTime = Action(dateTime, _factor, (TimePeriod)value);
            }
        }

        internal static DateTime Action(DateTime dateTime, int factor, long? duration)
        {
            if (duration == null)
            {
                return dateTime;
            }

            if (duration < Int32.MaxValue)
            {
                return dateTime.AddMilliseconds((int)(factor * duration));
            }

            var days = (int)(duration / (1000L * 60 * 60 * 24));
            var msec = (int)(duration - days * (1000L * 60 * 60 * 24));
            return dateTime
                .AddMilliseconds(factor * msec)
                .AddDays(factor * days);
        }

        public static DateTime ActionSafeOverflow(DateTime dateTime, int factor, TimePeriod tp)
        {
            if (Math.Abs(factor) == 1)
            {
                return Action(dateTime, factor, tp);
            }

            var max = tp.LargestAbsoluteValue();
            if (max == null || max == 0)
            {
                return dateTime;
            }

            return ActionHandleOverflow(dateTime, factor, tp, max.Value);
        }

        public static DateTime Action(DateTime dateTime, int factor, TimePeriod tp)
        {
            if (tp == null)
            {
                return dateTime;
            }

            TimeZone zone;

            //dateTime = dateTime.ToUniversalTime();

            if (tp.Years != null)
            {
                dateTime = dateTime.AddYears(tp.Years.Value * factor);
            }
            if (tp.Months != null)
            {
                dateTime = dateTime.AddMonths(tp.Months.Value * factor);
            }
            if (tp.Weeks != null)
            {
                dateTime = dateTime.AddDays(tp.Weeks.Value * 7 * factor);
            }
            if (tp.Days != null)
            {
                dateTime = dateTime.AddDays(tp.Days.Value * factor);
            }
            if (tp.Hours != null)
            {
                dateTime = dateTime.AddHours(tp.Hours.Value * factor);
            }
            if (tp.Minutes != null)
            {
                dateTime = dateTime.AddMinutes(tp.Minutes.Value * factor);
            }
            if (tp.Seconds != null)
            {
                dateTime = dateTime.AddSeconds(tp.Seconds.Value * factor);
            }
            if (tp.Milliseconds != null)
            {
                dateTime = dateTime.AddMilliseconds(tp.Milliseconds.Value * factor);
            }

            //dateTime = dateTime.ToLocalTime();

            return dateTime;
        }

        private static DateTime ActionHandleOverflow(DateTime dateTime, int factor, TimePeriod tp, int max)
        {
            if (max != 0 && factor > int.MaxValue / max)
            {
                // overflow
                int first = factor / 2;
                int second = (factor - first * 2) + first;
                dateTime = ActionHandleOverflow(dateTime, first, tp, max);
                dateTime = ActionHandleOverflow(dateTime, second, tp, max);
            }
            else
            {
                // no overflow
                dateTime = Action(dateTime, factor, tp);
            }

            return dateTime;
        }
    }
}
