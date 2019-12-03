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
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.
    CodegenRelational;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class EnumTakeWhileLastEventsForgeEval : EnumEval
    {
        private readonly EnumTakeWhileLastEventsForge _forge;
        private readonly ExprEvaluator _innerExpression;

        public EnumTakeWhileLastEventsForgeEval(
            EnumTakeWhileLastEventsForge forge,
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

            var all = TakeWhileLastEventBeanToArray(beans);
            var result = new ArrayDeque<object>();

            for (var i = all.Length - 1; i >= 0; i--) {
                eventsLambda[_forge.StreamNumLambda] = all[i];

                var pass = _innerExpression.Evaluate(eventsLambda, isNewData, context);
                if (pass == null || false.Equals(pass)) {
                    break;
                }

                result.AddFirst(all[i]);
            }

            return result;
        }

        public static CodegenExpression Codegen(
            EnumTakeWhileLastEventsForge forge,
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var scope = new ExprForgeCodegenSymbol(false, null);
            var methodNode = codegenMethodScope
                .MakeChildWithScope(
                    typeof(ICollection<EventBean>),
                    typeof(EnumTakeWhileLastEventsForgeEval),
                    scope,
                    codegenClassScope)
                .AddParam(EnumForgeCodegenNames.PARAMS_EVENTBEAN);
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
                .DeclareVar<EventBean>(
                    "item",
                    Cast(
                        typeof(EventBean),
                        ExprDotMethodChain(EnumForgeCodegenNames.REF_ENUMCOLL).Add("First")))
                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(forge.StreamNumLambda), @Ref("item"));
            CodegenLegoBooleanExpression.CodegenReturnValueIfNotNullAndNotPass(
                blockSingle,
                forge.InnerExpression.EvaluationType,
                innerValue,
                StaticMethod(typeof(Collections), "GetEmptyList", new [] { typeof(EventBean) }));
            blockSingle.BlockReturn(StaticMethod(typeof(Collections), "SingletonList", @Ref("item")));

            block
                .DeclareVar<ArrayDeque<EventBean>>("result", NewInstance(typeof(ArrayDeque<EventBean>)))
                .DeclareVar<EventBean[]>(
                    "all",
                    StaticMethod(
                        typeof(EnumTakeWhileLastEventsForgeEval),
                        "TakeWhileLastEventBeanToArray",
                        EnumForgeCodegenNames.REF_ENUMCOLL));

            var forEach = block.ForLoop(
                    typeof(int),
                    "i",
                    Op(ArrayLength(@Ref("all")), "-", Constant(1)),
                    Relational(@Ref("i"), GE, Constant(0)),
                    Decrement("i"))
                .AssignArrayElement(
                    EnumForgeCodegenNames.REF_EPS,
                    Constant(forge.StreamNumLambda),
                    ArrayAtIndex(@Ref("all"), @Ref("i")));
            CodegenLegoBooleanExpression.CodegenBreakIfNotNullAndNotPass(
                forEach,
                forge.InnerExpression.EvaluationType,
                innerValue);
            forEach.Expression(ExprDotMethod(@Ref("result"), "AddFirst", ArrayAtIndex(@Ref("all"), @Ref("i"))));
            block.MethodReturn(@Ref("result"));
            return LocalMethod(methodNode, args.Eps, args.Enumcoll, args.IsNewData, args.ExprCtx);
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="enumcoll">events</param>
        /// <returns>array</returns>
        public static T[] TakeWhileLastEventBeanToArray<T>(ICollection<T> enumcoll)
        {
            var size = enumcoll.Count;
            var all = new T[size];
            var count = 0;
            foreach (var item in enumcoll) {
                all[count++] = item;
            }

            return all;
        }
    }
} // end of namespace