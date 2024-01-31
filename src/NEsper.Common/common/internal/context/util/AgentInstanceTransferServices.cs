///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.common.@internal.schedule;

namespace com.espertech.esper.common.@internal.context.util
{
    public class AgentInstanceTransferServices
    {
        private readonly AgentInstanceContext agentInstanceContext;
        private readonly FilterService targetFilterService;
        private readonly SchedulingService targetSchedulingService;
        private readonly InternalEventRouter targetInternalEventRouter;

        public AgentInstanceTransferServices(
            AgentInstanceContext agentInstanceContext,
            FilterService targetFilterService,
            SchedulingService targetSchedulingService,
            InternalEventRouter targetInternalEventRouter)
        {
            this.agentInstanceContext = agentInstanceContext;
            this.targetFilterService = targetFilterService;
            this.targetSchedulingService = targetSchedulingService;
            this.targetInternalEventRouter = targetInternalEventRouter;
        }

        public AgentInstanceContext AgentInstanceContext => agentInstanceContext;

        public FilterService TargetFilterService => targetFilterService;

        public SchedulingService TargetSchedulingService => targetSchedulingService;

        public InternalEventRouter TargetInternalEventRouter => targetInternalEventRouter;
    }
} // end of namespace