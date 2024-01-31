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
    /// The '?' state in the regex NFA states.
    /// </summary>
    public class RowRecogNFAStateOneOptionalEvalNoCond : RowRecogNFAStateBase
    {
        public override bool Matches(
            EventBean[] eventsPerStream,
            AgentInstanceContext agentInstanceContext)
        {
            return true;
        }

        public override string ToString()
        {
            return "OptionalFilterEvent";
        }
    }
} // end of namespace