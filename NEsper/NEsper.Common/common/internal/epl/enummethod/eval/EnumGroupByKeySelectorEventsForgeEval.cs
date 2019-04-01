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
    public class EnumGroupByKeySelectorEventsForgeEval : EnumEval
    {
        private readonly EnumGroupByKeySelectorEventsForge forge;
        private readonly ExprEvaluator innerExpression;

        public EnumGroupByKeySelectorEventsForgeEval(
            EnumGroupByKeySelectorEventsForge forge,
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
            if (enumcoll.IsEmpty())
            {
                return Collections.GetEmptyMap<object, object>();
            }

            IDictionary<object, ICollection<object>> result = new LinkedHashMap<object, ICollection<object>>();

            ICollection<EventBean> beans = (ICollection<EventBean>) enumcoll;
            foreach (EventBean next in beans)
            {
                eventsLambda[forge.streamNumLambda] = next;

                object key = innerExpression.Evaluate(eventsLambda, isNewData, context);

                ICollection<object> value = result.Get(key);
                if (value == null)
                {
                    value = new List<object>();
                    result.Put(key, value);
                }

                value.Add(next.Underlying);
            }

            return result;
        }

        public static CodegenExpression Codegen(
            EnumGroupByKeySelectorEventsForge forge,
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            ExprForgeCodegenSymbol scope = new ExprForgeCodegenSymbol(false, null);
            CodegenMethod methodNode = codegenMethodScope.MakeChildWithScope(
                    typeof(IDictionary<object, object>), typeof(EnumGroupByKeySelectorEventsForgeEval), scope, codegenClassScope)
                .AddParam(EnumForgeCodegenNames.PARAMS);

            CodegenBlock block = methodNode.Block
                .IfCondition(ExprDotMethod(EnumForgeCodegenNames.REF_ENUMCOLL, "isEmpty"))
                .BlockReturn(StaticMethod(typeof(Collections), "emptyMap"))
                .DeclareVar(typeof(IDictionary<object, object>), "result", NewInstance(typeof(LinkedHashMap<object, object>)));
            CodegenBlock forEach = block.ForEach(typeof(EventBean), "next", EnumForgeCodegenNames.REF_ENUMCOLL)
                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(forge.streamNumLambda), @Ref("next"))
                .DeclareVar(typeof(object), "key", forge.innerExpression.EvaluateCodegen(typeof(object), methodNode, scope, codegenClassScope))
                .DeclareVar(
                    typeof(ICollection<object>), "value", Cast(typeof(ICollection<object>), ExprDotMethod(@Ref("result"), "get", @Ref("key"))))
                .IfRefNull("value")
                .AssignRef("value", NewInstance(typeof(List<object>)))
                .Expression(ExprDotMethod(@Ref("result"), "put", @Ref("key"), @Ref("value")))
                .BlockEnd()
                .Expression(ExprDotMethod(@Ref("value"), "add", ExprDotUnderlying(@Ref("next"))));
            block.MethodReturn(@Ref("result"));
            return LocalMethod(methodNode, args.Eps, args.Enumcoll, args.IsNewData, args.ExprCtx);
        }
    }
} // end of namespace