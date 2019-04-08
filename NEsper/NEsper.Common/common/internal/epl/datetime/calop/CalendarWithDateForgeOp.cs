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
	public class CalendarWithDateForgeOp : CalendarOp {
	    public const string METHOD_ACTIONSETYMDCALENDAR = "actionSetYMDCalendar";

	    private ExprEvaluator year;
	    private ExprEvaluator month;
	    private ExprEvaluator day;

	    public CalendarWithDateForgeOp(ExprEvaluator year, ExprEvaluator month, ExprEvaluator day) {
	        this.year = year;
	        this.month = month;
	        this.day = day;
	    }

	    public void Evaluate(DateTimeEx dateTimeEx, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
	        int? yearNum = GetInt(year, eventsPerStream, isNewData, context);
	        int? monthNum = GetInt(month, eventsPerStream, isNewData, context);
	        int? dayNum = GetInt(day, eventsPerStream, isNewData, context);
	        ActionSetYMDCalendar(dateTimeEx, yearNum, monthNum, dayNum);
	    }

	    public static CodegenExpression CodegenCalendar(CalendarWithDateForge forge, CodegenExpression cal, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol, CodegenClassScope codegenClassScope) {
	        CodegenMethod methodNode = codegenMethodScope.MakeChild(typeof(void), typeof(CalendarWithDateForgeOp), codegenClassScope).AddParam(typeof(DateTimeEx), "value");

	        CodegenBlock block = methodNode.Block;
	        CodegenDeclareInts(block, forge, methodNode, exprSymbol, codegenClassScope);
	        block.StaticMethod(typeof(CalendarWithDateForgeOp), METHOD_ACTIONSETYMDCALENDAR, @Ref("value"), @Ref("year"), @Ref("month"), @Ref("day"))
	                .MethodEnd();
	        return LocalMethod(methodNode, cal);
	    }

	    public DateTimeOffset Evaluate(DateTimeOffset dateTimeOffset, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
	        int? yearNum = GetInt(year, eventsPerStream, isNewData, context);
	        int? monthNum = GetInt(month, eventsPerStream, isNewData, context);
	        int? dayNum = GetInt(day, eventsPerStream, isNewData, context);
	        return ActionSetYMDLocalDateTime(dateTimeOffset, yearNum, monthNum, dayNum);
	    }

	    public static CodegenExpression CodegenDateTimeOffset(CalendarWithDateForge forge, CodegenExpression dto, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol, CodegenClassScope codegenClassScope) {
	        CodegenMethod methodNode = codegenMethodScope.MakeChild(typeof(DateTimeOffset), typeof(CalendarWithDateForgeOp), codegenClassScope).AddParam(typeof(DateTimeOffset), "value");

	        CodegenBlock block = methodNode.Block;
	        CodegenDeclareInts(block, forge, methodNode, exprSymbol, codegenClassScope);
	        block.MethodReturn(StaticMethod(typeof(CalendarWithDateForgeOp), "actionSetYMDLocalDateTime", @Ref("value"), @Ref("year"), @Ref("month"), @Ref("day")));
	        return LocalMethod(methodNode, dto);
	    }

	    public DateTime Evaluate(DateTime dateTime, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
	        int? yearNum = GetInt(year, eventsPerStream, isNewData, context);
	        int? monthNum = GetInt(month, eventsPerStream, isNewData, context);
	        int? dayNum = GetInt(day, eventsPerStream, isNewData, context);
	        return ActionSetYMDZonedDateTime(dateTime, yearNum, monthNum, dayNum);
	    }

	    public static CodegenExpression CodegenDateTime(CalendarWithDateForge forge, CodegenExpression dateTime, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol, CodegenClassScope codegenClassScope) {
	        CodegenMethod methodNode = codegenMethodScope.MakeChild(typeof(DateTime), typeof(CalendarWithDateForgeOp), codegenClassScope).AddParam(typeof(DateTime), "value");

	        CodegenBlock block = methodNode.Block;
	        CodegenDeclareInts(block, forge, methodNode, exprSymbol, codegenClassScope);
	        block.MethodReturn(StaticMethod(typeof(CalendarWithDateForgeOp), "actionSetYMDZonedDateTime", @Ref("value"), @Ref("year"), @Ref("month"), @Ref("day")));
	        return LocalMethod(methodNode, dateTime);
	    }

	    protected internal static int? GetInt(ExprEvaluator expr, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
	        object result = expr.Evaluate(eventsPerStream, isNewData, context);
	        if (result == null) {
	            return null;
	        }
	        return (int?) result;
	    }

	    /// <summary>
	    /// NOTE: Code-generation-invoked method, method name and parameter order matters
	    /// </summary>
	    /// <param name="cal">calendar</param>
	    /// <param name="year">year</param>
	    /// <param name="month">month</param>
	    /// <param name="day">day</param>
	    public static void ActionSetYMDCalendar(DateTimeEx cal, int? year, int? month, int? day) {
	        if (year != null) {
	            cal.Set(DateTimeEx.YEAR, year);
	        }
	        if (month != null) {
	            cal.Set(DateTimeEx.MONTH, month);
	        }
	        if (day != null) {
	            cal.Set(DateTimeEx.DATE, day);
	        }
	    }

	    /// <summary>
	    /// NOTE: Code-generation-invoked method, method name and parameter order matters
	    /// </summary>
	    /// <param name="dto">localdatetime</param>
	    /// <param name="year">year</param>
	    /// <param name="month">month</param>
	    /// <param name="day">day</param>
	    /// <returns>dto</returns>
	    public static DateTimeOffset ActionSetYMDLocalDateTime(DateTimeOffset dto, int? year, int? month, int? day) {
	        if (year != null) {
	            dto = dto.WithYear(year);
	        }
	        if (month != null) {
	            dto = dto.WithMonth(month);
	        }
	        if (day != null) {
	            dto = dto.WithDayOfMonth(day);
	        }
	        return dto;
	    }

	    /// <summary>
	    /// NOTE: Code-generation-invoked method, method name and parameter order matters
	    /// </summary>
	    /// <param name="dateTime">zoneddatetime</param>
	    /// <param name="year">year</param>
	    /// <param name="month">month</param>
	    /// <param name="day">day</param>
	    /// <returns>dto</returns>
	    public static DateTime ActionSetYMDZonedDateTime(DateTime dateTime, int? year, int? month, int? day) {
	        if (year != null) {
	            dateTime = dateTime.WithYear(year);
	        }
	        if (month != null) {
	            dateTime = dateTime.WithMonth(month);
	        }
	        if (day != null) {
	            dateTime = dateTime.WithDayOfMonth(day);
	        }
	        return dateTime;
	    }

	    private static void CodegenDeclareInts(CodegenBlock block, CalendarWithDateForge forge, CodegenMethod methodNode, ExprForgeCodegenSymbol exprSymbol, CodegenClassScope codegenClassScope) {
	        Type yearType = forge.year.EvaluationType;
	        Type monthType = forge.month.EvaluationType;
	        Type dayType = forge.day.EvaluationType;
	        block.DeclareVar(typeof(int?), "year", SimpleNumberCoercerFactory.CoercerInt.CoerceCodegenMayNull(forge.year.EvaluateCodegen(yearType, methodNode, exprSymbol, codegenClassScope), yearType, methodNode, codegenClassScope))
	                .DeclareVar(typeof(int?), "month", SimpleNumberCoercerFactory.CoercerInt.CoerceCodegenMayNull(forge.month.EvaluateCodegen(monthType, methodNode, exprSymbol, codegenClassScope), monthType, methodNode, codegenClassScope))
	                .DeclareVar(typeof(int?), "day", SimpleNumberCoercerFactory.CoercerInt.CoerceCodegenMayNull(forge.day.EvaluateCodegen(dayType, methodNode, exprSymbol, codegenClassScope), dayType, methodNode, codegenClassScope));
	    }
	}
} // end of namespace