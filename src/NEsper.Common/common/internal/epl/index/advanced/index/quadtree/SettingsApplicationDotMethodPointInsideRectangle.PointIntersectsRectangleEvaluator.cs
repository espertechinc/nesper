///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.epl.spatial.quadtree.core;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.index.advanced.index.quadtree
{
    public partial class SettingsApplicationDotMethodPointInsideRectangle
    {
        public sealed class PointIntersectsRectangleEvaluator : ExprEvaluator
        {
            private readonly ExprEvaluator pxEval;
            private readonly ExprEvaluator pyEval;
            private readonly ExprEvaluator xEval;
            private readonly ExprEvaluator yEval;
            private readonly ExprEvaluator widthEval;
            private readonly ExprEvaluator heightEval;

            internal PointIntersectsRectangleEvaluator(
                ExprEvaluator pxEval,
                ExprEvaluator pyEval,
                ExprEvaluator xEval,
                ExprEvaluator yEval,
                ExprEvaluator widthEval,
                ExprEvaluator heightEval)
            {
                this.pxEval = pxEval;
                this.pyEval = pyEval;
                this.xEval = xEval;
                this.yEval = yEval;
                this.widthEval = widthEval;
                this.heightEval = heightEval;
            }

            public object Evaluate(
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext context)
            {
                var px = pxEval.Evaluate(eventsPerStream, isNewData, context);
                if (px == null) {
                    return null;
                }

                var py = pyEval.Evaluate(eventsPerStream, isNewData, context);
                if (py == null) {
                    return null;
                }

                var x = xEval.Evaluate(eventsPerStream, isNewData, context);
                if (x == null) {
                    return null;
                }

                var y = yEval.Evaluate(eventsPerStream, isNewData, context);
                if (y == null) {
                    return null;
                }

                var width = widthEval.Evaluate(eventsPerStream, isNewData, context);
                if (width == null) {
                    return null;
                }

                var height = heightEval.Evaluate(eventsPerStream, isNewData, context);
                if (height == null) {
                    return null;
                }

                return BoundingBox.ContainsPoint(
                    x.AsDouble(),
                    y.AsDouble(),
                    width.AsDouble(),
                    height.AsDouble(),
                    px.AsDouble(),
                    py.AsDouble());
            }

            public static CodegenExpression Codegen(
                PointIntersectsRectangleForge forge,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var methodNode = codegenMethodScope.MakeChild(
                    typeof(bool?),
                    typeof(SettingsApplicationDotMethodRectangeIntersectsRectangle.RectangleIntersectsRectangleEvaluator
                    ),
                    codegenClassScope);

                var block = methodNode.Block;
                CodegenLegoCast.AsDoubleNullReturnNull(
                    block,
                    "px",
                    forge.pxEval,
                    methodNode,
                    exprSymbol,
                    codegenClassScope);
                CodegenLegoCast.AsDoubleNullReturnNull(
                    block,
                    "py",
                    forge.pyEval,
                    methodNode,
                    exprSymbol,
                    codegenClassScope);
                CodegenLegoCast.AsDoubleNullReturnNull(
                    block,
                    "x",
                    forge.xEval,
                    methodNode,
                    exprSymbol,
                    codegenClassScope);
                CodegenLegoCast.AsDoubleNullReturnNull(
                    block,
                    "y",
                    forge.yEval,
                    methodNode,
                    exprSymbol,
                    codegenClassScope);
                CodegenLegoCast.AsDoubleNullReturnNull(
                    block,
                    "width",
                    forge.widthEval,
                    methodNode,
                    exprSymbol,
                    codegenClassScope);
                CodegenLegoCast.AsDoubleNullReturnNull(
                    block,
                    "height",
                    forge.heightEval,
                    methodNode,
                    exprSymbol,
                    codegenClassScope);
                block.MethodReturn(
                    CodegenExpressionBuilder.StaticMethod(
                        typeof(BoundingBox),
                        "ContainsPoint",
                        CodegenExpressionBuilder.Ref("x"),
                        CodegenExpressionBuilder.Ref("y"),
                        CodegenExpressionBuilder.Ref("width"),
                        CodegenExpressionBuilder.Ref("height"),
                        CodegenExpressionBuilder.Ref("px"),
                        CodegenExpressionBuilder.Ref("py")));
                return CodegenExpressionBuilder.LocalMethod(methodNode);
            }
        }
    }
}