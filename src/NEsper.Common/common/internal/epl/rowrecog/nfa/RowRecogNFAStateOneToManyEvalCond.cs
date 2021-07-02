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
    /// The '+' state in the regex NFA states.
    /// </summary>
    public class RowRecogNFAStateOneToManyEvalCond : RowRecogNFAStateBase,
        RowRecogNFAState
    {
        private ExprEvaluator expression;

        public override bool Matches(
            EventBean[] eventsPerStream,
            AgentInstanceContext agentInstanceContext)
        {
            var result = expression.Evaluate(eventsPerStream, true, agentInstanceContext);
            if (result != null) {
                return true.Equals(result);
            }

            return false;
        }

        public override string ToString()
        {
            return "OneMany-Filtered";
        }

        public ExprEvaluator Expression {
            set { expression = value; }
        }
    }
} // end of namespace