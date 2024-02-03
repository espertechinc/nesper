///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
        public static readonly TimeAbacusMilliseconds INSTANCE = new TimeAbacusMilliseconds();

        private TimeAbacusMilliseconds()
        {
        }

        public long DeltaForSecondsDouble(double seconds)
        {
            return (long)Math.Round(1000d * seconds);
        }

        public CodegenExpression DeltaForSecondsDoubleCodegen(
            CodegenExpressionRef sec,
            CodegenClassScope codegenClassScope)
        {
            return Cast<long>(
                StaticMethod(typeof(Math), "Round", Op(Constant(1000d), "*", ExprDotMethod(sec, "AsDouble"))));
        }

        public long DeltaForSecondsNumber(object timeInSeconds)
        {
            if (timeInSeconds.IsFloatingPointNumber()) {
                return DeltaForSecondsDouble(timeInSeconds.AsDouble());
            }

            return 1000 * timeInSeconds.AsInt64();
        }

        public long DateTimeSet(
            long fromTime,
            DateTimeEx dateTime)
        {
            dateTime.SetUtcMillis(fromTime);
            return 0;
        }

        public CodegenExpression DateTimeSetCodegen(
            CodegenExpression startLong,
            CodegenExpression dateTime,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethodBuild(
                    codegenMethodScope.MakeChild(typeof(long), typeof(TimeAbacusMilliseconds), codegenClassScope)
                        .AddParam<long>("fromTime")
                        .AddParam<DateTimeEx>("dtx")
                        .Block
                        .Expression(ExprDotMethod(Ref("dtx"), "SetUtcMillis", Ref("fromTime")))
                        .MethodReturn(Constant(0)))
                .Pass(startLong)
                .Pass(dateTime)
                .Call();
        }

        public long DateTimeGet(
            DateTimeEx dateTime,
            long remainder)
        {
            return dateTime.UtcMillis;
        }

        public long OneSecond => 1000;

        public DateTimeEx ToDateTimeEx(long ts)
        {
            return DateTimeEx.UtcInstance(ts);
        }

        public CodegenExpression ToDateCodegen(CodegenExpression ts)
        {
            return StaticMethod(typeof(DateTimeEx), "UtcInstance", ts);
        }

        public CodegenExpression DateTimeGetCodegen(
            CodegenExpression dateTime,
            CodegenExpression startRemainder,
            CodegenClassScope codegenClassScope)
        {
            return ExprDotName(dateTime, "UtcMillis");
        }
    }
} // end of namespace