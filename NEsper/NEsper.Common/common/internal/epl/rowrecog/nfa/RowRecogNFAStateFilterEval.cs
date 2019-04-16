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
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.rowrecog.nfa
{
    /// <summary>
    /// NFA state for a single match that applies a filter.
    /// </summary>
    public class RowRecogNFAStateFilterEval : RowRecogNFAStateBase,
        RowRecogNFAState
    {
        private ExprEvaluator expression;
        private string expressionTextForAudit;

        public override bool Matches(
            EventBean[] eventsPerStream,
            AgentInstanceContext agentInstanceContext)
        {
            agentInstanceContext.InstrumentationProvider.QRegFilter(expressionTextForAudit, eventsPerStream);
            var result = expression.Evaluate(eventsPerStream, true, agentInstanceContext);
            if (result != null) {
                var resultAsBoolean = result.AsBoolean();
                agentInstanceContext.InstrumentationProvider.ARegFilter(resultAsBoolean);
                return resultAsBoolean;
            }

            agentInstanceContext.InstrumentationProvider.ARegFilter(false);
            return false;
        }

        public override string ToString()
        {
            return "FilterEvent";
        }

        public ExprEvaluator Expression {
            set => this.expression = value;
        }

        public string ExpressionTextForAudit {
            set => this.expressionTextForAudit = value;
        }
    }
} // end of namespace