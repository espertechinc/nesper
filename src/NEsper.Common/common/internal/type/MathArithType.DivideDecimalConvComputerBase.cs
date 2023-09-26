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
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.type
{
    public partial class MathArithType
    {
        /// <summary>
        ///     Computer for math op.
        /// </summary>
        public abstract class DivideDecimalConvComputerBase : Computer
        {
            private readonly Coercer _convOne;
            private readonly Coercer _convTwo;
            private readonly bool _divisionByZeroReturnsNull;

            /// <summary>
            ///     Ctor.
            /// </summary>
            /// <param name="convOne">convertor for LHS</param>
            /// <param name="convTwo">convertor for RHS</param>
            /// <param name="divisionByZeroReturnsNull">false for division-by-zero returns infinity, true for null</param>
            protected DivideDecimalConvComputerBase(
                Coercer convOne,
                Coercer convTwo,
                bool divisionByZeroReturnsNull)
            {
                _convOne = convOne;
                _convTwo = convTwo;
                _divisionByZeroReturnsNull = divisionByZeroReturnsNull;
            }

            public object Compute(
                object d1,
                object d2)
            {
                var s1 = _convOne.CoerceBoxed(d1).AsDecimal();
                var s2 = _convTwo.CoerceBoxed(d2).AsDecimal();
                if (s2 == 0.0m) {
                    if (_divisionByZeroReturnsNull) {
                        return null;
                    }

                    var result = s1.AsDouble() / 0;
                    return new decimal(result);
                }

                return DoDivide(s1, s2);
            }

            public CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope,
                CodegenExpressionRef left,
                CodegenExpressionRef right,
                Type ltype,
                Type rtype)
            {
                var resultType = typeof(decimal);
                if (ltype.IsNullable() || rtype.IsNullable() || _divisionByZeroReturnsNull) {
                    resultType = typeof(decimal?);
                }

                var block = codegenMethodScope
                    .MakeChild(resultType, typeof(DivideDecimalConvComputerBase), codegenClassScope)
                    .AddParam(ltype, "d1")
                    .AddParam(rtype, "d2")
                    .Block
                    .DeclareVar<decimal>(
                        "s1",
                        _convOne.CoerceCodegen(CodegenExpressionBuilder.Ref("d1"), ltype))
                    .DeclareVar<decimal>(
                        "s2",
                        _convTwo.CoerceCodegen(CodegenExpressionBuilder.Ref("d2"), rtype));
                var ifZeroDivisor =
                    block.IfCondition(
                        CodegenExpressionBuilder.EqualsIdentity(
                            CodegenExpressionBuilder.Ref("s2"),
                            CodegenExpressionBuilder.Constant(0.0m)));
                if (_divisionByZeroReturnsNull) {
                    ifZeroDivisor.BlockReturn(CodegenExpressionBuilder.ConstantNull());
                }
                else {
                    ifZeroDivisor.DeclareVar<decimal>(
                            "result",
                            CodegenExpressionBuilder.Op(
                                CodegenExpressionBuilder.Ref("s1"),
                                "/",
                                CodegenExpressionBuilder.Constant(0.0m)))
                        .BlockReturn(CodegenExpressionBuilder.Ref("result"));
                }

                var method = block.MethodReturn(
                    DoDivideCodegen(
                        CodegenExpressionBuilder.Ref("s1"),
                        CodegenExpressionBuilder.Ref("s2"),
                        codegenClassScope));
                return CodegenExpressionBuilder
                    .LocalMethodBuild(method)
                    .Pass(left)
                    .Pass(right)
                    .Call();
            }

            public abstract object DoDivide(
                decimal s1,
                decimal s2);

            public abstract CodegenExpression DoDivideCodegen(
                CodegenExpressionRef s1,
                CodegenExpressionRef s2,
                CodegenClassScope codegenClassScope);
        }
    }
}