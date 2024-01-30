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
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
    public class ExprDotNodeForgePropertyExprEvalIndexed : ExprEvaluator
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ExprDotNodeForgePropertyExpr _forge;
        private readonly ExprEvaluator _exprEvaluator;

        public ExprDotNodeForgePropertyExprEvalIndexed(
            ExprDotNodeForgePropertyExpr forge,
            ExprEvaluator exprEvaluator)
        {
            _forge = forge;
            _exprEvaluator = exprEvaluator;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var @event = eventsPerStream[_forge.StreamNum];
            if (@event == null) {
                return null;
            }

            var index = _exprEvaluator.Evaluate(eventsPerStream, isNewData, context);
            if (index == null || !index.IsInt32()) {
                Log.Warn(_forge.GetWarningText("integer", index));
                return null;
            }

            return _forge.IndexedGetter.Get(@event, index.AsInt32());
        }

        public static CodegenExpression Codegen(
            ExprDotNodeForgePropertyExpr forge,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(
                forge.EvaluationType,
                typeof(ExprDotNodeForgePropertyExprEvalIndexed),
                codegenClassScope);

            var refEPS = exprSymbol.GetAddEps(methodNode);
            methodNode.Block
                .DeclareVar<EventBean>("@event", ArrayAtIndex(refEPS, Constant(forge.StreamNum)))
                .IfRefNullReturnNull("@event")
                .DeclareVar<int?>(
                    "index",
                    forge.ExprForge.EvaluateCodegen(typeof(int?), methodNode, exprSymbol, codegenClassScope))
                .IfRefNullReturnNull("index")
                .MethodReturn(
                    CodegenLegoCast.CastSafeFromObjectType(
                        forge.EvaluationType,
                        forge.IndexedGetter.EventBeanGetIndexedCodegen(
                            methodNode,
                            codegenClassScope,
                            Ref("@event"),
                            Unbox(Ref("index")))));
            return LocalMethod(methodNode);
        }
    }
} // end of namespace