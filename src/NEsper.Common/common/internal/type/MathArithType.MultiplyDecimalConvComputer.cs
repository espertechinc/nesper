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

namespace com.espertech.esper.common.@internal.type
{
    public partial class MathArithType
    {
        /// <summary>
        ///     Computer for math op.
        /// </summary>
        public class MultiplyDecimalConvComputer : Computer
        {
            private readonly Coercer convOne;
            private readonly Coercer convTwo;

            /// <summary>
            ///     Ctor.
            /// </summary>
            /// <param name="convOne">conversion for LHS</param>
            /// <param name="convTwo">conversion for RHS</param>
            public MultiplyDecimalConvComputer(
                Coercer convOne,
                Coercer convTwo)
            {
                this.convOne = convOne;
                this.convTwo = convTwo;
            }

            public object Compute(
                object d1,
                object d2)
            {
                var s1 = convOne.CoerceBoxed(d1).AsDecimal();
                var s2 = convTwo.CoerceBoxed(d2).AsDecimal();
                return s1 * s2;
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
                    .MakeChild(typeof(decimal?), typeof(MultiplyDecimalConvComputer), codegenClassScope)
                    .AddParam(ltype, "d1")
                    .AddParam(rtype, "d2")
                    .Block
                    .DeclareVar<decimal?>("s1", convOne.CoerceCodegen(Ref("d1"), ltype, codegenMethodScope, codegenClassScope))
                    .DeclareVar<decimal?>("s2", convTwo.CoerceCodegen(Ref("d2"), rtype, codegenMethodScope, codegenClassScope))
                    .MethodReturn(
                        Op(
                            ExprDotName(Ref("s1"), "Value"),
                            "*",
                            ExprDotName(Ref("s2"), "Value")));
                return LocalMethodBuild(method).Pass(left).Pass(right).Call();
            }
        }
    }
}