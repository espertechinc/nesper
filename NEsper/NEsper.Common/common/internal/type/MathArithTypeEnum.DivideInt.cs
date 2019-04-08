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
    public partial class MathArithTypeEnum
    {
        /// <summary>
        ///     Computer for type-specific arith. operations.
        /// </summary>
        public class DivideInt : Computer
        {
            public object Compute(
                object i1,
                object i2)
            {
                var i2int = i2.AsInt();
                if (i2int == 0) {
                    return null;
                }

                return i1.AsInt() / i2int;
            }

            public CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope,
                CodegenExpressionRef left,
                CodegenExpressionRef right,
                Type ltype,
                Type rtype)
            {
                var method = codegenMethodScope.MakeChild(typeof(int?), typeof(DivideInt), codegenClassScope)
                    .AddParam(typeof(int), "i1").AddParam(typeof(int), "i2").Block
                    .IfCondition(CodegenExpressionBuilder.EqualsIdentity(CodegenExpressionBuilder.Ref("i2"), CodegenExpressionBuilder.Constant(0)))
                    .BlockReturn(CodegenExpressionBuilder.ConstantNull())
                    .MethodReturn(CodegenExpressionBuilder.Op(CodegenExpressionBuilder.Ref("i1"), "/", CodegenExpressionBuilder.Ref("i2")));
                return CodegenExpressionBuilder.LocalMethod(method, CodegenAsInt(left, ltype), CodegenAsInt(right, rtype));
            }
        }
    }
}