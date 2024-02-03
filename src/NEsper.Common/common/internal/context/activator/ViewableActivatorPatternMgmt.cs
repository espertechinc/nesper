///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.pattern.core;

namespace com.espertech.esper.common.@internal.context.activator
{
    public class ViewableActivatorPatternMgmt : AgentInstanceMgmtCallback
    {
        private readonly EvalRootState _rootState;

        public ViewableActivatorPatternMgmt(EvalRootState rootState)
        {
            _rootState = rootState;
        }


        public void Stop(AgentInstanceStopServices services)
        {
            _rootState.Stop();
        }

        public void Transfer(AgentInstanceTransferServices services)
        {
            _rootState.Accept(new EvalStateNodeVisitorStageTransfer(services));
        }
    }
}