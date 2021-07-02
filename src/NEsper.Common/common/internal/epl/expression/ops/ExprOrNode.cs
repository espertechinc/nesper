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
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    /// <summary>
    ///     Represents an OR expression in a filter expression tree.
    /// </summary>
    public class ExprOrNode : ExprNodeBase,
        ExprForgeInstrumentable
    {
        public override ExprForge Forge => this;

        public ExprNode ForgeRenderable => this;

        public bool IsConstantResult => false;

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.OR;

        public Type EvaluationType => typeof(bool?);

        ExprNodeRenderable ExprForge.ExprForgeRenderable => ForgeRenderable;

        public ExprEvaluator ExprEvaluator => new ExprOrNodeEval(
            this,
            ExprNodeUtilityQuery.GetEvaluatorsNoCompile(ChildNodes));

        public CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ExprOrNodeEval.Codegen(this, codegenMethodScope, exprSymbol, codegenClassScope);
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
                "ExprOr",
                requiredType,
                codegenMethodScope,
                exprSymbol,
                codegenClassScope).Build();
        }

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            // Sub-nodes must be returning boolean
            foreach (var child in ChildNodes) {
                var childType = child.Forge.EvaluationType;
                if (!childType.IsBoolean()) {
                    throw new ExprValidationException(
                        "Incorrect use of OR clause, sub-expressions do not return boolean");
                }
            }

            if (ChildNodes.Length <= 1) {
                throw new ExprValidationException("The OR operator requires at least 2 child expressions");
            }

            return null;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            var appendStr = "";
            foreach (var child in ChildNodes) {
                writer.Write(appendStr);
                child.ToEPL(writer, Precedence, flags);
                appendStr = " or ";
            }
        }

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprOrNode)) {
                return false;
            }

            return true;
        }
    }
} // end of namespace