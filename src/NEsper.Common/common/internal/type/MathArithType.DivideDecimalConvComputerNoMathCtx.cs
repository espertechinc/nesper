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

namespace com.espertech.esper.common.@internal.type
{
    public partial class MathArithType
    {
        public class DivideDecimalConvComputerNoMathCtx : DivideDecimalConvComputerBase
        {
            public DivideDecimalConvComputerNoMathCtx(
                Coercer convOne,
                Coercer convTwo,
                bool divisionByZeroReturnsNull)
                : base(convOne, convTwo, divisionByZeroReturnsNull)
            {
            }

            public override object DoDivide(
                decimal s1,
                decimal s2)
            {
                return s1 / s2;
            }

            public override CodegenExpression DoDivideCodegen(
                CodegenExpressionRef s1,
                CodegenExpressionRef s2,
                CodegenClassScope codegenClassScope)
            {
                return CodegenExpressionBuilder.Op(s1, "/", s2);
            }
        }
    }
}