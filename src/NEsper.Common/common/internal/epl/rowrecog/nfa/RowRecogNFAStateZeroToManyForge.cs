///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
    ///     The '*' state in the regex NFA states.
    /// </summary>
    public class RowRecogNFAStateZeroToManyForge : RowRecogNFAStateForgeBase
    {
        private readonly ExprNode expression;

        public RowRecogNFAStateZeroToManyForge(
            string nodeNum,
            string variableName,
            int streamNum,
            bool multiple,
            bool? isGreedy,
            bool exprRequiresMultimatchState,
            ExprNode expression)
            : base(nodeNum, variableName, streamNum, multiple, isGreedy, exprRequiresMultimatchState)
        {
            this.expression = expression;
            AddState(this);
        }

        internal override Type EvalClass => expression == null
            ? typeof(RowRecogNFAStateZeroToManyEvalNoCond)
            : typeof(RowRecogNFAStateZeroToManyEvalCond);

        public override string ToString()
        {
            if (expression == null) {
                return "ZeroMany-Unfiltered";
            }

            return "ZeroMany-Filtered";
        }

        internal override void AssignInline(
            CodegenExpression eval,
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            if (expression != null) {
                method.Block.SetProperty(
                    eval,
                    "Expression",
                    ExprNodeUtilityCodegen.CodegenEvaluator(expression.Forge, method, GetType(), classScope));
            }
        }
    }
} // end of namespace