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
    public class EnumSumEventsForgeEval : EnumEval
    {
        private readonly EnumSumEventsForge forge;
        private readonly ExprEvaluator innerExpression;

        public EnumSumEventsForgeEval(
            EnumSumEventsForge forge,
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
            var method = forge.sumMethodFactory.SumAggregator;

            var beans = (ICollection<EventBean>) enumcoll;
            foreach (var next in beans) {
                eventsLambda[forge.streamNumLambda] = next;

                var value = innerExpression.Evaluate(eventsLambda, isNewData, context);
                method.Enter(value);
            }

            return method.Value;
        }

        public static CodegenExpression Codegen(
            EnumSumEventsForge forge,
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var innerType = forge.innerExpression.EvaluationType;

            var scope = new ExprForgeCodegenSymbol(false, null);
            var methodNode = codegenMethodScope
                .MakeChildWithScope(
                    forge.sumMethodFactory.ValueType.GetBoxedType(),
                    typeof(EnumSumEventsForgeEval),
                    scope,
                    codegenClassScope)
                .AddParam(EnumForgeCodegenNames.PARAMS);

            var block = methodNode.Block;
            forge.sumMethodFactory.CodegenDeclare(block);

            var forEach = block.ForEach(typeof(EventBean), "next", EnumForgeCodegenNames.REF_ENUMCOLL)
                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(forge.streamNumLambda), @Ref("next"))
                .DeclareVar(
                    innerType.GetBoxedType(),
                    "value",
                    forge.innerExpression.EvaluateCodegen(innerType, methodNode, scope, codegenClassScope));
            if (!innerType.IsPrimitive) {
                forEach.IfRefNull("value").BlockContinue();
            }

            forge.sumMethodFactory.CodegenEnterNumberTypedNonNull(forEach, @Ref("value"));
            forge.sumMethodFactory.CodegenReturn(block);
            return LocalMethod(methodNode, args.Eps, args.Enumcoll, args.IsNewData, args.ExprCtx);
        }
    }
} // end of namespace