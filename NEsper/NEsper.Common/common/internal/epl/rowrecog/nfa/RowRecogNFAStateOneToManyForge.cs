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

namespace com.espertech.esper.common.@internal.epl.rowrecog.nfa
{
    /// <summary>
    ///     The '+' state in the regex NFA states.
    /// </summary>
    public class RowRecogNFAStateOneToManyForge : RowRecogNFAStateForgeBase
    {
        private readonly ExprNode expression;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="nodeNum">node num</param>
        /// <param name="variableName">variable name</param>
        /// <param name="streamNum">stream number</param>
        /// <param name="multiple">true for multiple matches</param>
        /// <param name="isGreedy">true for greedy</param>
        /// <param name="exprRequiresMultimatchState">indicator for multi-match state required</param>
        /// <param name="expression">filter expression</param>
        public RowRecogNFAStateOneToManyForge(
            string nodeNum,
            string variableName,
            int streamNum,
            bool multiple,
            bool? isGreedy,
            bool exprRequiresMultimatchState,
            ExprNode expression)
            : base(nodeNum, variableName, streamNum, multiple, exprRequiresMultimatchState, isGreedy)
        {
            this.expression = expression;
            AddState(this);
        }

        internal override Type EvalClass => expression == null
            ? typeof(RowRecogNFAStateOneToManyEvalNoCond)
            : typeof(RowRecogNFAStateOneToManyEvalCond);

        public override string ToString()
        {
            if (expression == null) {
                return "OneMany-Unfiltered";
            }

            return "OneMany-Filtered";
        }

        internal override void AssignInline(
            CodegenExpression eval,
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            if (expression != null) {
                method.Block.ExprDotMethod(
                    eval, "setExpression", ExprNodeUtilityCodegen.CodegenEvaluator(expression.Forge, method, GetType(), classScope));
            }
        }
    }
} // end of namespace