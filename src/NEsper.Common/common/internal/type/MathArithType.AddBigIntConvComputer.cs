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
        /// <summary>
        ///     Computer for math op.
        /// </summary>
        [Serializable]
        public class AddBigIntConvComputer : Computer
        {
            private readonly BigIntegerCoercer _convOne;
            private readonly BigIntegerCoercer _convTwo;

            /// <summary>
            ///     Ctor.
            /// </summary>
            /// <param name="convOne">conversion for LHS</param>
            /// <param name="convTwo">conversion for RHS</param>
            public AddBigIntConvComputer(
                BigIntegerCoercer convOne,
                BigIntegerCoercer convTwo)
            {
                this._convOne = convOne;
                this._convTwo = convTwo;
            }

            public object Compute(
                object d1,
                object d2)
            {
                var s1 = _convOne.CoerceBoxedBigInt(d1);
                var s2 = _convTwo.CoerceBoxedBigInt(d2);
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
                var leftAsBig = _convOne.CoerceBoxedBigIntCodegen(left, ltype);
                var rightAsBig = _convTwo.CoerceBoxedBigIntCodegen(right, rtype);
                return CodegenExpressionBuilder.Op(leftAsBig, "+", rightAsBig);
            }
        }
    }
}