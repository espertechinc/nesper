///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Numerics;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.average
{
    public class EnumAverageBigIntegerScalarNoParam : EnumForgeBasePlain,
        EnumEval
    {
        public EnumAverageBigIntegerScalarNoParam(int streamCountIncoming) : base(streamCountIncoming)
        {
        }

        public override EnumEval EnumEvaluator => this;

        public object EvaluateEnumMethod(
            EventBean[] eventsLambda,
            ICollection<object> enumcoll,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var sum = BigInteger.Zero;
            var count = 0;

            foreach (var next in enumcoll) {
                var num = next;
                if (num == null) {
                    continue;
                }

                count++;
                sum += num.AsBigInteger();
            }

            if (count == 0) {
                return null;
            }

            return sum / count;
        }

        public override CodegenExpression Codegen(
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var method = codegenMethodScope
                .MakeChild(typeof(BigInteger?), typeof(EnumAverageDecimalScalarNoParam), codegenClassScope)
                .AddParam(EnumForgeCodegenNames.PARAMS)
                .Block
                .DeclareVar<BigInteger>("sum", EnumValue(typeof(BigInteger), "Zero"))
                .DeclareVar<int>("count", Constant(0))
                .ForEach(typeof(object), "num", EnumForgeCodegenNames.REF_ENUMCOLL)
                .IfRefNull("num")
                .BlockContinue()
                .IncrementRef("count")
                .AssignRef("sum", Op(Ref("sum"), "+", ExprDotMethod(Ref("num"), "AsBigInteger")))
                .BlockEnd()
                .IfCondition(EqualsIdentity(Ref("count"), Constant(0)))
                .BlockReturn(ConstantNull())
                .MethodReturn(Op(Ref("sum"), "/", Ref("count")));

            return LocalMethod(method, args.Expressions);
        }
    }
} // end of namespace