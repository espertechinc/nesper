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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.type
{
    public partial class MathArithType
    {
        /// <summary>
        ///     Computer for type-specific arith. operations.
        /// </summary>
        [Serializable]
        public class DivideDecimalWMathContext : Computer
        {
            private readonly bool _divisionByZeroReturnsNull;
            private readonly MathContext _mathContext;

            /// <summary>
            ///     Ctor.
            /// </summary>
            /// <param name="divisionByZeroReturnsNull">false for division-by-zero returns infinity, true for null</param>
            /// <param name="mathContext">math context</param>
            public DivideDecimalWMathContext(
                bool divisionByZeroReturnsNull,
                MathContext mathContext)
            {
                this._divisionByZeroReturnsNull = divisionByZeroReturnsNull;
                this._mathContext = mathContext;
            }

            public object Compute(
                object d1,
                object d2)
            {
                var b1 = d1.AsDecimal();
                var b2 = d2.AsDecimal();
                if (b2 == 0.0m) {
                    if (_divisionByZeroReturnsNull) {
                        return null;
                    }

                    var result = b1.AsDecimal() / 0; // serves to create the right sign for infinity
                    return result;
                }

                return decimal.Round(
                    decimal.Divide(b1, b2),
                    _mathContext.Precision,
                    _mathContext.RoundingMode);
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
                    codegenClassScope.AddOrGetDefaultFieldSharable(new MathContextCodegenField(_mathContext));
                var block = codegenMethodScope
                    .MakeChild(typeof(decimal?), typeof(DivideDecimalWMathContext), codegenClassScope)
                    .AddParam(typeof(decimal?), "b1")
                    .AddParam(typeof(decimal?), "b2")
                    .Block;
                var ifZero = block.IfCondition(
                    EqualsIdentity(
                        Ref("b2"),
                        Constant(0.0m)));
                {
                    if (_divisionByZeroReturnsNull) {
                        ifZero.BlockReturn(ConstantNull());
                    }
                    else {
                        ifZero.BlockReturn(Op(ExprDotName(Ref("b1"), "Value"), "/", Constant(0.0m)));
                    }
                }
                var method = block.MethodReturn(
                    ExprDotMethod(math, "Apply", Op(Ref("b1"), "/", Ref("b2"))));

                return LocalMethod(method, left, right);
            }
        }
    }
}