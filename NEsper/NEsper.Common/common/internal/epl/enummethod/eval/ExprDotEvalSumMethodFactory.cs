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

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public interface ExprDotEvalSumMethodFactory
    {
        ExprDotEvalSumMethod SumAggregator { get; }
        Type ValueType { get; }
        void CodegenDeclare(CodegenBlock block);

        void CodegenEnterNumberTypedNonNull(
            CodegenBlock block,
            CodegenExpressionRef value);

        void CodegenEnterObjectTypedNonNull(
            CodegenBlock block,
            CodegenExpressionRef value);

        void CodegenReturn(CodegenBlock block);
    }

    public class ProxyExprDotEvalSumMethodFactory : ExprDotEvalSumMethodFactory
    {
        public Func<ExprDotEvalSumMethod> ProcSumAggregator;
        public ExprDotEvalSumMethod SumAggregator => ProcSumAggregator?.Invoke();

        public Func<Type> ProcValueType;
        public Type ValueType => ProcValueType?.Invoke();

        public Action<CodegenBlock> ProcCodegenDeclare;
        public void CodegenDeclare(CodegenBlock block) => ProcCodegenDeclare?.Invoke(block);

        public Action<CodegenBlock, CodegenExpressionRef> ProcCodegenEnterNumberTypedNonNull;

        public void CodegenEnterNumberTypedNonNull(
            CodegenBlock block,
            CodegenExpressionRef value) => ProcCodegenEnterNumberTypedNonNull.Invoke(block, value);

        public Action<CodegenBlock, CodegenExpressionRef> ProcCodegenEnterObjectTypedNonNull;

        public void CodegenEnterObjectTypedNonNull(
            CodegenBlock block,
            CodegenExpressionRef value) => ProcCodegenEnterObjectTypedNonNull?.Invoke(block, value);

        public Action<CodegenBlock> ProcCodegenReturn;
        public void CodegenReturn(CodegenBlock block) => ProcCodegenReturn?.Invoke(block);
    }
}