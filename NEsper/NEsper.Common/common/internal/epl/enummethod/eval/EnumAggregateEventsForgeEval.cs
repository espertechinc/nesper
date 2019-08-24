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
    public class EnumAggregateEventsForgeEval : EnumEval
    {
        private readonly EnumAggregateEventsForge forge;
        private readonly ExprEvaluator initialization;
        private readonly ExprEvaluator innerExpression;

        public EnumAggregateEventsForgeEval(
            EnumAggregateEventsForge forge,
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
            var value = initialization.Evaluate(eventsLambda, isNewData, context);

            if (enumcoll.IsEmpty()) {
                return value;
            }

            var beans = (ICollection<EventBean>) enumcoll;
            var resultEvent = new ObjectArrayEventBean(new object[1], forge.resultEventType);
            eventsLambda[forge.streamNumLambda] = resultEvent;
            var props = resultEvent.Properties;

            foreach (var next in beans) {
                props[0] = value;
                eventsLambda[forge.streamNumLambda + 1] = next;
                value = innerExpression.Evaluate(eventsLambda, isNewData, context);
            }

            return value;
        }

        public static CodegenExpression Codegen(
            EnumAggregateEventsForge forge,
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var typeMember = codegenClassScope.AddFieldUnshared(
                true,
                typeof(ObjectArrayEventType),
                Cast(
                    typeof(ObjectArrayEventType),
                    EventTypeUtility.ResolveTypeCodegen(forge.resultEventType, EPStatementInitServicesConstants.REF)));

            var scope = new ExprForgeCodegenSymbol(false, null);
            var methodNode = codegenMethodScope.MakeChildWithScope(
                    forge.initialization.EvaluationType,
                    typeof(EnumAggregateEventsForgeEval),
                    scope,
                    codegenClassScope)
                .AddParam(EnumForgeCodegenNames.PARAMS);

            var initType = forge.initialization.EvaluationType;
            var initTypeBoxed = initType.GetBoxedType();
            var unboxRequired = initType != initTypeBoxed;
            var innerType = forge.innerExpression.EvaluationType;

            var block = methodNode.Block;
            block.DeclareVar(
                    initTypeBoxed,
                    "value",
                    forge.initialization.EvaluateCodegen(initType, methodNode, scope, codegenClassScope))
                .IfCondition(ExprDotMethod(EnumForgeCodegenNames.REF_ENUMCOLL, "IsEmpty"))
                .BlockReturn(
                    unboxRequired
                        ? (CodegenExpression) ExprDotName(@Ref("value"), "Value")
                        : (CodegenExpression) @Ref("value"));
            block.DeclareVar<ObjectArrayEventBean>(
                    "resultEvent",
                    NewInstance<ObjectArrayEventBean>(NewArrayByLength(typeof(object), Constant(1)), typeMember))
                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(forge.streamNumLambda), @Ref("resultEvent"))
                .DeclareVar<object[]>("props", ExprDotName(@Ref("resultEvent"), "Properties"));
            block.ForEach(typeof(EventBean), "next", EnumForgeCodegenNames.REF_ENUMCOLL)
                .AssignArrayElement("props", Constant(0), @Ref("value"))
                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(forge.streamNumLambda + 1), @Ref("next"))
                .AssignRef(
                    "value",
                    forge.innerExpression.EvaluateCodegen(innerType, methodNode, scope, codegenClassScope))
                .BlockEnd();
            block.MethodReturn(
                unboxRequired
                    ? (CodegenExpression) ExprDotName(@Ref("value"), "Value")
                    : (CodegenExpression) @Ref("value"));
            return LocalMethod(methodNode, args.Eps, args.Enumcoll, args.IsNewData, args.ExprCtx);
        }
    }
} // end of namespace