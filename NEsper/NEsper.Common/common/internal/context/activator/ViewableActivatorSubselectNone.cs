///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;

namespace com.espertech.esper.common.@internal.context.activator
{
    public class ViewableActivatorSubselectNone : ViewableActivator
    {
        public EventType EventType { get; set; }

        public ViewableActivationResult Activate(
            AgentInstanceContext agentInstanceContext, bool isSubselect, bool isRecoveringResilient)
        {
            return new ViewableActivationResult(
                null, AgentInstanceStopCallbackNoAction.INSTANCE, null, false, false, null, null);
        }
    }
} // end of namespace