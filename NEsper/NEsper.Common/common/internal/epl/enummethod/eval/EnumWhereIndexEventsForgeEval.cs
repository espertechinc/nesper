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
    public class EnumWhereIndexEventsForgeEval : EnumEval
    {
        private readonly EnumWhereIndexEventsForge forge;
        private readonly ExprEvaluator innerExpression;

        public EnumWhereIndexEventsForgeEval(
            EnumWhereIndexEventsForge forge,
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

            var beans = (ICollection<EventBean>) enumcoll;
            var result = new ArrayDeque<object>();
            var indexEvent = new ObjectArrayEventBean(new object[1], forge.indexEventType);
            eventsLambda[forge.streamNumLambda + 1] = indexEvent;
            var props = indexEvent.Properties;

            var count = -1;
            foreach (var next in beans) {
                count++;

                props[0] = count;
                eventsLambda[forge.streamNumLambda] = next;

                var pass = innerExpression.Evaluate(eventsLambda, isNewData, context);
                if (pass == null || false.Equals(pass)) {
                    continue;
                }

                result.Add(next);
            }

            return result;
        }

        public static CodegenExpression Codegen(
            EnumWhereIndexEventsForge forge,
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
            var methodNode = codegenMethodScope
                .MakeChildWithScope(
                    typeof(ICollection<EventBean>),
                    typeof(EnumWhereIndexEventsForgeEval),
                    scope,
                    codegenClassScope)
                .AddParam(EnumForgeCodegenNames.PARAMS_EVENTBEAN);

            var block = methodNode.Block
                .IfCondition(ExprDotMethod(EnumForgeCodegenNames.REF_ENUMCOLL, "IsEmpty"))
                .BlockReturn(EnumForgeCodegenNames.REF_ENUMCOLL);
            block.DeclareVar<ArrayDeque<EventBean>>("result", NewInstance(typeof(ArrayDeque<EventBean>)))
                .DeclareVar<ObjectArrayEventBean>(
                    "indexEvent",
                    NewInstance<ObjectArrayEventBean>(NewArrayByLength(typeof(EventBean), Constant(1)), indexTypeMember))
                .AssignArrayElement(
                    EnumForgeCodegenNames.REF_EPS,
                    Constant(forge.streamNumLambda + 1),
                    @Ref("indexEvent"))
                .DeclareVar<object[]>("props", ExprDotName(@Ref("indexEvent"), "Properties"))
                .DeclareVar<int>("count", Constant(-1));
            var forEach = block.ForEach(typeof(EventBean), "next", EnumForgeCodegenNames.REF_ENUMCOLL)
                .Increment("count")
                .AssignArrayElement("props", Constant(0), @Ref("count"))
                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(forge.streamNumLambda), @Ref("next"));
            CodegenLegoBooleanExpression.CodegenContinueIfNotNullAndNotPass(
                forEach,
                forge.innerExpression.EvaluationType,
                forge.innerExpression.EvaluateCodegen(typeof(bool?), methodNode, scope, codegenClassScope));
            forEach.Expression(ExprDotMethod(@Ref("result"), "Add", @Ref("next")));
            block.MethodReturn(@Ref("result"));
            return LocalMethod(methodNode, args.Eps, args.Enumcoll, args.IsNewData, args.ExprCtx);
        }
    }
} // end of namespace