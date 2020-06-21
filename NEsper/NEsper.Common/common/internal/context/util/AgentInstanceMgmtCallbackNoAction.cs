///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.context.util
{
    public class AgentInstanceMgmtCallbackNoAction : AgentInstanceMgmtCallback
    {
        public static readonly AgentInstanceMgmtCallbackNoAction INSTANCE = new AgentInstanceMgmtCallbackNoAction();

        private AgentInstanceMgmtCallbackNoAction()
        {
        }

        public void Stop(AgentInstanceStopServices services)
        {
            // no action
        }

        public void Transfer(AgentInstanceTransferServices services)
        {
            // no action
        }
    }
} // end of namespace