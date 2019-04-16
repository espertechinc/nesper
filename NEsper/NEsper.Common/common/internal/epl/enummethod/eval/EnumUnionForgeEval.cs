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
    public class EnumUnionForgeEval : EnumEval
    {
        private readonly EnumUnionForge forge;
        private readonly ExprEnumerationEval evaluator;

        public EnumUnionForgeEval(
            EnumUnionForge forge,
            ExprEnumerationEval evaluator)
        {
            this.forge = forge;
            this.evaluator = evaluator;
        }

        public object EvaluateEnumMethod(
            EventBean[] eventsLambda,
            ICollection<object> enumcoll,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            ICollection other;
            if (forge.scalar) {
                other = evaluator.EvaluateGetROCollectionScalar(eventsLambda, isNewData, context);
            }
            else {
                other = evaluator.EvaluateGetROCollectionEvents(eventsLambda, isNewData, context);
            }

            if (other == null || other.IsEmpty()) {
                return enumcoll;
            }

            List<object> result = new List<object>(enumcoll.Count + other.Count);
            result.AddAll(enumcoll);
            result.AddAll(other);

            return result;
        }

        public static CodegenExpression Codegen(
            EnumUnionForge forge,
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            ExprForgeCodegenSymbol scope = new ExprForgeCodegenSymbol(false, null);
            CodegenMethod methodNode = codegenMethodScope
                .MakeChildWithScope(typeof(ICollection<object>), typeof(EnumUnionForgeEval), scope, codegenClassScope)
                .AddParam(EnumForgeCodegenNames.PARAMS);

            CodegenBlock block = methodNode.Block;
            if (forge.scalar) {
                block.DeclareVar(
                    typeof(ICollection<object>), "other",
                    forge.evaluatorForge.EvaluateGetROCollectionScalarCodegen(methodNode, scope, codegenClassScope));
            }
            else {
                block.DeclareVar(
                    typeof(ICollection<object>), "other",
                    forge.evaluatorForge.EvaluateGetROCollectionEventsCodegen(methodNode, scope, codegenClassScope));
            }

            block.IfCondition(Or(EqualsNull(@Ref("other")), ExprDotMethod(@Ref("other"), "isEmpty")))
                .BlockReturn(EnumForgeCodegenNames.REF_ENUMCOLL);
            block.DeclareVar(
                    typeof(List<object>), "result",
                    NewInstance(
                        typeof(List<object>),
                        Op(ExprDotMethod(EnumForgeCodegenNames.REF_ENUMCOLL, "size"), "+", ExprDotMethod(@Ref("other"), "size"))))
                .Expression(ExprDotMethod(@Ref("result"), "addAll", EnumForgeCodegenNames.REF_ENUMCOLL))
                .Expression(ExprDotMethod(@Ref("result"), "addAll", @Ref("other")))
                .MethodReturn(@Ref("result"));
            return LocalMethod(methodNode, args.Eps, args.Enumcoll, args.IsNewData, args.ExprCtx);
        }
    }
} // end of namespace