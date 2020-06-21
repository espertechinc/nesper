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
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class EnumAverageScalarLambdaForgeEval : EnumEval
    {
        private readonly EnumAverageScalarLambdaForge _forge;
        private readonly ExprEvaluator _innerExpression;

        public EnumAverageScalarLambdaForgeEval(
            EnumAverageScalarLambdaForge forge,
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
            var sum = 0d;
            var count = 0;
            var resultEvent = new ObjectArrayEventBean(new object[1], _forge.resultEventType);
            eventsLambda[_forge.StreamNumLambda] = resultEvent;
            var props = resultEvent.Properties;

            var values = (ICollection<object>) enumcoll;
            foreach (var next in values) {
                props[0] = next;

                var num = _innerExpression.Evaluate(eventsLambda, isNewData, context);
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
            EnumAverageScalarLambdaForge forge,
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var innerType = forge.InnerExpression.EvaluationType;
            var resultTypeMember = codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(ObjectArrayEventType),
                Cast(
                    typeof(ObjectArrayEventType),
                    EventTypeUtility.ResolveTypeCodegen(forge.resultEventType, EPStatementInitServicesConstants.REF)));

            var scope = new ExprForgeCodegenSymbol(false, null);
            var methodNode = codegenMethodScope.MakeChildWithScope(
                    typeof(double?),
                    typeof(EnumAverageEventsForgeEval),
                    scope,
                    codegenClassScope)
                .AddParam(EnumForgeCodegenNames.PARAMS);

            var block = methodNode.Block
                .DeclareVar<double>("sum", Constant(0d))
                .DeclareVar<int>("count", Constant(0))
                .DeclareVar<ObjectArrayEventBean>(
                    "resultEvent",
                    NewInstance<ObjectArrayEventBean>(
                        NewArrayByLength(typeof(object), Constant(1)),
                        resultTypeMember))
                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(forge.StreamNumLambda), Ref("resultEvent"))
                .DeclareVar<object[]>("props", ExprDotName(Ref("resultEvent"), "Properties"));

            var forEach = block.ForEach(typeof(object), "next", EnumForgeCodegenNames.REF_ENUMCOLL)
                .AssignArrayElement("props", Constant(0), Ref("next"))
                .DeclareVar(
                    innerType,
                    "num",
                    forge.InnerExpression.EvaluateCodegen(innerType, methodNode, scope, codegenClassScope));
            if (innerType.CanBeNull()) {
                forEach.IfRefNull("num").BlockContinue();
            }

            forEach.IncrementRef("count")
                .AssignRef(
                    "sum",
                    Op(
                        Ref("sum"),
                        "+",
                        SimpleNumberCoercerFactory.CoercerDouble.CodegenDouble(Ref("num"), innerType)))
                .BlockEnd();
            block.IfCondition(EqualsIdentity(Ref("count"), Constant(0)))
                .BlockReturn(ConstantNull())
                .MethodReturn(Op(Ref("sum"), "/", Ref("count")));
            return LocalMethod(methodNode, args.Eps, args.Enumcoll, args.IsNewData, args.ExprCtx);
        }
    }
} // end of namespace