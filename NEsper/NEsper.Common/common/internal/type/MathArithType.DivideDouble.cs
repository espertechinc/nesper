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
        public class DivideDouble : Computer
        {
            private readonly bool divisionByZeroReturnsNull;

            /// <summary>
            ///     Ctor.
            /// </summary>
            /// <param name="divisionByZeroReturnsNull">false for division-by-zero returns infinity, true for null</param>
            public DivideDouble(bool divisionByZeroReturnsNull)
            {
                this.divisionByZeroReturnsNull = divisionByZeroReturnsNull;
            }

            public object Compute(
                object d1,
                object d2)
            {
                var d2Double = d2.AsDouble();
                if (divisionByZeroReturnsNull && d2Double == 0) {
                    return null;
                }

                return d1.AsDouble() / d2Double;
            }

            public CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope,
                CodegenExpressionRef left,
                CodegenExpressionRef right,
                Type ltype,
                Type rtype)
            {
                if (!divisionByZeroReturnsNull) {
                    return CodegenExpressionBuilder.Op(
                        CodegenAsDouble(left, ltype),
                        "/",
                        CodegenAsDouble(right, rtype));
                }

                var method = codegenMethodScope.MakeChild(typeof(double?), typeof(DivideDouble), codegenClassScope)
                    .AddParam(ltype, "d1")
                    .AddParam(rtype, "d2")
                    .Block
                    .DeclareVar<double>("d2Double", CodegenAsDouble(CodegenExpressionBuilder.Ref("d2"), rtype))
                    .IfCondition(
                        CodegenExpressionBuilder.EqualsIdentity(
                            CodegenExpressionBuilder.Ref("d2Double"),
                            CodegenExpressionBuilder.Constant(0)))
                    .BlockReturn(CodegenExpressionBuilder.ConstantNull())
                    .MethodReturn(
                        CodegenExpressionBuilder.Op(
                            CodegenAsDouble(CodegenExpressionBuilder.Ref("d1"), ltype),
                            "/",
                            CodegenExpressionBuilder.Ref("d2Double")));
                return CodegenExpressionBuilder.LocalMethodBuild(method).Pass(left).Pass(right).Call();
            }
        }
    }
}