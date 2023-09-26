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
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.epl.spatial.quadtree.core;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.index.advanced.index.quadtree
{
    public class SettingsApplicationDotMethodRectangeIntersectsRectangle : SettingsApplicationDotMethodBase
    {
        public const string LOOKUP_OPERATION_NAME = "rectangle.intersects(rectangle)";
        public const string INDEXTYPE_NAME = "mxcifquadtree";

        public SettingsApplicationDotMethodRectangeIntersectsRectangle(
            ExprDotNodeImpl parent,
            string lhsName,
            ExprNode[] lhs,
            string dotMethodName,
            string rhsName,
            ExprNode[] rhs,
            ExprNode[] indexNamedParameter)
            : base(parent, lhsName, lhs, dotMethodName, rhsName, rhs, indexNamedParameter)

        {
        }

        protected override string OperationName => LOOKUP_OPERATION_NAME;

        protected override string IndexTypeName => INDEXTYPE_NAME;

        protected override ExprForge ValidateAll(
            string lhsName,
            ExprNode[] lhs,
            string rhsName,
            ExprNode[] rhs,
            ExprValidationContext validationContext)
        {
            EPLValidationUtil.ValidateParameterNumber(lhsName, LHS_VALIDATION_NAME, false, 4, lhs.Length);
            EPLValidationUtil.ValidateParametersTypePredefined(
                lhs,
                lhsName,
                LHS_VALIDATION_NAME,
                EPLExpressionParamType.NUMERIC);

            EPLValidationUtil.ValidateParameterNumber(rhsName, RHS_VALIDATION_NAME, true, 4, rhs.Length);
            EPLValidationUtil.ValidateParametersTypePredefined(
                rhs,
                rhsName,
                RHS_VALIDATION_NAME,
                EPLExpressionParamType.NUMERIC);

            var meXEval = lhs[0].Forge;
            var meYEval = lhs[1].Forge;
            var meWidthEval = lhs[2].Forge;
            var meHeightEval = lhs[3].Forge;

            var otherXEval = rhs[0].Forge;
            var otherYEval = rhs[1].Forge;
            var otherWidthEval = rhs[2].Forge;
            var otherHeightEval = rhs[3].Forge;
            return new RectangleIntersectsRectangleForge(
                parent,
                meXEval,
                meYEval,
                meWidthEval,
                meHeightEval,
                otherXEval,
                otherYEval,
                otherWidthEval,
                otherHeightEval);
        }

        public sealed class RectangleIntersectsRectangleForge : ExprForge
        {
            internal readonly ExprForge meHeightEval;
            internal readonly ExprForge meWidthEval;
            internal readonly ExprForge meXEval;
            internal readonly ExprForge meYEval;
            internal readonly ExprForge otherHeightEval;
            internal readonly ExprForge otherWidthEval;
            internal readonly ExprForge otherXEval;
            internal readonly ExprForge otherYEval;

            private readonly ExprDotNodeImpl parent;

            public RectangleIntersectsRectangleForge(
                ExprDotNodeImpl parent,
                ExprForge meXEval,
                ExprForge meYEval,
                ExprForge meWidthEval,
                ExprForge meHeightEval,
                ExprForge otherXEval,
                ExprForge otherYEval,
                ExprForge otherWidthEval,
                ExprForge otherHeightEval)
            {
                this.parent = parent;
                this.meXEval = meXEval;
                this.meYEval = meYEval;
                this.meWidthEval = meWidthEval;
                this.meHeightEval = meHeightEval;
                this.otherXEval = otherXEval;
                this.otherYEval = otherYEval;
                this.otherWidthEval = otherWidthEval;
                this.otherHeightEval = otherHeightEval;
            }

            public ExprEvaluator ExprEvaluator => new RectangleIntersectsRectangleEvaluator(
                meXEval.ExprEvaluator,
                meYEval.ExprEvaluator,
                meWidthEval.ExprEvaluator,
                meHeightEval.ExprEvaluator,
                otherXEval.ExprEvaluator,
                otherYEval.ExprEvaluator,
                otherWidthEval.ExprEvaluator,
                otherHeightEval.ExprEvaluator);

            public CodegenExpression EvaluateCodegen(
                Type requiredType,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return RectangleIntersectsRectangleEvaluator.Codegen(
                    this,
                    codegenMethodScope,
                    exprSymbol,
                    codegenClassScope);
            }

            public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

            public Type EvaluationType => typeof(bool?);

            public ExprNodeRenderable ExprForgeRenderable => parent;
        }

        public sealed class RectangleIntersectsRectangleEvaluator : ExprEvaluator
        {
            private readonly ExprEvaluator meHeightEval;
            private readonly ExprEvaluator meWidthEval;

            private readonly ExprEvaluator meXEval;
            private readonly ExprEvaluator meYEval;
            private readonly ExprEvaluator otherHeightEval;
            private readonly ExprEvaluator otherWidthEval;
            private readonly ExprEvaluator otherXEval;
            private readonly ExprEvaluator otherYEval;

            public RectangleIntersectsRectangleEvaluator(
                ExprEvaluator meXEval,
                ExprEvaluator meYEval,
                ExprEvaluator meWidthEval,
                ExprEvaluator meHeightEval,
                ExprEvaluator otherXEval,
                ExprEvaluator otherYEval,
                ExprEvaluator otherWidthEval,
                ExprEvaluator otherHeightEval)
            {
                this.meXEval = meXEval;
                this.meYEval = meYEval;
                this.meWidthEval = meWidthEval;
                this.meHeightEval = meHeightEval;
                this.otherXEval = otherXEval;
                this.otherYEval = otherYEval;
                this.otherWidthEval = otherWidthEval;
                this.otherHeightEval = otherHeightEval;
            }

            public object Evaluate(
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext context)
            {
                var meX = meXEval.Evaluate(eventsPerStream, isNewData, context);
                if (meX == null) {
                    return null;
                }

                var meY = meYEval.Evaluate(eventsPerStream, isNewData, context);
                if (meY == null) {
                    return null;
                }

                var meWidth = meWidthEval.Evaluate(eventsPerStream, isNewData, context);
                if (meWidth == null) {
                    return null;
                }

                var meHeight = meHeightEval.Evaluate(eventsPerStream, isNewData, context);
                if (meHeight == null) {
                    return null;
                }

                var otherX = otherXEval.Evaluate(eventsPerStream, isNewData, context);
                if (otherX == null) {
                    return null;
                }

                var otherY = otherYEval.Evaluate(eventsPerStream, isNewData, context);
                if (otherY == null) {
                    return null;
                }

                var otherWidth = otherWidthEval.Evaluate(eventsPerStream, isNewData, context);
                if (otherWidth == null) {
                    return null;
                }

                var otherHeight = otherHeightEval.Evaluate(eventsPerStream, isNewData, context);
                if (otherHeight == null) {
                    return null;
                }

                var x = meX.AsDouble();
                var y = meY.AsDouble();
                var width = meWidth.AsDouble();
                var height = meHeight.AsDouble();
                return BoundingBox.IntersectsBoxIncludingEnd(
                    x,
                    y,
                    x + width,
                    y + height,
                    otherX.AsDouble(),
                    otherY.AsDouble(),
                    otherWidth.AsDouble(),
                    otherHeight.AsDouble());
            }

            public static CodegenExpression Codegen(
                RectangleIntersectsRectangleForge forge,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var methodNode = codegenMethodScope.MakeChild(
                    typeof(bool?),
                    typeof(RectangleIntersectsRectangleEvaluator),
                    codegenClassScope);

                var block = methodNode.Block;
                CodegenLegoCast.AsDoubleNullReturnNull(
                    block,
                    "meX",
                    forge.meXEval,
                    methodNode,
                    exprSymbol,
                    codegenClassScope);
                CodegenLegoCast.AsDoubleNullReturnNull(
                    block,
                    "meY",
                    forge.meYEval,
                    methodNode,
                    exprSymbol,
                    codegenClassScope);
                CodegenLegoCast.AsDoubleNullReturnNull(
                    block,
                    "meWidth",
                    forge.meWidthEval,
                    methodNode,
                    exprSymbol,
                    codegenClassScope);
                CodegenLegoCast.AsDoubleNullReturnNull(
                    block,
                    "meHeight",
                    forge.meHeightEval,
                    methodNode,
                    exprSymbol,
                    codegenClassScope);
                CodegenLegoCast.AsDoubleNullReturnNull(
                    block,
                    "otherX",
                    forge.otherXEval,
                    methodNode,
                    exprSymbol,
                    codegenClassScope);
                CodegenLegoCast.AsDoubleNullReturnNull(
                    block,
                    "otherY",
                    forge.otherYEval,
                    methodNode,
                    exprSymbol,
                    codegenClassScope);
                CodegenLegoCast.AsDoubleNullReturnNull(
                    block,
                    "otherWidth",
                    forge.otherWidthEval,
                    methodNode,
                    exprSymbol,
                    codegenClassScope);
                CodegenLegoCast.AsDoubleNullReturnNull(
                    block,
                    "otherHeight",
                    forge.otherHeightEval,
                    methodNode,
                    exprSymbol,
                    codegenClassScope);
                block.MethodReturn(
                    StaticMethod(
                        typeof(BoundingBox),
                        "IntersectsBoxIncludingEnd",
                        Ref("meX"),
                        Ref("meY"),
                        Op(Ref("meX"), "+", Ref("meWidth")),
                        Op(Ref("meY"), "+", Ref("meHeight")),
                        Ref("otherX"),
                        Ref("otherY"),
                        Ref("otherWidth"),
                        Ref("otherHeight")));
                return LocalMethod(methodNode);
            }
        }
    }
} // end of namespace