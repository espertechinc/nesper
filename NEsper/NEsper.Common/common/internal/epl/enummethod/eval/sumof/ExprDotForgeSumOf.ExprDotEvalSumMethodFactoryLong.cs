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

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public partial class ExprDotForgeSumOf
    {
        internal class ExprDotEvalSumMethodFactoryLong : ExprDotEvalSumMethodFactory
        {
            internal static readonly ExprDotEvalSumMethodFactoryLong INSTANCE = new ExprDotEvalSumMethodFactoryLong();

            private ExprDotEvalSumMethodFactoryLong()
            {
            }

            public ExprDotEvalSumMethod SumAggregator => new ExprDotEvalSumMethodLong();

            public Type ValueType => typeof(long);

            public void CodegenDeclare(CodegenBlock block)
            {
                block.DeclareVar<long>("sum", Constant(0));
                block.DeclareVar<long>("cnt", Constant(0));
            }

            public void CodegenEnterNumberTypedNonNull(
                CodegenBlock block,
                CodegenExpressionRef value)
            {
                block.IncrementRef("cnt");
                block.AssignCompound("sum", "+", Unbox(value));
            }

            public void CodegenEnterObjectTypedNonNull(
                CodegenBlock block,
                CodegenExpressionRef value)
            {
                block.IncrementRef("cnt");
                block.AssignCompound("sum", "+", StaticMethod(typeof(TypeExtensions), "AsInt64", value));
            }

            public void CodegenReturn(CodegenBlock block)
            {
                CodegenReturnSumOrNull(block);
            }
        }
    }
}