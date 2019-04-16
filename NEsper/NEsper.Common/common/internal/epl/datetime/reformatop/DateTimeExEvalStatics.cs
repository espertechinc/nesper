///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

namespace com.espertech.esper.common.@internal.epl.datetime.reformatop
{
    public class DateTimeExEvalStatics
    {
        public static readonly DateTimeExEval MINUTE_OF_HOUR = new ProxyDateTimeExEval {
            ProcEvaluateInternal = dtx => dtx.Minute,
            ProcCodegen = input => CodegenExpressionBuilder.GetProperty(input, "Minute")
        };

        public static readonly DateTimeExEval MONTH_OF_YEAR = new ProxyDateTimeExEval {
            ProcEvaluateInternal = dtx => dtx.Month,
            ProcCodegen = input => CodegenExpressionBuilder.GetProperty(input, "Month")
        };

        public static readonly DateTimeExEval DAY_OF_MONTH = new ProxyDateTimeExEval {
            ProcEvaluateInternal = dtx => dtx.Day,
            ProcCodegen = input => CodegenExpressionBuilder.GetProperty(input, "Day")
        };

        public static readonly DateTimeExEval DAY_OF_WEEK = new ProxyDateTimeExEval {
            ProcEvaluateInternal = dtx => dtx.DayOfWeek,
            ProcCodegen = input => CodegenExpressionBuilder.GetProperty(input, "DayOfWeek")
        };

        public static readonly DateTimeExEval DAY_OF_YEAR = new ProxyDateTimeExEval {
            ProcEvaluateInternal = dtx => dtx.DayOfYear,
            ProcCodegen = input => CodegenExpressionBuilder.GetProperty(input, "DayOfYear")
        };

        public static readonly DateTimeExEval HOUR_OF_DAY = new ProxyDateTimeExEval {
            ProcEvaluateInternal = dtx => dtx.Hour,
            ProcCodegen = input => CodegenExpressionBuilder.GetProperty(input, "Hour")
        };

        public static readonly DateTimeExEval MILLIS_OF_SECOND = new ProxyDateTimeExEval {
            ProcEvaluateInternal = dtx => dtx.Millisecond,
            ProcCodegen = input => CodegenExpressionBuilder.GetProperty(input, "Millisecond")
        };

        public static readonly DateTimeExEval SECOND_OF_MINUTE = new ProxyDateTimeExEval {
            ProcEvaluateInternal = dtx => dtx.Second,
            ProcCodegen = input => CodegenExpressionBuilder.GetProperty(input, "Second")
        };

        public static readonly DateTimeExEval WEEKYEAR = new ProxyDateTimeExEval {
            ProcEvaluateInternal = dtx => dtx.WeekOfYear,
            ProcCodegen = input => CodegenExpressionBuilder.GetProperty(input, "WeekOfYear")
        };

        public static readonly DateTimeExEval YEAR = new ProxyDateTimeExEval {
            ProcEvaluateInternal = dtx => dtx.Year,
            ProcCodegen = input => CodegenExpressionBuilder.GetProperty(input, "Year")
        };
    }
} // end of namespace