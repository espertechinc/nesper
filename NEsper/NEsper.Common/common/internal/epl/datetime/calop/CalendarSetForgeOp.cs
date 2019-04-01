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
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.datetime.calop
{
	public class CalendarSetForgeOp : CalendarOp {

	    private readonly CalendarFieldEnum fieldName;
	    private readonly ExprEvaluator valueExpr;

	    public CalendarSetForgeOp(CalendarFieldEnum fieldName, ExprEvaluator valueExpr) {
	        this.fieldName = fieldName;
	        this.valueExpr = valueExpr;
	    }

	    public void Evaluate(DateTimeEx dateTimeEx, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
	        int? value = CalendarOpUtil.GetInt(valueExpr, eventsPerStream, isNewData, context);
	        if (value == null) {
	            return;
	        }
	        dateTimeEx.Set(fieldName.CalendarField, value);
	    }

	    public static CodegenExpression CodegenCalendar(CalendarSetForge forge, CodegenExpression cal, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol, CodegenClassScope codegenClassScope) {
	        CodegenExpression calField = Constant(forge.fieldName.CalendarField);
	        Type evaluationType = forge.valueExpr.EvaluationType;
	        if (evaluationType.IsPrimitive) {
	            CodegenExpression valueExpr = forge.valueExpr.EvaluateCodegen(evaluationType, codegenMethodScope, exprSymbol, codegenClassScope);
	            return ExprDotMethod(cal, "set", calField, valueExpr);
	        }

	        CodegenMethod methodNode = codegenMethodScope.MakeChild(typeof(void), typeof(CalendarSetForgeOp), codegenClassScope).AddParam(typeof(DateTimeEx), "cal");
	        CodegenExpression valueExpr = forge.valueExpr.EvaluateCodegen(evaluationType, methodNode, exprSymbol, codegenClassScope);
	        methodNode.Block
	                .DeclareVar(typeof(int), "value", SimpleNumberCoercerFactory.SimpleNumberCoercerInt.CoerceCodegenMayNull(valueExpr, forge.valueExpr.EvaluationType, methodNode, codegenClassScope))
	                .IfRefNullReturnNull("value")
	                .Expression(ExprDotMethod(cal, "set", calField, @Ref("value")))
	                .MethodEnd();
	        return LocalMethod(methodNode, cal);
	    }

	    public DateTimeOffset Evaluate(DateTimeOffset dateTimeOffset, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
	        int? value = CalendarOpUtil.GetInt(valueExpr, eventsPerStream, isNewData, context);
	        if (value == null) {
	            return dateTimeOffset;
	        }
	        return dateTimeOffset.With(fieldName.ChronoField, value);
	    }

	    public static CodegenExpression CodegenDateTimeOffset(CalendarSetForge forge, CodegenExpression dto, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol, CodegenClassScope codegenClassScope) {
	        ChronoField chronoField = forge.fieldName.ChronoField;
	        CodegenMethod methodNode = codegenMethodScope.MakeChild(typeof(DateTimeOffset), typeof(CalendarSetForgeOp), codegenClassScope).AddParam(typeof(DateTimeOffset), "dto");
	        Type evaluationType = forge.valueExpr.EvaluationType;

	        methodNode.Block
	                .DeclareVar(typeof(int), "value", SimpleNumberCoercerFactory.SimpleNumberCoercerInt.CoerceCodegenMayNull(forge.valueExpr.EvaluateCodegen(evaluationType, methodNode, exprSymbol, codegenClassScope), evaluationType, methodNode, codegenClassScope))
	                .IfRefNull("value").BlockReturn(@Ref("dto"))
	                .MethodReturn(ExprDotMethod(@Ref("dto"), "with", EnumValue(typeof(ChronoField), chronoField.Name()), @Ref("value")));
	        return LocalMethod(methodNode, dto);
	    }

	    public DateTime Evaluate(DateTime dateTime, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
	        int? value = CalendarOpUtil.GetInt(valueExpr, eventsPerStream, isNewData, context);
	        if (value == null) {
	            return dateTime;
	        }
	        return dateTime.With(fieldName.ChronoField, value);
	    }

	    public static CodegenExpression CodegenDateTime(CalendarSetForge forge, CodegenExpression dateTime, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol, CodegenClassScope codegenClassScope) {
	        ChronoField chronoField = forge.fieldName.ChronoField;
	        CodegenMethod methodNode = codegenMethodScope.MakeChild(typeof(DateTime), typeof(CalendarSetForgeOp), codegenClassScope).AddParam(typeof(DateTime), "dateTime");
	        Type evaluationType = forge.valueExpr.EvaluationType;

	        methodNode.Block
	                .DeclareVar(typeof(int), "value", SimpleNumberCoercerFactory.SimpleNumberCoercerInt.CoerceCodegenMayNull(forge.valueExpr.EvaluateCodegen(evaluationType, methodNode, exprSymbol, codegenClassScope), evaluationType, methodNode, codegenClassScope))
	                .IfRefNull("value").BlockReturn(@Ref("dateTime"))
	                .MethodReturn(ExprDotMethod(@Ref("dateTime"), "with", EnumValue(typeof(ChronoField), chronoField.Name()), @Ref("value")));
	        return LocalMethod(methodNode, dateTime);
	    }
	}
} // end of namespace