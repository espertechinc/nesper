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
        public class DivideDecimalWMathContext : Computer
        {
            private readonly bool divisionByZeroReturnsNull;
            private readonly MathContext mathContext;

            /// <summary>
            ///     Ctor.
            /// </summary>
            /// <param name="divisionByZeroReturnsNull">false for division-by-zero returns infinity, true for null</param>
            /// <param name="mathContext">math context</param>
            public DivideDecimalWMathContext(
                bool divisionByZeroReturnsNull,
                MathContext mathContext)
            {
                this.divisionByZeroReturnsNull = divisionByZeroReturnsNull;
                this.mathContext = mathContext;
            }

            public object Compute(
                object d1,
                object d2)
            {
                var b1 = d1.AsDecimal();
                var b2 = d2.AsDecimal();
                if (b2.AsDouble() == 0) {
                    if (divisionByZeroReturnsNull) {
                        return null;
                    }

                    var result = b1.AsDouble() / 0; // serves to create the right sign for infinity
                    return new decimal(result);
                }

                return decimal.Round(
                    decimal.Divide(b1, b2),
                    mathContext.Precision,
                    mathContext.RoundingMode);
            }

            public CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope,
                CodegenExpressionRef left,
                CodegenExpressionRef right,
                Type ltype,
                Type rtype)
            {
                CodegenExpression math =
                    codegenClassScope.AddOrGetFieldSharable(new MathContextCodegenField(mathContext));
                var block = codegenMethodScope
                    .MakeChild(typeof(decimal?), typeof(DivideDecimalWMathContext), codegenClassScope)
                    .AddParam(typeof(decimal?), "b1").AddParam(typeof(decimal?), "b2").Block;
                var ifZero = block.IfCondition(
                    CodegenExpressionBuilder.EqualsIdentity(
                        CodegenExpressionBuilder.ExprDotMethod(CodegenExpressionBuilder.Ref("b2"), "doubleValue"),
                        CodegenExpressionBuilder.Constant(0)));
                {
                    if (divisionByZeroReturnsNull) {
                        ifZero.BlockReturn(CodegenExpressionBuilder.ConstantNull());
                    }
                    else {
                        ifZero.BlockReturn(
                            CodegenExpressionBuilder.NewInstance(
                                typeof(decimal?),
                                CodegenExpressionBuilder.Op(
                                    CodegenExpressionBuilder.ExprDotMethod(CodegenExpressionBuilder.Ref("b1"), "doubleValue"), "/",
                                    CodegenExpressionBuilder.Constant(0))));
                    }
                }
                var method = block.MethodReturn(
                    CodegenExpressionBuilder.ExprDotMethod(CodegenExpressionBuilder.Ref("b1"), "divide", CodegenExpressionBuilder.Ref("b2"), math));
                return CodegenExpressionBuilder.LocalMethod(method, left, right);
            }
        }
    }
}