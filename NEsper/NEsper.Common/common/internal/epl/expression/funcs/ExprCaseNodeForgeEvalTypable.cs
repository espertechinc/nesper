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
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    public class ExprCaseNodeForgeEvalTypable : ExprTypableReturnEval
    {
        private readonly ExprCaseNodeForge forge;
        private readonly ExprEvaluator evaluator;

        public ExprCaseNodeForgeEvalTypable(ExprCaseNodeForge forge)
        {
            this.forge = forge;
            this.evaluator = forge.ExprEvaluator;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return evaluator.Evaluate(eventsPerStream, isNewData, context);
        }

        public object[] EvaluateTypableSingle(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            IDictionary<string, object> map = (IDictionary<string, object>) evaluator.Evaluate(eventsPerStream, isNewData, context);
            object[] row = new object[map.Count];
            int index = -1;
            foreach (KeyValuePair<string, object> entry in forge.mapResultType) {
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
            CodegenMethod methodNode = codegenMethodScope.MakeChild(typeof(object[]), typeof(ExprCaseNodeForgeEvalTypable), codegenClassScope);

            CodegenBlock block = methodNode.Block
                .DeclareVar(
                    typeof(IDictionary<object, object>), "map",
                    Cast(
                        typeof(IDictionary<object, object>),
                        forge.EvaluateCodegen(typeof(IDictionary<object, object>), methodNode, exprSymbol, codegenClassScope)))
                .DeclareVar(typeof(object[]), "row", NewArrayByLength(typeof(object), ExprDotMethod(@Ref("map"), "size")));
            int index = -1;
            foreach (KeyValuePair<string, object> entry in forge.mapResultType) {
                index++;
                block.AssignArrayElement(@Ref("row"), Constant(index), ExprDotMethod(@Ref("map"), "get", Constant(entry.Key)));
            }

            block.MethodReturn(@Ref("row"));
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