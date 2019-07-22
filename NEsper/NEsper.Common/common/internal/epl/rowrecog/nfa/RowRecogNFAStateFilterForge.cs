///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.rowrecog.nfa
{
    /// <summary>
    /// NFA state for a single match that applies a filter.
    /// </summary>
    public class RowRecogNFAStateFilterForge : RowRecogNFAStateForgeBase
    {
        private readonly ExprNode expression;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="nodeNum">node num</param>
        /// <param name="variableName">variable name</param>
        /// <param name="streamNum">stream number</param>
        /// <param name="multiple">true for multiple matches</param>
        /// <param name="expression">filter expression</param>
        /// <param name="exprRequiresMultimatchState">indicator for multi-match state required</param>
        public RowRecogNFAStateFilterForge(
            string nodeNum,
            string variableName,
            int streamNum,
            bool multiple,
            bool exprRequiresMultimatchState,
            ExprNode expression)
            : base(nodeNum, variableName, streamNum, multiple, null, exprRequiresMultimatchState)
        {
            this.expression = expression;
        }

        public override string ToString()
        {
            return "FilterEvent";
        }

        internal override Type EvalClass {
            get { return typeof(RowRecogNFAStateFilterEval); }
        }

        internal override void AssignInline(
            CodegenExpression eval,
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            method.Block.SetProperty(
                eval,
                "Expression",
                ExprNodeUtilityCodegen.CodegenEvaluator(expression.Forge, method, this.GetType(), classScope));
            if (classScope.IsInstrumented) {
                method.Block.SetProperty(
                    eval,
                    "ExpressionTextForAudit",
                    Constant(ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(expression)));
            }
        }
    }
} // end of namespace