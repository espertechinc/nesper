///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.@base;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.average {
    public class EnumAverageDoubleEvent : ThreeFormEventPlain {
        public EnumAverageDoubleEvent(ExprDotEvalParamLambda lambda) : base(lambda)
        {
        }

        public override EnumEval EnumEvaluator {
            get {
                var inner = InnerExpression.ExprEvaluator;

                return new ProxyEnumEval(
                    (
                        eventsLambda,
                        enumcoll,
                        isNewData,
                        context) => {
                        var sum = 0d;
                        var rowcount = 0;

                        var beans = (ICollection<EventBean>)enumcoll;
                        foreach (var next in beans) {
                            eventsLambda[StreamNumLambda] = next;

                            var num = inner.Evaluate(eventsLambda, isNewData, context);
                            if (num == null) {
                                continue;
                            }

                            rowcount++;
                            sum += num.AsDouble();
                        }

                        if (rowcount == 0) {
                            return null;
                        }

                        return sum / rowcount;
                    });
            }
        }

        public override Type ReturnTypeOfMethod(Type desiredReturnType)
        {
            return typeof(double?);
        }

        public override CodegenExpression ReturnIfEmptyOptional(Type desiredReturnType)
        {
            return null;
        }

        public override void InitBlock(
            CodegenBlock block,
            CodegenMethod methodNode,
            ExprForgeCodegenSymbol scope,
            CodegenClassScope codegenClassScope, Type desiredReturnType)
        {
            block
                .DeclareVar<double>("sum", Constant(0d))
                .DeclareVar<int>("rowcount", Constant(0));
        }

        public override void ForEachBlock(
            CodegenBlock block,
            CodegenMethod methodNode,
            ExprForgeCodegenSymbol scope,
            CodegenClassScope codegenClassScope, Type desiredReturnType)
        {
            var innerType = InnerExpression.EvaluationType;
            block.DeclareVar(
                innerType,
                "num",
                InnerExpression.EvaluateCodegen(innerType, methodNode, scope, codegenClassScope));
            if (!innerType.IsPrimitive) {
                block.IfRefNull("num").BlockContinue();
            }

            var lhs = Ref("sum");
            var rhs = SimpleNumberCoercerFactory.CoercerDouble.CodegenDouble(Ref("num"), innerType);

            block.IncrementRef("rowcount")
                .AssignRef("sum", Op(lhs, "+", rhs))
                .BlockEnd();
        }

        public override void ReturnResult(CodegenBlock block)
        {
            block.IfCondition(EqualsIdentity(Ref("rowcount"), Constant(0)))
                .BlockReturn(ConstantNull())
                .MethodReturn(Op(Ref("sum"), "/", Ref("rowcount")));
        }
    }
} // end of namespace