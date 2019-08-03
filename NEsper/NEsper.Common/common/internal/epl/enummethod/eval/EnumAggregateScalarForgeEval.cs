///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class EnumAggregateScalarForgeEval : EnumEval
    {
        private readonly EnumAggregateScalarForge forge;
        private readonly ExprEvaluator initialization;
        private readonly ExprEvaluator innerExpression;

        public EnumAggregateScalarForgeEval(
            EnumAggregateScalarForge forge,
            ExprEvaluator initialization,
            ExprEvaluator innerExpression)
        {
            this.forge = forge;
            this.initialization = initialization;
            this.innerExpression = innerExpression;
        }

        public object EvaluateEnumMethod(
            EventBean[] eventsLambda,
            ICollection<object> enumcoll,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            object value = initialization.Evaluate(eventsLambda, isNewData, context);

            if (enumcoll.IsEmpty()) {
                return value;
            }

            ObjectArrayEventBean resultEvent = new ObjectArrayEventBean(new object[1], forge.resultEventType);
            ObjectArrayEventBean evalEvent = new ObjectArrayEventBean(new object[1], forge.EvalEventType);
            eventsLambda[forge.streamNumLambda] = resultEvent;
            eventsLambda[forge.streamNumLambda + 1] = evalEvent;
            object[] resultProps = resultEvent.Properties;
            object[] evalProps = evalEvent.Properties;

            foreach (object next in enumcoll) {
                resultProps[0] = value;
                evalProps[0] = next;
                value = innerExpression.Evaluate(eventsLambda, isNewData, context);
            }

            return value;
        }

        public static CodegenExpression Codegen(
            EnumAggregateScalarForge forge,
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            CodegenExpressionField resultTypeMember = codegenClassScope.AddFieldUnshared(
                true,
                typeof(ObjectArrayEventType),
                Cast(
                    typeof(ObjectArrayEventType),
                    EventTypeUtility.ResolveTypeCodegen(forge.resultEventType, EPStatementInitServicesConstants.REF)));
            CodegenExpressionField evalTypeMember = codegenClassScope.AddFieldUnshared(
                true,
                typeof(ObjectArrayEventType),
                Cast(
                    typeof(ObjectArrayEventType),
                    EventTypeUtility.ResolveTypeCodegen(forge.evalEventType, EPStatementInitServicesConstants.REF)));

            ExprForgeCodegenSymbol scope = new ExprForgeCodegenSymbol(false, null);
            CodegenMethod methodNode = codegenMethodScope.MakeChildWithScope(
                    forge.initialization.EvaluationType,
                    typeof(EnumAggregateScalarForgeEval),
                    scope,
                    codegenClassScope)
                .AddParam(EnumForgeCodegenNames.PARAMS);

            Type initializationEvalType = forge.initialization.EvaluationType;
            Type innerEvalType = forge.innerExpression.EvaluationType;
            CodegenBlock block = methodNode.Block;
            block.DeclareVar(
                    initializationEvalType,
                    "value",
                    forge.initialization.EvaluateCodegen(initializationEvalType, methodNode, scope, codegenClassScope))
                .IfCondition(ExprDotMethod(EnumForgeCodegenNames.REF_ENUMCOLL, "IsEmpty"))
                .BlockReturn(@Ref("value"));
            block.DeclareVar<ObjectArrayEventBean>(
                    "resultEvent",
                    NewInstance<ObjectArrayEventBean>(NewArrayByLength(typeof(object), Constant(1)), resultTypeMember))
                .DeclareVar<ObjectArrayEventBean>(
                    "evalEvent",
                    NewInstance<ObjectArrayEventBean>(NewArrayByLength(typeof(object), Constant(1)), evalTypeMember))
                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(forge.streamNumLambda), @Ref("resultEvent"))
                .AssignArrayElement(
                    EnumForgeCodegenNames.REF_EPS,
                    Constant(forge.streamNumLambda + 1),
                    @Ref("evalEvent"))
                .DeclareVar<object[]>("resultProps", ExprDotName(@Ref("resultEvent"), "Properties"))
                .DeclareVar<object[]>("evalProps", ExprDotName(@Ref("evalEvent"), "Properties"));
            block.ForEach(typeof(object), "next", EnumForgeCodegenNames.REF_ENUMCOLL)
                .AssignArrayElement("resultProps", Constant(0), @Ref("value"))
                .AssignArrayElement("evalProps", Constant(0), @Ref("next"))
                .AssignRef(
                    "value",
                    forge.innerExpression.EvaluateCodegen(innerEvalType, methodNode, scope, codegenClassScope))
                .BlockEnd();
            block.MethodReturn(@Ref("value"));
            return LocalMethod(methodNode, args.Eps, args.Enumcoll, args.IsNewData, args.ExprCtx);
        }
    }
} // end of namespace