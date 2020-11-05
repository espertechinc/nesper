///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
    ///     Base for states.
    /// </summary>
    public abstract class RowRecogNFAStateBase : RowRecogNFAState
    {
        public bool? IsGreedy { get; set; }

        public string NodeNumNested { get; set; }

        public string VariableName { get; set; }

        public int StreamNum { get; set; }

        public bool IsMultiple { get; set; }

        public virtual RowRecogNFAState[] NextStates { get; set; }

        public int NodeNumFlat { get; set; }

        public virtual bool IsExprRequiresMultimatchState { get; set; }

        public abstract bool Matches(
            EventBean[] eventsPerStream,
            AgentInstanceContext agentInstanceContext);
    }
} // end of namespace