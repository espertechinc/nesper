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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.datetime.calop;
using com.espertech.esper.epl.datetime.interval;
using com.espertech.esper.epl.datetime.reformatop;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.dot;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.epl.rettype;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.datetime.eval
{
    public class ExprDotEvalDT : ExprDotEval
    {
        private readonly EPType _returnType;
        private readonly DTLocalEvaluator _evaluator;

        public ExprDotEvalDT(
            IList<CalendarOp> calendarOps,
            TimeZoneInfo timeZone,
            TimeAbacus timeAbacus,
            ReformatOp reformatOp,
            IntervalOp intervalOp,
            Type inputType,
            EventType inputEventType)
        {
            this._evaluator = GetEvaluator(
                calendarOps, timeZone, timeAbacus, inputType, inputEventType, reformatOp, intervalOp);

            if (intervalOp != null)
            {
                _returnType = EPTypeHelper.SingleValue(typeof (bool?));
            }
            else if (reformatOp != null)
            {
                _returnType = EPTypeHelper.SingleValue(reformatOp.ReturnType);
            }
            else
            {
                // only calendar ops
                if (inputEventType != null)
                {
                    _returnType =
                        EPTypeHelper.SingleValue(
                            inputEventType.GetPropertyType(inputEventType.StartTimestampPropertyName));
                }
                else
                {
                    _returnType = EPTypeHelper.SingleValue(inputType);
                }
            }
        }

        internal static void EvaluateDtxOps(
            IList<CalendarOp> calendarOps,
            DateTimeEx cal,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            foreach (var calendarOp in calendarOps)
            {
                calendarOp.Evaluate(cal, eventsPerStream, isNewData, exprEvaluatorContext);
            }
        }

        public EPType TypeInfo
        {
            get { return _returnType; }
        }

        public void Visit(ExprDotEvalVisitor visitor)
        {
            visitor.VisitDateTime();
        }

        public DTLocalEvaluator GetEvaluator(
            IList<CalendarOp> calendarOps,
            TimeZoneInfo timeZone,
            TimeAbacus timeAbacus,
            Type inputType,
            EventType inputEventType,
            ReformatOp reformatOp,
            IntervalOp intervalOp)
        {
            if (inputEventType == null)
            {
                if (reformatOp != null)
                {
                    if (TypeHelper.IsSubclassOrImplementsInterface(inputType, typeof (DateTimeEx)))
                    {
                        if (calendarOps.IsEmpty())
                            return new DTLocalEvaluatorDtxReformat(reformatOp);
                        return new DTLocalEvaluatorDtxOpsReformat(calendarOps, reformatOp);
                    }
                    else if (TypeHelper.IsSubclassOrImplementsInterface(inputType, typeof (DateTime)))
                    {
                        if (calendarOps.IsEmpty())
                        {
                            return new DTLocalEvaluatorDateTimeReformat(reformatOp);
                        }
                        return new DTLocalEvaluatorDateTimeOpsReformat(calendarOps, reformatOp, timeZone);
                    }
                    else if (TypeHelper.IsSubclassOrImplementsInterface(inputType, typeof (DateTimeOffset)))
                    {
                        if (calendarOps.IsEmpty())
                        {
                            return new DTLocalEvaluatorDateTimeOffsetReformat(reformatOp);
                        }
                        return new DTLocalEvaluatorDateTimeOffsetOpsReformat(calendarOps, reformatOp, timeZone);
                    }
                    else if (TypeHelper.GetBoxedType(inputType) == typeof(long))
                    {
                        if (calendarOps.IsEmpty())
                        {
                            return new DTLocalEvaluatorLongReformat(reformatOp);
                        }
                        return new DTLocalEvaluatorLongOpsReformat(calendarOps, reformatOp, timeZone, timeAbacus);
                    }
                }
                else if (intervalOp != null)
                {
                    if (TypeHelper.IsSubclassOrImplementsInterface(inputType, typeof (DateTimeEx)))
                    {
                        if (calendarOps.IsEmpty())
                        {
                            return new DTLocalEvaluatorDtxInterval(intervalOp);
                        }
                        return new DTLocalEvaluatorDtxOpsInterval(calendarOps, intervalOp, timeZone);
                    }
                    else if (TypeHelper.IsSubclassOrImplementsInterface(inputType, typeof (DateTime)))
                    {
                        if (calendarOps.IsEmpty())
                        {
                            return new DTLocalEvaluatorDateTimeInterval(intervalOp);
                        }
                        return new DTLocalEvaluatorDateTimeOpsInterval(calendarOps, intervalOp, timeZone);
                    }
                    else if (TypeHelper.IsSubclassOrImplementsInterface(inputType, typeof (DateTimeOffset)))
                    {
                        if (calendarOps.IsEmpty())
                        {
                            return new DTLocalEvaluatorDateTimeOffsetInterval(intervalOp);
                        }
                        return new DTLocalEvaluatorDateTimeOffsetOpsInterval(calendarOps, intervalOp, timeZone);
                    }
                    else if (TypeHelper.GetBoxedType(inputType) == typeof(long))
                    {
                        if (calendarOps.IsEmpty())
                        {
                            return new DTLocalEvaluatorLongInterval(intervalOp);
                        }
                        return new DTLocalEvaluatorLongOpsInterval(calendarOps, intervalOp, timeZone, timeAbacus);
                    }
                }
                else
                {
                    // only calendar ops, nothing else
                    if (TypeHelper.IsSubclassOrImplementsInterface(inputType, typeof (DateTimeEx)))
                    {
                        return new DTLocalEvaluatorDtxOpsDtx(calendarOps);
                    }
                    else if (TypeHelper.IsSubclassOrImplementsInterface(inputType, typeof (DateTime)))
                    {
                        return new DTLocalEvaluatorDtxOpsDateTime(calendarOps, timeZone);
                    }
                    else if (TypeHelper.IsSubclassOrImplementsInterface(inputType, typeof (DateTimeOffset)))
                    {
                        return new DTLocalEvaluatorDtxOpsDateTimeOffset(calendarOps, timeZone);
                    }
                    else if (TypeHelper.GetBoxedType(inputType) == typeof(long))
                    {
                        return new DTLocalEvaluatorDtxOpsLong(calendarOps, timeZone, timeAbacus);
                    }
                }
                throw new ArgumentException("Invalid input type '" + inputType + "'");
            }

            var getter = inputEventType.GetGetter(inputEventType.StartTimestampPropertyName);
            var getterResultType = inputEventType.GetPropertyType(inputEventType.StartTimestampPropertyName);

            if (reformatOp != null)
            {
                var inner = GetEvaluator(
                    calendarOps, timeZone, timeAbacus, getterResultType, null, reformatOp, null);
                return new DTLocalEvaluatorBeanReformat(getter, inner);
            }
            if (intervalOp == null)
            {
                // only calendar ops
                var inner = GetEvaluator(
                    calendarOps, timeZone, timeAbacus, getterResultType, null, null, null);
                return new DTLocalEvaluatorBeanCalOps(getter, inner);
            }

            // have interval ops but no end timestamp
            if (inputEventType.EndTimestampPropertyName == null)
            {
                var inner = GetEvaluator(
                    calendarOps, timeZone, timeAbacus, getterResultType, null, null, intervalOp);
                return new DTLocalEvaluatorBeanIntervalNoEndTS(getter, inner);
            }

            // interval ops and have end timestamp
            var getterEndTimestamp = inputEventType.GetGetter(inputEventType.EndTimestampPropertyName);
            var innerX =
                (DTLocalEvaluatorIntervalComp)
                    GetEvaluator(calendarOps, timeZone, timeAbacus, getterResultType, null, null, intervalOp);
            return new DTLocalEvaluatorBeanIntervalWithEnd(getter, getterEndTimestamp, innerX);
        }

        public object Evaluate(
            object target,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (target == null)
            {
                return null;
            }
            return _evaluator.Evaluate(target, eventsPerStream, isNewData, exprEvaluatorContext);
        }

        public interface DTLocalEvaluator
        {
            object Evaluate(
                object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext);
        }

        /// <summary>Interval methods.</summary>
        private interface DTLocalEvaluatorIntervalComp
        {
            object Evaluate(
                object startTimestamp,
                object endTimestamp,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext);
        }

        private abstract class DTLocalEvaluatorReformatBase : DTLocalEvaluator
        {
            protected readonly ReformatOp ReformatOp;

            protected DTLocalEvaluatorReformatBase(ReformatOp reformatOp)
            {
                ReformatOp = reformatOp;
            }

            public abstract object Evaluate(object target, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext);
        }

        private abstract class DTLocalEvaluatorCalopReformatBase : DTLocalEvaluator
        {
            protected readonly IList<CalendarOp> CalendarOps;
            protected readonly ReformatOp ReformatOp;

            protected DTLocalEvaluatorCalopReformatBase(IList<CalendarOp> calendarOps, ReformatOp reformatOp)
            {
                CalendarOps = calendarOps;
                ReformatOp = reformatOp;
            }

            public abstract object Evaluate(object target, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext);
        }

        private class DTLocalEvaluatorDtxReformat : DTLocalEvaluatorReformatBase
        {
            internal DTLocalEvaluatorDtxReformat(ReformatOp reformatOp)
                : base(reformatOp)
            {
            }

            public override object Evaluate(
                object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                return ReformatOp.Evaluate((DateTimeEx) target, eventsPerStream, isNewData, exprEvaluatorContext);
            }
        }

        private class DTLocalEvaluatorDtxOpsReformat : DTLocalEvaluatorCalopReformatBase
        {
            internal DTLocalEvaluatorDtxOpsReformat(IList<CalendarOp> calendarOps, ReformatOp reformatOp)
                : base(calendarOps, reformatOp)
            {
            }

            public override object Evaluate(
                object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var dtx = ((DateTimeEx) target).Clone();
                EvaluateDtxOps(CalendarOps, dtx, eventsPerStream, isNewData, exprEvaluatorContext);
                return ReformatOp.Evaluate(dtx, eventsPerStream, isNewData, exprEvaluatorContext);
            }
        }

        private class DTLocalEvaluatorDateTimeReformat : DTLocalEvaluatorReformatBase
        {
            internal DTLocalEvaluatorDateTimeReformat(ReformatOp reformatOp)
                : base(reformatOp)
            {
            }

            public override object Evaluate(
                object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                return ReformatOp.Evaluate((DateTime) target, eventsPerStream, isNewData, exprEvaluatorContext);
            }
        }

        private class DTLocalEvaluatorDateTimeOffsetReformat : DTLocalEvaluatorReformatBase
        {
            internal DTLocalEvaluatorDateTimeOffsetReformat(ReformatOp reformatOp)
                : base(reformatOp)
            {
            }

            public override object Evaluate(
                object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                return ReformatOp.Evaluate((DateTimeOffset) target, eventsPerStream, isNewData, exprEvaluatorContext);
            }
        }

        private class DTLocalEvaluatorDateTimeOpsReformat : DTLocalEvaluatorCalopReformatBase
        {
            private readonly TimeZoneInfo _timeZone;

            internal DTLocalEvaluatorDateTimeOpsReformat(
                IList<CalendarOp> calendarOps,
                ReformatOp reformatOp,
                TimeZoneInfo timeZone)
                : base(calendarOps, reformatOp)
            {
                _timeZone = timeZone;
            }

            public override object Evaluate(
                object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var dtx = DateTimeEx.GetInstance(_timeZone);
                dtx.SetUtcMillis(target.Time);
                EvaluateDtxOps(CalendarOps, dtx, eventsPerStream, isNewData, exprEvaluatorContext);
                return ReformatOp.Evaluate(dtx, eventsPerStream, isNewData, exprEvaluatorContext);
            }
        }

        private class DTLocalEvaluatorLongReformat : DTLocalEvaluatorReformatBase
        {
            internal DTLocalEvaluatorLongReformat(ReformatOp reformatOp)
                : base(reformatOp)
            {
            }

            public override object Evaluate(
                object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                return ReformatOp.Evaluate((long) target, eventsPerStream, isNewData, exprEvaluatorContext);
            }
        }

        private class DTLocalEvaluatorLongOpsReformat : DTLocalEvaluatorCalopReformatBase
        {
            private readonly TimeZoneInfo _timeZone;
            private readonly TimeAbacus _timeAbacus;

            internal DTLocalEvaluatorLongOpsReformat(
                IList<CalendarOp> calendarOps,
                ReformatOp reformatOp,
                TimeZoneInfo timeZone,
                TimeAbacus timeAbacus)
                : base(calendarOps, reformatOp)
            {
                _timeZone = timeZone;
                _timeAbacus = timeAbacus;
            }

            public override object Evaluate(
                object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var dtx = DateTimeEx.GetInstance(_timeZone);
                _timeAbacus.CalendarSet((long) target, dtx);
                EvaluateDtxOps(CalendarOps, dtx, eventsPerStream, isNewData, exprEvaluatorContext);
                return ReformatOp.Evaluate(dtx, eventsPerStream, isNewData, exprEvaluatorContext);
            }
        }

        private abstract class DTLocalEvaluatorIntervalBase
            : DTLocalEvaluator
            , DTLocalEvaluatorIntervalComp
        {
            protected readonly IntervalOp IntervalOp;

            protected DTLocalEvaluatorIntervalBase(IntervalOp intervalOp)
            {
                IntervalOp = intervalOp;
            }

            public abstract object Evaluate(
                object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext);

            public abstract object Evaluate(
                object startTimestamp,
                object endTimestamp,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext);
        }

        private abstract class DTLocalEvaluatorCalOpsIntervalBase
            : DTLocalEvaluator
            , DTLocalEvaluatorIntervalComp
        {
            protected readonly IList<CalendarOp> CalendarOps;
            protected readonly IntervalOp IntervalOp;

            protected DTLocalEvaluatorCalOpsIntervalBase(IList<CalendarOp> calendarOps, IntervalOp intervalOp)
            {
                CalendarOps = calendarOps;
                IntervalOp = intervalOp;
            }

            public abstract object Evaluate(
                object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext);

            public abstract object Evaluate(
                object startTimestamp,
                object endTimestamp,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext);
        }

        private class DTLocalEvaluatorDtxInterval : DTLocalEvaluatorIntervalBase
        {
            internal DTLocalEvaluatorDtxInterval(IntervalOp intervalOp)
                : base(intervalOp)
            {
            }

            public override object Evaluate(
                object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var time = ((DateTimeEx) target).TimeInMillis;
                return IntervalOp.Evaluate(time, time, eventsPerStream, isNewData, exprEvaluatorContext);
            }

            public override object Evaluate(
                object startTimestamp,
                object endTimestamp,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var start = ((DateTimeEx) startTimestamp).TimeInMillis;
                var end = ((DateTimeEx) endTimestamp).TimeInMillis;
                return IntervalOp.Evaluate(start, end, eventsPerStream, isNewData, exprEvaluatorContext);
            }
        }

        private class DTLocalEvaluatorDtxOpsInterval : DTLocalEvaluatorCalOpsIntervalBase
        {
            private readonly TimeZoneInfo _timeZone;

            internal DTLocalEvaluatorDtxOpsInterval(
                IList<CalendarOp> calendarOps,
                IntervalOp intervalOp,
                TimeZoneInfo timeZone)
                : base(calendarOps, intervalOp)
            {
                _timeZone = timeZone;
            }

            public override object Evaluate(
                object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var dtx = ((DateTimeEx) target).Clone();
                EvaluateDtxOps(CalendarOps, dtx, eventsPerStream, isNewData, exprEvaluatorContext);
                var time = dtx.TimeInMillis;
                return IntervalOp.Evaluate(time, time, eventsPerStream, isNewData, exprEvaluatorContext);
            }

            public override object Evaluate(
                object startTimestamp,
                object endTimestamp,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var startLong = ((DateTimeEx) startTimestamp).TimeInMillis;
                var endLong = ((DateTimeEx) endTimestamp).TimeInMillis;
                var dtx = DateTimeEx.GetInstance(_timeZone);
                dtx.SetUtcMillis(startLong);
                EvaluateDtxOps(CalendarOps, dtx, eventsPerStream, isNewData, exprEvaluatorContext);
                var startTime = dtx.TimeInMillis;
                var endTime = startTime + (endLong - startLong);
                return IntervalOp.Evaluate(startTime, endTime, eventsPerStream, isNewData, exprEvaluatorContext);
            }
        }

        private class DTLocalEvaluatorDateTimeInterval : DTLocalEvaluatorIntervalBase
        {
            internal DTLocalEvaluatorDateTimeInterval(IntervalOp intervalOp)
                : base(intervalOp)
            {
            }

            public override object Evaluate(
                object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var time = ((DateTime) target).UtcMillis();
                return IntervalOp.Evaluate(time, time, eventsPerStream, isNewData, exprEvaluatorContext);
            }

            public override object Evaluate(
                object startTimestamp,
                object endTimestamp,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var start = ((DateTime)startTimestamp).UtcMillis();
                var end = ((DateTime)endTimestamp).UtcMillis();
                return IntervalOp.Evaluate(start, end, eventsPerStream, isNewData, exprEvaluatorContext);
            }
        }

        private class DTLocalEvaluatorDateTimeOpsInterval : DTLocalEvaluatorCalOpsIntervalBase
        {
            private readonly TimeZoneInfo _timeZone;

            internal DTLocalEvaluatorDateTimeOpsInterval(
                IList<CalendarOp> calendarOps,
                IntervalOp intervalOp,
                TimeZoneInfo timeZone)
                : base(calendarOps, intervalOp)
            {
                _timeZone = timeZone;
            }

            public override object Evaluate(
                object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var dtx = DateTimeEx.GetInstance(_timeZone);
                dtx.SetUtcMillis(target.Time);
                EvaluateDtxOps(CalendarOps, dtx, eventsPerStream, isNewData, exprEvaluatorContext);
                var time = dtx.TimeInMillis;
                return IntervalOp.Evaluate(time, time, eventsPerStream, isNewData, exprEvaluatorContext);
            }

            public override object Evaluate(
                object startTimestamp,
                object endTimestamp,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var startLong = ((DateTime) startTimestamp).UtcMillis();
                var endLong = ((DateTime) endTimestamp).UtcMillis();
                var dtx = DateTimeEx.GetInstance(_timeZone);
                dtx.SetUtcMillis(startLong);
                EvaluateDtxOps(CalendarOps, dtx, eventsPerStream, isNewData, exprEvaluatorContext);
                var startTime = dtx.TimeInMillis;
                var endTime = startTime + (endLong - startLong);
                return IntervalOp.Evaluate(startTime, endTime, eventsPerStream, isNewData, exprEvaluatorContext);
            }
        }

        private class DTLocalEvaluatorLongInterval : DTLocalEvaluatorIntervalBase
        {
            internal DTLocalEvaluatorLongInterval(IntervalOp intervalOp)
                : base(intervalOp)
            {
            }

            public override object Evaluate(
                object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var time = (long) target;
                return IntervalOp.Evaluate(time, time, eventsPerStream, isNewData, exprEvaluatorContext);
            }

            public override object Evaluate(
                object startTimestamp,
                object endTimestamp,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var startTime = (long) startTimestamp;
                var endTime = (long) endTimestamp;
                return IntervalOp.Evaluate(startTime, endTime, eventsPerStream, isNewData, exprEvaluatorContext);
            }
        }

        private class DTLocalEvaluatorLongOpsInterval : DTLocalEvaluatorCalOpsIntervalBase
        {
            private readonly TimeZoneInfo _timeZone;
            private readonly TimeAbacus _timeAbacus;

            internal DTLocalEvaluatorLongOpsInterval(
                IList<CalendarOp> calendarOps,
                IntervalOp intervalOp,
                TimeZoneInfo timeZone,
                TimeAbacus timeAbacus)
                : base(calendarOps, intervalOp)
            {
                _timeZone = timeZone;
                _timeAbacus = timeAbacus;
            }

            public override object Evaluate(
                object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var dtx = DateTimeEx.GetInstance(_timeZone);
                var startRemainder = _timeAbacus.CalendarSet((long) target, dtx);
                EvaluateDtxOps(CalendarOps, dtx, eventsPerStream, isNewData, exprEvaluatorContext);
                var time = _timeAbacus.CalendarGet(dtx, startRemainder);
                return IntervalOp.Evaluate(time, time, eventsPerStream, isNewData, exprEvaluatorContext);
            }

            public override object Evaluate(
                object startTimestamp,
                object endTimestamp,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var startLong = (long) startTimestamp;
                var endLong = (long) endTimestamp;
                var dtx = DateTimeEx.GetInstance(_timeZone);
                var startRemainder = _timeAbacus.CalendarSet(startLong, dtx);
                EvaluateDtxOps(CalendarOps, dtx, eventsPerStream, isNewData, exprEvaluatorContext);
                var startTime = _timeAbacus.CalendarGet(dtx, startRemainder);
                var endTime = startTime + (endLong - startLong);
                return IntervalOp.Evaluate(startTime, endTime, eventsPerStream, isNewData, exprEvaluatorContext);
            }
        }

        private class DTLocalEvaluatorBeanReformat : DTLocalEvaluator
        {
            private readonly EventPropertyGetter _getter;
            private readonly DTLocalEvaluator _inner;

            internal DTLocalEvaluatorBeanReformat(EventPropertyGetter getter, DTLocalEvaluator inner)
            {
                _getter = getter;
                _inner = inner;
            }

            public object Evaluate(
                object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var timestamp = _getter.Get((EventBean) target);
                if (timestamp == null)
                {
                    return null;
                }
                return _inner.Evaluate(timestamp, eventsPerStream, isNewData, exprEvaluatorContext);
            }
        }

        private class DTLocalEvaluatorBeanIntervalNoEndTS : DTLocalEvaluator
        {
            private readonly EventPropertyGetter _getter;
            private readonly DTLocalEvaluator _inner;

            internal DTLocalEvaluatorBeanIntervalNoEndTS(EventPropertyGetter getter, DTLocalEvaluator inner)
            {
                _getter = getter;
                _inner = inner;
            }

            public object Evaluate(
                object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var timestamp = _getter.Get((EventBean) target);
                if (timestamp == null)
                {
                    return null;
                }
                return _inner.Evaluate(timestamp, eventsPerStream, isNewData, exprEvaluatorContext);
            }
        }

        private class DTLocalEvaluatorBeanIntervalWithEnd : DTLocalEvaluator
        {
            private readonly EventPropertyGetter _getterStartTimestamp;
            private readonly EventPropertyGetter _getterEndTimestamp;
            private readonly DTLocalEvaluatorIntervalComp _inner;

            internal DTLocalEvaluatorBeanIntervalWithEnd(
                EventPropertyGetter getterStartTimestamp,
                EventPropertyGetter getterEndTimestamp,
                DTLocalEvaluatorIntervalComp inner)
            {
                _getterStartTimestamp = getterStartTimestamp;
                _getterEndTimestamp = getterEndTimestamp;
                _inner = inner;
            }

            public object Evaluate(
                object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var startTimestamp = _getterStartTimestamp.Get((EventBean) target);
                if (startTimestamp == null)
                {
                    return null;
                }
                var endTimestamp = _getterEndTimestamp.Get((EventBean) target);
                if (endTimestamp == null)
                {
                    return null;
                }
                return _inner.Evaluate(startTimestamp, endTimestamp, eventsPerStream, isNewData, exprEvaluatorContext);
            }
        }

        private class DTLocalEvaluatorBeanCalOps : DTLocalEvaluator
        {
            private readonly EventPropertyGetter _getter;
            private readonly DTLocalEvaluator _inner;

            internal DTLocalEvaluatorBeanCalOps(EventPropertyGetter getter, DTLocalEvaluator inner)
            {
                _getter = getter;
                _inner = inner;
            }

            public object Evaluate(
                object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var timestamp = _getter.Get((EventBean) target);
                if (timestamp == null)
                {
                    return null;
                }
                return _inner.Evaluate(timestamp, eventsPerStream, isNewData, exprEvaluatorContext);
            }
        }

        private abstract class DTLocalEvaluatorDtxOpsDtxBase
        {
            protected readonly IList<CalendarOp> CalendarOps;

            protected DTLocalEvaluatorDtxOpsDtxBase(IList<CalendarOp> calendarOps)
            {
                CalendarOps = calendarOps;
            }
        }

        private class DTLocalEvaluatorDtxOpsLong
            : DTLocalEvaluatorDtxOpsDtxBase
            , DTLocalEvaluator
        {
            private readonly TimeZoneInfo _timeZone;
            private readonly TimeAbacus _timeAbacus;

            internal DTLocalEvaluatorDtxOpsLong(IList<CalendarOp> calendarOps, TimeZoneInfo timeZone, TimeAbacus timeAbacus)
                : base(calendarOps)
            {
                _timeZone = timeZone;
                _timeAbacus = timeAbacus;
            }

            public object Evaluate(
                object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var longValue = (long) target;
                var dtx = DateTimeEx.GetInstance(_timeZone);
                var remainder = _timeAbacus.CalendarSet(longValue, dtx);

                EvaluateDtxOps(CalendarOps, dtx, eventsPerStream, isNewData, exprEvaluatorContext);

                return _timeAbacus.CalendarGet(dtx, remainder);
            }
        }

        private class DTLocalEvaluatorDtxOpsDateTime
            : DTLocalEvaluatorDtxOpsDtxBase
            , DTLocalEvaluator
        {
            private readonly TimeZoneInfo _timeZone;

            internal DTLocalEvaluatorDtxOpsDateTime(IList<CalendarOp> calendarOps, TimeZoneInfo timeZone)
                : base(calendarOps)
            {
                _timeZone = timeZone;
            }

            public object Evaluate(
                object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var dateValue = (DateTime)target;
                var dtx = DateTimeEx.GetInstance(_timeZone);
                dtx.SetUtcMillis(dateValue.UtcMillis());

                EvaluateDtxOps(CalendarOps, dtx, eventsPerStream, isNewData, exprEvaluatorContext);

                return dtx.DateTime.DateTime;
            }
        }

        private class DTLocalEvaluatorDtxOpsDateTimeOffset
            : DTLocalEvaluatorDtxOpsDtxBase
            , DTLocalEvaluator
        {
            private readonly TimeZoneInfo _timeZone;

            internal DTLocalEvaluatorDtxOpsDateTimeOffset(IList<CalendarOp> calendarOps, TimeZoneInfo timeZone)
                : base(calendarOps)
            {
                _timeZone = timeZone;
            }

            public object Evaluate(
                object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var dateValue = (DateTimeOffset) target;
                var dtx = new DateTimeEx(dateValue, _timeZone);
                
                EvaluateDtxOps(CalendarOps, dtx, eventsPerStream, isNewData, exprEvaluatorContext);

                return dtx.DateTime;
            }
        }

        private class DTLocalEvaluatorDtxOpsDtx
            : DTLocalEvaluatorDtxOpsDtxBase
            , DTLocalEvaluator
        {
            internal DTLocalEvaluatorDtxOpsDtx(IList<CalendarOp> calendarOps)
                : base(calendarOps)
            {
            }

            public object Evaluate(
                object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var dtxValue = (DateTimeEx) target;
                var dtx = dtxValue.Clone();

                EvaluateDtxOps(CalendarOps, dtx, eventsPerStream, isNewData, exprEvaluatorContext);

                return dtx;
            }
        }
    }
} // end of namespace
