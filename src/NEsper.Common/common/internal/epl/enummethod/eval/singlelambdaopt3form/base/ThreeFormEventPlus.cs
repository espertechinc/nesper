///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.core;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.enummethod.codegen.EnumForgeCodegenNames; //REF_ENUMCOLL

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.@base {
    public abstract class ThreeFormEventPlus : EnumForgeBaseWFields {
        protected readonly int NumParameters;

        public abstract Type ReturnTypeOfMethod(Type desiredReturnType);

        public abstract CodegenExpression ReturnIfEmptyOptional(Type desiredReturnType);

        public abstract void InitBlock(
            CodegenBlock block,
            CodegenMethod methodNode,
            ExprForgeCodegenSymbol scope,
            CodegenClassScope codegenClassScope,
            Type desiredReturnType);

        public virtual bool HasForEachLoop()
        {
            return true;
        }

        public abstract void ForEachBlock(
            CodegenBlock block,
            CodegenMethod methodNode,
            ExprForgeCodegenSymbol scope,
            CodegenClassScope codegenClassScope,
            Type desiredReturnType);

        public abstract void ReturnResult(CodegenBlock block);

        public ThreeFormEventPlus(
            ExprDotEvalParamLambda lambda,
            ObjectArrayEventType indexEventType,
            int numParameters)
            : base(lambda, indexEventType)
        {
            this.NumParameters = numParameters;
        }

        public override CodegenExpression Codegen(
            EnumForgeCodegenParams premade,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var indexTypeMember = codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(ObjectArrayEventType),
                Cast(
                    typeof(ObjectArrayEventType),
                    EventTypeUtility.ResolveTypeCodegen(FieldEventType, EPStatementInitServicesConstants.REF)));

            var scope = new ExprForgeCodegenSymbol(false, null);
            var returnType = ReturnTypeOfMethod(premade.DesiredReturnType);
            var methodNode = codegenMethodScope
                .MakeChildWithScope(returnType, GetType(), scope, codegenClassScope)
                .AddParam(ExprForgeCodegenNames.FP_EPS)
                .AddParam(premade.EnumcollType, REF_ENUMCOLL.Ref)
                .AddParam(ExprForgeCodegenNames.FP_ISNEWDATA)
                .AddParam(ExprForgeCodegenNames.FP_EXPREVALCONTEXT);
            var block = methodNode.Block;

            var returnEmpty = ReturnIfEmptyOptional(premade.DesiredReturnType);
            if (returnEmpty != null) {
                block
                    .IfCondition(ExprDotMethod(REF_ENUMCOLL, "IsEmpty"))
                    .BlockReturn(returnEmpty);
            }

            block
                .CommentFullLine(MethodBase.GetCurrentMethod()!.DeclaringType!.FullName + "." +
                                 MethodBase.GetCurrentMethod()!.Name)
                .DeclareVar<ObjectArrayEventBean>(
                    "indexEvent",
                    NewInstance(
                        typeof(ObjectArrayEventBean),
                        NewArrayByLength(typeof(object), Constant(NumParameters - 1)),
                        indexTypeMember))
                .AssignArrayElement(REF_EPS, Constant(StreamNumLambda + 1), Ref("indexEvent"))
                .DeclareVar<object[]>("props", ExprDotName(Ref("indexEvent"), "Properties"));
            block.DeclareVar<int>("count", Constant(-1));
            if (NumParameters == 3) {
                block.AssignArrayElement(Ref("props"), Constant(1), ExprDotName(REF_ENUMCOLL, "Count"));
            }

            InitBlock(block, methodNode, scope, codegenClassScope, premade.DesiredReturnType);

            if (HasForEachLoop()) {
                var forEach = block.ForEach<EventBean>("next", REF_ENUMCOLL)
                    .IncrementRef("count")
                    .AssignArrayElement("props", Constant(0), Ref("count"))
                    .AssignArrayElement(REF_EPS, Constant(StreamNumLambda), Ref("next"));
                ForEachBlock(forEach, methodNode, scope, codegenClassScope, premade.DesiredReturnType);
            }

            ReturnResult(block);
            return LocalMethod(methodNode, premade.Eps, premade.Enumcoll, premade.IsNewData, premade.ExprCtx);
        }
    }
} // end of namespace