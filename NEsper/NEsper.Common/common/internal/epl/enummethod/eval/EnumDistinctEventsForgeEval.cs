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
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.CodegenRelational;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class EnumDistinctEventsForgeEval : EnumEval
    {
        private readonly EnumDistinctEventsForge forge;
        private readonly ExprEvaluator innerExpression;

        public EnumDistinctEventsForgeEval(
            EnumDistinctEventsForge forge,
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
            ICollection<EventBean> beans = (ICollection<EventBean>) enumcoll;
            if (beans.Count <= 1)
            {
                return beans;
            }

            IDictionary<IComparable, EventBean> distinct = new LinkedHashMap<IComparable, EventBean>();
            foreach (EventBean next in beans)
            {
                eventsLambda[forge.streamNumLambda] = next;

                IComparable comparable = (IComparable) innerExpression.Evaluate(eventsLambda, isNewData, context);
                if (!distinct.ContainsKey(comparable))
                {
                    distinct.Put(comparable, next);
                }
            }

            return distinct.Values;
        }

        public static CodegenExpression Codegen(
            EnumDistinctEventsForge forge,
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            Type innerType = Boxing.GetBoxedType(forge.innerExpression.EvaluationType);

            ExprForgeCodegenSymbol scope = new ExprForgeCodegenSymbol(false, null);
            CodegenMethod methodNode = codegenMethodScope.MakeChildWithScope(
                    typeof(ICollection<object>), typeof(EnumDistinctEventsForgeEval), scope, codegenClassScope)
                .AddParam(EnumForgeCodegenNames.PARAMS);

            CodegenBlock block = methodNode.Block
                .IfCondition(Relational(ExprDotMethod(EnumForgeCodegenNames.REF_ENUMCOLL, "size"), LE, Constant(1)))
                .BlockReturn(EnumForgeCodegenNames.REF_ENUMCOLL)
                .DeclareVar(
                    typeof(IDictionary<object, object>), "distinct",
                    NewInstance(typeof(LinkedHashMap<object, object>)));
            block.ForEach(typeof(EventBean), "next", EnumForgeCodegenNames.REF_ENUMCOLL)
                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(forge.streamNumLambda), @Ref("next"))
                .DeclareVar(
                    innerType, "comparable",
                    forge.innerExpression.EvaluateCodegen(innerType, methodNode, scope, codegenClassScope))
                .IfCondition(Not(ExprDotMethod(@Ref("distinct"), "containsKey", @Ref("comparable"))))
                .Expression(ExprDotMethod(@Ref("distinct"), "put", @Ref("comparable"), @Ref("next")))
                .BlockEnd();
            block.MethodReturn(ExprDotMethod(@Ref("distinct"), "values"));
            return LocalMethod(methodNode, args.Eps, args.Enumcoll, args.IsNewData, args.ExprCtx);
        }
    }
} // end of namespace