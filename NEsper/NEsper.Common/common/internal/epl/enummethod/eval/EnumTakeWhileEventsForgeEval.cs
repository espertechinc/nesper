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
using com.espertech.esper.common.client.collection;
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
        private readonly EnumTakeWhileEventsForge _forge;
        private readonly ExprEvaluator _innerExpression;

        public EnumTakeWhileEventsForgeEval(
            EnumTakeWhileEventsForge forge,
            ExprEvaluator innerExpression)
        {
            _forge = forge;
            _innerExpression = innerExpression;
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
            if (enumcoll.Count == 1) {
                var item = beans.First();
                eventsLambda[_forge.StreamNumLambda] = item;

                var pass = _innerExpression.Evaluate(eventsLambda, isNewData, context);
                if (pass == null || false.Equals(pass)) {
                    return Collections.GetEmptyList<object>();
                }

                return Collections.SingletonList(item);
            }

            var result = new ArrayDeque<object>();

            foreach (var next in beans) {
                eventsLambda[_forge.StreamNumLambda] = next;

                var pass = _innerExpression.Evaluate(eventsLambda, isNewData, context);
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
            var returnType = typeof(FlexCollection);
            var paramTypes = EnumForgeCodegenNames.PARAMS;

            var scope = new ExprForgeCodegenSymbol(false, null);
            var methodNode = codegenMethodScope
                .MakeChildWithScope(
                    returnType,
                    typeof(EnumTakeWhileEventsForgeEval),
                    scope,
                    codegenClassScope)
                .AddParam(paramTypes);
            var innerValue = forge.InnerExpression.EvaluateCodegen(
                typeof(bool?),
                methodNode,
                scope,
                codegenClassScope);

            var block = methodNode.Block
                .IfCondition(ExprDotMethod(EnumForgeCodegenNames.REF_ENUMCOLL, "IsEmpty"))
                .BlockReturn(EnumForgeCodegenNames.REF_ENUMCOLL);

            var blockSingle = block
                .IfCondition(EqualsIdentity(ExprDotName(EnumForgeCodegenNames.REF_ENUMCOLL, "Count"), Constant(1)))
                .DeclareVar<EventBean>("item",
                    ExprDotMethod(ExprDotName(EnumForgeCodegenNames.REF_ENUMCOLL, "EventBeanCollection"), "First"))
                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(forge.StreamNumLambda), Ref("item"));
            CodegenLegoBooleanExpression.CodegenReturnValueIfNotNullAndNotPass(
                blockSingle,
                forge.InnerExpression.EvaluationType,
                innerValue,
                FlexEmpty());
            blockSingle.BlockReturn(FlexEvent(Ref("item")));

            block.DeclareVar<ArrayDeque<EventBean>>("result", NewInstance(typeof(ArrayDeque<EventBean>)));
            var forEach = block
                .ForEach(typeof(EventBean), "next", EnumForgeCodegenNames.REF_ENUMCOLL)
                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(forge.StreamNumLambda), Ref("next"));
            CodegenLegoBooleanExpression.CodegenBreakIfNotNullAndNotPass(
                forEach,
                forge.InnerExpression.EvaluationType,
                innerValue);
            forEach.Expression(ExprDotMethod(Ref("result"), "Add", Ref("next")));
            block.MethodReturn(FlexWrap(Ref("result")));
            return LocalMethod(methodNode, args.Eps, args.Enumcoll, args.IsNewData, args.ExprCtx);
        }
    }
} // end of namespace