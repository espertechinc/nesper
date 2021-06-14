///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.context;
using com.espertech.esper.common.client.metric;
using com.espertech.esper.common.client.variable;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.@internal.kernel.stage;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
    internal class EPRuntimeEnv
    {
        public EPRuntimeEnv(
            EPServicesContext services,
            EPEventServiceSPI eventService,
            EPDeploymentServiceSPI deploymentService,
            EPEventTypeService eventTypeService,
            EPContextPartitionService contextPartitionService,
            EPVariableService variableService,
            EPMetricsService metricsService,
            EPFireAndForgetService fireAndForgetService,
            EPStageServiceSPI stageService)
        {
            Services = services;
            Runtime = eventService;
            DeploymentService = deploymentService;
            EventTypeService = eventTypeService;
            ContextPartitionService = contextPartitionService;
            VariableService = variableService;
            MetricsService = metricsService;
            FireAndForgetService = fireAndForgetService;
            StageService = stageService;
        }

        public EPServicesContext Services { get; }

        public EPEventServiceSPI Runtime { get; }

        public EPDeploymentServiceSPI DeploymentService { get; }

        public EPEventTypeService EventTypeService { get; }

        public EPEventServiceSPI EventService => Runtime;

        public EPContextPartitionService ContextPartitionService { get; }

        public EPVariableService VariableService { get; }

        public EPMetricsService MetricsService { get; }

        public EPFireAndForgetService FireAndForgetService { get; }
        
        public EPStageServiceSPI StageService { get; }
    }
} // end of namespace