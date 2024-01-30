///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.common.@internal.rettype;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
    public class ExprDotNodeForgeStreamEvalEventBean : ExprEvaluator
    {
        private readonly ExprDotNodeForgeStream _forge;
        private readonly ExprDotEval[] _evaluators;

        public ExprDotNodeForgeStreamEvalEventBean(
            ExprDotNodeForgeStream forge,
            ExprDotEval[] evaluators)
        {
            _forge = forge;
            _evaluators = evaluators;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var theEvent = eventsPerStream[_forge.StreamNumber];
            if (theEvent == null) {
                return null;
            }

            return ExprDotNodeUtility.EvaluateChain(
                _forge.Evaluators,
                _evaluators,
                theEvent,
                eventsPerStream,
                isNewData,
                exprEvaluatorContext);
        }

        public static CodegenExpression Codegen(
            ExprDotNodeForgeStream forge,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(
                forge.EvaluationType,
                typeof(ExprDotNodeForgeStreamEvalEventBean),
                codegenClassScope);
            var refEPS = exprSymbol.GetAddEps(methodNode);

            var typeInformation = ConstantNull();
            if (codegenClassScope.IsInstrumented) {
                typeInformation =
                    codegenClassScope.AddOrGetDefaultFieldSharable(
                        new EPChainableTypeCodegenSharable(
                            EPChainableTypeHelper.SingleEvent(forge.EventType),
                            codegenClassScope));
            }

            methodNode.Block
                .DeclareVar<EventBean>("@event", ArrayAtIndex(refEPS, Constant(forge.StreamNumber)))
                .Apply(
                    InstrumentationCode.Instblock(
                        codegenClassScope,
                        "qExprDotChain",
                        typeInformation,
                        Ref("@event"),
                        Constant(forge.Evaluators.Length)))
                .IfRefNull("@event")
                .Apply(InstrumentationCode.Instblock(codegenClassScope, "aExprDotChain"))
                .BlockReturn(ConstantNull())
                .DeclareVar(
                    forge.EvaluationType,
                    "result",
                    ExprDotNodeUtility.EvaluateChainCodegen(
                        methodNode,
                        exprSymbol,
                        codegenClassScope,
                        Ref("@event"),
                        typeof(EventBean),
                        forge.Evaluators,
                        null))
                .Apply(InstrumentationCode.Instblock(codegenClassScope, "aExprDotChain"))
                .MethodReturn(Ref("result"));
            return LocalMethod(methodNode);
        }
    }
} // end of namespace