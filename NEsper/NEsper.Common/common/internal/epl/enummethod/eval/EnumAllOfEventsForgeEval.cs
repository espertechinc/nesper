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
    public class EnumAllOfEventsForgeEval : EnumEval
    {
        private readonly EnumAllOfEventsForge _forge;
        private readonly ExprEvaluator _innerExpression;

        public EnumAllOfEventsForgeEval(
            EnumAllOfEventsForge forge,
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
            if (enumcoll.IsEmpty()) {
                return true;
            }

            var beans = (ICollection<EventBean>) enumcoll;
            foreach (var next in beans) {
                eventsLambda[_forge.StreamNumLambda] = next;

                var result = _innerExpression.Evaluate(eventsLambda, isNewData, context);
                if (result == null || (!(Boolean) result)) {
                    return false;
                }
            }

            return true;
        }

        public static CodegenExpression Codegen(
            EnumAllOfEventsForge forge,
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var scope = new ExprForgeCodegenSymbol(false, null);
            var methodNode = codegenMethodScope.MakeChildWithScope(
                    typeof(bool),
                    typeof(EnumAllOfEventsForgeEval),
                    scope,
                    codegenClassScope)
                .AddParam(EnumForgeCodegenNames.PARAMS);

            var block = methodNode.Block;
            block.IfConditionReturnConst(ExprDotMethod(EnumForgeCodegenNames.REF_ENUMCOLL, "IsEmpty"), true);

            var forEach = block.ForEach(typeof(EventBean), "next", EnumForgeCodegenNames.REF_ENUMCOLL)
                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(forge.StreamNumLambda), Ref("next"));
            CodegenLegoBooleanExpression.CodegenReturnBoolIfNullOrBool(
                forEach,
                forge.InnerExpression.EvaluationType,
                forge.InnerExpression.EvaluateCodegen(typeof(bool?), methodNode, scope, codegenClassScope),
                true,
                false,
                false,
                false);
            block.MethodReturn(ConstantTrue());
            return LocalMethod(methodNode, args.Eps, args.Enumcoll, args.IsNewData, args.ExprCtx);
        }
    }
} // end of namespace