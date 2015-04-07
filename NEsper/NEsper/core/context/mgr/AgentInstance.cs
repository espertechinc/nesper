///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.core.context.util;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.core.context.mgr
{
    public class AgentInstance
    {
        public AgentInstance(StopCallback stopCallback,
                             AgentInstanceContext agentInstanceContext,
                             Viewable finalView)
        {
            StopCallback = stopCallback;
            AgentInstanceContext = agentInstanceContext;
            FinalView = finalView;
        }

        public StopCallback StopCallback { get; private set; }

        public AgentInstanceContext AgentInstanceContext { get; private set; }

        public Viewable FinalView { get; private set; }
    }
}