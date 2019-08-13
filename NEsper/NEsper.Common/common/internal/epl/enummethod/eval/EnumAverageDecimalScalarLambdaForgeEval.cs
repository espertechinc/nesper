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
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class EnumAverageDecimalScalarLambdaForgeEval : EnumEval
    {
        private readonly EnumAverageDecimalScalarLambdaForge forge;
        private readonly ExprEvaluator innerExpression;

        public EnumAverageDecimalScalarLambdaForgeEval(
            EnumAverageDecimalScalarLambdaForge forge,
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
            EnumAverageDecimalEventsForgeEval.AggregatorAvgBigDecimal agg =
                new EnumAverageDecimalEventsForgeEval.AggregatorAvgBigDecimal(forge.optionalMathContext);
            ObjectArrayEventBean resultEvent = new ObjectArrayEventBean(new object[1], forge.resultEventType);
            eventsLambda[forge.streamNumLambda] = resultEvent;
            object[] props = resultEvent.Properties;

            ICollection<object> values = (ICollection<object>) enumcoll;
            foreach (object next in values) {
                props[0] = next;

                var num = innerExpression.Evaluate(eventsLambda, isNewData, context);
                if (num == null) {
                    continue;
                }

                agg.Enter(num);
            }

            return agg.Value;
        }

        public static CodegenExpression Codegen(
            EnumAverageDecimalScalarLambdaForge forge,
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            Type innerType = forge.innerExpression.EvaluationType;
            CodegenExpressionField resultTypeMember = codegenClassScope.AddFieldUnshared(
                true,
                typeof(ObjectArrayEventType),
                Cast(
                    typeof(ObjectArrayEventType),
                    EventTypeUtility.ResolveTypeCodegen(forge.resultEventType, EPStatementInitServicesConstants.REF)));
            CodegenExpression math =
                codegenClassScope.AddOrGetFieldSharable(new MathContextCodegenField(forge.optionalMathContext));

            ExprForgeCodegenSymbol scope = new ExprForgeCodegenSymbol(false, null);
            CodegenMethod methodNode = codegenMethodScope.MakeChildWithScope(
                    typeof(decimal),
                    typeof(EnumAverageDecimalScalarLambdaForgeEval),
                    scope,
                    codegenClassScope)
                .AddParam(EnumForgeCodegenNames.PARAMS);

            CodegenBlock block = methodNode.Block;
            block.DeclareVar<EnumAverageDecimalEventsForgeEval.AggregatorAvgBigDecimal>(
                    "agg",
                    NewInstance(typeof(EnumAverageDecimalEventsForgeEval.AggregatorAvgBigDecimal), math))
                .DeclareVar<ObjectArrayEventBean>(
                    "resultEvent",
                    NewInstance<ObjectArrayEventBean>(
                        NewArrayByLength(typeof(object), Constant(1)),
                        resultTypeMember))
                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(forge.streamNumLambda), @Ref("resultEvent"))
                .DeclareVar<object[]>("props", ExprDotName(@Ref("resultEvent"), "Properties"));

            CodegenBlock forEach = block.ForEach(typeof(object), "next", EnumForgeCodegenNames.REF_ENUMCOLL)
                .AssignArrayElement("props", Constant(0), @Ref("next"))
                .DeclareVar(
                    innerType,
                    "num",
                    forge.innerExpression.EvaluateCodegen(typeof(object), methodNode, scope, codegenClassScope));
            if (!innerType.IsPrimitive) {
                forEach.IfRefNull("num").BlockContinue();
            }

            forEach.Expression(ExprDotMethod(@Ref("agg"), "Enter", @Ref("num")));
            block.MethodReturn(ExprDotName(@Ref("agg"), "Value"));
            return LocalMethod(methodNode, args.Eps, args.Enumcoll, args.IsNewData, args.ExprCtx);
        }
    }
} // end of namespace