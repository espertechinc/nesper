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

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    public class ExprBetweenNodeForge : ExprForgeInstrumentable
    {
        public ExprBetweenNodeForge(
            ExprBetweenNodeImpl parent,
            ExprBetweenNodeImpl.ExprBetweenComp computer,
            bool isAlwaysFalse)
        {
            ForgeRenderable = parent;
            Computer = computer;
            IsAlwaysFalse = isAlwaysFalse;
        }

        public ExprBetweenNodeImpl ForgeRenderable { get; }

        public ExprBetweenNodeImpl.ExprBetweenComp Computer { get; }

        public bool IsAlwaysFalse { get; }

        public ExprEvaluator ExprEvaluator {
            get {
                if (IsAlwaysFalse) {
                    return new ProxyExprEvaluator(
                        (
                            eventsPerStream,
                            isNewData,
                            context) => false);
                }

                var nodes = ForgeRenderable.ChildNodes;
                return new ExprBetweenNodeForgeEval(
                    this, nodes[0].Forge.ExprEvaluator, nodes[1].Forge.ExprEvaluator, nodes[2].Forge.ExprEvaluator);
            }
        }

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            if (IsAlwaysFalse) {
                return ConstantFalse();
            }

            return ExprBetweenNodeForgeEval.Codegen(this, codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return new InstrumentationBuilderExpr(
                    GetType(), this, "ExprBetween", requiredType, codegenMethodScope, exprSymbol, codegenClassScope)
                .Build();
        }

        public Type EvaluationType => typeof(bool?);

        ExprNodeRenderable ExprForge.ForgeRenderable => ForgeRenderable;
    }
} // end of namespace