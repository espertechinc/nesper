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
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    /// <summary>
    ///     Represents the CAST(expression, type) function is an expression tree.
    /// </summary>
    public class ExprCastNodeForge : ExprForgeInstrumentable
    {
        internal ExprCastNodeForge(
            ExprCastNode parent,
            ExprCastNode.CasterParserComputerForge casterParserComputerForge,
            Type targetType,
            bool isConstant,
            object constant)
        {
            ForgeRenderableCast = parent;
            CasterParserComputerForge = casterParserComputerForge;
            EvaluationType = targetType;
            IsConstant = isConstant;
            Constant = constant;
        }

        public ExprNodeRenderable ExprForgeRenderable => ForgeRenderableCast;

        public ExprCastNode ForgeRenderableCast { get; }

        public object Constant { get; }

        public bool IsConstant { get; }

        public ExprCastNode.CasterParserComputerForge CasterParserComputerForge { get; }

        public ExprForgeConstantType ForgeConstantType {
            get {
                if (IsConstant) {
                    return ExprForgeConstantType.DEPLOYCONST;
                }

                return ExprForgeConstantType.NONCONST;
            }
        }

        public ExprEvaluator ExprEvaluator {
            get {
                if (IsConstant) {
                    return new ExprCastNodeForgeConstEval(this, Constant);
                }

                return new ExprCastNodeForgeNonConstEval(
                    this, 
                    ForgeRenderableCast.ChildNodes[0].Forge.ExprEvaluator,
                    CasterParserComputerForge.EvaluatorComputer);
            }
        }

        public Type EvaluationType { get; }

        public CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            if (IsConstant) {
                if (Constant == null) {
                    return ConstantNull();
                }

                return ExprCastNodeForgeConstEval.Codegen(this, codegenClassScope);
            }

            return ExprCastNodeForgeNonConstEval.Codegen(this, codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return new InstrumentationBuilderExpr(
                GetType(), this, "ExprCast", requiredType, codegenMethodScope, exprSymbol, codegenClassScope).Build();
        }
    }
} // end of namespace