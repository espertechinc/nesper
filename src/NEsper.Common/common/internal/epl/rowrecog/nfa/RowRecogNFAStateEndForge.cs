///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.rowrecog.nfa
{
    /// <summary>
    ///     End state in the regex NFA states.
    /// </summary>
    public class RowRecogNFAStateEndForge : RowRecogNFAStateForgeBase
    {
        public RowRecogNFAStateEndForge()
            : base("endstate", null, -1, false, null, false)
        {
        }

        public override IList<RowRecogNFAStateForge> NextStates => Collections.GetEmptyList<RowRecogNFAStateForge>();

        public override bool IsExprRequiresMultimatchState => throw new UnsupportedOperationException();

        internal override Type EvalClass => typeof(RowRecogNFAStateEndEval);

        internal override void AssignInline(
            CodegenExpression eval,
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            throw new IllegalStateException("Cannot build end state, end node is implied by node-num=-1");
        }
    }
} // end of namespace