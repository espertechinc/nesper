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
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class EnumAverageEventsForgeEval : EnumEval
    {
        private readonly EnumAverageEventsForge forge;
        private readonly ExprEvaluator innerExpression;

        public EnumAverageEventsForgeEval(
            EnumAverageEventsForge forge,
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
            double sum = 0d;
            int count = 0;

            ICollection<EventBean> beans = (ICollection<EventBean>) enumcoll;
            foreach (EventBean next in beans) {
                eventsLambda[forge.streamNumLambda] = next;

                var num = innerExpression.Evaluate(eventsLambda, isNewData, context);
                if (num == null) {
                    continue;
                }

                count++;
                sum += num.AsDouble();
            }

            if (count == 0) {
                return null;
            }

            return sum / count;
        }

        public static CodegenExpression Codegen(
            EnumAverageEventsForge forge,
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            Type innerType = forge.innerExpression.EvaluationType;

            ExprForgeCodegenSymbol scope = new ExprForgeCodegenSymbol(false, null);
            CodegenMethod methodNode = codegenMethodScope.MakeChildWithScope(
                    typeof(double?), typeof(EnumAverageEventsForgeEval), scope, codegenClassScope)
                .AddParam(EnumForgeCodegenNames.PARAMS);

            CodegenBlock block = methodNode.Block
                .DeclareVar(typeof(double), "sum", Constant(0d))
                .DeclareVar(typeof(int), "count", Constant(0));
            CodegenBlock forEach = block.ForEach(typeof(EventBean), "next", EnumForgeCodegenNames.REF_ENUMCOLL)
                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(forge.streamNumLambda), @Ref("next"))
                .DeclareVar(
                    innerType, "num",
                    forge.innerExpression.EvaluateCodegen(innerType, methodNode, scope, codegenClassScope));
            if (!innerType.IsPrimitive) {
                forEach.IfRefNull("num").BlockContinue();
            }

            forEach.Increment("count")
                .AssignRef(
                    "sum",
                    Op(
                        @Ref("sum"), "+",
                        SimpleNumberCoercerFactory.SimpleNumberCoercerDouble.CodegenDouble(@Ref("num"), innerType)))
                .BlockEnd();
            block.IfCondition(EqualsIdentity(@Ref("count"), Constant(0))).BlockReturn(ConstantNull())
                .MethodReturn(Op(@Ref("sum"), "/", @Ref("count")));
            return LocalMethod(methodNode, args.Eps, args.Enumcoll, args.IsNewData, args.ExprCtx);
        }
    }
} // end of namespace