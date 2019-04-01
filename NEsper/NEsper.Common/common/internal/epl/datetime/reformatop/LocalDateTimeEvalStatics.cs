///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.datetime.reformatop
{
	public class LocalDateTimeEvalStatics {

	    public readonly static DateTimeExEval MINUTE_OF_HOUR = new ProxyLocalDateTimeEval() {
	        ProcEvaluateInternal = (dto) =>  {
	            return dto.Minute;
	        },

	        ProcCodegen = (inner) =>  {
	            return ExprDotMethod(inner, "getMinute");
	        },
	    };

	    public readonly static DateTimeExEval MONTH_OF_YEAR = new ProxyLocalDateTimeEval() {
	        ProcEvaluateInternal = (dto) =>  {
	            return dto.MonthValue;
	        },

	        ProcCodegen = (inner) =>  {
	            return ExprDotMethod(inner, "getMonthValue");
	        },
	    };

	    public readonly static DateTimeExEval DAY_OF_MONTH = new ProxyLocalDateTimeEval() {
	        ProcEvaluateInternal = (dto) =>  {
	            return dto.DayOfMonth;
	        },

	        ProcCodegen = (inner) =>  {
	            return ExprDotMethod(inner, "getDayOfMonth");
	        },
	    };

	    public readonly static DateTimeExEval DAY_OF_WEEK = new ProxyLocalDateTimeEval() {
	        ProcEvaluateInternal = (dto) =>  {
	            return dto.DayOfWeek;
	        },

	        ProcCodegen = (inner) =>  {
	            return ExprDotMethod(inner, "getDayOfWeek");
	        },
	    };

	    public readonly static DateTimeExEval DAY_OF_YEAR = new ProxyLocalDateTimeEval() {
	        ProcEvaluateInternal = (dto) =>  {
	            return dto.DayOfYear;
	        },

	        ProcCodegen = (inner) =>  {
	            return ExprDotMethod(inner, "getDayOfYear");
	        },
	    };

	    public readonly static DateTimeExEval ERA = new ProxyLocalDateTimeEval() {
	        ProcEvaluateInternal = (dto) =>  {
	            return dto.Get(ChronoField.ERA);
	        },

	        ProcCodegen = (inner) =>  {
	            return ExprDotMethod(inner, "get", EnumValue(typeof(ChronoField), "ERA"));
	        },
	    };

	    public readonly static DateTimeExEval HOUR_OF_DAY = new ProxyLocalDateTimeEval() {
	        ProcEvaluateInternal = (dto) =>  {
	            return dto.Hour;
	        },

	        ProcCodegen = (inner) =>  {
	            return ExprDotMethod(inner, "getHour");
	        },
	    };

	    public readonly static DateTimeExEval MILLIS_OF_SECOND = new ProxyLocalDateTimeEval() {
	        ProcEvaluateInternal = (dto) =>  {
	            return dto.Get(ChronoField.MILLI_OF_SECOND);
	        },

	        ProcCodegen = (inner) =>  {
	            return ExprDotMethod(inner, "get", EnumValue(typeof(ChronoField), "MILLI_OF_SECOND"));
	        },
	    };

	    public readonly static DateTimeExEval SECOND_OF_MINUTE = new ProxyLocalDateTimeEval() {
	        ProcEvaluateInternal = (dto) =>  {
	            return dto.Second;
	        },

	        ProcCodegen = (inner) =>  {
	            return ExprDotMethod(inner, "getSecond");
	        },
	    };

	    public readonly static DateTimeExEval WEEKYEAR = new ProxyLocalDateTimeEval() {
	        ProcEvaluateInternal = (dto) =>  {
	            return dto.Get(ChronoField.ALIGNED_WEEK_OF_YEAR);
	        },

	        ProcCodegen = (inner) =>  {
	            return ExprDotMethod(inner, "get", EnumValue(typeof(ChronoField), "ALIGNED_WEEK_OF_YEAR"));
	        },
	    };

	    public readonly static DateTimeExEval YEAR = new ProxyLocalDateTimeEval() {
	        ProcEvaluateInternal = (dto) =>  {
	            return dto.Year;
	        },

	        ProcCodegen = (inner) =>  {
	            return ExprDotMethod(inner, "getYear");
	        },
	    };
	}
} // end of namespace