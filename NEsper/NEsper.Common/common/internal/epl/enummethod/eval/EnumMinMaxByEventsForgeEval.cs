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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.
    CodegenRelational;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class EnumMinMaxByEventsForgeEval : EnumEval
    {
        private readonly EnumMinMaxByEventsForge _forge;
        private readonly ExprEvaluator _innerExpression;

        public EnumMinMaxByEventsForgeEval(
            EnumMinMaxByEventsForge forge,
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
            IComparable minKey = null;
            EventBean result = null;

            var beans = (ICollection<EventBean>) enumcoll;
            foreach (var next in beans) {
                eventsLambda[_forge.StreamNumLambda] = next;

                var comparable = _innerExpression.Evaluate(eventsLambda, isNewData, context);
                if (comparable == null) {
                    continue;
                }

                if (minKey == null) {
                    minKey = (IComparable) comparable;
                    result = next;
                }
                else {
                    if (_forge.max) {
                        if (minKey.CompareTo(comparable) < 0) {
                            minKey = (IComparable) comparable;
                            result = next;
                        }
                    }
                    else {
                        if (minKey.CompareTo(comparable) > 0) {
                            minKey = (IComparable) comparable;
                            result = next;
                        }
                    }
                }
            }

            return result; // unpack of EventBean to underlying performed at another stage
        }

        public static CodegenExpression Codegen(
            EnumMinMaxByEventsForge forge,
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var innerType = forge.InnerExpression.EvaluationType;
            var innerTypeBoxed = Boxing.GetBoxedType(innerType);
            var paramTypes = EnumForgeCodegenNames.PARAMS;
            
            var scope = new ExprForgeCodegenSymbol(false, null);
            var methodNode = codegenMethodScope
                .MakeChildWithScope(typeof(EventBean), typeof(EnumMinMaxByEventsForgeEval), scope, codegenClassScope)
                .AddParam(paramTypes);

            var block = methodNode.Block
                .DeclareVar(innerTypeBoxed, "minKey", ConstantNull())
                .DeclareVar<EventBean>("result", ConstantNull());

            var forEach = block.ForEach(typeof(EventBean), "next", EnumForgeCodegenNames.REF_ENUMCOLL)
                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(forge.StreamNumLambda), Ref("next"))
                .DeclareVar(
                    innerTypeBoxed,
                    "value",
                    forge.InnerExpression.EvaluateCodegen(innerTypeBoxed, methodNode, scope, codegenClassScope))
                .IfRefNull("value")
                .BlockContinue();

            forEach.IfCondition(EqualsNull(Ref("minKey")))
                .AssignRef("minKey", Ref("value"))
                .AssignRef("result", Ref("next"))
                .IfElse()
                .IfCondition(
                    Relational(
                        ExprDotMethod(Unbox(Ref("minKey"), innerTypeBoxed), "CompareTo", Ref("value")),
                        forge.max ? LT : GT,
                        Constant(0)))
                .AssignRef("minKey", Ref("value"))
                .AssignRef("result", Ref("next"));

            block.MethodReturn(Ref("result"));
            return LocalMethod(methodNode, args.Eps, args.Enumcoll, args.IsNewData, args.ExprCtx);
        }
    }
} // end of namespace