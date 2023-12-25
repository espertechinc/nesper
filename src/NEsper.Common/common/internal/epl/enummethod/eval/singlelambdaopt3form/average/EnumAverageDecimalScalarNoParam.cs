///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.average
{
    public class EnumAverageDecimalScalarNoParam : EnumForgeBasePlain,
        EnumEval
    {
        public EnumAverageDecimalScalarNoParam(int streamCountIncoming) : base(streamCountIncoming)
        {
        }

        public override EnumEval EnumEvaluator => this;

        public object EvaluateEnumMethod(
            EventBean[] eventsLambda,
            ICollection<object> enumcoll,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var sum = 0.0m;
            var count = 0;

            foreach (var next in enumcoll) {
                var num = next;
                if (num == null) {
                    continue;
                }

                count++;
                sum += num.AsDecimal();
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
                .MakeChild(typeof(decimal?), typeof(EnumAverageDecimalScalarNoParam), codegenClassScope)
                .AddParam(ExprForgeCodegenNames.FP_EPS)
                .AddParam(args.EnumcollType, EnumForgeCodegenNames.REF_ENUMCOLL.Ref)
                .AddParam(ExprForgeCodegenNames.FP_ISNEWDATA)
                .AddParam(ExprForgeCodegenNames.FP_EXPREVALCONTEXT)
                .Block
                .DeclareVar<decimal>("sum", Constant(0.0m))
                .DeclareVar<int>("count", Constant(0))
                .DeclareVar(args.EnumcollType, "coll", EnumForgeCodegenNames.REF_ENUMCOLL)
                .ForEachVar("num", Ref("coll"))
                .IfRefNull("num")
                .BlockContinue()
                .IncrementRef("count")
                .AssignRef("sum", Op(Ref("sum"), "+", ExprDotMethod(Ref("num"), "AsDecimal")))
                .BlockEnd()
                .IfCondition(EqualsIdentity(Ref("count"), Constant(0)))
                .BlockReturn(ConstantNull())
                .MethodReturn(Op(Ref("sum"), "/", Ref("count")));
            return LocalMethod(method, args.Expressions);
        }
    }
} // end of namespace