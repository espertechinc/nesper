///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;

namespace com.espertech.esper.common.@internal.epl.rowrecog.nfa
{
    /// <summary>
    ///     Match-recognize NFA states provides this information.
    /// </summary>
    public interface RowRecogNFAState
    {
        /// <summary>
        ///     For multiple-quantifiers.
        /// </summary>
        /// <value>indicator</value>
        bool IsMultiple { get; }

        /// <summary>
        ///     Returns the nested node number.
        /// </summary>
        /// <value>num</value>
        string NodeNumNested { get; }

        /// <summary>
        ///     Returns the absolute node num.
        /// </summary>
        /// <value>num</value>
        int NodeNumFlat { get; }

        /// <summary>
        ///     Returns the variable name.
        /// </summary>
        /// <value>name</value>
        string VariableName { get; }

        /// <summary>
        ///     Returns stream number.
        /// </summary>
        /// <value>stream num</value>
        int StreamNum { get; }

        /// <summary>
        ///     Returns greedy indicator.
        /// </summary>
        /// <value>greedy indicator</value>
        bool? IsGreedy { get; }

        /// <summary>
        ///     Returns the next states.
        /// </summary>
        /// <value>states</value>
        RowRecogNFAState[] NextStates { get; }

        /// <summary>
        ///     Whether or not the match-expression requires multimatch state
        /// </summary>
        /// <value>indicator</value>
        bool IsExprRequiresMultimatchState { get; }

        /// <summary>
        ///     Evaluate a match.
        /// </summary>
        /// <param name="eventsPerStream">variable values</param>
        /// <param name="agentInstanceContext">expression evaluation context</param>
        /// <returns>match indicator</returns>
        bool Matches(
            EventBean[] eventsPerStream,
            AgentInstanceContext agentInstanceContext);
    }
} // end of namespace