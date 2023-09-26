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

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    public class ExprNewStructNodeForgeEval : ExprTypableReturnEval
    {
        private readonly ExprEvaluator[] _evaluators;

        private readonly ExprNewStructNodeForge _forge;

        public ExprNewStructNodeForgeEval(
            ExprNewStructNodeForge forge,
            ExprEvaluator[] evaluators)
        {
            _forge = forge;
            _evaluators = evaluators;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var columnNames = _forge.ForgeRenderable.ColumnNames;
            IDictionary<string, object> props = new Dictionary<string, object>();
            for (var i = 0; i < _evaluators.Length; i++) {
                props.Put(columnNames[i], _evaluators[i].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext));
            }

            return props;
        }

        public static CodegenExpression Codegen(
            ExprNewStructNodeForge forge,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(
                typeof(IDictionary<string, object>),
                typeof(ExprNewStructNodeForgeEval),
                codegenClassScope);

            var block = methodNode.Block
                .DeclareVar<IDictionary<string, object>>(
                    "props",
                    NewInstance(typeof(HashMap<string, object>)));
            var nodes = forge.ForgeRenderable.ChildNodes;
            var columnNames = forge.ForgeRenderable.ColumnNames;
            for (var i = 0; i < nodes.Length; i++) {
                var child = nodes[i].Forge;
                block.ExprDotMethod(
                    Ref("props"),
                    "Put",
                    Constant(columnNames[i]),
                    child.EvaluateCodegen(typeof(object), methodNode, exprSymbol, codegenClassScope));
            }

            block.MethodReturn(Ref("props"));
            return LocalMethod(methodNode);
        }

        public object[] EvaluateTypableSingle(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var columnNames = _forge.ForgeRenderable.ColumnNames;
            var rows = new object[columnNames.Length];
            for (var i = 0; i < columnNames.Length; i++) {
                rows[i] = _evaluators[i].Evaluate(eventsPerStream, isNewData, context);
            }

            return rows;
        }

        public static CodegenExpression CodegenTypeableSingle(
            ExprNewStructNodeForge forge,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(
                typeof(object[]),
                typeof(ExprNewStructNodeForgeEval),
                codegenClassScope);

            var block = methodNode.Block
                .DeclareVar<object[]>(
                    "rows",
                    NewArrayByLength(typeof(object), Constant(forge.ForgeRenderable.ColumnNames.Length)));
            for (var i = 0; i < forge.ForgeRenderable.ColumnNames.Length; i++) {
                block.AssignArrayElement(
                    "rows",
                    Constant(i),
                    forge.ForgeRenderable.ChildNodes[i]
                        .Forge.EvaluateCodegen(
                            typeof(object),
                            methodNode,
                            exprSymbol,
                            codegenClassScope));
            }

            block.MethodReturn(Ref("rows"));
            return LocalMethod(methodNode);
        }

        public object[][] EvaluateTypableMulti(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return null;
        }
    }
} // end of namespace