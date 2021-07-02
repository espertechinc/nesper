///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.declared.compiletime.ExprDeclaredForgeBase;

namespace com.espertech.esper.common.@internal.epl.expression.declared.compiletime
{
    public class ExprDeclaredForgeConstant : ExprForgeInstrumentable,
        ExprEvaluator
    {
        private readonly bool _audit;
        private readonly ExprDeclaredNodeImpl _parent;
        private readonly Type _returnType;
        private readonly ExpressionDeclItem _prototype;
        private readonly string _statementName;
        private readonly object _value;

        public ExprDeclaredForgeConstant(
            ExprDeclaredNodeImpl parent,
            Type returnType,
            ExpressionDeclItem prototype,
            object value,
            bool audit,
            string statementName)
        {
            this._parent = parent;
            this._returnType = returnType;
            this._prototype = prototype;
            this._value = value;
            this._audit = audit;
            this._statementName = statementName;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return _value;
        }

        public ExprEvaluator ExprEvaluator => this;

        public CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            if (!_audit) {
                return Constant(_value);
            }

            if (_returnType.IsNullType()) {
                return ConstantNull();
            }
            
            var methodNode = codegenMethodScope.MakeChild(
                _returnType,
                typeof(ExprDeclaredForgeConstant),
                codegenClassScope);

            methodNode.Block
                .Expression(
                    ExprDotMethodChain(exprSymbol.GetAddExprEvalCtx(methodNode))
                        .Get("AuditProvider")
                        .Add(
                            "Exprdef",
                            Constant(_parent.Prototype.Name),
                            Constant(_value),
                            exprSymbol.GetAddExprEvalCtx(methodNode)))
                .MethodReturn(Constant(_value));
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
                .Qparams(GetInstrumentationQParams(_parent, codegenClassScope))
                .Build();
        }

        public Type EvaluationType => _returnType;

        public ExprNodeRenderable ExprForgeRenderable => _parent;

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.COMPILETIMECONST;
    }
} // end of namespace