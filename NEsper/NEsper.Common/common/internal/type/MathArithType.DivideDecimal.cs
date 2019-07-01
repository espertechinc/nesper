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
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.type
{
    public partial class MathArithType
    {
        /// <summary>
        ///     Computer for type-specific arith. operations.
        /// </summary>
        public class DivideDecimal : Computer
        {
            private readonly bool divisionByZeroReturnsNull;

            /// <summary>
            ///     Ctor.
            /// </summary>
            /// <param name="divisionByZeroReturnsNull">false for division-by-zero returns infinity, true for null</param>
            public DivideDecimal(bool divisionByZeroReturnsNull)
            {
                this.divisionByZeroReturnsNull = divisionByZeroReturnsNull;
            }

            public object Compute(
                object d1,
                object d2)
            {
                var b1 = d1.AsDecimal();
                var b2 = d2.AsDecimal();
                if (b2 == 0.0m) {
                    if (divisionByZeroReturnsNull) {
                        return null;
                    }

                    return b1 / 0.0m; // serves to create the right sign for infinity
                }

                return b1 / b2;
            }

            public CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope,
                CodegenExpressionRef left,
                CodegenExpressionRef right,
                Type ltype,
                Type rtype)
            {
                var block = codegenMethodScope.MakeChild(typeof(decimal?), typeof(DivideDecimal), codegenClassScope)
                    .AddParam(typeof(decimal?), "b1").AddParam(typeof(decimal?), "b2").Block;
                var ifBlock = block.IfCondition(
                    CodegenExpressionBuilder.EqualsIdentity(
                        CodegenExpressionBuilder.ExprDotMethod(CodegenExpressionBuilder.Ref("b1"), "doubleValue"),
                        CodegenExpressionBuilder.Constant(0d)));
                if (divisionByZeroReturnsNull) {
                    ifBlock.BlockReturn(CodegenExpressionBuilder.ConstantNull());
                }
                else {
                    ifBlock.BlockReturn(
                        CodegenExpressionBuilder.NewInstance(
                            typeof(decimal?),
                            CodegenExpressionBuilder.Op(
                                CodegenExpressionBuilder.ExprDotMethod(CodegenExpressionBuilder.Ref("b1"), "doubleValue"), "/",
                                CodegenExpressionBuilder.Constant(0d))));
                }

                var method = block.MethodReturn(
                    CodegenExpressionBuilder.ExprDotMethod(CodegenExpressionBuilder.Ref("b1"), "divide", CodegenExpressionBuilder.Ref("b2")));
                return CodegenExpressionBuilder.LocalMethod(method, left, right);
            }
        }
    }
}