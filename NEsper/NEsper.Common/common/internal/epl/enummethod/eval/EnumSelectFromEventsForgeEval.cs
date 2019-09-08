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
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class EnumSelectFromEventsForgeEval : EnumEval
    {
        private readonly EnumSelectFromEventsForge forge;
        private readonly ExprEvaluator innerExpression;

        public EnumSelectFromEventsForgeEval(
            EnumSelectFromEventsForge forge,
            ExprEvaluator innerExpression)
        {
            this.forge = forge;
            this.innerExpression = innerExpression;
        }

        public object EvaluateEnumMethod(
            EventBean[] eventsLambda,
            ICollection<object> enumcoll,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            if (enumcoll.IsEmpty()) {
                return enumcoll;
            }

            ICollection<EventBean> beans = (ICollection<EventBean>) enumcoll;
            Deque<object> result = new ArrayDeque<object>(enumcoll.Count);
            foreach (EventBean next in beans) {
                eventsLambda[forge.streamNumLambda] = next;

                object item = innerExpression.Evaluate(eventsLambda, isNewData, context);
                if (item != null) {
                    result.Add(item);
                }
            }

            return result;
        }

        public static CodegenExpression Codegen(
            EnumSelectFromEventsForge forge,
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            ExprForgeCodegenSymbol scope = new ExprForgeCodegenSymbol(false, null);
            CodegenMethod methodNode = codegenMethodScope
                .MakeChildWithScope(
                    typeof(ICollection<object>),
                    typeof(EnumSelectFromEventsForgeEval),
                    scope,
                    codegenClassScope)
                .AddParam(EnumForgeCodegenNames.PARAMS);

            CodegenBlock block = methodNode.Block
                .IfCondition(ExprDotMethod(EnumForgeCodegenNames.REF_ENUMCOLL, "IsEmpty"))
                .BlockReturn(StaticMethod(typeof(CompatExtensions), "Unwrap", new [] { typeof(object) }, EnumForgeCodegenNames.REF_ENUMCOLL))
                .DeclareVar<ArrayDeque<object>>(
                    "result",
                    NewInstance<ArrayDeque<object>>(ExprDotName(EnumForgeCodegenNames.REF_ENUMCOLL, "Count")));
            CodegenBlock forEach = block.ForEach(typeof(EventBean), "next", EnumForgeCodegenNames.REF_ENUMCOLL)
                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(forge.streamNumLambda), @Ref("next"))
                .DeclareVar<object>(
                    "item",
                    forge.innerExpression.EvaluateCodegen(typeof(object), methodNode, scope, codegenClassScope))
                .IfCondition(NotEqualsNull(@Ref("item")))
                .Expression(ExprDotMethod(@Ref("result"), "Add", @Ref("item")));
            block.MethodReturn(@Ref("result"));
            return LocalMethod(methodNode, args.Eps, args.Enumcoll, args.IsNewData, args.ExprCtx);
        }
    }
} // end of namespace