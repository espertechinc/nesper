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
    public class EnumFirstOfPredicateEventsForgeEval : EnumEval
    {
        private readonly EnumFirstOfPredicateEventsForge forge;
        private readonly ExprEvaluator innerExpression;

        public EnumFirstOfPredicateEventsForgeEval(
            EnumFirstOfPredicateEventsForge forge,
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
            ICollection<EventBean> beans = (ICollection<EventBean>) enumcoll;
            foreach (EventBean next in beans) {
                eventsLambda[forge.streamNumLambda] = next;

                object pass = innerExpression.Evaluate(eventsLambda, isNewData, context);
                if (pass == null || false.Equals(pass)) {
                    continue;
                }

                return next;
            }

            return null;
        }

        public static CodegenExpression Codegen(
            EnumFirstOfPredicateEventsForge forge,
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            ExprForgeCodegenSymbol scope = new ExprForgeCodegenSymbol(false, null);
            CodegenMethod methodNode = codegenMethodScope
                .MakeChildWithScope(typeof(EventBean), typeof(EnumFirstOfPredicateEventsForgeEval), scope, codegenClassScope)
                .AddParam(EnumForgeCodegenNames.PARAMS);

            CodegenBlock block = methodNode.Block;
            CodegenBlock forEach = block.ForEach(typeof(EventBean), "next", EnumForgeCodegenNames.REF_ENUMCOLL)
                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(forge.streamNumLambda), @Ref("next"));
            CodegenLegoBooleanExpression.CodegenContinueIfNotNullAndNotPass(
                forEach, forge.innerExpression.EvaluationType,
                forge.innerExpression.EvaluateCodegen(typeof(bool?), methodNode, scope, codegenClassScope));
            forEach.BlockReturn(@Ref("next"));
            block.MethodReturn(ConstantNull());
            return LocalMethod(methodNode, args.Eps, args.Enumcoll, args.IsNewData, args.ExprCtx);
        }
    }
} // end of namespace