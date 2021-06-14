///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.rowrecog.nfa
{
    /// <summary>
    ///     The '?' state in the regex NFA states.
    /// </summary>
    public class RowRecogNFAStateOneOptionalEvalCond : RowRecogNFAStateBase,
        RowRecogNFAState
    {
        public ExprEvaluator Expression { get; set; }

        public override bool Matches(
            EventBean[] eventsPerStream,
            AgentInstanceContext agentInstanceContext)
        {
            var result = (bool?) Expression.Evaluate(eventsPerStream, true, agentInstanceContext);
            if (result != null) {
                return result.Value;
            }

            return false;
        }

        public override string ToString()
        {
            return "OptionalFilterEvent-Filtered";
        }
    }
} // end of namespace