///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.util;
using com.espertech.esper.compat;
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

        public void Evaluate(DateTimeEx dateTime, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            var value = _param.Evaluate(new EvaluateParams(eventsPerStream, isNewData, context));
            if (value.IsNumber())
            {
                Action(dateTime, _factor, value.AsLong());
            }
            else
            {
                Action(dateTime, _factor, (TimePeriod) value);
            }
        }

        internal static DateTimeEx Action(DateTimeEx dateTime, int factor, long? duration)
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
            var newDate = dateTime
                .AddMilliseconds(factor * msec)
                .AddDays(factor * days, DateTimeMathStyle.Java);

            return newDate;
        }

        public static void ActionSafeOverflow(DateTimeEx dateTime, int factor, TimePeriod tp)
        {
            if (Math.Abs(factor) == 1)
            {
                Action(dateTime, factor, tp);
                return;
            }

            var max = tp.LargestAbsoluteValue();
            if (max == null || max == 0)
            {
                return;
            }

            ActionHandleOverflow(dateTime, factor, tp, max.Value);
        }

        public static void Action(DateTimeEx dateTime, int factor, TimePeriod tp)
        {
            if (tp == null)
            {
                return;
            }

            if (tp.Years != null)
            {
                dateTime.AddYears(tp.Years.Value * factor);
            }
            if (tp.Months != null)
            {
                dateTime.AddMonths(tp.Months.Value * factor, DateTimeMathStyle.Java);
            }
            if (tp.Weeks != null)
            {
                dateTime.AddDays(tp.Weeks.Value * 7 * factor, DateTimeMathStyle.Java);
            }
            if (tp.Days != null)
            {
                dateTime.AddDays(tp.Days.Value * factor, DateTimeMathStyle.Java);
            }
            if (tp.Hours != null)
            {
                dateTime.AddHours(tp.Hours.Value * factor, DateTimeMathStyle.Java);
            }
            if (tp.Minutes != null)
            {
                dateTime.AddMinutes(tp.Minutes.Value * factor, DateTimeMathStyle.Java);
            }
            if (tp.Seconds != null)
            {
                dateTime.AddSeconds(tp.Seconds.Value * factor, DateTimeMathStyle.Java);
            }
            if (tp.Milliseconds != null)
            {
                dateTime.AddMilliseconds(tp.Milliseconds.Value * factor, DateTimeMathStyle.Java);
            }
        }

        private static void ActionHandleOverflow(DateTimeEx dateTime, int factor, TimePeriod tp, int max)
        {
            if (max != 0 && factor > int.MaxValue / max)
            {
                // overflow
                int first = factor / 2;
                int second = (factor - first * 2) + first;
                ActionHandleOverflow(dateTime, first, tp, max);
                ActionHandleOverflow(dateTime, second, tp, max);
            }
            else
            {
                // no overflow
                Action(dateTime, factor, tp);
            }
        }
    }
}
