///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.rowrecog.nfa
{
    /// <summary>
    ///     End state in the regex NFA states.
    /// </summary>
    public class RowRecogNFAStateEndEval : RowRecogNFAStateBase
    {
        private static readonly RowRecogNFAState[] EMPTY_ARRAY = new RowRecogNFAState[0];

        public RowRecogNFAStateEndEval()
        {
            NodeNumFlat = -1;
            StreamNum = -1;
            NodeNumNested = "end-state";
        }

        public override RowRecogNFAState[] NextStates => EMPTY_ARRAY;

        public override bool IsExprRequiresMultimatchState => throw new UnsupportedOperationException();

        public override bool Matches(EventBean[] eventsPerStream, AgentInstanceContext agentInstanceContext)
        {
            throw new UnsupportedOperationException();
        }
    }
} // end of namespace