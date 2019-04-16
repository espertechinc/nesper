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
    public partial class MathArithTypeEnum
    {
        /// <summary>
        ///     Computer for math op.
        /// </summary>
        public class SubtractDecimalConvComputer : Computer
        {
            private readonly SimpleNumberDecimalCoercer convOne;
            private readonly SimpleNumberDecimalCoercer convTwo;

            /// <summary>
            ///     Ctor.
            /// </summary>
            /// <param name="convOne">convertor for LHS</param>
            /// <param name="convTwo">convertor for RHS</param>
            public SubtractDecimalConvComputer(
                SimpleNumberDecimalCoercer convOne,
                SimpleNumberDecimalCoercer convTwo)
            {
                this.convOne = convOne;
                this.convTwo = convTwo;
            }

            public object Compute(
                object d1,
                object d2)
            {
                decimal s1 = convOne.CoerceBoxedDecimal(d1);
                decimal s2 = convTwo.CoerceBoxedDecimal(d2);
                return s1 - s2;
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
                    .MakeChild(typeof(decimal?), typeof(SubtractDecimalConvComputer), codegenClassScope)
                    .AddParam(ltype, "d1").AddParam(rtype, "d2").Block
                    .DeclareVar(typeof(decimal?), "s1", convOne.CoerceBoxedDecimalCodegen(CodegenExpressionBuilder.Ref("d1"), ltype))
                    .DeclareVar(typeof(decimal?), "s2", convTwo.CoerceBoxedDecimalCodegen(CodegenExpressionBuilder.Ref("d2"), rtype))
                    .MethodReturn(
                        CodegenExpressionBuilder.ExprDotMethod(CodegenExpressionBuilder.Ref("s1"), "subtract", CodegenExpressionBuilder.Ref("s2")));
                return CodegenExpressionBuilder.LocalMethodBuild(method).Pass(left).Pass(right).Call();
            }
        }
    }
}