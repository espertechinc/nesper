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
using com.espertech.esper.common.@internal.epl.table.core;

namespace com.espertech.esper.common.@internal.context.activator
{
    public class ViewableActivatorTable : ViewableActivator
    {
        public Table Table { get; set; }

        public ExprEvaluator FilterEval { get; set; }

        public EventType EventType => Table.MetaData.PublicEventType;

        public ViewableActivationResult Activate(
            AgentInstanceContext agentInstanceContext,
            bool isSubselect,
            bool isRecoveringResilient)
        {
            var state = Table.GetTableInstance(agentInstanceContext.AgentInstanceId);
            return new ViewableActivationResult(
                new TableStateViewableInternal(state, FilterEval),
                AgentInstanceMgmtCallbackNoAction.INSTANCE,
                null,
                false,
                false,
                null,
                null,
                null);
        }
    }
} // end of namespace