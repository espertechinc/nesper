///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.type
{
    public partial class MathArithTypeEnum
    {
        public class DivideDecimalConvComputerNoMathCtx : DivideDecimalConvComputerBase
        {
            public DivideDecimalConvComputerNoMathCtx(
                SimpleNumberDecimalCoercer convOne,
                SimpleNumberDecimalCoercer convTwo,
                bool divisionByZeroReturnsNull)
                : base(convOne, convTwo, divisionByZeroReturnsNull)
            {
            }

            public override object DoDivide(
                decimal s1,
                decimal s2)
            {
                return s1.Divide(s2);
            }

            public override CodegenExpression DoDivideCodegen(
                CodegenExpressionRef s1,
                CodegenExpressionRef s2,
                CodegenClassScope codegenClassScope)
            {
                return CodegenExpressionBuilder.ExprDotMethod(s1, "divide", s2);
            }
        }
    }
}