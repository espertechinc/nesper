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

namespace com.espertech.esper.common.@internal.type
{
    public partial class MathArithType
    {
        [Serializable]
        public class DivideDecimalConvComputerWithMathCtx : DivideDecimalConvComputerBase
        {
            private readonly MathContext _mathContext;

            public DivideDecimalConvComputerWithMathCtx(
                SimpleNumberCoercer convOne,
                SimpleNumberCoercer convTwo,
                bool divisionByZeroReturnsNull,
                MathContext mathContext)
                : base(convOne, convTwo, divisionByZeroReturnsNull)
            {
                _mathContext = mathContext;
            }

            public override object DoDivide(
                decimal s1,
                decimal s2)
            {
                return decimal.Round(
                    decimal.Divide(s1, s2),
                    _mathContext.Precision,
                    _mathContext.RoundingMode);
            }

            public override CodegenExpression DoDivideCodegen(
                CodegenExpressionRef s1,
                CodegenExpressionRef s2,
                CodegenClassScope codegenClassScope)
            {
                CodegenExpression math = codegenClassScope.AddOrGetDefaultFieldSharable(new MathContextCodegenField(_mathContext));
                return CodegenExpressionBuilder.ExprDotMethod(
                    math, "Apply", CodegenExpressionBuilder.Op(s1, "/", s2));
            }
        }
    }
}