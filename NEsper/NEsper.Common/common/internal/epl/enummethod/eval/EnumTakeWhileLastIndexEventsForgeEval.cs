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
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.
    CodegenRelational;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class EnumTakeWhileLastIndexEventsForgeEval : EnumEval
    {
        private readonly EnumTakeWhileLastIndexEventsForge forge;
        private readonly ExprEvaluator innerExpression;

        public EnumTakeWhileLastIndexEventsForgeEval(
            EnumTakeWhileLastIndexEventsForge forge,
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

            var indexEvent = new ObjectArrayEventBean(new object[1], forge.indexEventType);
            eventsLambda[forge.streamNumLambda + 1] = indexEvent;
            var props = indexEvent.Properties;

            var beans = (ICollection<EventBean>) enumcoll;
            if (enumcoll.Count == 1) {
                var item = beans.First();
                props[0] = 0;
                eventsLambda[forge.streamNumLambda] = item;

                var pass = innerExpression.Evaluate(eventsLambda, isNewData, context);
                if (pass == null || false.Equals(pass)) {
                    return Collections.GetEmptyList<object>();
                }

                return Collections.SingletonList(item);
            }

            var all = EnumTakeWhileLastEventsForgeEval.TakeWhileLastEventBeanToArray(beans);
            var result = new ArrayDeque<object>();
            var index = 0;

            for (var i = all.Length - 1; i >= 0; i--) {
                props[0] = index++;
                eventsLambda[forge.streamNumLambda] = all[i];

                var pass = innerExpression.Evaluate(eventsLambda, isNewData, context);
                if (pass == null || false.Equals(pass)) {
                    break;
                }

                result.AddFirst(all[i]);
            }

            return result;
        }

        public static CodegenExpression Codegen(
            EnumTakeWhileLastIndexEventsForge forge,
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var indexTypeMember = codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(ObjectArrayEventType),
                Cast(
                    typeof(ObjectArrayEventType),
                    EventTypeUtility.ResolveTypeCodegen(forge.indexEventType, EPStatementInitServicesConstants.REF)));

            var scope = new ExprForgeCodegenSymbol(false, null);
            var methodNode = codegenMethodScope.MakeChildWithScope(
                    typeof(ICollection<object>),
                    typeof(EnumTakeWhileLastIndexEventsForgeEval),
                    scope,
                    codegenClassScope)
                .AddParam(EnumForgeCodegenNames.PARAMS_EVENTBEAN);

            var innerValue = forge.innerExpression.EvaluateCodegen(
                typeof(bool?),
                methodNode,
                scope,
                codegenClassScope);
            var block = methodNode.Block
                .IfCondition(ExprDotMethod(EnumForgeCodegenNames.REF_ENUMCOLL, "IsEmpty"))
                .BlockReturn(EnumForgeCodegenNames.REF_ENUMCOLL);
            block.DeclareVar<ObjectArrayEventBean>(
                    "indexEvent",
                    NewInstance<ObjectArrayEventBean>(NewArrayByLength(typeof(object), Constant(1)), indexTypeMember))
                .AssignArrayElement(
                    EnumForgeCodegenNames.REF_EPS,
                    Constant(forge.streamNumLambda + 1),
                    @Ref("indexEvent"))
                .DeclareVar<object[]>("props", ExprDotName(@Ref("indexEvent"), "Properties"));

            var blockSingle = block
                .IfCondition(EqualsIdentity(ExprDotName(EnumForgeCodegenNames.REF_ENUMCOLL, "Count"), Constant(1)))
                .DeclareVar<EventBean>(
                    "item",
                    Cast(
                        typeof(EventBean),
                        ExprDotMethodChain(EnumForgeCodegenNames.REF_ENUMCOLL).Add("First")))
                .AssignArrayElement("props", Constant(0), Constant(0))
                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(forge.streamNumLambda), @Ref("item"));
            CodegenLegoBooleanExpression.CodegenReturnValueIfNotNullAndNotPass(
                blockSingle,
                forge.innerExpression.EvaluationType,
                innerValue,
                StaticMethod(typeof(Collections), "GetEmptyList"));
            blockSingle.BlockReturn(StaticMethod(typeof(Collections), "SingletonList", @Ref("item")));

            block.DeclareVar<ArrayDeque<object>>("result", NewInstance(typeof(ArrayDeque<object>)))
                .DeclareVar<EventBean[]>(
                    "all",
                    StaticMethod(
                        typeof(EnumTakeWhileLastEventsForgeEval),
                        "TakeWhileLastEventBeanToArray",
                        EnumForgeCodegenNames.REF_ENUMCOLL))
                .DeclareVar<int>("index", Constant(0));
            var forEach = block.ForLoop(
                    typeof(int),
                    "i",
                    Op(ArrayLength(@Ref("all")), "-", Constant(1)),
                    Relational(@Ref("i"), GE, Constant(0)),
                    Decrement("i"))
                .AssignArrayElement("props", Constant(0), Increment("index"))
                .AssignArrayElement(
                    EnumForgeCodegenNames.REF_EPS,
                    Constant(forge.streamNumLambda),
                    ArrayAtIndex(@Ref("all"), @Ref("i")));
            CodegenLegoBooleanExpression.CodegenBreakIfNotNullAndNotPass(
                forEach,
                forge.innerExpression.EvaluationType,
                innerValue);
            forEach.Expression(ExprDotMethod(@Ref("result"), "AddFirst", ArrayAtIndex(@Ref("all"), @Ref("i"))));
            block.MethodReturn(@Ref("result"));
            return LocalMethod(methodNode, args.Eps, args.Enumcoll, args.IsNewData, args.ExprCtx);
        }
    }
} // end of namespace