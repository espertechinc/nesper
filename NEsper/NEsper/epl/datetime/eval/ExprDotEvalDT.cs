///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.epl.rettype;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.datetime.eval
{
    public class ExprDotEvalDT : ExprDotEval
    {
        private readonly DTLocalEvaluator _evaluator;
        private readonly EPType _returnType;

        public ExprDotEvalDT(
            IList<CalendarOp> calendarOps,
            TimeZoneInfo timeZone,
            ReformatOp reformatOp,
            IntervalOp intervalOp,
            Type inputType,
            EventType inputEventType)
        {
            _evaluator = GetEvaluator(calendarOps, timeZone, inputType, inputEventType, reformatOp, intervalOp);

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
                    _returnType = EPTypeHelper.SingleValue(
                        inputEventType.GetPropertyType(inputEventType.StartTimestampPropertyName));
                }
                else
                {
                    _returnType = EPTypeHelper.SingleValue(inputType);
                }
            }
        }

        #region ExprDotEval Members

        public EPType TypeInfo
        {
            get { return _returnType; }
        }

        public void Visit(ExprDotEvalVisitor visitor)
        {
            visitor.VisitDateTime();
        }

        public Object Evaluate(
            Object target,
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

        #endregion

        public DTLocalEvaluator GetEvaluator(
            IList<CalendarOp> calendarOps,
            TimeZoneInfo timeZone,
            Type inputType,
            EventType inputEventType,
            ReformatOp reformatOp,
            IntervalOp intervalOp)
        {
            if (inputEventType == null)
            {
                if (reformatOp != null)
                {
                    if (inputType.GetBoxedType() == typeof (DateTime?))
                    {
                        if (calendarOps.IsEmpty())
                        {
                            return new DTLocalEvaluatorDateTimeReformat(reformatOp);
                        }
                        return new DTLocalEvaluatorDateTimeOpsReformat(calendarOps, reformatOp, timeZone);
                    }
                    if (inputType.GetBoxedType() == typeof(DateTimeOffset?))
                    {
                        if (calendarOps.IsEmpty())
                        {
                            return new DTLocalEvaluatorDateTimeReformat(reformatOp);
                        }
                        return new DTLocalEvaluatorDateTimeOpsReformat(calendarOps, reformatOp, timeZone);
                    }
                    if (inputType.GetBoxedType() == typeof(long?))
                    {
                        if (calendarOps.IsEmpty())
                        {
                            return new DTLocalEvaluatorLongReformat(reformatOp);
                        }
                        return new DTLocalEvaluatorLongOpsReformat(calendarOps, reformatOp, timeZone);
                    }
                }
                else if (intervalOp != null)
                {
                    if (inputType.GetBoxedType() == typeof (DateTime?))
                    {
                        if (calendarOps.IsEmpty())
                        {
                            return new DTLocalEvaluatorDateTimeInterval(intervalOp);
                        }
                        return new DTLocalEvaluatorDateTimeOpsInterval(calendarOps, intervalOp, timeZone);
                    }
                    if (inputType.GetBoxedType() == typeof(DateTimeOffset?))
                    {
                        if (calendarOps.IsEmpty())
                        {
                            return new DTLocalEvaluatorDateTimeInterval(intervalOp);
                        }
                        return new DTLocalEvaluatorDateTimeOpsInterval(calendarOps, intervalOp, timeZone);
                    }
                    if (inputType.GetBoxedType() == typeof (long?))
                    {
                        if (calendarOps.IsEmpty())
                        {
                            return new DTLocalEvaluatorLongInterval(intervalOp);
                        }
                        return new DTLocalEvaluatorLongOpsInterval(calendarOps, intervalOp, timeZone);
                    }
                }
                else
                {
                    // only calendar ops, nothing else
                    if (inputType.GetBoxedType() == typeof (DateTime?))
                    {
                        return new DTLocalEvaluatorCalOpsDateTime(calendarOps, timeZone);
                    }
                    if (inputType.GetBoxedType() == typeof(DateTimeOffset?))
                    {
                        return new DTLocalEvaluatorCalOpsDateTime(calendarOps, timeZone);
                    }
                    if (inputType.GetBoxedType() == typeof (long?))
                    {
                        return new DTLocalEvaluatorCalOpsLong(calendarOps, timeZone);
                    }
                }
                throw new ArgumentException("Invalid input type '" + inputType + "'");
            }

            var getter = inputEventType.GetGetter(inputEventType.StartTimestampPropertyName);
            var getterResultType = inputEventType.GetPropertyType(inputEventType.StartTimestampPropertyName);

            if (reformatOp != null)
            {
                var inner = GetEvaluator(calendarOps, timeZone, getterResultType, null, reformatOp, null);
                return new DTLocalEvaluatorBeanReformat(getter, inner);
            }
            if (intervalOp == null)
            {
                // only calendar ops
                var inner = GetEvaluator(calendarOps, timeZone, getterResultType, null, null, null);
                return new DTLocalEvaluatorBeanCalOps(getter, inner);
            }

            // have interval ops but no end timestamp
            if (inputEventType.EndTimestampPropertyName == null)
            {
                var inner = GetEvaluator(calendarOps, timeZone, getterResultType, null, null, intervalOp);
                return new DTLocalEvaluatorBeanIntervalNoEndTS(getter, inner);
            }

            // interval ops and have end timestamp
            var getterEndTimestamp = inputEventType.GetGetter(inputEventType.EndTimestampPropertyName);
            var intervalComp =
                (DTLocalEvaluatorIntervalComp)GetEvaluator(calendarOps, timeZone, getterResultType, null, null, intervalOp);
            return new DTLocalEvaluatorBeanIntervalWithEnd(getter, getterEndTimestamp, intervalComp);
        }

        public static void EvaluateCalOps(IEnumerable<CalendarOp> calendarOps, DateTimeEx dateTime, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            foreach (var calendarOp in calendarOps)
            {
                calendarOp.Evaluate(dateTime, eventsPerStream, isNewData, exprEvaluatorContext);
            }
        }

        #region Nested type: DTLocalEvaluator

        public interface DTLocalEvaluator
        {
            Object Evaluate(
                Object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext);
        }

        #endregion

        #region Nested type: DTLocalEvaluatorBeanCalOps

        private class DTLocalEvaluatorBeanCalOps : DTLocalEvaluator
        {
            private readonly EventPropertyGetter _getter;
            private readonly DTLocalEvaluator _inner;

            internal DTLocalEvaluatorBeanCalOps(
                EventPropertyGetter getter,
                DTLocalEvaluator inner)
            {
                _getter = getter;
                _inner = inner;
            }

            #region DTLocalEvaluator Members

            public Object Evaluate(
                Object target,
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

            #endregion
        }

        #endregion

        #region Nested type: DTLocalEvaluatorBeanIntervalNoEndTS

        private class DTLocalEvaluatorBeanIntervalNoEndTS : DTLocalEvaluator
        {
            private readonly EventPropertyGetter _getter;
            private readonly DTLocalEvaluator _inner;

            internal DTLocalEvaluatorBeanIntervalNoEndTS(
                EventPropertyGetter getter,
                DTLocalEvaluator inner)
            {
                _getter = getter;
                _inner = inner;
            }

            #region DTLocalEvaluator Members

            public Object Evaluate(
                Object target,
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

            #endregion
        }

        #endregion

        #region Nested type: DTLocalEvaluatorBeanIntervalWithEnd

        private class DTLocalEvaluatorBeanIntervalWithEnd : DTLocalEvaluator
        {
            private readonly EventPropertyGetter _getterEndTimestamp;
            private readonly EventPropertyGetter _getterStartTimestamp;
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

            #region DTLocalEvaluator Members

            public Object Evaluate(
                Object target,
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

            #endregion
        }

        #endregion

        #region Nested type: DTLocalEvaluatorBeanReformat

        private class DTLocalEvaluatorBeanReformat : DTLocalEvaluator
        {
            private readonly EventPropertyGetter _getter;
            private readonly DTLocalEvaluator _inner;

            internal DTLocalEvaluatorBeanReformat(
                EventPropertyGetter getter,
                DTLocalEvaluator inner)
            {
                _getter = getter;
                _inner = inner;
            }

            #region DTLocalEvaluator Members

            public Object Evaluate(
                Object target,
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

            #endregion
        }

        #endregion

        #region Nested type: DTLocalEvaluatorCalOpsCalBase

        private abstract class DTLocalEvaluatorCalOpsCalBase
        {
            protected readonly IList<CalendarOp> CalendarOps;

            protected DTLocalEvaluatorCalOpsCalBase(IList<CalendarOp> calendarOps)
            {
                CalendarOps = calendarOps;
            }
        }

        #endregion

        #region Nested type: DTLocalEvaluatorCalOpsDateTime

        private class DTLocalEvaluatorCalOpsDateTime
            : DTLocalEvaluatorCalOpsCalBase
            , DTLocalEvaluator
        {
            private readonly TimeZoneInfo _timeZone;

            internal DTLocalEvaluatorCalOpsDateTime(IList<CalendarOp> calendarOps, TimeZoneInfo timeZone)
                : base(calendarOps)
            {
                _timeZone = timeZone;
            }

            #region DTLocalEvaluator Members

            public Object Evaluate(
                Object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var dateValue = new DateTimeEx(target.AsDateTimeOffset(_timeZone), _timeZone);
                EvaluateCalOps(CalendarOps, dateValue, eventsPerStream, isNewData, exprEvaluatorContext);
                return dateValue.DateTime;
            }

            #endregion
        }

        #endregion

        #region Nested type: DTLocalEvaluatorCalOpsIntervalBase

        private abstract class DTLocalEvaluatorCalOpsIntervalBase
            : DTLocalEvaluator
            , DTLocalEvaluatorIntervalComp
        {
            protected readonly IList<CalendarOp> CalendarOps;
            protected readonly IntervalOp IntervalOp;

            protected DTLocalEvaluatorCalOpsIntervalBase(
                IList<CalendarOp> calendarOps,
                IntervalOp intervalOp)
            {
                CalendarOps = calendarOps;
                IntervalOp = intervalOp;
            }

            #region DTLocalEvaluator Members

            public abstract object Evaluate(
                object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext);

            #endregion

            #region DTLocalEvaluatorIntervalComp Members

            public abstract object Evaluate(
                object startTimestamp,
                object endTimestamp,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext);

            #endregion
        }

        #endregion

        #region Nested type: DTLocalEvaluatorCalOpsLong

        private class DTLocalEvaluatorCalOpsLong
            : DTLocalEvaluatorCalOpsCalBase
            , DTLocalEvaluator
        {
            private readonly TimeZoneInfo _timeZone;

            internal DTLocalEvaluatorCalOpsLong(IList<CalendarOp> calendarOps, TimeZoneInfo timeZone)
                : base(calendarOps)
            {
                _timeZone = timeZone;
            }

            #region DTLocalEvaluator Members

            public Object Evaluate(
                Object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var longValue = (long?) target;
                var dateTime = new DateTimeEx(longValue.Value.TimeFromMillis(_timeZone), _timeZone);
                EvaluateCalOps(CalendarOps, dateTime, eventsPerStream, isNewData, exprEvaluatorContext);
                return dateTime.TimeInMillis;
            }

            #endregion
        }

        #endregion

        #region Nested type: DTLocalEvaluatorCalopReformatBase

        private abstract class DTLocalEvaluatorCalopReformatBase : DTLocalEvaluator
        {
            protected readonly IList<CalendarOp> CalendarOps;
            protected readonly ReformatOp ReformatOp;

            protected DTLocalEvaluatorCalopReformatBase(
                IList<CalendarOp> calendarOps,
                ReformatOp reformatOp)
            {
                CalendarOps = calendarOps;
                ReformatOp = reformatOp;
            }

            #region DTLocalEvaluator Members

            public abstract object Evaluate(
                object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext);

            #endregion
        }

        #endregion

        #region Nested type: DTLocalEvaluatorDateTimeInterval

        private class DTLocalEvaluatorDateTimeInterval : DTLocalEvaluatorIntervalBase
        {
            internal DTLocalEvaluatorDateTimeInterval(IntervalOp intervalOp)
                : base(intervalOp)
            {
            }

            public override Object Evaluate(
                Object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var time = target.AsDateTimeOffset().TimeInMillis();
                return IntervalOp.Evaluate(time, time, eventsPerStream, isNewData, exprEvaluatorContext);
            }

            public override Object Evaluate(
                Object startTimestamp,
                Object endTimestamp,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var start = startTimestamp.AsDateTimeOffset().TimeInMillis();
                var end = endTimestamp.AsDateTimeOffset().TimeInMillis();
                return IntervalOp.Evaluate(start, end, eventsPerStream, isNewData, exprEvaluatorContext);
            }
        }

        #endregion

        #region Nested type: DTLocalEvaluatorDateTimeOpsInterval

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

            public override Object Evaluate(
                Object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var dateTime = new DateTimeEx(target.AsDateTimeOffset(_timeZone), _timeZone);
                EvaluateCalOps(CalendarOps, dateTime, eventsPerStream, isNewData, exprEvaluatorContext);
                var time = dateTime.TimeInMillis;
                return IntervalOp.Evaluate(time, time, eventsPerStream, isNewData, exprEvaluatorContext);
            }

            public override Object Evaluate(
                Object startTimestamp,
                Object endTimestamp,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var startLong = startTimestamp.AsDateTimeOffset(_timeZone).TimeInMillis();
                var endLong = startTimestamp.AsDateTimeOffset(_timeZone).TimeInMillis();
                var dateTime = new DateTimeEx(startLong.TimeFromMillis(_timeZone), _timeZone);
                EvaluateCalOps(CalendarOps, dateTime, eventsPerStream, isNewData, exprEvaluatorContext);
                var startTime = dateTime.TimeInMillis;
                var endTime = startTime + (endLong - startLong);
                return IntervalOp.Evaluate(startTime, endTime, eventsPerStream, isNewData, exprEvaluatorContext);
            }
        }

        #endregion

        #region Nested type: DTLocalEvaluatorDateTimeOpsReformat

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

            public override Object Evaluate(
                Object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var dateTime = new DateTimeEx(target.AsDateTimeOffset(_timeZone), _timeZone);
                EvaluateCalOps(CalendarOps, dateTime, eventsPerStream, isNewData, exprEvaluatorContext);
                return ReformatOp.Evaluate(dateTime.DateTime, eventsPerStream, isNewData, exprEvaluatorContext);
            }
        }

        #endregion

        #region Nested type: DTLocalEvaluatorDateTimeReformat

        private class DTLocalEvaluatorDateTimeReformat : DTLocalEvaluatorReformatBase
        {
            internal DTLocalEvaluatorDateTimeReformat(ReformatOp reformatOp)
                : base(reformatOp)
            {
            }

            public override Object Evaluate(
                Object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                 return ReformatOp.Evaluate((DateTimeOffset) target, eventsPerStream, isNewData, exprEvaluatorContext);
            }
        }

        #endregion

        #region Nested type: DTLocalEvaluatorIntervalBase

        private abstract class DTLocalEvaluatorIntervalBase
            : DTLocalEvaluator
            , DTLocalEvaluatorIntervalComp
        {
            protected readonly IntervalOp IntervalOp;

            protected DTLocalEvaluatorIntervalBase(IntervalOp intervalOp)
            {
                IntervalOp = intervalOp;
            }

            #region DTLocalEvaluator Members

            public abstract object Evaluate(
                object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext);

            #endregion

            #region DTLocalEvaluatorIntervalComp Members

            public abstract object Evaluate(
                object startTimestamp,
                object endTimestamp,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext);

            #endregion
        }

        #endregion

        #region Nested type: DTLocalEvaluatorIntervalComp

        /// <summary>
        /// Interval methods.
        /// </summary>
        private interface DTLocalEvaluatorIntervalComp
        {
            /// <summary>
            /// Evaluates the specified start timestamp.
            /// </summary>
            /// <param name="startTimestamp">The start timestamp.</param>
            /// <param name="endTimestamp">The end timestamp.</param>
            /// <param name="eventsPerStream">The events per stream.</param>
            /// <param name="isNewData">if set to <c>true</c> [is new data].</param>
            /// <param name="exprEvaluatorContext">The expr evaluator context.</param>
            /// <returns></returns>
            Object Evaluate(
                Object startTimestamp,
                Object endTimestamp,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext);
        }

        #endregion

        #region Nested type: DTLocalEvaluatorLongInterval

        private class DTLocalEvaluatorLongInterval : DTLocalEvaluatorIntervalBase
        {
            internal DTLocalEvaluatorLongInterval(IntervalOp intervalOp)
                : base(intervalOp)
            {
            }

            public override Object Evaluate(
                Object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var time = target.AsLong();
                return IntervalOp.Evaluate(time, time, eventsPerStream, isNewData, exprEvaluatorContext);
            }

            public override Object Evaluate(
                Object startTimestamp,
                Object endTimestamp,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var startTime = startTimestamp.AsLong();
                var endTime = endTimestamp.AsLong();
                return IntervalOp.Evaluate(startTime, endTime, eventsPerStream, isNewData, exprEvaluatorContext);
            }
        }

        #endregion

        #region Nested type: DTLocalEvaluatorLongOpsInterval

        private class DTLocalEvaluatorLongOpsInterval : DTLocalEvaluatorCalOpsIntervalBase
        {
            private readonly TimeZoneInfo _timeZone;

            internal DTLocalEvaluatorLongOpsInterval(
                IList<CalendarOp> calendarOps,
                IntervalOp intervalOp,
                TimeZoneInfo timeZone)
                : base(calendarOps, intervalOp)
            {
                _timeZone = timeZone;
            }

            public override Object Evaluate(
                Object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var dateTime = new DateTimeEx(target.AsLong().TimeFromMillis(_timeZone), _timeZone);
                EvaluateCalOps(CalendarOps, dateTime, eventsPerStream, isNewData, exprEvaluatorContext);
                var time = dateTime.TimeInMillis;
                return IntervalOp.Evaluate(time, time, eventsPerStream, isNewData, exprEvaluatorContext);
            }

            public override Object Evaluate(
                Object startTimestamp,
                Object endTimestamp,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var startLong = startTimestamp.AsLong();
                var endLong = endTimestamp.AsLong();
                var dateTime = new DateTimeEx(startLong.TimeFromMillis(_timeZone), _timeZone);
                EvaluateCalOps(CalendarOps, dateTime, eventsPerStream, isNewData, exprEvaluatorContext);
                var startTime = dateTime.TimeInMillis;
                var endTime = startTime + (endLong - startLong);
                return IntervalOp.Evaluate(startTime, endTime, eventsPerStream, isNewData, exprEvaluatorContext);
            }
        }

        #endregion

        #region Nested type: DTLocalEvaluatorLongOpsReformat

        private class DTLocalEvaluatorLongOpsReformat : DTLocalEvaluatorCalopReformatBase
        {
            private readonly TimeZoneInfo _timeZone;

            internal DTLocalEvaluatorLongOpsReformat(
                IList<CalendarOp> calendarOps,
                ReformatOp reformatOp,
                TimeZoneInfo timeZone)
                : base(calendarOps, reformatOp)
            {
                _timeZone = timeZone;
            }

            public override Object Evaluate(
                Object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var dateTime = new DateTimeEx(target.AsLong().TimeFromMillis(_timeZone), _timeZone);
                EvaluateCalOps(CalendarOps, dateTime, eventsPerStream, isNewData, exprEvaluatorContext);
                return ReformatOp.Evaluate(dateTime.DateTime, eventsPerStream, isNewData, exprEvaluatorContext);
            }
        }

        #endregion

        #region Nested type: DTLocalEvaluatorLongReformat

        private class DTLocalEvaluatorLongReformat : DTLocalEvaluatorReformatBase
        {
            internal DTLocalEvaluatorLongReformat(ReformatOp reformatOp)
                : base(reformatOp)
            {
            }

            public override Object Evaluate(
                Object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                return ReformatOp.Evaluate(target.AsLong(), eventsPerStream, isNewData, exprEvaluatorContext);
            }
        }

        #endregion

        #region Nested type: DTLocalEvaluatorReformatBase

        private abstract class DTLocalEvaluatorReformatBase : DTLocalEvaluator
        {
            protected readonly ReformatOp ReformatOp;

            protected DTLocalEvaluatorReformatBase(ReformatOp reformatOp)
            {
                ReformatOp = reformatOp;
            }

            #region DTLocalEvaluator Members

            public abstract object Evaluate(
                object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext);

            #endregion
        }

        #endregion
    }
}