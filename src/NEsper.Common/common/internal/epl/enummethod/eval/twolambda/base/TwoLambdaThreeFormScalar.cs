///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

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
    public abstract class TwoLambdaThreeFormScalar : EnumForgeBasePlain
    {
        private readonly ExprForge secondExpression;
        private readonly ObjectArrayEventType resultEventType;
        private readonly int numParameters;

        public ExprForge SecondExpression => secondExpression;

        public ObjectArrayEventType ResultEventType => resultEventType;

        public int NumParameters => numParameters;

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

        public TwoLambdaThreeFormScalar(
            ExprForge innerExpression,
            int streamCountIncoming,
            ExprForge secondExpression,
            ObjectArrayEventType resultEventType,
            int numParameters) : base(innerExpression, streamCountIncoming)
        {
            this.secondExpression = secondExpression;
            this.resultEventType = resultEventType;
            this.numParameters = numParameters;
        }

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
                    EventTypeUtility.ResolveTypeCodegen(resultEventType, EPStatementInitServicesConstants.REF)));

            var scope = new ExprForgeCodegenSymbol(false, null);
            var methodNode = codegenMethodScope
                .MakeChildWithScope(typeof(IDictionary<object, object>), GetType(), scope, codegenClassScope)
                .AddParam(ExprForgeCodegenNames.FP_EPS)
                .AddParam(premade.EnumcollType, EnumForgeCodegenNames.REF_ENUMCOLL.Ref)
                .AddParam(ExprForgeCodegenNames.FP_ISNEWDATA)
                .AddParam(ExprForgeCodegenNames.FP_EXPREVALCONTEXT);
            var hasIndex = numParameters >= 2;
            var hasSize = numParameters >= 3;

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
                    "resultEvent",
                    NewInstance(
                        typeof(ObjectArrayEventBean),
                        NewArrayByLength(typeof(object), Constant(numParameters)),
                        resultTypeMember))
                .AssignArrayElement(REF_EPS, Constant(StreamNumLambda), Ref("resultEvent"))
                .DeclareVar<object[]>("props", ExprDotName(Ref("resultEvent"), "Properties"));
            if (hasIndex) {
                methodNode.Block.DeclareVar<int>("count", Constant(-1));
            }

            if (hasSize) {
                methodNode.Block.AssignArrayElement(Ref("props"), Constant(2), ExprDotName(REF_ENUMCOLL, "Count"));
            }

            var forEach = methodNode.Block.ForEach<object>("next", REF_ENUMCOLL)
                .AssignArrayElement("props", Constant(0), Ref("next"));
            if (hasIndex) {
                forEach.IncrementRef("count").AssignArrayElement("props", Constant(1), Ref("count"));
            }

            ForEachBlock(forEach, methodNode, scope, codegenClassScope);

            ReturnResult(methodNode.Block);
            return LocalMethod(methodNode, premade.Eps, premade.Enumcoll, premade.IsNewData, premade.ExprCtx);
        }
    }
} // end of namespace