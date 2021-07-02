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
        /// <summary>
        ///     Computer for math op.
        /// </summary>
        [Serializable]
        public class AddDecimalConvComputer : Computer
        {
            private readonly Coercer _convOne;
            private readonly Coercer _convTwo;

            /// <summary>
            ///     Ctor.
            /// </summary>
            /// <param name="convOne">conversion for LHS</param>
            /// <param name="convTwo">conversion for RHS</param>
            public AddDecimalConvComputer(
                Coercer convOne,
                Coercer convTwo)
            {
                _convOne = convOne;
                _convTwo = convTwo;
            }

            public object Compute(
                object d1,
                object d2)
            {
                decimal s1 = _convOne.CoerceBoxed(d1).AsDecimal();
                decimal s2 = _convTwo.CoerceBoxed(d2).AsDecimal();
                return s1 + s2;
            }

            public CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope,
                CodegenExpressionRef left,
                CodegenExpressionRef right,
                Type ltype,
                Type rtype)
            {
                var leftValue = _convOne.CoerceCodegen(left, ltype);
                var rightValue = _convTwo.CoerceCodegen(right, rtype);
                return CodegenExpressionBuilder.Op(leftValue, "+", rightValue);
            }
        }
    }
}