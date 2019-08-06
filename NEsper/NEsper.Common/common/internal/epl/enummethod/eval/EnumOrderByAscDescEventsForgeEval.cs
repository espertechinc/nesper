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
    public class EnumOrderByAscDescEventsForgeEval : EnumEval
    {
        private readonly EnumOrderByAscDescEventsForge forge;
        private readonly ExprEvaluator innerExpression;

        public EnumOrderByAscDescEventsForgeEval(
            EnumOrderByAscDescEventsForge forge,
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
            var sort = new OrderedDictionary<IComparable, object>();
            var hasColl = false;

            var beans = (ICollection<EventBean>) enumcoll;
            foreach (var next in beans) {
                eventsLambda[forge.streamNumLambda] = next;

                var comparable = (IComparable) innerExpression.Evaluate(eventsLambda, isNewData, context);
                var entry = sort.Get(comparable);

                if (entry == null) {
                    sort.Put(comparable, next);
                    continue;
                }

                if (entry is ICollection<EventBean>) {
                    ((ICollection<EventBean>) entry).Add(next);
                    continue;
                }

                Deque<object> coll = new ArrayDeque<object>(2);
                coll.Add(entry);
                coll.Add(next);
                sort.Put(comparable, coll);
                hasColl = true;
            }

            return EnumOrderBySortEval(sort, hasColl, forge.descending);
        }

        public static CodegenExpression Codegen(
            EnumOrderByAscDescEventsForge forge,
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var innerBoxedType = forge.innerExpression.EvaluationType.GetBoxedType();

            var scope = new ExprForgeCodegenSymbol(false, null);
            var methodNode = codegenMethodScope.MakeChildWithScope(
                    typeof(ICollection<object>),
                    typeof(EnumOrderByAscDescEventsForgeEval),
                    scope,
                    codegenClassScope)
                .AddParam(EnumForgeCodegenNames.PARAMS);

            var block = methodNode.Block
                .DeclareVar<OrderedDictionary<object, object>>(
                    "sort",
                    NewInstance(typeof(OrderedDictionary<object, object>)))
                .DeclareVar<bool>("hasColl", ConstantFalse());
            block.ForEach(typeof(EventBean), "next", EnumForgeCodegenNames.REF_ENUMCOLL)
                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(forge.streamNumLambda), Ref("next"))
                .DeclareVar(
                    innerBoxedType,
                    "value",
                    forge.innerExpression.EvaluateCodegen(innerBoxedType, methodNode, scope, codegenClassScope))
                .DeclareVar<object>("entry", ExprDotMethod(Ref("sort"), "Get", Ref("value")))
                .IfCondition(EqualsNull(Ref("entry")))
                .Expression(ExprDotMethod(Ref("sort"), "Put", Ref("value"), Ref("next")))
                .BlockContinue()
                .IfCondition(InstanceOf(Ref("entry"), typeof(ICollection<object>)))
                .ExprDotMethod(Cast(typeof(ICollection<object>), Ref("entry")), "Add", Ref("next"))
                .BlockContinue()
                .DeclareVar<Deque<object>>("coll", NewInstance<ArrayDeque<object>>(Constant(2)))
                .ExprDotMethod(Ref("coll"), "Add", Ref("entry"))
                .ExprDotMethod(Ref("coll"), "Add", Ref("next"))
                .ExprDotMethod(Ref("sort"), "Put", Ref("value"), Ref("coll"))
                .AssignRef("hasColl", ConstantTrue())
                .BlockEnd();
            block.MethodReturn(
                StaticMethod(
                    typeof(EnumOrderByAscDescEventsForgeEval),
                    "EnumOrderBySortEval",
                    Ref("sort"),
                    Ref("hasColl"),
                    Constant(forge.descending)));
            return LocalMethod(methodNode, args.Eps, args.Enumcoll, args.IsNewData, args.ExprCtx);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="sort">sorted</param>
        /// <param name="hasColl">collection flag</param>
        /// <param name="descending">true for descending</param>
        /// <returns>collection</returns>
        public static ICollection<object> EnumOrderBySortEval(
            OrderedDictionary<IComparable, object> sort,
            bool hasColl,
            bool descending)
        {
            IDictionary<IComparable, object> sorted;
            if (descending) {
                sorted = sort.Invert();
            }
            else {
                sorted = sort;
            }

            if (!hasColl) {
                return sorted.Values;
            }

            Deque<object> coll = new ArrayDeque<object>();
            foreach (var entry in sorted) {
                if (entry.Value is ICollection<object>) {
                    coll.AddAll((ICollection<object>) entry.Value);
                }
                else {
                    coll.Add(entry.Value);
                }
            }

            return coll;
        }
    }
} // end of namespace