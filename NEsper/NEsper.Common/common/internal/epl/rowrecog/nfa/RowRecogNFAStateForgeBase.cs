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
using com.espertech.esper.common.@internal.context.aifactory.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.rowrecog.nfa
{
    /// <summary>
    ///     Base for states.
    /// </summary>
    public abstract class RowRecogNFAStateForgeBase : RowRecogNFAStateForge
    {
        private readonly bool exprRequiresMultimatchState;
        private readonly IList<RowRecogNFAStateForge> nextStates;

        public RowRecogNFAStateForgeBase(
            string nodeNum,
            string variableName,
            int streamNum,
            bool multiple,
            bool? isGreedy,
            bool exprRequiresMultimatchState)
        {
            NodeNumNested = nodeNum;
            VariableName = variableName;
            StreamNum = streamNum;
            IsMultiple = multiple;
            IsGreedy = isGreedy;
            this.exprRequiresMultimatchState = exprRequiresMultimatchState;
            nextStates = new List<RowRecogNFAStateForge>();
        }

        internal abstract Type EvalClass { get; }

        public bool IsMultiple { get; }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(EvalClass, GetType(), classScope);
            method.Block
                .DeclareVar(EvalClass, "eval", NewInstance(EvalClass))
                .SetProperty(Ref("eval"), "NodeNumNested", Constant(NodeNumNested))
                .SetProperty(Ref("eval"), "VariableName", Constant(VariableName))
                .SetProperty(Ref("eval"), "StreamNum", Constant(StreamNum))
                .SetProperty(Ref("eval"), "Multiple", Constant(IsMultiple))
                .SetProperty(Ref("eval"), "Greedy", Constant(IsGreedy))
                .SetProperty(Ref("eval"), "NodeNumFlat", Constant(NodeNumFlat))
                .SetProperty(Ref("eval"), "ExprRequiresMultimatchState", Constant(exprRequiresMultimatchState));
            AssignInline(Ref("eval"), method, symbols, classScope);
            method.Block.MethodReturn(Ref("eval"));
            return LocalMethod(method);
        }

        public int NodeNumFlat { get; set; }

        public string NodeNumNested { get; }

        public virtual IList<RowRecogNFAStateForge> NextStates => nextStates;

        public string VariableName { get; }

        public int StreamNum { get; }

        public bool? IsGreedy { get; }

        public virtual bool IsExprRequiresMultimatchState => exprRequiresMultimatchState;

        internal abstract void AssignInline(
            CodegenExpression eval,
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope);

        /// <summary>
        ///     Assign a node number.
        /// </summary>
        /// <param name="nodeNumFlat">flat number</param>
        public RowRecogNFAStateForgeBase WithNodeNumFlat(int nodeNumFlat)
        {
            NodeNumFlat = nodeNumFlat;
            return this;
        }

        /// <summary>
        ///     Add a next state.
        /// </summary>
        /// <param name="next">state to add</param>
        public void AddState(RowRecogNFAStateForge next)
        {
            nextStates.Add(next);
        }
    }
} // end of namespace