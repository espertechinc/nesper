///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Numerics;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.type
{
    public partial class MathArithType
    {
        /// <summary>
        ///     Computer for math op.
        /// </summary>
        public class DivideBigIntConvComputer : Computer
        {
            private readonly BigIntegerCoercer _convOne;
            private readonly BigIntegerCoercer _convTwo;

            /// <summary>
            ///     Ctor.
            /// </summary>
            /// <param name="convOne">convertor for LHS</param>
            /// <param name="convTwo">convertor for RHS</param>
            public DivideBigIntConvComputer(
                BigIntegerCoercer convOne,
                BigIntegerCoercer convTwo)
            {
                _convOne = convOne;
                _convTwo = convTwo;
            }

            public object Compute(
                object d1,
                object d2)
            {
                var s1 = _convOne.CoerceBoxedBigInt(d1);
                var s2 = _convTwo.CoerceBoxedBigInt(d2);
                if (s2.Equals(BigInteger.Zero)) {
                    return null;
                }

                return s1 / s2;
            }

            public CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope,
                CodegenExpressionRef left,
                CodegenExpressionRef right,
                Type ltype,
                Type rtype)
            {
                var method = codegenMethodScope
                    .MakeChild(typeof(BigInteger?), typeof(DivideBigIntConvComputer), codegenClassScope)
                    .AddParam(ltype, "d1")
                    .AddParam(rtype, "d2")
                    .Block
                    .DeclareVar<BigInteger?>("s1", _convOne.CoerceBoxedBigIntCodegen(Ref("d1"), ltype))
                    .DeclareVar<BigInteger?>("s2", _convTwo.CoerceBoxedBigIntCodegen(Ref("d2"), rtype))
                    .IfCondition(EqualsIdentity(Ref("s2"), EnumValue(typeof(BigInteger), "Zero")))
                    .BlockReturn(ConstantNull())
                    .MethodReturn(Op(Ref("s1"), "/", Ref("s2")));
                return LocalMethodBuild(method).Pass(left).Pass(right).Call();
            }
        }
    }
}