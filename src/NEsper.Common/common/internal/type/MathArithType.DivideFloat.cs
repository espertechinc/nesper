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
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.type
{
    public partial class MathArithType
    {
        /// <summary>
        ///     Computer for type-specific arith. operations.
        /// </summary>
        public class DivideFloat : Computer
        {
            public object Compute(
                object d1,
                object d2)
            {
                if (d1 == null || d2 == null) {
                    return null;
                }

                var d2Float = d2.AsFloat();
                if (d2Float == 0) {
                    return null;
                }

                return d1.AsFloat() / d2Float;
            }

            public CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope,
                CodegenExpressionRef left,
                CodegenExpressionRef right,
                Type ltype,
                Type rtype)
            {
                return Op(CodegenAsFloat(left, ltype), "/", CodegenAsFloat(right, rtype));
            }
        }
    }
}