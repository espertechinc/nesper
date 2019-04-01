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
    public class SettingsApplicationDotMethodPointInsideRectange : SettingsApplicationDotMethodBase
    {
        public const string LOOKUP_OPERATION_NAME = "point.inside(rectangle)";
        public const string INDEXTYPE_NAME = "pointregionquadtree";

        public SettingsApplicationDotMethodPointInsideRectange(ExprDotNodeImpl parent, string lhsName, ExprNode[] lhs, string dotMethodName, string rhsName, ExprNode[] rhs, ExprNode[] indexNamedParameter)

             : base(parent, lhsName, lhs, dotMethodName, rhsName, rhs, indexNamedParameter)

        {
        }

        protected override ExprForge ValidateAll(string lhsName, ExprNode[] lhs, string rhsName, ExprNode[] rhs, ExprValidationContext validationContext)
        {
            EPLValidationUtil.ValidateParameterNumber(lhsName, LHS_VALIDATION_NAME, false, 2, lhs.Length);
            EPLValidationUtil.ValidateParametersTypePredefined(lhs, lhsName, LHS_VALIDATION_NAME, EPLExpressionParamType.NUMERIC);

            EPLValidationUtil.ValidateParameterNumber(rhsName, RHS_VALIDATION_NAME, true, 4, rhs.Length);
            EPLValidationUtil.ValidateParametersTypePredefined(rhs, rhsName, RHS_VALIDATION_NAME, EPLExpressionParamType.NUMERIC);

            ExprForge pxEval = lhs[0].Forge;
            ExprForge pyEval = lhs[1].Forge;
            ExprForge xEval = rhs[0].Forge;
            ExprForge yEval = rhs[1].Forge;
            ExprForge widthEval = rhs[2].Forge;
            ExprForge heightEval = rhs[3].Forge;
            return new PointIntersectsRectangleForge(parent, pxEval, pyEval, xEval, yEval, widthEval, heightEval);
        }

        protected override string OperationName
        {
            get { return LOOKUP_OPERATION_NAME; }
        }

        protected override string IndexTypeName
        {
            get { return INDEXTYPE_NAME; }
        }

        public sealed class PointIntersectsRectangleForge : ExprForge
        {
            private readonly ExprDotNodeImpl parent;
            internal readonly ExprForge pxEval;
            internal readonly ExprForge pyEval;
            internal readonly ExprForge xEval;
            internal readonly ExprForge yEval;
            internal readonly ExprForge widthEval;
            internal readonly ExprForge heightEval;

            public PointIntersectsRectangleForge(ExprDotNodeImpl parent, ExprForge pxEval, ExprForge pyEval, ExprForge xEval, ExprForge yEval, ExprForge widthEval, ExprForge heightEval)
            {
                this.parent = parent;
                this.pxEval = pxEval;
                this.pyEval = pyEval;
                this.xEval = xEval;
                this.yEval = yEval;
                this.widthEval = widthEval;
                this.heightEval = heightEval;
            }

            public ExprEvaluator ExprEvaluator
            {
                get => new PointIntersectsRectangleEvaluator(pxEval.ExprEvaluator, pyEval.ExprEvaluator, xEval.ExprEvaluator, yEval.ExprEvaluator, widthEval.ExprEvaluator, heightEval.ExprEvaluator);
            }

            public CodegenExpression EvaluateCodegen(Type requiredType, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol, CodegenClassScope codegenClassScope)
            {
                return PointIntersectsRectangleEvaluator.Codegen(this, codegenMethodScope, exprSymbol, codegenClassScope);
            }

            public ExprForgeConstantType ForgeConstantType
            {
                get => ExprForgeConstantType.NONCONST;
            }

            public Type EvaluationType
            {
                get => typeof(bool?);
            }

            public ExprNodeRenderable ForgeRenderable
            {
                get => parent;
            }
        }

        public sealed class PointIntersectsRectangleEvaluator : ExprEvaluator
        {
            private readonly ExprEvaluator pxEval;
            private readonly ExprEvaluator pyEval;
            private readonly ExprEvaluator xEval;
            private readonly ExprEvaluator yEval;
            private readonly ExprEvaluator widthEval;
            private readonly ExprEvaluator heightEval;

            private PointIntersectsRectangleEvaluator(ExprEvaluator pxEval, ExprEvaluator pyEval, ExprEvaluator xEval, ExprEvaluator yEval, ExprEvaluator widthEval, ExprEvaluator heightEval)
            {
                this.pxEval = pxEval;
                this.pyEval = pyEval;
                this.xEval = xEval;
                this.yEval = yEval;
                this.widthEval = widthEval;
                this.heightEval = heightEval;
            }

            public object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
            {
                var px = pxEval.Evaluate(eventsPerStream, isNewData, context);
                if (px == null)
                {
                    return null;
                }
                var py = pyEval.Evaluate(eventsPerStream, isNewData, context);
                if (py == null)
                {
                    return null;
                }
                var x = xEval.Evaluate(eventsPerStream, isNewData, context);
                if (x == null)
                {
                    return null;
                }
                var y = yEval.Evaluate(eventsPerStream, isNewData, context);
                if (y == null)
                {
                    return null;
                }
                var width = widthEval.Evaluate(eventsPerStream, isNewData, context);
                if (width == null)
                {
                    return null;
                }
                var height = heightEval.Evaluate(eventsPerStream, isNewData, context);
                if (height == null)
                {
                    return null;
                }
                return BoundingBox.ContainsPoint(x.AsDouble(), y.AsDouble(), width.AsDouble(), height.AsDouble(), px.AsDouble(), py.AsDouble());
            }

            public static CodegenExpression Codegen(
                PointIntersectsRectangleForge forge,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                CodegenMethod methodNode = codegenMethodScope.MakeChild(typeof(bool?), typeof(SettingsApplicationDotMethodRectangeIntersectsRectangle.RectangleIntersectsRectangleEvaluator), codegenClassScope);

                CodegenBlock block = methodNode.Block;
                CodegenLegoCast.AsDoubleNullReturnNull(block, "px", forge.pxEval, methodNode, exprSymbol, codegenClassScope);
                CodegenLegoCast.AsDoubleNullReturnNull(block, "py", forge.pyEval, methodNode, exprSymbol, codegenClassScope);
                CodegenLegoCast.AsDoubleNullReturnNull(block, "x", forge.xEval, methodNode, exprSymbol, codegenClassScope);
                CodegenLegoCast.AsDoubleNullReturnNull(block, "y", forge.yEval, methodNode, exprSymbol, codegenClassScope);
                CodegenLegoCast.AsDoubleNullReturnNull(block, "width", forge.widthEval, methodNode, exprSymbol, codegenClassScope);
                CodegenLegoCast.AsDoubleNullReturnNull(block, "height", forge.heightEval, methodNode, exprSymbol, codegenClassScope);
                block.MethodReturn(StaticMethod(typeof(BoundingBox), "containsPoint", @Ref("x"), @Ref("y"), @Ref("width"), @Ref("height"), @Ref("px"), @Ref("py")));
                return LocalMethod(methodNode);
            }
        }
    }
} // end of namespace