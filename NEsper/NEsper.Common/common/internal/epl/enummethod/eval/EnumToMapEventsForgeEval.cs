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
        private readonly EnumToMapEventsForge _forge;
        private readonly ExprEvaluator _innerExpression;
        private readonly ExprEvaluator _secondExpression;

        public EnumToMapEventsForgeEval(
            EnumToMapEventsForge forge,
            ExprEvaluator innerExpression,
            ExprEvaluator secondExpression)
        {
            _forge = forge;
            _innerExpression = innerExpression;
            _secondExpression = secondExpression;
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
                eventsLambda[_forge.StreamNumLambda] = next;

                var key = _innerExpression.Evaluate(eventsLambda, isNewData, context);
                var value = _secondExpression.Evaluate(eventsLambda, isNewData, context);
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
                    typeof(IDictionary<string, object>),
                    typeof(EnumToMapEventsForgeEval),
                    scope,
                    codegenClassScope)
                .AddParam(EnumForgeCodegenNames.PARAMS_EVENTBEAN);

            var block = methodNode.Block
                .IfCondition(ExprDotMethod(EnumForgeCodegenNames.REF_ENUMCOLL, "IsEmpty"))
                .BlockReturn(StaticMethod(typeof(Collections), "GetEmptyMap", new[] { typeof(string), typeof(object) }));
            block.DeclareVar<IDictionary<string, object>>("map", NewInstance(typeof(HashMap<string, object>)));
            block.ForEach(typeof(EventBean), "next", EnumForgeCodegenNames.REF_ENUMCOLL)
                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(forge.StreamNumLambda), Ref("next"))
                .DeclareVar<string>(
                    "key",
                    forge.InnerExpression.EvaluateCodegen(typeof(string), methodNode, scope, codegenClassScope))
                .DeclareVar<object>(
                    "value",
                    forge.secondExpression.EvaluateCodegen(typeof(object), methodNode, scope, codegenClassScope))
                .Expression(ExprDotMethod(Ref("map"), "Put", Ref("key"), Ref("value")));
            block.MethodReturn(Ref("map"));
            return LocalMethod(methodNode, args.Eps, args.Enumcoll, args.IsNewData, args.ExprCtx);
        }
    }
} // end of namespace