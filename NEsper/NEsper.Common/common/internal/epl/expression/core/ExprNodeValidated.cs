///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.visitor;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    /// <summary>
    ///     A placeholder for another expression node that has been validated already.
    /// </summary>
    public class ExprNodeValidated : ExprNodeBase,
        ExprForge,
        ExprEvaluator
    {
        private readonly ExprNode inner;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="inner">nested expression node</param>
        public ExprNodeValidated(ExprNode inner)
        {
            this.inner = inner;
        }

        public override ExprForge Forge => this;

        public override ExprPrecedenceEnum Precedence => inner.Precedence;

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return inner.Forge.ExprEvaluator.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
        }

        public ExprEvaluator ExprEvaluator => this;

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return inner.Forge.EvaluateCodegen(requiredType, codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public Type EvaluationType => inner.Forge.EvaluationType;

        public ExprNodeRenderable ExprForgeRenderable => this;

        public ExprForgeConstantType ForgeConstantType => inner.Forge.ForgeConstantType;

        public override void ToEPL(
            TextWriter writer,
            ExprPrecedenceEnum parentPrecedence,
            ExprNodeRenderableFlags flags)
        {
            inner.ToEPL(writer, parentPrecedence, flags);
        }

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            inner.ToEPL(writer, ExprPrecedenceEnum.MINIMUM, flags);
        }

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (node is ExprNodeValidated nodeValidated) {
                return inner.EqualsNode(nodeValidated.inner, false);
            }

            return inner.EqualsNode(node, false);
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            return null;
        }

        public override void Accept(ExprNodeVisitor visitor)
        {
            if (visitor.IsVisit(this)) {
                visitor.Visit(this);
                inner.Accept(visitor);
            }
        }
    }
} // end of namespace