///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.datetime.calop;
using com.espertech.esper.common.@internal.epl.datetime.eval;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.epl.join.analyze;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat;
using com.espertech.esper.compat.datetime;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.datetime.reformatop
{
    public class ReformatGetFieldForge : ReformatForge,
        ReformatOp
    {
        private readonly DateTimeFieldEnum _fieldNum;
        private readonly TimeAbacus _timeAbacus;

        public ReformatGetFieldForge(
            DateTimeFieldEnum fieldNum,
            TimeAbacus timeAbacus)
        {
            this._fieldNum = fieldNum;
            this._timeAbacus = timeAbacus;
        }

        public ReformatOp Op => this;

        public CodegenExpression CodegenLong(
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(typeof(int), typeof(ReformatGetFieldForge), codegenClassScope)
                .AddParam(typeof(long), "ts");
            var timeZoneField =
                codegenClassScope.AddOrGetDefaultFieldSharable(RuntimeSettingsTimeZoneField.INSTANCE);
            methodNode.Block
                .DeclareVar<DateTimeEx>("dateTime", StaticMethod(typeof(DateTimeEx), "GetInstance", timeZoneField))
                .Expression(_timeAbacus.DateTimeSetCodegen(Ref("ts"), Ref("dateTime"), methodNode, codegenClassScope))
                .MethodReturn(CodegenGet(Ref("dateTime")));
            return LocalMethod(methodNode, inner);
        }

        public CodegenExpression CodegenDateTimeEx(
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return CodegenGet(inner);
        }

        public CodegenExpression CodegenDateTimeOffset(
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return CodegenGet(inner);
        }

        public CodegenExpression CodegenDateTime(
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return CodegenGet(inner);
        }

        public Type ReturnType => typeof(int?);

        public FilterExprAnalyzerAffector GetFilterDesc(
            EventType[] typesPerStream,
            DateTimeMethodEnum currentMethod,
            IList<ExprNode> currentParameters,
            ExprDotNodeFilterAnalyzerInput inputDesc)
        {
            return null;
        }

        public object Evaluate(
            long ts,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var dateTime = DateTimeEx.NowUtc();
            _timeAbacus.DateTimeSet(ts, dateTime);
            return GetValueUsingFieldEnum(dateTime, _fieldNum);
        }

        public object Evaluate(
            DateTime dateTime,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return GetValueUsingFieldEnum(dateTime, _fieldNum);
        }

        public object Evaluate(
            DateTimeEx dateTimeEx,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return GetValueUsingFieldEnum(dateTimeEx, _fieldNum);
        }

        public object Evaluate(
            DateTimeOffset dateTimeOffset,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return GetValueUsingFieldEnum(dateTimeOffset, _fieldNum);
        }

        private CodegenExpression CodegenGet(CodegenExpression dateTime)
        {
            return StaticMethod(
                typeof(ReformatGetFieldForge),
                "GetValueUsingFieldEnum",
                dateTime,
                Constant(_fieldNum));
        }

        public static int GetValueUsingFieldEnum(
            DateTime dateTime,
            DateTimeFieldEnum fieldEnum)
        {
            return GetValueUsingFieldEnum(DateTimeEx.UtcInstance(dateTime), fieldEnum);
        }

        public static int GetValueUsingFieldEnum(
            DateTimeEx dateTime,
            DateTimeFieldEnum fieldEnum)
        {
            switch (fieldEnum) {
                case DateTimeFieldEnum.YEAR:
                    return dateTime.Year;

                case DateTimeFieldEnum.MONTH:
                    return dateTime.Month;

                case DateTimeFieldEnum.DAY:
                    return dateTime.Day;

                case DateTimeFieldEnum.HOUR:
                    return dateTime.Hour;

                case DateTimeFieldEnum.MINUTE:
                    return dateTime.Minute;

                case DateTimeFieldEnum.SECOND:
                    return dateTime.Second;

                case DateTimeFieldEnum.MILLISEC:
                    return dateTime.Millisecond;

                case DateTimeFieldEnum.WEEK:
                    return dateTime.WeekOfYear;
            }

            throw new ArgumentException("unknown field", nameof(fieldEnum));
        }

        public static int GetValueUsingFieldEnum(
            DateTimeOffset dateTime,
            DateTimeFieldEnum fieldEnum)
        {
            switch (fieldEnum) {
                case DateTimeFieldEnum.YEAR:
                    return dateTime.Year;

                case DateTimeFieldEnum.MONTH:
                    return dateTime.Month;

                case DateTimeFieldEnum.DAY:
                    return dateTime.Day;

                case DateTimeFieldEnum.HOUR:
                    return dateTime.Hour;

                case DateTimeFieldEnum.MINUTE:
                    return dateTime.Minute;

                case DateTimeFieldEnum.SECOND:
                    return dateTime.Second;

                case DateTimeFieldEnum.MILLISEC:
                    return dateTime.Millisecond;

                case DateTimeFieldEnum.WEEK:
                    return DateTimeMath.GetWeekOfYear(dateTime);
            }

            throw new ArgumentOutOfRangeException(nameof(fieldEnum), fieldEnum, "unknown field");
        }
    }
} // end of namespace