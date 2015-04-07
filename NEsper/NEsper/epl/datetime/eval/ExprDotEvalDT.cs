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
using com.espertech.esper.epl.expression;
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
            ReformatOp reformatOp,
            IntervalOp intervalOp,
            Type inputType,
            EventType inputEventType)
        {
            _evaluator = GetEvaluator(calendarOps, inputType, inputEventType, reformatOp, intervalOp);

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
                            return new DTLocalEvaluatorDateReformat(reformatOp);
                        }
                        return new DTLocalEvaluatorDateOpsReformat(calendarOps, reformatOp);
                    }
                    if (inputType.GetBoxedType() == typeof (long?))
                    {
                        if (calendarOps.IsEmpty())
                        {
                            return new DTLocalEvaluatorLongReformat(reformatOp);
                        }
                        return new DTLocalEvaluatorLongOpsReformat(calendarOps, reformatOp);
                    }
                }
                else if (intervalOp != null)
                {
                    if (inputType.GetBoxedType() == typeof (DateTime?))
                    {
                        if (calendarOps.IsEmpty())
                        {
                            return new DTLocalEvaluatorDateInterval(intervalOp);
                        }
                        return new DTLocalEvaluatorDateOpsInterval(calendarOps, intervalOp);
                    }
                    if (inputType.GetBoxedType() == typeof (long?))
                    {
                        if (calendarOps.IsEmpty())
                        {
                            return new DTLocalEvaluatorLongInterval(intervalOp);
                        }
                        return new DTLocalEvaluatorLongOpsInterval(calendarOps, intervalOp);
                    }
                }
                else
                {
                    // only calendar ops, nothing else
                    if (inputType.GetBoxedType() == typeof (DateTime?))
                    {
                        return new DTLocalEvaluatorCalOpsDate(calendarOps);
                    }
                    if (inputType.GetBoxedType() == typeof (long?))
                    {
                        return new DTLocalEvaluatorCalOpsLong(calendarOps);
                    }
                }
                throw new ArgumentException("Invalid input type '" + inputType + "'");
            }

            EventPropertyGetter getter = inputEventType.GetGetter(inputEventType.StartTimestampPropertyName);
            Type getterResultType = inputEventType.GetPropertyType(inputEventType.StartTimestampPropertyName);

            if (reformatOp != null)
            {
                DTLocalEvaluator inner = GetEvaluator(calendarOps, getterResultType, null, reformatOp, null);
                return new DTLocalEvaluatorBeanReformat(getter, inner);
            }
            if (intervalOp == null)
            {
                // only calendar ops
                DTLocalEvaluator inner = GetEvaluator(calendarOps, getterResultType, null, null, null);
                return new DTLocalEvaluatorBeanCalOps(getter, inner);
            }

            // have interval ops but no end timestamp
            if (inputEventType.EndTimestampPropertyName == null)
            {
                DTLocalEvaluator inner = GetEvaluator(calendarOps, getterResultType, null, null, intervalOp);
                return new DTLocalEvaluatorBeanIntervalNoEndTS(getter, inner);
            }

            // interval ops and have end timestamp
            EventPropertyGetter getterEndTimestamp = inputEventType.GetGetter(inputEventType.EndTimestampPropertyName);
            var intervalComp =
                (DTLocalEvaluatorIntervalComp) GetEvaluator(calendarOps, getterResultType, null, null, intervalOp);
            return new DTLocalEvaluatorBeanIntervalWithEnd(getter, getterEndTimestamp, intervalComp);
        }

        public static void EvaluateCalOps(
            IEnumerable<CalendarOp> calendarOps,
            ref DateTime dateTime,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            foreach (CalendarOp calendarOp in calendarOps)
            {
                calendarOp.Evaluate(ref dateTime, eventsPerStream, isNewData, exprEvaluatorContext);
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
                Object timestamp = _getter.Get((EventBean) target);
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
                Object timestamp = _getter.Get((EventBean) target);
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
                Object startTimestamp = _getterStartTimestamp.Get((EventBean) target);
                if (startTimestamp == null)
                {
                    return null;
                }
                Object endTimestamp = _getterEndTimestamp.Get((EventBean) target);
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
                Object timestamp = _getter.Get((EventBean) target);
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

        #region Nested type: DTLocalEvaluatorCalOpsDate

        private class DTLocalEvaluatorCalOpsDate
            : DTLocalEvaluatorCalOpsCalBase
            , DTLocalEvaluator
        {
            internal DTLocalEvaluatorCalOpsDate(IList<CalendarOp> calendarOps)
                : base(calendarOps)
            {
            }

            #region DTLocalEvaluator Members

            public Object Evaluate(
                Object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var dateValue = (DateTime) target;
                EvaluateCalOps(CalendarOps, ref dateValue, eventsPerStream, isNewData, exprEvaluatorContext);
                return dateValue;
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
            internal DTLocalEvaluatorCalOpsLong(IList<CalendarOp> calendarOps)
                : base(calendarOps)
            {
            }

            #region DTLocalEvaluator Members

            public Object Evaluate(
                Object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var longValue = (long?) target;
                DateTime dateTime = longValue.Value.TimeFromMillis();
                EvaluateCalOps(CalendarOps, ref dateTime, eventsPerStream, isNewData, exprEvaluatorContext);
                return dateTime.TimeInMillis();
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

        #region Nested type: DTLocalEvaluatorDateInterval

        private class DTLocalEvaluatorDateInterval : DTLocalEvaluatorIntervalBase
        {
            internal DTLocalEvaluatorDateInterval(IntervalOp intervalOp)
                : base(intervalOp)
            {
            }

            public override Object Evaluate(
                Object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                long time = ((DateTime) target).TimeInMillis();
                return IntervalOp.Evaluate(time, time, eventsPerStream, isNewData, exprEvaluatorContext);
            }

            public override Object Evaluate(
                Object startTimestamp,
                Object endTimestamp,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                long start = ((DateTime) startTimestamp).TimeInMillis();
                long end = ((DateTime) endTimestamp).TimeInMillis();
                return IntervalOp.Evaluate(start, end, eventsPerStream, isNewData, exprEvaluatorContext);
            }
        }

        #endregion

        #region Nested type: DTLocalEvaluatorDateOpsInterval

        private class DTLocalEvaluatorDateOpsInterval : DTLocalEvaluatorCalOpsIntervalBase
        {
            internal DTLocalEvaluatorDateOpsInterval(
                IList<CalendarOp> calendarOps,
                IntervalOp intervalOp)
                : base(calendarOps, intervalOp)
            {
            }

            public override Object Evaluate(
                Object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var dateTime = (DateTime) target;
                EvaluateCalOps(CalendarOps, ref dateTime, eventsPerStream, isNewData, exprEvaluatorContext);
                long time = dateTime.TimeInMillis();
                return IntervalOp.Evaluate(time, time, eventsPerStream, isNewData, exprEvaluatorContext);
            }

            public override Object Evaluate(
                Object startTimestamp,
                Object endTimestamp,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                long startLong = ((DateTime) startTimestamp).TimeInMillis();
                long endLong = ((DateTime) endTimestamp).TimeInMillis();
                DateTime dateTime = startLong.TimeFromMillis();
                EvaluateCalOps(CalendarOps, ref dateTime, eventsPerStream, isNewData, exprEvaluatorContext);
                long startTime = dateTime.TimeInMillis();
                long endTime = startTime + (endLong - startLong);
                return IntervalOp.Evaluate(startTime, endTime, eventsPerStream, isNewData, exprEvaluatorContext);
            }
        }

        #endregion

        #region Nested type: DTLocalEvaluatorDateOpsReformat

        private class DTLocalEvaluatorDateOpsReformat : DTLocalEvaluatorCalopReformatBase
        {
            internal DTLocalEvaluatorDateOpsReformat(
                IList<CalendarOp> calendarOps,
                ReformatOp reformatOp)
                : base(calendarOps, reformatOp)
            {
            }

            public override Object Evaluate(
                Object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var dateTime = (DateTime) target;
                EvaluateCalOps(CalendarOps, ref dateTime, eventsPerStream, isNewData, exprEvaluatorContext);
                return ReformatOp.Evaluate(dateTime, eventsPerStream, isNewData, exprEvaluatorContext);
            }
        }

        #endregion

        #region Nested type: DTLocalEvaluatorDateReformat

        private class DTLocalEvaluatorDateReformat : DTLocalEvaluatorReformatBase
        {
            internal DTLocalEvaluatorDateReformat(ReformatOp reformatOp)
                : base(reformatOp)
            {
            }

            public override Object Evaluate(
                Object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                return ReformatOp.Evaluate((DateTime) target, eventsPerStream, isNewData, exprEvaluatorContext);
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
                long time = target.AsLong();
                return IntervalOp.Evaluate(time, time, eventsPerStream, isNewData, exprEvaluatorContext);
            }

            public override Object Evaluate(
                Object startTimestamp,
                Object endTimestamp,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                long startTime = startTimestamp.AsLong();
                long endTime = endTimestamp.AsLong();
                return IntervalOp.Evaluate(startTime, endTime, eventsPerStream, isNewData, exprEvaluatorContext);
            }
        }

        #endregion

        #region Nested type: DTLocalEvaluatorLongOpsInterval

        private class DTLocalEvaluatorLongOpsInterval : DTLocalEvaluatorCalOpsIntervalBase
        {
            internal DTLocalEvaluatorLongOpsInterval(
                IList<CalendarOp> calendarOps,
                IntervalOp intervalOp)
                : base(calendarOps, intervalOp)
            {
            }

            public override Object Evaluate(
                Object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                DateTime dateTime = target.AsLong().TimeFromMillis();
                EvaluateCalOps(CalendarOps, ref dateTime, eventsPerStream, isNewData, exprEvaluatorContext);
                long time = dateTime.TimeInMillis();
                return IntervalOp.Evaluate(time, time, eventsPerStream, isNewData, exprEvaluatorContext);
            }

            public override Object Evaluate(
                Object startTimestamp,
                Object endTimestamp,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                long startLong = startTimestamp.AsLong();
                long endLong = endTimestamp.AsLong();
                DateTime dateTime = startLong.TimeFromMillis();
                EvaluateCalOps(CalendarOps, ref dateTime, eventsPerStream, isNewData, exprEvaluatorContext);
                long startTime = dateTime.TimeInMillis();
                long endTime = startTime + (endLong - startLong);
                return IntervalOp.Evaluate(startTime, endTime, eventsPerStream, isNewData, exprEvaluatorContext);
            }
        }

        #endregion

        #region Nested type: DTLocalEvaluatorLongOpsReformat

        private class DTLocalEvaluatorLongOpsReformat : DTLocalEvaluatorCalopReformatBase
        {
            internal DTLocalEvaluatorLongOpsReformat(
                IList<CalendarOp> calendarOps,
                ReformatOp reformatOp)
                : base(calendarOps, reformatOp)
            {
            }

            public override Object Evaluate(
                Object target,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var dateTime = target.AsLong().TimeFromMillis();
                EvaluateCalOps(CalendarOps, ref dateTime, eventsPerStream, isNewData, exprEvaluatorContext);
                return ReformatOp.Evaluate(dateTime, eventsPerStream, isNewData, exprEvaluatorContext);
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