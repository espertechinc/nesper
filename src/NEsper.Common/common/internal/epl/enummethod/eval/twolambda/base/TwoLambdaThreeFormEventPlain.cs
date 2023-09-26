///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.enummethod.eval;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethodeval.twolambda.@base
{
    public abstract class TwoLambdaThreeFormEventPlain : EnumForgeBasePlain
    {
        private ExprForge _secondExpression;

        public ExprForge SecondExpression => _secondExpression;

        public abstract Type ReturnType();

        public abstract CodegenExpression ReturnIfEmptyOptional();

        public abstract void InitBlock(
            CodegenBlock block,
            CodegenMethod methodNode,
            ExprForgeCodegenSymbol scope,
            CodegenClassScope codegenClassScope);

        public abstract void ForEachBlock(
            CodegenBlock block,
            CodegenMethod methodNode,
            ExprForgeCodegenSymbol scope,
            CodegenClassScope codegenClassScope);

        public abstract void ReturnResult(CodegenBlock block);

        public TwoLambdaThreeFormEventPlain(
            ExprForge innerExpression,
            int streamCountIncoming,
            ExprForge secondExpression) : base(innerExpression, streamCountIncoming)
        {
            _secondExpression = secondExpression;
        }

        public override CodegenExpression Codegen(
            EnumForgeCodegenParams premade,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var scope = new ExprForgeCodegenSymbol(false, null);
            var methodNode = codegenMethodScope
                .MakeChildWithScope(ReturnType(), GetType(), scope, codegenClassScope)
                .AddParam(EnumForgeCodegenNames.PARAMS);

            var returnIfEmpty = ReturnIfEmptyOptional();
            if (returnIfEmpty != null) {
                methodNode.Block
                    .IfCondition(ExprDotMethod(EnumForgeCodegenNames.REF_ENUMCOLL, "IsEmpty"))
                    .BlockReturn(returnIfEmpty);
            }

            InitBlock(methodNode.Block, methodNode, scope, codegenClassScope);

            var forEach = methodNode.Block
                .ForEach(typeof(EventBean), "next", EnumForgeCodegenNames.REF_ENUMCOLL)
                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(StreamNumLambda), Ref("next"));
            ForEachBlock(forEach, methodNode, scope, codegenClassScope);

            ReturnResult(methodNode.Block);
            return LocalMethod(methodNode, premade.Eps, premade.Enumcoll, premade.IsNewData, premade.ExprCtx);
        }
    }
} // end of namespace