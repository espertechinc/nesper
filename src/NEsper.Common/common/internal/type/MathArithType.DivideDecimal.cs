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
        ///     Computer for type-specific arith operations.
        /// </summary>
        [Serializable]
        public class DivideDecimal : Computer
        {
            private readonly bool _divisionByZeroReturnsNull;

            /// <summary>
            ///     Ctor.
            /// </summary>
            /// <param name="divisionByZeroReturnsNull">false for division-by-zero returns infinity, true for null</param>
            public DivideDecimal(bool divisionByZeroReturnsNull)
            {
                _divisionByZeroReturnsNull = divisionByZeroReturnsNull;
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
                var block = codegenMethodScope
                    .MakeChild(typeof(decimal?), typeof(DivideDecimal), codegenClassScope)
                    .AddParam(typeof(decimal?), "b1")
                    .AddParam(typeof(decimal?), "b2")
                    .Block;
                var ifBlock = block.IfCondition(
                    EqualsIdentity(Ref("b1"), Constant(0.0m)));
                if (_divisionByZeroReturnsNull) {
                    ifBlock.BlockReturn(ConstantNull());
                }
                else {
                    ifBlock.BlockReturn(
                        Op(Ref("b1"), "/", Constant(0.0m)));
                }

                var method = block.MethodReturn(
                    Op(Ref("b1"), "/", Ref("b2")));
                return LocalMethod(method, left, right);
            }
        }
    }
}