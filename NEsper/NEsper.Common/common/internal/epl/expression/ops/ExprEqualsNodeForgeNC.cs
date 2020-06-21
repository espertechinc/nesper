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
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    public class ExprEqualsNodeForgeNC : ExprEqualsNodeForge
    {
        public ExprEqualsNodeForgeNC(ExprEqualsNodeImpl parent)
            : base(parent)
        {
        }

        public override ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public override ExprEvaluator ExprEvaluator {
            get {
                var lhs = ForgeRenderable.ChildNodes[0].Forge;
                var rhs = ForgeRenderable.ChildNodes[1].Forge;
                var lhsType = lhs.EvaluationType;

                if (!ForgeRenderable.IsIs) {
                    if (lhsType != null && lhsType.IsArray) {
                        var componentType = lhsType.GetElementType();
                        if (componentType == typeof(bool)) {
                            return new ExprEqualsNodeForgeNCEvalEqualsArrayBoolean(ForgeRenderable, lhs.ExprEvaluator, rhs.ExprEvaluator);
                        }
                        else if (componentType == typeof(byte)) {
                            return new ExprEqualsNodeForgeNCEvalEqualsArrayByte(ForgeRenderable, lhs.ExprEvaluator, rhs.ExprEvaluator);
                        }
                        else if (componentType == typeof(char)) {
                            return new ExprEqualsNodeForgeNCEvalEqualsArrayChar(ForgeRenderable, lhs.ExprEvaluator, rhs.ExprEvaluator);
                        }
                        else if (componentType == typeof(long)) {
                            return new ExprEqualsNodeForgeNCEvalEqualsArrayLong(ForgeRenderable, lhs.ExprEvaluator, rhs.ExprEvaluator);
                        }
                        else if (componentType == typeof(double)) {
                            return new ExprEqualsNodeForgeNCEvalEqualsArrayDouble(ForgeRenderable, lhs.ExprEvaluator, rhs.ExprEvaluator);
                        }
                        else if (componentType == typeof(float)) {
                            return new ExprEqualsNodeForgeNCEvalEqualsArrayFloat(ForgeRenderable, lhs.ExprEvaluator, rhs.ExprEvaluator);
                        }
                        else if (componentType == typeof(short)) {
                            return new ExprEqualsNodeForgeNCEvalEqualsArrayShort(ForgeRenderable, lhs.ExprEvaluator, rhs.ExprEvaluator);
                        }
                        else if (componentType == typeof(int)) {
                            return new ExprEqualsNodeForgeNCEvalEqualsArrayInt(ForgeRenderable, lhs.ExprEvaluator, rhs.ExprEvaluator);
                        }

                        return new ExprEqualsNodeForgeNCEvalEqualsArrayObject(ForgeRenderable, lhs.ExprEvaluator, rhs.ExprEvaluator);
                    }

                    return new ExprEqualsNodeForgeNCEvalEqualsNonArray(ForgeRenderable, lhs.ExprEvaluator, rhs.ExprEvaluator);
                }

                if (lhsType != null && lhsType.IsArray) {
                    var componentType = lhsType.GetElementType();
                    if (componentType == typeof(bool)) {
                        return new ExprEqualsNodeForgeNCEvalIsArrayBoolean(ForgeRenderable, lhs.ExprEvaluator, rhs.ExprEvaluator);
                    }
                    else if (componentType == typeof(byte)) {
                        return new ExprEqualsNodeForgeNCEvalIsArrayByte(ForgeRenderable, lhs.ExprEvaluator, rhs.ExprEvaluator);
                    }
                    else if (componentType == typeof(char)) {
                        return new ExprEqualsNodeForgeNCEvalIsArrayChar(ForgeRenderable, lhs.ExprEvaluator, rhs.ExprEvaluator);
                    }
                    else if (componentType == typeof(long)) {
                        return new ExprEqualsNodeForgeNCEvalIsArrayLong(ForgeRenderable, lhs.ExprEvaluator, rhs.ExprEvaluator);
                    }
                    else if (componentType == typeof(double)) {
                        return new ExprEqualsNodeForgeNCEvalIsArrayDouble(ForgeRenderable, lhs.ExprEvaluator, rhs.ExprEvaluator);
                    }
                    else if (componentType == typeof(float)) {
                        return new ExprEqualsNodeForgeNCEvalIsArrayFloat(ForgeRenderable, lhs.ExprEvaluator, rhs.ExprEvaluator);
                    }
                    else if (componentType == typeof(short)) {
                        return new ExprEqualsNodeForgeNCEvalIsArrayShort(ForgeRenderable, lhs.ExprEvaluator, rhs.ExprEvaluator);
                    }
                    else if (componentType == typeof(int)) {
                        return new ExprEqualsNodeForgeNCEvalIsArrayInt(ForgeRenderable, lhs.ExprEvaluator, rhs.ExprEvaluator);
                    }

                    return new ExprEqualsNodeForgeNCEvalIsArrayObject(ForgeRenderable, lhs.ExprEvaluator, rhs.ExprEvaluator);
                }

                return new ExprEqualsNodeForgeNCEvalIsNonArray(ForgeRenderable, lhs.ExprEvaluator, rhs.ExprEvaluator);
            }
        }

        public override CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return new InstrumentationBuilderExpr(
                GetType(),
                this,
                ForgeRenderable.IsIs ? "ExprIs" : "ExprEquals",
                requiredType,
                codegenMethodScope,
                exprSymbol,
                codegenClassScope).Build();
        }

        public override CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var lhs = ForgeRenderable.ChildNodes[0].Forge;
            var rhs = ForgeRenderable.ChildNodes[1].Forge;
            if (!ForgeRenderable.IsIs) {
                if (lhs.EvaluationType == null || rhs.EvaluationType == null) {
                    return ConstantNull();
                }

                return LocalMethod(
                    ExprEqualsNodeForgeNCForgeEquals.Codegen(
                        this,
                        codegenMethodScope,
                        exprSymbol,
                        codegenClassScope,
                        lhs,
                        rhs));
            }

            return LocalMethod(
                ExprEqualsNodeForgeNCForgeIs.Codegen(
                    this, 
                    codegenMethodScope,
                    exprSymbol,
                    codegenClassScope,
                    lhs,
                    rhs));
        }
    }
} // end of namespace