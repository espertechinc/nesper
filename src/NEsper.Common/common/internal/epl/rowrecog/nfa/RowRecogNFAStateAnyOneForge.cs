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

namespace com.espertech.esper.common.@internal.epl.rowrecog.nfa
{
    /// <summary>
    ///     Any-quantifier.
    /// </summary>
    public class RowRecogNFAStateAnyOneForge : RowRecogNFAStateForgeBase
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="nodeNum">node num</param>
        /// <param name="variableName">variable</param>
        /// <param name="streamNum">stream num</param>
        /// <param name="multiple">indicator</param>
        public RowRecogNFAStateAnyOneForge(
            string nodeNum,
            string variableName,
            int streamNum,
            bool multiple)
            : base(
                nodeNum,
                variableName,
                streamNum,
                multiple,
                null,
                false)
        {
        }

        internal override Type EvalClass => typeof(RowRecogNFAStateAnyOneEval);

        public override string ToString()
        {
            return "AnyEvent";
        }

        internal override void AssignInline(
            CodegenExpression eval,
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
        }
    }
} // end of namespace