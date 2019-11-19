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
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public partial class ExprDotForgeSumOf
    {
        internal class ExprDotEvalSumMethodFactoryBigInteger : ExprDotEvalSumMethodFactory
        {
            internal static readonly ExprDotEvalSumMethodFactoryBigInteger INSTANCE =
                new ExprDotEvalSumMethodFactoryBigInteger();

            private ExprDotEvalSumMethodFactoryBigInteger()
            {
            }

            public ExprDotEvalSumMethod SumAggregator => new ExprDotEvalSumMethodBigInteger();

            public Type ValueType => typeof(BigInteger);

            public void CodegenDeclare(CodegenBlock block)
            {
                block.DeclareVar<BigInteger>("sum", EnumValue(typeof(BigInteger), "Zero"));
                block.DeclareVar<long>("cnt", Constant(0));
            }

            public void CodegenEnterNumberTypedNonNull(
                CodegenBlock block,
                CodegenExpressionRef value)
            {
                block.Increment("cnt");
                block.AssignCompound("sum", "+", Unbox(value));
            }

            public void CodegenEnterObjectTypedNonNull(
                CodegenBlock block,
                CodegenExpressionRef value)
            {
                block.Increment("cnt");
                block.AssignCompound("sum", "+", StaticMethod(typeof(TypeExtensions), "AsBigInteger", value));
            }

            public void CodegenReturn(CodegenBlock block)
            {
                CodegenReturnSumOrNull(block);
            }
        }
    }
}