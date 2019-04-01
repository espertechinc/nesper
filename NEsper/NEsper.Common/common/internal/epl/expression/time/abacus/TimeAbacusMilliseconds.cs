///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.time.abacus
{
    public class TimeAbacusMilliseconds : TimeAbacus
    {
        public readonly static TimeAbacusMilliseconds INSTANCE = new TimeAbacusMilliseconds();

        private TimeAbacusMilliseconds()
        {
        }

        public long DeltaForSecondsDouble(double seconds)
        {
            return Math.Round(1000d * seconds);
        }

        public CodegenExpression DeltaForSecondsDoubleCodegen(CodegenExpressionRef sec, CodegenClassScope codegenClassScope)
        {
            return StaticMethod(typeof(Math), "round", Op(Constant(1000d), "*", sec));
        }

        public long DeltaForSecondsNumber(object timeInSeconds)
        {
            if (TypeHelper.IsFloatingPointNumber(timeInSeconds))
            {
                return DeltaForSecondsDouble(timeInSeconds.AsDouble());
            }
            return 1000 * timeInSeconds.AsLong();
        }

        public long DateTimeSet(long fromTime, DateTimeEx dateTime)
        {
            dateTime.TimeInMillis = fromTime;
            return 0;
        }

        public CodegenExpression DateTimeSetCodegen(CodegenExpression startLong, CodegenExpression dateTime, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope)
        {
            return LocalMethodBuild(codegenMethodScope.MakeChild(typeof(long), typeof(TimeAbacusMilliseconds), codegenClassScope).AddParam(typeof(long), "fromTime").AddParam(typeof(Calendar), "cal").Block
                    .Expression(ExprDotMethod(@Ref("cal"), "setTimeInMillis", @Ref("fromTime")))
                    .MethodReturn(Constant(0))).Pass(startLong).Pass(dateTime).Call();
        }

        public long DateTimeGet(DateTimeEx dateTime, long remainder)
        {
            return dateTime.TimeInMillis;
        }

        public long OneSecond
        {
            get => 1000;
        }

        public DateTimeEx ToDate(long ts)
        {
            return new Date(ts);
        }

        public CodegenExpression ToDateCodegen(CodegenExpression ts)
        {
            return NewInstance(typeof(DateTimeEx), ts);
        }

        public CodegenExpression DateTimeGetCodegen(CodegenExpression dateTime, CodegenExpression startRemainder, CodegenClassScope codegenClassScope)
        {
            return ExprDotMethod(dateTime, "getTimeInMillis");
        }
    }
} // end of namespace