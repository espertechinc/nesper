///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.@internal.kernel.service;

namespace com.espertech.esper.runtime.@internal.kernel.stage
{
    public class EPStageDeploymentServiceImpl : EPStageDeploymentServiceSPI
    {
        private readonly EPServicesContext servicesContext;
        private readonly StageSpecificServices stageSpecificServices;
        private readonly string stageUri;

        public EPStageDeploymentServiceImpl(
            string stageUri,
            EPServicesContext servicesContext,
            StageSpecificServices stageSpecificServices)
        {
            this.stageUri = stageUri;
            this.servicesContext = servicesContext;
            this.stageSpecificServices = stageSpecificServices;
        }

        public EPDeployment GetDeployment(string deploymentId)
        {
            return EPDeploymentServiceUtil.ToDeployment(stageSpecificServices.DeploymentLifecycleService, deploymentId);
        }

        public IDictionary<string, DeploymentInternal> DeploymentMap => stageSpecificServices.DeploymentLifecycleService.DeploymentMap;

        public string[] Deployments => stageSpecificServices.DeploymentLifecycleService.DeploymentIds;
    }
} // end of namespace