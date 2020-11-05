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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    /// <summary>
    /// Represents a lesser or greater then (&lt;/&lt;=/&gt;/&gt;=) expression in a filter expression tree.
    /// </summary>
    public class ExprRelationalOpNodeForge : ExprForgeInstrumentable
    {
        private readonly ExprRelationalOpNodeImpl _parent;
        private readonly RelationalOpEnumComputer _computer;
        private readonly Type _coercionType;

        public ExprRelationalOpNodeForge(ExprRelationalOpNodeImpl parent,
            RelationalOpEnumComputer computer,
            Type coercionType)
        {
            _parent = parent;
            _computer = computer;
            _coercionType = coercionType;
        }

        public ExprEvaluator ExprEvaluator {
            get => new ExprRelationalOpNodeForgeEval(
                this,
                _parent.ChildNodes[0].Forge.ExprEvaluator,
                _parent.ChildNodes[1].Forge.ExprEvaluator);
        }

        public CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ExprRelationalOpNodeForgeEval.Codegen(this, codegenMethodScope, exprSymbol, codegenClassScope);
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
                    "ExprRelOp",
                    requiredType,
                    codegenMethodScope,
                    exprSymbol,
                    codegenClassScope)
                .Qparam(Constant(_parent.RelationalOpEnum.GetExpressionText()))
                .Build();
        }

        public Type EvaluationType => typeof(bool?);

        public ExprRelationalOpNodeImpl ForgeRenderable => _parent;

        ExprNodeRenderable ExprForge.ExprForgeRenderable => ForgeRenderable;

        public RelationalOpEnumComputer Computer => _computer;

        public Type CoercionType => _coercionType;

        public ExprForgeConstantType ForgeConstantType {
            get => ExprForgeConstantType.NONCONST;
        }
    }
} // end of namespace