///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.agg.method.avg.AggregatorAvgBig;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class EnumAverageDecimalEventsForgeEval : EnumEval
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly EnumAverageDecimalEventsForge _forge;
        private readonly ExprEvaluator _innerExpression;

        public EnumAverageDecimalEventsForgeEval(
            EnumAverageDecimalEventsForge forge,
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
            var agg = new AggregatorAvgDecimal(_forge.optionalMathContext);

            var beans = (ICollection<EventBean>) enumcoll;
            foreach (var next in beans) {
                eventsLambda[_forge.streamNumLambda] = next;

                var num = _innerExpression.Evaluate(eventsLambda, isNewData, context);
                if (num == null) {
                    continue;
                }

                agg.Enter(num);
            }

            return agg.Value;
        }

        public static CodegenExpression Codegen(
            EnumAverageDecimalEventsForge forge,
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var innerType = forge.innerExpression.EvaluationType;

            CodegenExpression math =
                codegenClassScope.AddOrGetDefaultFieldSharable(new MathContextCodegenField(forge.optionalMathContext));

            var scope = new ExprForgeCodegenSymbol(false, null);
            var methodNode = codegenMethodScope.MakeChildWithScope(
                    typeof(decimal?),
                    typeof(EnumAverageDecimalEventsForgeEval),
                    scope,
                    codegenClassScope)
                .AddParam(EnumForgeCodegenNames.PARAMS_EVENTBEAN);

            var block = methodNode.Block;
            block.DeclareVar<AggregatorAvgDecimal>(
                "agg",
                NewInstance<AggregatorAvgDecimal>(math));
            var forEach = block.ForEach(typeof(EventBean), "next", EnumForgeCodegenNames.REF_ENUMCOLL)
                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(forge.streamNumLambda), Ref("next"))
                .DeclareVar(
                    innerType,
                    "num",
                    forge.innerExpression.EvaluateCodegen(innerType, methodNode, scope, codegenClassScope));
            if (!innerType.IsPrimitive) {
                forEach.IfRefNull("num").BlockContinue();
            }

            forEach.Expression(ExprDotMethod(Ref("agg"), "Enter", Ref("num")))
                .BlockEnd();
            block.MethodReturn(ExprDotName(Ref("agg"), "Value"));
            return LocalMethod(methodNode, args.Eps, args.Enumcoll, args.IsNewData, args.ExprCtx);
        }

        public class AggregatorAvgDecimal
        {
            private readonly MathContext _optionalMathContext;
            private long _cnt;
            private decimal _sum;

            public AggregatorAvgDecimal(MathContext optionalMathContext)
            {
                _sum = 0.0m;
                _optionalMathContext = optionalMathContext;
            }

            public decimal? Value => _optionalMathContext.GetValueDivide(_sum, _cnt);

            public void Enter(object @object)
            {
                if (@object == null) {
                    return;
                }

                _cnt++;
                if (@object is BigInteger bigInt) {
                    _sum += (decimal) bigInt;
                    return;
                }

                _sum += (decimal) @object;
            }
        }
    }
} // end of namespace