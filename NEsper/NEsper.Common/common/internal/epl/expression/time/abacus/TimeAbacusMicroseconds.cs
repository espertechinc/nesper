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
    [Serializable]
    public class TimeAbacusMicroseconds : TimeAbacus
    {
        public static readonly TimeAbacusMicroseconds INSTANCE = new TimeAbacusMicroseconds();

        private TimeAbacusMicroseconds()
        {
        }

        public long DeltaForSecondsDouble(double seconds)
        {
            return (long) Math.Round(1000000d * seconds);
        }

        public long DeltaForSecondsNumber(object timeInSeconds)
        {
            if (timeInSeconds.IsFloatingPointNumber()) {
                return DeltaForSecondsDouble(timeInSeconds.AsDouble());
            }

            return 1000000 * timeInSeconds.AsLong();
        }

        public long DateTimeSet(
            long fromTime,
            DateTimeEx dateTime)
        {
            var millis = fromTime / 1000;
            dateTime.SetUtcMillis(millis);
            return fromTime - millis * 1000;
        }

        public long DateTimeGet(
            DateTimeEx dateTime,
            long remainder)
        {
            return dateTime.UtcMillis * 1000 + remainder;
        }

        public long OneSecond => 1000000;

        public CodegenExpression DeltaForSecondsDoubleCodegen(
            CodegenExpressionRef sec,
            CodegenClassScope codegenClassScope)
        {
            return Cast<long>(StaticMethod(typeof(Math), "Round", Op(Constant(1000000d), "*", ExprDotName(sec, "Value"))));
        }

        public CodegenExpression DateTimeSetCodegen(
            CodegenExpression startLong,
            CodegenExpression dateTime,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var method = codegenMethodScope.MakeChild(typeof(long), typeof(TimeAbacusMicroseconds), codegenClassScope)
                .AddParam(typeof(long), "fromTime")
                .AddParam(typeof(DateTimeEx), "dtx")
                .Block
                .DeclareVar<long>("millis", Op(Ref("fromTime"), "/", Constant(1000)))
                .Expression(ExprDotMethod(Ref("dtx"), "SetUtcMillis", Ref("millis")))
                .MethodReturn(Op(Ref("fromTime"), "-", Op(Ref("millis"), "*", Constant(1000))));
            return LocalMethodBuild(method).Pass(startLong).Pass(dateTime).Call();
        }

        public CodegenExpression DateTimeGetCodegen(
            CodegenExpression dateTime,
            CodegenExpression startRemainder,
            CodegenClassScope codegenClassScope)
        {
            return Op(Op(ExprDotName(dateTime, "UtcMillis"), "*", Constant(1000)), "+", startRemainder);
        }

        public DateTimeEx ToDateTimeEx(long ts)
        {
            return DateTimeEx.UtcInstance(ts / 1000); // return new Date(ts / 1000);
        }

        public CodegenExpression ToDateCodegen(CodegenExpression ts)
        {
            return StaticMethod(typeof(DateTimeEx), "UtcInstance", Op(ts, "/", Constant(1000)));
        }
    }
} // end of namespace