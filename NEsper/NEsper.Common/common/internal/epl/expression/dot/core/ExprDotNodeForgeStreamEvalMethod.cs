///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.metrics.instrumentation.InstrumentationCode;

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
    public class ExprDotNodeForgeStreamEvalMethod : ExprEvaluator
    {
        private readonly ExprDotNodeForgeStream forge;
        private readonly ExprDotEval[] evaluators;

        public ExprDotNodeForgeStreamEvalMethod(
            ExprDotNodeForgeStream forge,
            ExprDotEval[] evaluators)
        {
            this.forge = forge;
            this.evaluators = evaluators;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            // get underlying event
            EventBean @event = eventsPerStream[forge.StreamNumber];
            if (@event == null) {
                return null;
            }

            object inner = @event.Underlying;

            inner = ExprDotNodeUtility.EvaluateChain(forge.Evaluators, evaluators, inner, eventsPerStream, isNewData, exprEvaluatorContext);
            return inner;
        }

        public static CodegenExpression Codegen(
            ExprDotNodeForgeStream forge,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            Type evaluationType = forge.EvaluationType;
            Type eventUndType = forge.EventType.UnderlyingType;
            CodegenMethod methodNode = codegenMethodScope.MakeChild(evaluationType, typeof(ExprDotNodeForgeStreamEvalMethod), codegenClassScope);
            CodegenExpressionRef refEPS = exprSymbol.GetAddEPS(methodNode);

            CodegenBlock block = methodNode.Block
                .Apply(Instblock(codegenClassScope, "qExprStreamUndMethod", Constant(ExprNodeUtilityPrint.ToExpressionStringMinPrecedence(forge))))
                .DeclareVar(typeof(EventBean), "event", ArrayAtIndex(refEPS, Constant(forge.StreamNumber)));
            if (evaluationType == typeof(void)) {
                block.IfCondition(EqualsNull(@Ref("event")))
                    .Apply(Instblock(codegenClassScope, "aExprStreamUndMethod", ConstantNull()))
                    .BlockReturnNoValue();
            }
            else {
                block.IfRefNull("event")
                    .Apply(Instblock(codegenClassScope, "aExprStreamUndMethod", ConstantNull()))
                    .BlockReturn(ConstantNull());
            }

            CodegenExpression typeInformation = ConstantNull();
            if (codegenClassScope.IsInstrumented) {
                typeInformation = codegenClassScope.AddOrGetFieldSharable(
                    new EPTypeCodegenSharable(EPTypeHelper.SingleValue(forge.EventType.UnderlyingType), codegenClassScope));
            }

            block.DeclareVar(eventUndType, "inner", Cast(eventUndType, ExprDotMethod(@Ref("event"), "getUnderlying")))
                .Apply(Instblock(codegenClassScope, "qExprDotChain", typeInformation, @Ref("inner"), Constant(forge.Evaluators.Length)));
            CodegenExpression invoke = ExprDotNodeUtility.EvaluateChainCodegen(
                methodNode, exprSymbol, codegenClassScope, @Ref("inner"), eventUndType, forge.Evaluators, null);
            if (evaluationType == typeof(void)) {
                block.Expression(invoke)
                    .Apply(Instblock(codegenClassScope, "aExprDotChain"))
                    .Apply(Instblock(codegenClassScope, "aExprStreamUndMethod", ConstantNull()))
                    .MethodEnd();
            }
            else {
                block.DeclareVar(evaluationType, "result", invoke)
                    .Apply(Instblock(codegenClassScope, "aExprDotChain"))
                    .Apply(Instblock(codegenClassScope, "aExprStreamUndMethod", @Ref("result")))
                    .MethodReturn(@Ref("result"));
            }

            return LocalMethod(methodNode);
        }
    }
} // end of namespace