///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.core;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.enummethod.codegen.EnumForgeCodegenNames; //REF_ENUMCOLL;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.@base {
    public abstract class ThreeFormScalar : EnumForgeBasePlain {
        protected readonly ObjectArrayEventType fieldEventType;
        protected readonly int numParameters;

        public abstract Type ReturnTypeOfMethod(Type inputCollectionType);
        public abstract CodegenExpression ReturnIfEmptyOptional(Type inputCollectionType);

        public abstract void InitBlock(
            CodegenBlock block,
            CodegenMethod methodNode,
            ExprForgeCodegenSymbol scope,
            CodegenClassScope codegenClassScope,
            Type inputCollectionType);

        public virtual bool HasForEachLoop()
        {
            return true;
        }

        public abstract void ForEachBlock(
            CodegenBlock block,
            CodegenMethod methodNode,
            ExprForgeCodegenSymbol scope,
            CodegenClassScope codegenClassScope,
            Type inputCollectionType);

        public abstract void ReturnResult(CodegenBlock block);

        public ThreeFormScalar(
            ExprDotEvalParamLambda lambda,
            ObjectArrayEventType fieldEventType,
            int numParameters) : base(lambda)
        {
            this.fieldEventType = fieldEventType;
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
                Cast(typeof(ObjectArrayEventType),
                    EventTypeUtility.ResolveTypeCodegen(fieldEventType, EPStatementInitServicesConstants.REF)));
            var scope = new ExprForgeCodegenSymbol(false, null);
            var methodNode = codegenMethodScope
                .MakeChildWithScope(ReturnTypeOfMethod(premade.EnumcollType), GetType(), scope, codegenClassScope)
                .AddParam(ExprForgeCodegenNames.FP_EPS)
                .AddParam(premade.EnumcollType, REF_ENUMCOLL.Ref)
                .AddParam(ExprForgeCodegenNames.FP_ISNEWDATA)
                .AddParam(ExprForgeCodegenNames.FP_EXPREVALCONTEXT);

            var block = methodNode.Block;
            var hasIndex = numParameters >= 2;
            var hasSize = numParameters >= 3;
            var returnEmpty = ReturnIfEmptyOptional(premade.EnumcollType);
            if (returnEmpty != null) {
                block
                    .IfCondition(ExprDotMethod(REF_ENUMCOLL, "IsEmpty"))
                    .BlockReturn(returnEmpty);
            }

            block
                .CommentFullLine(MethodBase.GetCurrentMethod()!.DeclaringType!.FullName + "." +
                                 MethodBase.GetCurrentMethod()!.Name)
                .DeclareVar<ObjectArrayEventBean>("resultEvent",
                    NewInstance(
                        typeof(ObjectArrayEventBean),
                        NewArrayByLength(typeof(object), Constant(numParameters)),
                        resultTypeMember))
                .AssignArrayElement(REF_EPS, Constant(StreamNumLambda), Ref("resultEvent"))
                .DeclareVar<object[]>("props", ExprDotName(Ref("resultEvent"), "Properties"));
            if (hasIndex) {
                block.DeclareVar<int>("count", Constant(-1));
            }

            if (hasSize) {
                block.AssignArrayElement(Ref("props"), Constant(2), ExprDotName(REF_ENUMCOLL, "Count"));
            }

            InitBlock(block, methodNode, scope, codegenClassScope, premade.EnumcollType);
            if (HasForEachLoop()) {
                var forEach = block
                    .ForEachVar("next", REF_ENUMCOLL)
                    .AssignArrayElement("props", Constant(0), Ref("next"));
                if (hasIndex) {
                    forEach.IncrementRef("count").AssignArrayElement("props", Constant(1), Ref("count"));
                }

                ForEachBlock(forEach, methodNode, scope, codegenClassScope, premade.EnumcollType);
            }

            ReturnResult(block);
            return LocalMethod(methodNode, premade.Eps, premade.Enumcoll, premade.IsNewData, premade.ExprCtx);
        }

        public int NumParameters => numParameters;
    }
} // end of namespace