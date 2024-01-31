///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.datetime;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.datetime.calop
{
    public class CalendarSetForgeOp : CalendarOp
    {
        private readonly DateTimeFieldEnum _field;
        private readonly ExprEvaluator _valueExpr;

        public CalendarSetForgeOp(
            DateTimeFieldEnum field,
            ExprEvaluator valueExpr)
        {
            _field = field;
            _valueExpr = valueExpr;
        }

        public DateTimeEx Evaluate(
            DateTimeEx dateTimeEx,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var value = CalendarOpUtil.GetInt(_valueExpr, eventsPerStream, isNewData, context);
            if (value == null) {
                return dateTimeEx;
            }

            return dateTimeEx.SetFieldValue(_field, value.Value);
        }

        public DateTimeOffset Evaluate(
            DateTimeOffset dateTimeOffset,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var value = CalendarOpUtil.GetInt(_valueExpr, eventsPerStream, isNewData, context);
            if (value == null) {
                return dateTimeOffset;
            }

            return dateTimeOffset.SetFieldValue(_field, value.Value);
        }

        public DateTime Evaluate(
            DateTime dateTime,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var value = CalendarOpUtil.GetInt(_valueExpr, eventsPerStream, isNewData, context);
            if (value == null) {
                return dateTime;
            }

            return dateTime.SetFieldValue(_field, value.Value);
        }

        public static CodegenExpression CodegenCalendar(
            CalendarSetForge forge,
            CodegenExpression dateTimeEx,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var field = Constant(forge.field);
            var evaluationType = forge.valueExpr.EvaluationType;
            if (evaluationType.CanNotBeNull()) {
                var valueExpr = forge.valueExpr
                    .EvaluateCodegen(evaluationType, codegenMethodScope, exprSymbol, codegenClassScope);
                return ExprDotMethod(dateTimeEx, "SetFieldValue", field, valueExpr);
            }
            else {
                var methodNode = codegenMethodScope
                    .MakeChild(typeof(void), typeof(CalendarSetForgeOp), codegenClassScope)
                    .AddParam(typeof(DateTimeEx), "dtx");
                var valueExpr = forge.valueExpr
                    .EvaluateCodegen(evaluationType, methodNode, exprSymbol, codegenClassScope);

                methodNode.Block
                    .DeclareVar<int?>(
                        "value",
                        SimpleNumberCoercerFactory.CoercerInt.CoerceCodegenMayNull(
                            valueExpr,
                            forge.valueExpr.EvaluationType,
                            methodNode,
                            codegenClassScope))
                    .IfRefNullReturnNull("value")
                    .Expression(
                        ExprDotMethod(
                            dateTimeEx,
                            "SetFieldValue",
                            field,
                            Unbox<int?>(Ref("value"))))
                    .MethodEnd();
                return LocalMethod(methodNode, dateTimeEx);
            }
        }

        public static CodegenExpression CodegenDateTimeOffset(
            CalendarSetForge forge,
            CodegenExpression dto,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var field = forge.field;
            var methodNode = codegenMethodScope
                .MakeChild(typeof(DateTimeOffset), typeof(CalendarSetForgeOp), codegenClassScope)
                .AddParam(typeof(DateTimeOffset), "dto");
            var evaluationType = forge.valueExpr.EvaluationType;

            methodNode.Block
                .DeclareVar<int?>(
                    "value",
                    SimpleNumberCoercerFactory.CoercerInt.CoerceCodegenMayNull(
                        forge.valueExpr.EvaluateCodegen(evaluationType, methodNode, exprSymbol, codegenClassScope),
                        evaluationType,
                        methodNode,
                        codegenClassScope))
                .IfRefNull("value")
                .BlockReturn(Ref("dto"))
                .TypeReference(typeof(DateTimeOffsetHelper))
                .MethodReturn(
                    StaticMethod(
                        typeof(DateTimeFieldMath),
                        "SetFieldValue",
                        Ref("dto"),
                        EnumValue(field),
                        Unbox<int?>(Ref("value"))));
            return LocalMethod(methodNode, dto);
        }

        public static CodegenExpression CodegenDateTime(
            CalendarSetForge forge,
            CodegenExpression dateTime,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var field = forge.field;
            var methodNode = codegenMethodScope
                .MakeChild(typeof(DateTime), typeof(CalendarSetForgeOp), codegenClassScope)
                .AddParam(typeof(DateTime), "dateTime");
            var evaluationType = forge.valueExpr.EvaluationType;

            methodNode.Block
                .DeclareVar<int?>(
                    "value",
                    SimpleNumberCoercerFactory.CoercerInt.CoerceCodegenMayNull(
                        forge.valueExpr.EvaluateCodegen(evaluationType, methodNode, exprSymbol, codegenClassScope),
                        evaluationType,
                        methodNode,
                        codegenClassScope))
                .IfRefNull("value")
                .BlockReturn(Ref("dateTime"))
                .TypeReference(typeof(DateTimeHelper))
                .MethodReturn(
                    StaticMethod(
                        typeof(DateTimeFieldMath),
                        "SetFieldValue",
                        Ref("dateTime"),
                        EnumValue(field),
                        Unbox<int?>(Ref("value"))));
            return LocalMethod(methodNode, dateTime);
        }
    }
} // end of namespace