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
    public class EnumTakeWhileIndexEventsForgeEval : EnumEval
    {
        private readonly EnumTakeWhileIndexEventsForge forge;
        private readonly ExprEvaluator innerExpression;

        public EnumTakeWhileIndexEventsForgeEval(
            EnumTakeWhileIndexEventsForge forge,
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

            ObjectArrayEventBean indexEvent = new ObjectArrayEventBean(new object[1], forge.indexEventType);
            eventsLambda[forge.streamNumLambda + 1] = indexEvent;
            object[] props = indexEvent.Properties;

            ICollection<EventBean> beans = (ICollection<EventBean>) enumcoll;
            if (enumcoll.Count == 1) {
                EventBean item = beans.First();
                props[0] = 0;
                eventsLambda[forge.streamNumLambda] = item;

                object pass = innerExpression.Evaluate(eventsLambda, isNewData, context);
                if (pass == null || false.Equals(pass)) {
                    return Collections.GetEmptyList<object>();
                }

                return Collections.SingletonList(item);
            }

            ArrayDeque<object> result = new ArrayDeque<object>();

            int count = -1;
            foreach (EventBean next in beans) {
                count++;

                props[0] = count;
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
            EnumTakeWhileIndexEventsForge forge,
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            CodegenExpressionField indexTypeMember = codegenClassScope.AddFieldUnshared(
                true, typeof(ObjectArrayEventType),
                Cast(typeof(ObjectArrayEventType), EventTypeUtility.ResolveTypeCodegen(forge.indexEventType, EPStatementInitServicesConstants.REF)));

            ExprForgeCodegenSymbol scope = new ExprForgeCodegenSymbol(false, null);
            CodegenMethod methodNode = codegenMethodScope
                .MakeChildWithScope(typeof(ICollection<object>), typeof(EnumTakeWhileIndexEventsForgeEval), scope, codegenClassScope)
                .AddParam(EnumForgeCodegenNames.PARAMS);

            CodegenExpression innerValue = forge.innerExpression.EvaluateCodegen(typeof(bool?), methodNode, scope, codegenClassScope);
            CodegenBlock block = methodNode.Block
                .IfCondition(ExprDotMethod(EnumForgeCodegenNames.REF_ENUMCOLL, "isEmpty"))
                .BlockReturn(EnumForgeCodegenNames.REF_ENUMCOLL)
                .DeclareVar(
                    typeof(ObjectArrayEventBean), "indexEvent",
                    NewInstance<ObjectArrayEventBean>(NewArrayByLength(typeof(object), Constant(1)), indexTypeMember))
                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(forge.streamNumLambda + 1), @Ref("indexEvent"))
                .DeclareVar(typeof(object[]), "props", ExprDotMethod(@Ref("indexEvent"), "getProperties"));

            CodegenBlock blockSingle = block.IfCondition(EqualsIdentity(ExprDotMethod(EnumForgeCodegenNames.REF_ENUMCOLL, "size"), Constant(1)))
                .DeclareVar(
                    typeof(EventBean), "item",
                    Cast(typeof(EventBean), ExprDotMethodChain(EnumForgeCodegenNames.REF_ENUMCOLL).Add("iterator").Add("next")))
                .AssignArrayElement("props", Constant(0), Constant(0))
                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(forge.streamNumLambda), @Ref("item"));
            CodegenLegoBooleanExpression.CodegenReturnValueIfNotNullAndNotPass(
                blockSingle, forge.innerExpression.EvaluationType, innerValue, StaticMethod(typeof(Collections), "emptyList"));
            blockSingle.BlockReturn(StaticMethod(typeof(Collections), "singletonList", @Ref("item")));

            block.DeclareVar(typeof(ArrayDeque<object>), "result", NewInstance(typeof(ArrayDeque<object>)))
                .DeclareVar(typeof(int), "count", Constant(-1));
            CodegenBlock forEach = block.ForEach(typeof(EventBean), "next", EnumForgeCodegenNames.REF_ENUMCOLL)
                .Increment("count")
                .AssignArrayElement("props", Constant(0), @Ref("count"))
                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(forge.streamNumLambda), @Ref("next"));
            CodegenLegoBooleanExpression.CodegenBreakIfNotNullAndNotPass(forEach, forge.innerExpression.EvaluationType, innerValue);
            forEach.Expression(ExprDotMethod(@Ref("result"), "add", @Ref("next")));
            block.MethodReturn(@Ref("result"));
            return LocalMethod(methodNode, args.Eps, args.Enumcoll, args.IsNewData, args.ExprCtx);
        }
    }
} // end of namespace