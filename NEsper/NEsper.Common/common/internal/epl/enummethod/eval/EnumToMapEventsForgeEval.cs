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
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class EnumToMapEventsForgeEval : EnumEval
    {
        private readonly EnumToMapEventsForge forge;
        private readonly ExprEvaluator innerExpression;
        private readonly ExprEvaluator secondExpression;

        public EnumToMapEventsForgeEval(
            EnumToMapEventsForge forge,
            ExprEvaluator innerExpression,
            ExprEvaluator secondExpression)
        {
            this.forge = forge;
            this.innerExpression = innerExpression;
            this.secondExpression = secondExpression;
        }

        public object EvaluateEnumMethod(
            EventBean[] eventsLambda,
            ICollection<object> enumcoll,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            if (enumcoll.IsEmpty()) {
                return new EmptyDictionary<object, object>();
            }

            var map = new Dictionary<object, object>();
            var beans = (ICollection<EventBean>) enumcoll;
            foreach (var next in beans) {
                eventsLambda[forge.streamNumLambda] = next;

                var key = innerExpression.Evaluate(eventsLambda, isNewData, context);
                var value = secondExpression.Evaluate(eventsLambda, isNewData, context);
                map.Put(key, value);
            }

            return map;
        }

        public static CodegenExpression Codegen(
            EnumToMapEventsForge forge,
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var scope = new ExprForgeCodegenSymbol(false, null);
            var methodNode = codegenMethodScope.MakeChildWithScope(
                    typeof(IDictionary<object, object>),
                    typeof(EnumToMapEventsForgeEval),
                    scope,
                    codegenClassScope)
                .AddParam(EnumForgeCodegenNames.PARAMS);

            var block = methodNode.Block
                .IfCondition(ExprDotMethod(EnumForgeCodegenNames.REF_ENUMCOLL, "IsEmpty"))
                .BlockReturn(StaticMethod(typeof(Collections), "GetEmptyMap", new[] { typeof(object), typeof(object) }));
            block.DeclareVar<IDictionary<object, object>>("map", NewInstance(typeof(Dictionary<object, object>)));
            block.ForEach(typeof(EventBean), "next", EnumForgeCodegenNames.REF_ENUMCOLL)
                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(forge.streamNumLambda), Ref("next"))
                .DeclareVar<object>(
                    "key",
                    forge.innerExpression.EvaluateCodegen(typeof(object), methodNode, scope, codegenClassScope))
                .DeclareVar<object>(
                    "value",
                    forge.secondExpression.EvaluateCodegen(typeof(object), methodNode, scope, codegenClassScope))
                .Expression(ExprDotMethod(Ref("map"), "Put", Ref("key"), Ref("value")));
            block.MethodReturn(Ref("map"));
            return LocalMethod(methodNode, args.Eps, args.Enumcoll, args.IsNewData, args.ExprCtx);
        }
    }
} // end of namespace