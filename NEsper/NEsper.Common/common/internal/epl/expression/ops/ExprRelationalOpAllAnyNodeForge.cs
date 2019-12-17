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
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    public class ExprRelationalOpAllAnyNodeForge : ExprForgeInstrumentable
    {
        private readonly ExprRelationalOpAllAnyNode parent;
        private readonly RelationalOpEnumComputer computer;
        private readonly bool hasCollectionOrArray;

        public ExprRelationalOpAllAnyNodeForge(
            ExprRelationalOpAllAnyNode parent,
            RelationalOpEnumComputer computer,
            bool hasCollectionOrArray)
        {
            this.parent = parent;
            this.computer = computer;
            this.hasCollectionOrArray = hasCollectionOrArray;
        }

        public ExprEvaluator ExprEvaluator {
            get => new ExprRelationalOpAllAnyNodeForgeEval(
                this,
                ExprNodeUtilityQuery.GetEvaluatorsNoCompile(parent.ChildNodes));
        }

        public ExprForgeConstantType ForgeConstantType {
            get => ExprForgeConstantType.NONCONST;
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return new InstrumentationBuilderExpr(
                    GetType(),
                    this,
                    "ExprRelOpAnyOrAll",
                    requiredType,
                    codegenMethodScope,
                    exprSymbol,
                    codegenClassScope)
                .Qparam(Constant(parent.RelationalOpEnum.GetExpressionText()))
                .Build();
        }

        public CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ExprRelationalOpAllAnyNodeForgeEval.Codegen(this, codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public Type EvaluationType {
            get => typeof(bool?);
        }

        public ExprRelationalOpAllAnyNode ForgeRenderable {
            get => parent;
        }

        ExprNodeRenderable ExprForge.ExprForgeRenderable => ForgeRenderable;

        public RelationalOpEnumComputer Computer {
            get { return computer; }
        }

        public bool IsCollectionOrArray {
            get => hasCollectionOrArray;
        }
    }
} // end of namespace