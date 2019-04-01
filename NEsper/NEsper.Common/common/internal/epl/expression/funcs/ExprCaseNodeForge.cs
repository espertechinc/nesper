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
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    public class ExprCaseNodeForge : ExprTypableReturnForge,
        ExprForgeInstrumentable
    {
        internal readonly IDictionary<string, object> mapResultType;

        internal ExprCaseNodeForge(
            ExprCaseNode parent, 
            Type resultType, 
            IDictionary<string, object> mapResultType, 
            bool isNumericResult,
            bool mustCoerce, 
            SimpleNumberCoercer coercer, 
            IList<UniformPair<ExprNode>> whenThenNodeList,
            ExprNode optionalCompareExprNode, 
            ExprNode optionalElseExprNode)
        {
            ForgeRenderable = parent;
            EvaluationType = resultType;
            this.mapResultType = mapResultType;
            IsNumericResult = isNumericResult;
            IsMustCoerce = mustCoerce;
            Coercer = coercer;
            WhenThenNodeList = whenThenNodeList;
            OptionalCompareExprNode = optionalCompareExprNode;
            OptionalElseExprNode = optionalElseExprNode;
        }

        public IList<UniformPair<ExprNode>> WhenThenNodeList { get; }

        public ExprNode OptionalCompareExprNode { get; }

        public ExprNode OptionalElseExprNode { get; }

        public ExprCaseNode ForgeRenderable { get; }

        public bool IsNumericResult { get; }

        public bool IsMustCoerce { get; }

        public SimpleNumberCoercer Coercer { get; }

        public ExprTypableReturnEval TypableReturnEvaluator => new ExprCaseNodeForgeEvalTypable(this);

        public CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            if (!ForgeRenderable.IsCase2) {
                return ExprCaseNodeForgeEvalSyntax1.Codegen(this, codegenMethodScope, exprSymbol, codegenClassScope);
            }

            return ExprCaseNodeForgeEvalSyntax2.Codegen(this, codegenMethodScope, exprSymbol, codegenClassScope);
        }

        ExprNodeRenderable ExprForge.ForgeRenderable => ForgeRenderable;

        public Type EvaluationType { get; }

        public ExprEvaluator ExprEvaluator {
            get {
                IList<UniformPair<ExprEvaluator>> evals = new List<UniformPair<ExprEvaluator>>();
                foreach (var pair in WhenThenNodeList) {
                    evals.Add(new UniformPair<ExprEvaluator>(pair.First.Forge.ExprEvaluator, pair.Second.Forge.ExprEvaluator));
                }

                if (!ForgeRenderable.IsCase2) {
                    return new ExprCaseNodeForgeEvalSyntax1(
                        this, evals, OptionalElseExprNode == null ? null : OptionalElseExprNode.Forge.ExprEvaluator);
                }

                return new ExprCaseNodeForgeEvalSyntax2(
                    this, evals, OptionalCompareExprNode.Forge.ExprEvaluator,
                    OptionalElseExprNode == null ? null : OptionalElseExprNode.Forge.ExprEvaluator);
            }
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return new InstrumentationBuilderExpr(
                    GetType(), this, "ExprCase", requiredType, codegenMethodScope, exprSymbol, codegenClassScope)
                .Build();
        }

        public CodegenExpression EvaluateTypableSingleCodegen(
            CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ExprCaseNodeForgeEvalTypable.CodegenTypeableSingle(
                this, codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public CodegenExpression EvaluateTypableMultiCodegen(
            CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public bool? IsMultirow => mapResultType == null ? (bool?) null : false;

        public IDictionary<string, object> RowProperties => mapResultType;
    }
} // end of namespace