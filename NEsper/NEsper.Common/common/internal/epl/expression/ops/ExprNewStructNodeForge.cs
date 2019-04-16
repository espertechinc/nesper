///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    public class ExprNewStructNodeForge : ExprTypableReturnForge,
        ExprForgeInstrumentable
    {
        public ExprNewStructNodeForge(
            ExprNewStructNode parent,
            bool isAllConstants,
            LinkedHashMap<string, object> eventType)
        {
            ForgeRenderable = parent;
            IsAllConstants = isAllConstants;
            EventType = eventType;
        }

        public ExprTypableReturnEval TypableReturnEvaluator => (ExprTypableReturnEval) ExprEvaluator;

        public bool IsAllConstants { get; }

        public LinkedHashMap<string, object> EventType { get; }

        ExprNodeRenderable ExprForge.ForgeRenderable => ForgeRenderable;

        public ExprNewStructNode ForgeRenderable { get; }

        public IDictionary<string, object> RowProperties => EventType;

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return new InstrumentationBuilderExpr(
                GetType(), this, "ExprNew", requiredType, codegenMethodScope, exprSymbol, codegenClassScope).Build();
        }

        public CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ExprNewStructNodeForgeEval.Codegen(this, codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public Type EvaluationType => typeof(IDictionary<string, object>);

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public ExprEvaluator ExprEvaluator {
            get {
                var evaluators = ExprNodeUtilityQuery.GetEvaluatorsNoCompile(ForgeRenderable.ChildNodes);
                return new ExprNewStructNodeForgeEval(this, evaluators);
            }
        }

        public bool? IsMultirow {
            get {
                return false; // New itself can only return a single row
            }
        }

        public CodegenExpression EvaluateTypableSingleCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ExprNewStructNodeForgeEval.CodegenTypeableSingle(
                this, codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public CodegenExpression EvaluateTypableMultiCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }
    }
} // end of namespace