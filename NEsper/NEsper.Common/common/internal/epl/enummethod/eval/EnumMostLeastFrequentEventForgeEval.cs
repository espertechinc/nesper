///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

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
    public class EnumMostLeastFrequentEventForgeEval : EnumEval
    {
        private readonly EnumMostLeastFrequentEventForge forge;
        private readonly ExprEvaluator innerExpression;

        public EnumMostLeastFrequentEventForgeEval(EnumMostLeastFrequentEventForge forge, ExprEvaluator innerExpression)
        {
            this.forge = forge;
            this.innerExpression = innerExpression;
        }

        public object EvaluateEnumMethod(
            EventBean[] eventsLambda, ICollection<object> enumcoll, bool isNewData, ExprEvaluatorContext context)
        {
            if (enumcoll.IsEmpty()) {
                return null;
            }

            IDictionary<object, int> items = new LinkedHashMap<object, int>();
            var beans = (ICollection<EventBean>) enumcoll;

            foreach (var next in beans) {
                eventsLambda[forge.streamNumLambda] = next;

                var item = innerExpression.Evaluate(eventsLambda, isNewData, context);

                int? existing = items.Get(item);
                if (existing == null) {
                    existing = 1;
                }
                else {
                    existing++;
                }

                items.Put(item, existing.Value);
            }

            return GetEnumMostLeastFrequentResult(items, forge.isMostFrequent);
        }

        public static CodegenExpression Codegen(
            EnumMostLeastFrequentEventForge forge,
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var returnType = Boxing.GetBoxedType(forge.innerExpression.EvaluationType);
            var scope = new ExprForgeCodegenSymbol(false, null);
            var methodNode = codegenMethodScope.MakeChildWithScope(
                    returnType, typeof(EnumMostLeastFrequentEventForgeEval), scope, codegenClassScope)
                .AddParam(EnumForgeCodegenNames.PARAMS);

            var block = methodNode.Block
                .IfCondition(ExprDotMethod(EnumForgeCodegenNames.REF_ENUMCOLL, "isEmpty"))
                .BlockReturn(ConstantNull())
                .DeclareVar(
                    typeof(IDictionary<string, object>), "items", NewInstance(typeof(LinkedHashMap<string, object>)));
            var forEach = block.ForEach(typeof(EventBean), "next", EnumForgeCodegenNames.REF_ENUMCOLL)
                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(forge.streamNumLambda), Ref("next"))
                .DeclareVar(
                    typeof(object), "item",
                    forge.innerExpression.EvaluateCodegen(typeof(object), methodNode, scope, codegenClassScope))
                .DeclareVar(
                    typeof(int?), "existing", Cast(typeof(int?), ExprDotMethod(Ref("items"), "get", Ref("item"))))
                .IfCondition(EqualsNull(Ref("existing")))
                .AssignRef("existing", Constant(1))
                .IfElse()
                .Increment("existing")
                .BlockEnd()
                .ExprDotMethod(Ref("items"), "put", Ref("item"), Ref("existing"));
            block.MethodReturn(
                Cast(
                    returnType,
                    StaticMethod(
                        typeof(EnumMostLeastFrequentEventForgeEval), "getEnumMostLeastFrequentResult", Ref("items"),
                        Constant(forge.isMostFrequent))));
            return LocalMethod(methodNode, args.Eps, args.Enumcoll, args.IsNewData, args.ExprCtx);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="items">items</param>
        /// <param name="mostFrequent">flag</param>
        /// <returns>value</returns>
        public static object GetEnumMostLeastFrequentResult(IDictionary<object, int> items, bool mostFrequent)
        {
            if (mostFrequent) {
                object maxKey = null;
                var max = int.MinValue;
                foreach (var entry in items) {
                    if (entry.Value > max) {
                        maxKey = entry.Key;
                        max = entry.Value;
                    }
                }

                return maxKey;
            }

            var min = int.MaxValue;
            object minKey = null;
            foreach (var entry in items) {
                if (entry.Value < min) {
                    minKey = entry.Key;
                    min = entry.Value;
                }
            }

            return minKey;
        }
    }
} // end of namespace