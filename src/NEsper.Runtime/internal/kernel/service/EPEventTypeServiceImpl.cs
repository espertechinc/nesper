///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.runtime.client;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
    public class EPEventTypeServiceImpl : EPEventTypeService
    {
        private readonly EPServicesContext services;

        public EPEventTypeServiceImpl(EPServicesContext services)
        {
            this.services = services;
        }

        public EventType GetEventTypePreconfigured(string eventTypeName)
        {
            return services.EventTypeRepositoryBus.GetTypeByName(eventTypeName);
        }

        public EventType GetEventType(string deploymentId, string eventTypeName)
        {
            DeploymentInternal deployment = services.DeploymentLifecycleService.GetDeploymentById(deploymentId);
            if (deployment == null)
            {
                return null;
            }
            string moduleName = deployment.ModuleProvider.ModuleName;
            return services.EventTypePathRegistry.GetWithModule(eventTypeName, moduleName);
        }
        
        public EventType GetBusEventType(string eventTypeName)
        {
            return services.EventTypeRepositoryBus.GetTypeByName(eventTypeName);
        }
    }
} // end of namespace