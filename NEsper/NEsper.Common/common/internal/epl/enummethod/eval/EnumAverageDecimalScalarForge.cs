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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class EnumAverageDecimalScalarForge : EnumForgeBase,
        EnumEval
    {
        private readonly MathContext optionalMathContext;

        public EnumAverageDecimalScalarForge(
            int streamCountIncoming,
            MathContext optionalMathContext)
            : base(
                streamCountIncoming)
        {
            this.optionalMathContext = optionalMathContext;
        }

        public override EnumEval EnumEvaluator => this;

        public object EvaluateEnumMethod(
            EventBean[] eventsLambda,
            ICollection<object> enumcoll,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var agg = new EnumAverageDecimalEventsForgeEval.AggregatorAvgBigDecimal(optionalMathContext);

            foreach (object next in enumcoll) {
                var num = next;
                if (num == null) {
                    continue;
                }

                agg.Enter(num);
            }

            return agg.Value;
        }

        public override CodegenExpression Codegen(
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            CodegenExpression math =
                codegenClassScope.AddOrGetFieldSharable(new MathContextCodegenField(optionalMathContext));
            var method = codegenMethodScope
                .MakeChild(typeof(decimal), typeof(EnumAverageScalarForge), codegenClassScope)
                .AddParam(EnumForgeCodegenNames.PARAMS)
                .Block
                .DeclareVar<EnumAverageDecimalEventsForgeEval.AggregatorAvgBigDecimal>(
                    "agg",
                    NewInstance(typeof(EnumAverageDecimalEventsForgeEval.AggregatorAvgBigDecimal), math))
                .ForEach(typeof(object), "num", EnumForgeCodegenNames.REF_ENUMCOLL)
                .IfRefNull("num")
                .BlockContinue()
                .Expression(ExprDotMethod(Ref("agg"), "enter", Ref("num")))
                .BlockEnd()
                .MethodReturn(ExprDotMethod(Ref("agg"), "getValue"));
            return LocalMethod(method, args.Expressions);
        }
    }
} // end of namespace