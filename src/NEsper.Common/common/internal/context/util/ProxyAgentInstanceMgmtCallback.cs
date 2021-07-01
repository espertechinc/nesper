///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.context.util
{
    public class ProxyAgentInstanceMgmtCallback : AgentInstanceMgmtCallback
    {
        public Action<AgentInstanceStopServices> ProcStop;
        public Action<AgentInstanceTransferServices> ProcTransfer;

        public ProxyAgentInstanceMgmtCallback() : this(null, null)
        {
        }

        public ProxyAgentInstanceMgmtCallback(Action<AgentInstanceStopServices> procStop) : this(procStop, null)
        {
        }

        public ProxyAgentInstanceMgmtCallback(Action<AgentInstanceTransferServices> procTransfer) : this(null, procTransfer)
        {
        }

        public ProxyAgentInstanceMgmtCallback(
            Action<AgentInstanceStopServices> procStop,
            Action<AgentInstanceTransferServices> procTransfer)
        {
            ProcStop = procStop;
            ProcTransfer = procTransfer;
        }

        public void Stop(AgentInstanceStopServices services)
        {
            ProcStop?.Invoke(services);
        }

        public void Transfer(AgentInstanceTransferServices services)
        {
            ProcTransfer?.Invoke(services);
        }
    }
}