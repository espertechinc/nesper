///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

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
    public class EnumTakeWhileEventsForgeEval : EnumEval
    {
        private readonly EnumTakeWhileEventsForge forge;
        private readonly ExprEvaluator innerExpression;

        public EnumTakeWhileEventsForgeEval(
            EnumTakeWhileEventsForge forge,
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
            if (enumcoll.Count == 1) {
                EventBean item = beans.First();
                eventsLambda[forge.streamNumLambda] = item;

                object pass = innerExpression.Evaluate(eventsLambda, isNewData, context);
                if (pass == null || false.Equals(pass)) {
                    return Collections.GetEmptyList<object>();
                }

                return Collections.SingletonList(item);
            }

            ArrayDeque<object> result = new ArrayDeque<object>();

            foreach (EventBean next in beans) {
                eventsLambda[forge.streamNumLambda] = next;

                object pass = innerExpression.Evaluate(eventsLambda, isNewData, context);
                if (pass == null || false.Equals(pass)) {
                    break;
                }

                result.Add(next);
            }

            return result;
        }

        public static CodegenExpression Codegen(
            EnumTakeWhileEventsForge forge,
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            ExprForgeCodegenSymbol scope = new ExprForgeCodegenSymbol(false, null);
            CodegenMethod methodNode = codegenMethodScope
                .MakeChildWithScope(
                    typeof(ICollection<object>),
                    typeof(EnumTakeWhileEventsForgeEval),
                    scope,
                    codegenClassScope)
                .AddParam(EnumForgeCodegenNames.PARAMS);
            CodegenExpression innerValue = forge.innerExpression.EvaluateCodegen(
                typeof(bool?),
                methodNode,
                scope,
                codegenClassScope);

            CodegenBlock block = methodNode.Block
                .IfCondition(ExprDotMethod(EnumForgeCodegenNames.REF_ENUMCOLL, "IsEmpty"))
                .BlockReturn(EnumForgeCodegenNames.REF_ENUMCOLL);

            CodegenBlock blockSingle = block
                .IfCondition(EqualsIdentity(ExprDotMethod(EnumForgeCodegenNames.REF_ENUMCOLL, "Size"), Constant(1)))
                .DeclareVar<EventBean>(
                    "item",
                    Cast(
                        typeof(EventBean),
                        ExprDotMethodChain(EnumForgeCodegenNames.REF_ENUMCOLL).Add("First")))
                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(forge.streamNumLambda), @Ref("item"));
            CodegenLegoBooleanExpression.CodegenReturnValueIfNotNullAndNotPass(
                blockSingle,
                forge.innerExpression.EvaluationType,
                innerValue,
                StaticMethod(typeof(Collections), "EmptyList"));
            blockSingle.BlockReturn(StaticMethod(typeof(Collections), "SingletonList", @Ref("item")));

            block.DeclareVar<ArrayDeque<object>>("result", NewInstance(typeof(ArrayDeque<object>)));
            CodegenBlock forEach = block.ForEach(typeof(EventBean), "next", EnumForgeCodegenNames.REF_ENUMCOLL)
                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(forge.streamNumLambda), @Ref("next"));
            CodegenLegoBooleanExpression.CodegenBreakIfNotNullAndNotPass(
                forEach,
                forge.innerExpression.EvaluationType,
                innerValue);
            forEach.Expression(ExprDotMethod(@Ref("result"), "Add", @Ref("next")));
            block.MethodReturn(@Ref("result"));
            return LocalMethod(methodNode, args.Eps, args.Enumcoll, args.IsNewData, args.ExprCtx);
        }
    }
} // end of namespace