///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.core;

namespace com.espertech.esper.common.@internal.epl.index.advanced.index.quadtree
{
    public partial class SettingsApplicationDotMethodPointInsideRectangle
    {
        public sealed class PointIntersectsRectangleForge : ExprForge
        {
            private readonly ExprDotNodeImpl parent;
            internal readonly ExprForge pxEval;
            internal readonly ExprForge pyEval;
            internal readonly ExprForge xEval;
            internal readonly ExprForge yEval;
            internal readonly ExprForge widthEval;
            internal readonly ExprForge heightEval;

            public PointIntersectsRectangleForge(
                ExprDotNodeImpl parent,
                ExprForge pxEval,
                ExprForge pyEval,
                ExprForge xEval,
                ExprForge yEval,
                ExprForge widthEval,
                ExprForge heightEval)
            {
                this.parent = parent;
                this.pxEval = pxEval;
                this.pyEval = pyEval;
                this.xEval = xEval;
                this.yEval = yEval;
                this.widthEval = widthEval;
                this.heightEval = heightEval;
            }

            public ExprEvaluator ExprEvaluator {
                get => new PointIntersectsRectangleEvaluator(
                    pxEval.ExprEvaluator, pyEval.ExprEvaluator, xEval.ExprEvaluator, yEval.ExprEvaluator, widthEval.ExprEvaluator,
                    heightEval.ExprEvaluator);
            }

            public CodegenExpression EvaluateCodegen(
                Type requiredType,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return PointIntersectsRectangleEvaluator.Codegen(this, codegenMethodScope, exprSymbol, codegenClassScope);
            }

            public ExprForgeConstantType ForgeConstantType {
                get => ExprForgeConstantType.NONCONST;
            }

            public Type EvaluationType {
                get => typeof(bool?);
            }

            public ExprNodeRenderable ForgeRenderable {
                get => parent;
            }
        }
    }
}