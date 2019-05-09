///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.enummethod.dot
{
    /// <summary>
    ///     Represents the case-when-then-else control flow function is an expression tree.
    /// </summary>
    public class ExprLambdaGoesNode : ExprNodeBase,
        ExprForge,
        ExprEvaluator,
        ExprDeclaredOrLambdaNode
    {
        public ExprLambdaGoesNode(IList<string> goesToNames)
        {
            GoesToNames = goesToNames;
        }

        public IList<string> GoesToNames { get; }

        public bool IsConstantResult => false;

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.MINIMUM;

        public override ExprForge Forge => this;

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            throw new UnsupportedOperationException();
        }

        public ExprEvaluator ExprEvaluator => this;

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            throw new UnsupportedOperationException();
        }

        public Type EvaluationType => null;

        public ExprNodeRenderable ExprForgeRenderable => this;

        public bool IsValidated => true;

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            throw new UnsupportedOperationException();
        }

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            return false;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
        }
    }
} // end of namespace