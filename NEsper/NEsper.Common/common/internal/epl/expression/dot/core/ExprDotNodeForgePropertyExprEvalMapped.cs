///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.logging;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
    public class ExprDotNodeForgePropertyExprEvalMapped : ExprEvaluator
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ExprEvaluator exprEvaluator;

        private readonly ExprDotNodeForgePropertyExpr forge;

        public ExprDotNodeForgePropertyExprEvalMapped(
            ExprDotNodeForgePropertyExpr forge,
            ExprEvaluator exprEvaluator)
        {
            this.forge = forge;
            this.exprEvaluator = exprEvaluator;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var @event = eventsPerStream[forge.StreamNum];
            if (@event == null) {
                return null;
            }

            var result = exprEvaluator.Evaluate(eventsPerStream, isNewData, context);
            if (result != null && !(result is string)) {
                Log.Warn(forge.GetWarningText("string", result));
                return null;
            }

            return forge.MappedGetter.Get(@event, (string) result);
        }

        public static CodegenExpression Codegen(
            ExprDotNodeForgePropertyExpr forge,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(
                forge.EvaluationType,
                typeof(ExprDotNodeForgePropertyExprEvalMapped),
                codegenClassScope);
            var refEPS = exprSymbol.GetAddEPS(methodNode);

            methodNode.Block
                .DeclareVar<EventBean>("event", ArrayAtIndex(refEPS, Constant(forge.StreamNum)))
                .IfRefNullReturnNull("event")
                .DeclareVar<string>(
                    "result",
                    forge.ExprForge.EvaluateCodegen(typeof(string), methodNode, exprSymbol, codegenClassScope))
                .IfRefNullReturnNull("result")
                .MethodReturn(
                    CodegenLegoCast.CastSafeFromObjectType(
                        forge.EvaluationType,
                        forge.MappedGetter.EventBeanGetMappedCodegen(
                            methodNode,
                            codegenClassScope,
                            Ref("event"),
                            Ref("result"))));

            return LocalMethod(methodNode);
        }
    }
} // end of namespace