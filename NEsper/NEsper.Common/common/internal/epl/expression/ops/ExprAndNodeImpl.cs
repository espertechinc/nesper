///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    /// <summary>
    /// Represents an And-condition.
    /// </summary>
    public class ExprAndNodeImpl : ExprNodeBase,
        ExprForgeInstrumentable,
        ExprAndNode
    {
        public ExprAndNodeImpl()
        {
        }

        public Type EvaluationType {
            get => typeof(bool?);
        }

        public ExprEvaluator ExprEvaluator {
            get => new ExprAndNodeEval(this, ExprNodeUtilityQuery.GetEvaluatorsNoCompile(ChildNodes));
        }

        public CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ExprAndNodeEval.Codegen(this, codegenMethodScope, exprSymbol, codegenClassScope);
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
                    "ExprAnd",
                    requiredType,
                    codegenMethodScope,
                    exprSymbol,
                    codegenClassScope)
                .Build();
        }

        public ExprForgeConstantType ForgeConstantType {
            get => ExprForgeConstantType.NONCONST;
        }

        public override ExprForge Forge {
            get => this;
        }

        public ExprNodeRenderable ExprForgeRenderable {
            get => this;
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            // Sub-nodes must be returning boolean
            foreach (ExprNode child in ChildNodes) {
                Type childType = child.Forge.EvaluationType;
                if (!TypeHelper.IsBoolean(childType)) {
                    throw new ExprValidationException(
                        "Incorrect use of AND clause, sub-expressions do not return boolean");
                }
            }

            if (ChildNodes.Length <= 1) {
                throw new ExprValidationException("The AND operator requires at least 2 child expressions");
            }

            return null;
        }

        public bool IsConstantResult {
            get => false;
        }

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            string appendStr = "";
            foreach (ExprNode child in ChildNodes) {
                writer.Write(appendStr);
                child.ToEPL(writer, Precedence, flags);
                appendStr = " and ";
            }
        }

        public override ExprPrecedenceEnum Precedence {
            get => ExprPrecedenceEnum.AND;
        }

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprAndNodeImpl)) {
                return false;
            }

            return true;
        }
    }
} // end of namespace