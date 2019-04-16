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

namespace com.espertech.esper.common.@internal.type
{
    public partial class MathArithTypeEnum
    {
        /// <summary>
        ///     Computer for type-specific arith. operations.
        /// </summary>
        public class MultiplyBigInt : Computer
        {
            public object Compute(
                object d1,
                object d2)
            {
                var b1 = (BigInteger) d1;
                var b2 = (BigInteger) d2;
                return b1 * b2;
            }

            public CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope,
                CodegenExpressionRef left,
                CodegenExpressionRef right,
                Type ltype,
                Type rtype)
            {
                return CodegenExpressionBuilder.ExprDotMethod(left, "multiply", right);
            }
        }
    }
}