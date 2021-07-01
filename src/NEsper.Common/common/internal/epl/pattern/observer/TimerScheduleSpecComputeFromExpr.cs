///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.epl.expression.time.eval;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.epl.pattern.observer.TimerScheduleObserverForge;

namespace com.espertech.esper.common.@internal.epl.pattern.observer
{
    public class TimerScheduleSpecComputeFromExpr : TimerScheduleSpecCompute
    {
        private ExprEvaluator date;
        private ExprEvaluator repetitions;
        private TimePeriodEval timePeriod;

        public ExprEvaluator Date {
            set { this.date = value; }
        }

        public ExprEvaluator Repetitions {
            set { this.repetitions = value; }
        }

        public TimePeriodEval TimePeriod {
            set { this.timePeriod = value; }
        }

        public TimerScheduleSpec Compute(
            MatchedEventConvertor optionalConvertor,
            MatchedEventMap beginState,
            ExprEvaluatorContext exprEvaluatorContext,
            TimeZoneInfo timeZone,
            TimeAbacus timeAbacus)
        {
            EventBean[] eventsPerStream = optionalConvertor == null ? null : optionalConvertor.Invoke(beginState);
            return Compute(date, repetitions, timePeriod, eventsPerStream, exprEvaluatorContext, timeZone, timeAbacus);
        }

        protected internal static TimerScheduleSpec Compute(
            ExprEvaluator date,
            ExprEvaluator repetitions,
            TimePeriodEval timePeriod,
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext,
            TimeZoneInfo timeZone,
            TimeAbacus timeAbacus)
        {
            DateTimeEx optionalDate = null;
            long? optionalRemainder = null;
            if (date != null) {
                object param = PatternExpressionUtil.EvaluateChecked(
                    TimerScheduleObserverForge.NAME_OBSERVER,
                    date,
                    eventsPerStream,
                    exprEvaluatorContext);
                if (param is string) {
                    optionalDate = TimerScheduleISO8601Parser.ParseDate((string) param);
                }
                else if (TypeHelper.IsNumber(param)) {
                    long msec = param.AsInt64();
                    optionalDate = DateTimeEx.GetInstance(timeZone);
                    optionalRemainder = timeAbacus.DateTimeSet(msec, optionalDate);
                }
                else if (param is DateTimeEx dateTimeEx) {
                    optionalDate = DateTimeEx.GetInstance(timeZone, dateTimeEx);
                }
                else if (param is DateTime dateTime) {
                    optionalDate = DateTimeEx.GetInstance(timeZone, dateTime);
                }
                else if (param is DateTimeOffset dateTimeOffset) {
                    optionalDate = DateTimeEx.GetInstance(timeZone, dateTimeOffset);
                }
                else if (param == null) {
                    throw new EPException("Null date-time value returned from date evaluation");
                }
                else {
                    throw new EPException("Unrecognized date-time value " + param.GetType());
                }
            }

            TimePeriod optionalTimePeriod = null;
            if (timePeriod != null) {
                try {
                    optionalTimePeriod = timePeriod.TimePeriodEval(eventsPerStream, true, exprEvaluatorContext);
                }
                catch (EPException) {
                    throw;
                }
                catch (Exception ex) {
                    PatternExpressionUtil.HandleRuntimeEx(ex, NAME_OBSERVER);
                }
            }

            long? optionalRepeatCount = null;
            if (repetitions != null) {
                object param = PatternExpressionUtil.EvaluateChecked(
                    NAME_OBSERVER,
                    repetitions,
                    eventsPerStream,
                    exprEvaluatorContext);
                if (param != null) {
                    optionalRepeatCount = (param).AsInt64();
                }
            }

            if (optionalDate == null && optionalTimePeriod == null) {
                throw new EPException("Required date or time period are both null for " + NAME_OBSERVER);
            }

            return new TimerScheduleSpec(optionalDate, optionalRemainder, optionalRepeatCount, optionalTimePeriod);
        }
    }
} // end of namespace