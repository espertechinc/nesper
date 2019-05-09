///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.common.@internal.util;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    public class ExprArrayNodeForge : ExprForgeInstrumentable,
        ExprEnumerationForge
    {
        private readonly Array constantResult;

        public ExprArrayNodeForge(
            ExprArrayNode parent,
            Type arrayReturnType,
            object[] constantResult)
        {
            Parent = parent;
            ArrayReturnType = arrayReturnType;
            this.constantResult = constantResult;
            IsMustCoerce = false;
            Coercer = null;
        }

        public ExprArrayNodeForge(
            ExprArrayNode parent,
            Type arrayReturnType,
            bool mustCoerce,
            SimpleNumberCoercer coercer,
            object constantResult)
        {
            Parent = parent;
            ArrayReturnType = arrayReturnType;
            IsMustCoerce = mustCoerce;
            Coercer = coercer;
            this.constantResult = (Array) constantResult;
        }

        public ExprArrayNode ForgeRenderableArray => Parent;

        public ExprNodeRenderable ExprForgeRenderable => Parent;

        public ExprNodeRenderable EnumForgeRenderable => Parent;

        public ExprArrayNode Parent { get; }

        public Type ArrayReturnType { get; }

        public bool IsMustCoerce { get; }

        public SimpleNumberCoercer Coercer { get; }

        public object ConstantResult => constantResult;

        public CodegenExpression EvaluateGetROCollectionScalarCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ExprArrayNodeForgeEval.CodegenEvaluateGetROCollectionScalar(
                this, codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public EventType GetEventTypeCollection(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            return null;
        }

        public Type ComponentTypeCollection {
            get => Parent.ComponentTypeCollection;
        }

        public EventType GetEventTypeSingle(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            return null;
        }

        public CodegenExpression EvaluateGetROCollectionEventsCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        public CodegenExpression EvaluateGetEventBeanCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        public ExprForgeConstantType ForgeConstantType {
            get {
                if (constantResult != null) {
                    return ExprForgeConstantType.COMPILETIMECONST;
                }

                return ExprForgeConstantType.NONCONST;
            }
        }

        public ExprEvaluator ExprEvaluator {
            get {
                if (constantResult != null) {
                    return new ProxyExprEvaluator(
                        (
                            eventsPerStream,
                            isNewData,
                            context) => constantResult);
                }

                return new ExprArrayNodeForgeEval(
                    this, ExprNodeUtilityQuery.GetEvaluatorsNoCompile(Parent.ChildNodes));
            }
        }

        public CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            if (constantResult != null) {
                return Constant(constantResult);
            }

            return ExprArrayNodeForgeEval.Codegen(this, codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return new InstrumentationBuilderExpr(
                GetType(), this, "ExprArray", requiredType, codegenMethodScope, exprSymbol, codegenClassScope).Build();
        }

        public Type EvaluationType => Array.CreateInstance(ArrayReturnType, 0).GetType();

        public ExprEnumerationEval ExprEvaluatorEnumeration {
            get {
                if (constantResult != null) {
                    var constantResultList = new List<object>();
                    for (var i = 0; i < Parent.ChildNodes.Length; i++) {
                        constantResultList.Add(constantResult.GetValue(i));
                    }

                    return new ProxyExprEnumerationEval {
                        ProcEvaluateGetROCollectionEvents = (
                            _,
                            __,
                            ___) => null,
                        ProcEvaluateGetROCollectionScalar = (
                            _,
                            __,
                            ___) => constantResultList,
                        ProcEvaluateGetEventBean = (
                            _,
                            __,
                            ___) => null
                    };
                }

                return new ExprArrayNodeForgeEval(
                    this, ExprNodeUtilityQuery.GetEvaluatorsNoCompile(Parent.ChildNodes));
            }
        }
    }
} // end of namespace