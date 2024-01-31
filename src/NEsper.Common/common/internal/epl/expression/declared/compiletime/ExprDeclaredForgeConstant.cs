///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.metrics.instrumentation;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.declared.compiletime.ExprDeclaredForgeBase;

namespace com.espertech.esper.common.@internal.epl.expression.declared.compiletime
{
    public class ExprDeclaredForgeConstant : ExprForgeInstrumentable,
        ExprEvaluator
    {
        private readonly bool audit;
        private readonly ExprDeclaredNodeImpl parent;
        private readonly ExpressionDeclItem prototype;
        private readonly string statementName;
        private readonly object value;

        public ExprDeclaredForgeConstant(
            ExprDeclaredNodeImpl parent,
            Type returnType,
            ExpressionDeclItem prototype,
            object value,
            bool audit,
            string statementName)
        {
            this.parent = parent;
            EvaluationType = returnType;
            this.prototype = prototype;
            this.value = value;
            this.audit = audit;
            this.statementName = statementName;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return value;
        }

        public ExprEvaluator ExprEvaluator => this;

        public CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            if (!audit) {
                return Constant(value);
            }

            if (EvaluationType == null) {
                return ConstantNull();
            }

            var methodNode = codegenMethodScope.MakeChild(
                EvaluationType,
                typeof(ExprDeclaredForgeConstant),
                codegenClassScope);

            methodNode.Block
                .Expression(
                    ExprDotMethodChain(exprSymbol.GetAddExprEvalCtx(methodNode))
                        .Get("AuditProvider")
                        .Add(
                            "Exprdef",
                            Constant(parent.Prototype.Name),
                            Constant(value),
                            exprSymbol.GetAddExprEvalCtx(methodNode)))
                .MethodReturn(Constant(value));
            return LocalMethod(methodNode);
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
                    "ExprDeclared",
                    requiredType,
                    codegenMethodScope,
                    exprSymbol,
                    codegenClassScope)
                .Qparams(GetInstrumentationQParams(parent, codegenClassScope))
                .Build();
        }

        public Type EvaluationType { get; }

        public ExprNodeRenderable ExprForgeRenderable => parent;

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.COMPILETIMECONST;
    }
} // end of namespace