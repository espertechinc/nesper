///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;

namespace com.espertech.esper.common.@internal.epl.rowrecog.nfa
{
    /// <summary>
    /// Match-recognize NFA states provides this information.
    /// </summary>
    public interface RowRecogNFAStateForge
    {
        /// <summary>
        /// Returns the nested node number.
        /// </summary>
        /// <value>num</value>
        string NodeNumNested { get; }

        /// <summary>
        /// Returns the absolute node num.
        /// </summary>
        /// <value>num</value>
        int NodeNumFlat { get; }

        /// <summary>
        /// Returns the variable name.
        /// </summary>
        /// <value>name</value>
        string VariableName { get; }

        /// <summary>
        /// Returns stream number.
        /// </summary>
        /// <value>stream num</value>
        int StreamNum { get; }

        /// <summary>
        /// Returns greedy indicator.
        /// </summary>
        /// <value>greedy indicator</value>
        bool? IsGreedy { get; }

        /// <summary>
        /// Returns the next states.
        /// </summary>
        /// <value>states</value>
        IList<RowRecogNFAStateForge> NextStates { get; }

        /// <summary>
        /// Whether or not the match-expression requires multimatch state
        /// </summary>
        /// <value>indicator</value>
        bool IsExprRequiresMultimatchState { get; }

        CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbol,
            CodegenClassScope classScope);
    }
} // end of namespace