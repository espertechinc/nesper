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
using com.espertech.esper.epl.datetime.eval.reformat;
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
                    _returnType = EPTypeHelper.SingleValue(
                        inputEventType.GetPropertyType(inputEventType.StartTimestampPropertyName));
                }
                else
                {
                    _returnType = EPTypeHelper.SingleValue(inputType);
                }
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

        internal DTLocalEvaluator GetEvaluator(
            IList<CalendarOp> calendarOps,
            TimeZoneInfo timeZone,
            TimeAbacus timeAbacus,
            Type inputType,
            EventType inputEventType,
            ReformatOp reformatOp,
            IntervalOp intervalOp)
        {
            inputType = TypeHelper.GetBoxedType(inputType);

            if (inputEventType == null)
            {
                if (reformatOp != null)
                {
                    if (TypeHelper.IsSubclassOrImplementsInterface(inputType, typeof (DateTimeEx)))
                    {
                        return calendarOps.IsEmpty()
                            ? (DTLocalEvaluator) new DTLocalEvaluatorDtxReformat(reformatOp)
                            : (DTLocalEvaluator) new DTLocalEvaluatorDtxOpsReformat(calendarOps, reformatOp);
                    }
                    else if (TypeHelper.IsSubclassOrImplementsInterface(inputType, typeof (DateTime?)))
                    {
                        return calendarOps.IsEmpty()
                            ? (DTLocalEvaluator) new DTLocalEvaluatorDateTimeReformat(reformatOp)
                            : (DTLocalEvaluator) new DTLocalEvaluatorDateTimeOpsReformat(calendarOps, reformatOp, timeZone);
                    }
                    else if (TypeHelper.IsSubclassOrImplementsInterface(inputType, typeof (DateTimeOffset?)))
                    {
                        return calendarOps.IsEmpty()
                            ? (DTLocalEvaluator) new DTLocalEvaluatorDtoReformat(reformatOp)
                            : (DTLocalEvaluator) new DTLocalEvaluatorDtoOpsReformat(calendarOps, reformatOp, timeZone);
                    }
                    else if (inputType == typeof(long?))
                    {
                        return calendarOps.IsEmpty()
                            ? (DTLocalEvaluator) new DTLocalEvaluatorLongReformat(reformatOp)
                            : (DTLocalEvaluator) new DTLocalEvaluatorLongOpsReformat(calendarOps, reformatOp, timeZone, timeAbacus);
                    }
                }
                else if (intervalOp != null)
                {
                    if (TypeHelper.IsSubclassOrImplementsInterface(inputType, typeof (DateTimeEx)))
                    {
                        return calendarOps.IsEmpty()
                            ? (DTLocalEvaluator) new DTLocalEvaluatorDtxInterval(intervalOp)
                            : (DTLocalEvaluator) new DTLocalEvaluatorDtxOpsInterval(calendarOps, intervalOp, timeZone);
                    }
                    else if (TypeHelper.IsSubclassOrImplementsInterface(inputType, typeof (DateTime?)))
                    {
                        return calendarOps.IsEmpty()
                            ? (DTLocalEvaluator) new DTLocalEvaluatorDateTimeInterval(intervalOp)
                            : (DTLocalEvaluator) new DTLocalEvaluatorDateTimeOpsInterval(calendarOps, intervalOp, timeZone);
                    }
                    else if (TypeHelper.IsSubclassOrImplementsInterface(inputType, typeof (DateTimeOffset?)))
                    {
                        return calendarOps.IsEmpty()
                            ? (DTLocalEvaluator) new DTLocalEvaluatorDtoInterval(intervalOp)
                            : (DTLocalEvaluator) new DTLocalEvaluatorDtoOpsInterval(calendarOps, intervalOp, timeZone);
                    }
                    else if (inputType == typeof(long?))
                    {
                        return calendarOps.IsEmpty()
                            ? (DTLocalEvaluator) new DTLocalEvaluatorLongInterval(intervalOp)
                            : (DTLocalEvaluator) new DTLocalEvaluatorLongOpsInterval(calendarOps, intervalOp, timeZone, timeAbacus);
                    }
                }
                else
                {
                    // only calendar ops, nothing else
                    if (TypeHelper.IsSubclassOrImplementsInterface(inputType, typeof (DateTimeEx)))
                    {
                        return new DTLocalEvaluatorDtxOpsDtx(calendarOps);
                    }
                    else if (TypeHelper.IsSubclassOrImplementsInterface(inputType, typeof (DateTime?)))
                    {
                        return new DTLocalEvaluatorDtxOpsDateTime(calendarOps, timeZone);
                    }
                    else if (TypeHelper.IsSubclassOrImplementsInterface(inputType, typeof (DateTimeOffset?)))
                    {
                        return new DTLocalEvaluatorDtxOpsDateTimeOffset(calendarOps, timeZone);
                    }
                    else if (inputType == typeof(long?))
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

        public object Evaluate(object target, EvaluateParams evaluateParams)
        {
            if (target == null)
            {
                return null;
            }
            return _evaluator.Evaluate(target, evaluateParams);
        }
    }
} // end of namespace
