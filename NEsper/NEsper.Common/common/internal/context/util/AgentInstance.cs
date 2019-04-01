///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.@internal.context.util
{
    public class AgentInstance
    {
        public AgentInstance(
            AgentInstanceStopCallback stopCallback, AgentInstanceContext agentInstanceContext, Viewable finalView)
        {
            StopCallback = stopCallback;
            AgentInstanceContext = agentInstanceContext;
            FinalView = finalView;
        }

        public AgentInstanceStopCallback StopCallback { get; }

        public AgentInstanceContext AgentInstanceContext { get; }

        public Viewable FinalView { get; }
    }
} // end of namespace