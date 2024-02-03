///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.enummethod.codegen.EnumForgeCodegenNames;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.twolambda.@base
{
    public abstract class TwoLambdaThreeFormEventPlus : EnumForgeBaseWFields
    {
        private readonly ExprForge _secondExpression;
        private readonly int _numParameters;

        public ExprForge SecondExpression => _secondExpression;

        public int NumParameters => _numParameters;

        protected TwoLambdaThreeFormEventPlus(
            ExprForge innerExpression,
            int streamNumLambda,
            ObjectArrayEventType indexEventType,
            ExprForge secondExpression,
            int numParameters) : base(innerExpression, streamNumLambda, indexEventType)
        {
            _secondExpression = secondExpression;
            _numParameters = numParameters;
        }

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

        public override CodegenExpression Codegen(
            EnumForgeCodegenParams premade,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var resultTypeMember = codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(ObjectArrayEventType),
                Cast(
                    typeof(ObjectArrayEventType),
                    EventTypeUtility.ResolveTypeCodegen(FieldEventType, EPStatementInitServicesConstants.REF)));

            var scope = new ExprForgeCodegenSymbol(false, null);
            var methodNode = codegenMethodScope
                .MakeChildWithScope(ReturnType(), GetType(), scope, codegenClassScope)
                .AddParam(ExprForgeCodegenNames.FP_EPS)
                .AddParam(premade.EnumcollType, REF_ENUMCOLL.Ref)
                .AddParam(ExprForgeCodegenNames.FP_ISNEWDATA)
                .AddParam(ExprForgeCodegenNames.FP_EXPREVALCONTEXT);
            var hasSize = _numParameters >= 3;

            var returnIfEmpty = ReturnIfEmptyOptional();
            if (returnIfEmpty != null) {
                methodNode.Block
                    .IfCondition(ExprDotMethod(REF_ENUMCOLL, "IsEmpty"))
                    .BlockReturn(returnIfEmpty);
            }

            InitBlock(methodNode.Block, methodNode, scope, codegenClassScope);

            methodNode.Block
                .CommentFullLine(MethodBase.GetCurrentMethod()!.DeclaringType!.FullName + "." + MethodBase.GetCurrentMethod()!.Name)
                .DeclareVar<ObjectArrayEventBean>(
                    "indexEvent",
                    NewInstance(
                        typeof(ObjectArrayEventBean),
                        NewArrayByLength(typeof(object), Constant(_numParameters - 1)),
                        resultTypeMember))
                .AssignArrayElement(REF_EPS, Constant(StreamNumLambda + 1), Ref("indexEvent"))
                .DeclareVar<object[]>("props", ExprDotName(Ref("indexEvent"), "Properties"))
                .DeclareVar<int>("count", Constant(-1));
            if (hasSize) {
                methodNode.Block.AssignArrayElement(Ref("props"), Constant(1), ExprDotName(REF_ENUMCOLL, "Count"));
            }

            var forEach = methodNode.Block.ForEach<EventBean>("next", REF_ENUMCOLL)
                .IncrementRef("count")
                .AssignArrayElement("props", Constant(0), Ref("count"))
                .AssignArrayElement(REF_EPS, Constant(StreamNumLambda), Ref("next"));
            ForEachBlock(forEach, methodNode, scope, codegenClassScope);

            ReturnResult(methodNode.Block);
            return LocalMethod(methodNode, premade.Eps, premade.Enumcoll, premade.IsNewData, premade.ExprCtx);
        }
    }
} // end of namespace