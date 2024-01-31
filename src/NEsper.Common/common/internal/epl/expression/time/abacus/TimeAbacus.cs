///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.expression.time.abacus
{
    public interface TimeAbacus
    {
        long OneSecond { get; }
        long DeltaForSecondsNumber(object timeInSeconds);

        long DeltaForSecondsDouble(double seconds);

        long DateTimeSet(
            long fromTime,
            DateTimeEx dateTime);

        long DateTimeGet(
            DateTimeEx dateTime,
            long remainder);

        DateTimeEx ToDateTimeEx(long ts);

        CodegenExpression DateTimeSetCodegen(
            CodegenExpression startLong,
            CodegenExpression dateTime,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope);

        CodegenExpression DateTimeGetCodegen(
            CodegenExpression dateTime,
            CodegenExpression startRemainder,
            CodegenClassScope codegenClassScope);

        CodegenExpression ToDateCodegen(CodegenExpression ts);

        CodegenExpression DeltaForSecondsDoubleCodegen(
            CodegenExpressionRef sec,
            CodegenClassScope codegenClassScope);
    }
} // end of namespace