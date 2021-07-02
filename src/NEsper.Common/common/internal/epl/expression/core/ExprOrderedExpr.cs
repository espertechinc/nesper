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

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    /// <summary>
    ///     A placeholder expression for view/pattern object parameters that allow
    ///     sorting expression values ascending or descending.
    /// </summary>
    [Serializable]
    public class ExprOrderedExpr : ExprNodeBase,
        ExprForge,
        ExprEvaluator
    {
        [NonSerialized] private ExprEvaluator _evaluator;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="descending">is true for descending sorts</param>
        public ExprOrderedExpr(bool descending)
        {
            IsDescending = descending;
        }

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        public override ExprForge Forge => this;

        /// <summary>
        ///     Returns true for descending sort.
        /// </summary>
        /// <returns>indicator for ascending or descending sort</returns>
        public bool IsDescending { get; }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return _evaluator.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
        }

        public ExprEvaluator ExprEvaluator => this;

        public Type EvaluationType => ChildNodes[0].Forge.EvaluationType;

        public ExprNodeRenderable ExprForgeRenderable => this;

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ChildNodes[0].Forge.EvaluateCodegen(requiredType, codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            ChildNodes[0].ToEPL(writer, ExprPrecedenceEnum.MINIMUM, flags);
            if (IsDescending) {
                writer.Write(" desc");
            }
        }

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprOrderedExpr)) {
                return false;
            }

            var other = (ExprOrderedExpr) node;
            return other.IsDescending == IsDescending;
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            _evaluator = ChildNodes[0].Forge.ExprEvaluator;
            // always valid
            return null;
        }
    }
} // end of namespace