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
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.
    CodegenRelational;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class EnumMinMaxEventsForgeEval : EnumEval
    {
        private readonly EnumMinMaxEventsForge forge;
        private readonly ExprEvaluator innerExpression;

        public EnumMinMaxEventsForgeEval(
            EnumMinMaxEventsForge forge,
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
            IComparable minKey = null;

            var beans = (ICollection<EventBean>) enumcoll;
            foreach (var next in beans) {
                eventsLambda[forge.streamNumLambda] = next;

                var comparable = innerExpression.Evaluate(eventsLambda, isNewData, context);
                if (comparable == null) {
                    continue;
                }

                if (minKey == null) {
                    minKey = (IComparable) comparable;
                }
                else {
                    if (forge.max) {
                        if (minKey.CompareTo(comparable) < 0) {
                            minKey = (IComparable) comparable;
                        }
                    }
                    else {
                        if (minKey.CompareTo(comparable) > 0) {
                            minKey = (IComparable) comparable;
                        }
                    }
                }
            }

            return minKey;
        }

        public static CodegenExpression Codegen(
            EnumMinMaxEventsForge forge,
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var innerType = forge.innerExpression.EvaluationType;
            var innerTypeBoxed = Boxing.GetBoxedType(innerType);
            //var paramTypes = (innerType == typeof(EventBean))
            //    ? EnumForgeCodegenNames.PARAMS_EVENTBEAN
            //    : EnumForgeCodegenNames.PARAMS_OBJECT;
            var paramTypes = EnumForgeCodegenNames.PARAMS_EVENTBEAN;
            
            var scope = new ExprForgeCodegenSymbol(false, null);
            var methodNode = codegenMethodScope
                .MakeChildWithScope(innerTypeBoxed, typeof(EnumMinMaxEventsForgeEval), scope, codegenClassScope)
                .AddParam(paramTypes);

            var block = methodNode.Block
                .DeclareVar(innerTypeBoxed, "minKey", ConstantNull());

            var forEach = block.ForEach(typeof(EventBean), "next", EnumForgeCodegenNames.REF_ENUMCOLL)
                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(forge.streamNumLambda), @Ref("next"))
                .DeclareVar(
                    innerTypeBoxed,
                    "value",
                    forge.innerExpression.EvaluateCodegen(innerTypeBoxed, methodNode, scope, codegenClassScope));
            if (innerType.CanBeNull()) {
                forEach.IfRefNull("value").BlockContinue();
            }

            forEach.IfCondition(EqualsNull(@Ref("minKey")))
                .AssignRef("minKey", @Ref("value"))
                .IfElse()
                .IfCondition(
                    Relational(
                        ExprDotMethod(Unbox(@Ref("minKey"), innerTypeBoxed), "CompareTo", @Ref("value")),
                        forge.max ? LT : GT,
                        Constant(0)))
                .AssignRef("minKey", @Ref("value"));

            block.MethodReturn(@Ref("minKey"));
            return LocalMethod(methodNode, args.Eps, args.Enumcoll, args.IsNewData, args.ExprCtx);
        }
    }
} // end of namespace