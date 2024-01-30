///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    public class ExprCaseNodeForgeEvalTypable : ExprTypableReturnEval
    {
        private readonly ExprCaseNodeForge _forge;
        private readonly ExprEvaluator _evaluator;

        public ExprCaseNodeForgeEvalTypable(ExprCaseNodeForge forge)
        {
            _forge = forge;
            _evaluator = forge.ExprEvaluator;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return _evaluator.Evaluate(eventsPerStream, isNewData, context);
        }

        public object[] EvaluateTypableSingle(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var map =
                (IDictionary<string, object>)_evaluator.Evaluate(eventsPerStream, isNewData, context);
            var row = new object[map.Count];
            var index = -1;
            foreach (var entry in _forge.mapResultType) {
                index++;
                row[index] = map.Get(entry.Key);
            }

            return row;
        }

        public static CodegenExpression CodegenTypeableSingle(
            ExprCaseNodeForge forge,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(
                typeof(object[]),
                typeof(ExprCaseNodeForgeEvalTypable),
                codegenClassScope);


            var block = methodNode.Block
                .DeclareVar<IDictionary<object, object>>(
                    "map",
                    StaticMethod(
                        typeof(CompatExtensions),
                        "UnwrapDictionary",
                        forge.EvaluateCodegen(
                            typeof(IDictionary<object, object>),
                            methodNode,
                            exprSymbol,
                            codegenClassScope)))
                .DeclareVar<object[]>(
                    "row",
                    NewArrayByLength(typeof(object), ExprDotName(Ref("map"), "Count")));
            var index = -1;
            foreach (var entry in forge.mapResultType) {
                index++;
                block.AssignArrayElement(
                    Ref("row"),
                    Constant(index),
                    ExprDotMethod(Ref("map"), "Get", Constant(entry.Key)));
            }

            block.MethodReturn(Ref("row"));
            return LocalMethod(methodNode);
        }

        public object[][] EvaluateTypableMulti(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return null; // always single-row
        }
    }
} // end of namespace