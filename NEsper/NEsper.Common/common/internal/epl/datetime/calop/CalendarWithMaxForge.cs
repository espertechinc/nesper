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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.datetime.calop
{
	public class CalendarWithMaxForge : CalendarForge, CalendarOp {

	    private readonly CalendarFieldEnum fieldName;

	    public CalendarWithMaxForge(CalendarFieldEnum fieldName) {
	        this.fieldName = fieldName;
	    }

	    public CalendarOp EvalOp {
	        get => this;
	    }

	    public CodegenExpression CodegenDateTimeEx(CodegenExpression dateTimeEx, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol, CodegenClassScope codegenClassScope) {
	        return ExprDotMethod(dateTimeEx, "set", Constant(fieldName.CalendarField), ExprDotMethod(dateTimeEx, "getActualMaximum", Constant(fieldName.CalendarField)));
	    }

	    public void Evaluate(DateTimeEx dateTimeEx, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
	        dateTimeEx.Set(fieldName.CalendarField, dateTimeEx.GetActualMaximum(fieldName.CalendarField));
	    }

	    public DateTimeOffset Evaluate(DateTimeOffset dateTimeOffset, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
	        ValueRange range = dateTimeOffset.Range(fieldName.ChronoField);
	        return dateTimeOffset.With(fieldName.ChronoField, range.Maximum);
	    }

	    public CodegenExpression CodegenDateTimeOffset(CodegenExpression dateTimeOffset, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol, CodegenClassScope codegenClassScope) {
	        return CodegenDateTimeOffsetZDTMinMax(dateTimeOffset, true, fieldName);
	    }

	    public DateTime Evaluate(DateTime dateTime, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
	        ValueRange range = dateTime.Range(fieldName.ChronoField);
	        return dateTime.With(fieldName.ChronoField, range.Maximum);
	    }

	    public CodegenExpression CodegenDateTime(CodegenExpression dateTime, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol, CodegenClassScope codegenClassScope) {
	        return CodegenDateTimeOffsetZDTMinMax(dateTime, true, fieldName);
	    }

	    protected internal static CodegenExpression CodegenDateTimeOffsetZDTMinMax(CodegenExpression val, bool max, CalendarFieldEnum fieldName) {
	        CodegenExpression chronoField = EnumValue(typeof(ChronoField), fieldName.ChronoField.Name());
	        CodegenExpression valueRange = ExprDotMethod(val, "range", chronoField);
	        return ExprDotMethod(val, "with", chronoField, ExprDotMethod(valueRange, max ? "getMaximum" : "getMinimum"));
	    }
	}
} // end of namespace