///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.runtime.@internal.deploymentlifesvc;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
    public class EPServicesHA
    {
        public EPServicesHA(
            RuntimeExtensionServices runtimeExtensionServices,
            DeploymentRecoveryService deploymentRecoveryService,
            ListenerRecoveryService listenerRecoveryService,
            StatementIdRecoveryService statementIdRecoveryService,
            long? currentTimeAsRecovered,
            IDictionary<int, long> currentTimeStageAsRecovered)
        {
            RuntimeExtensionServices = runtimeExtensionServices;
            DeploymentRecoveryService = deploymentRecoveryService;
            ListenerRecoveryService = listenerRecoveryService;
            StatementIdRecoveryService = statementIdRecoveryService;
            CurrentTimeAsRecovered = currentTimeAsRecovered;
            CurrentTimeStageAsRecovered = currentTimeStageAsRecovered;
        }

        public RuntimeExtensionServices RuntimeExtensionServices { get; }

        public DeploymentRecoveryService DeploymentRecoveryService { get; }

        public ListenerRecoveryService ListenerRecoveryService { get; }

        public StatementIdRecoveryService StatementIdRecoveryService { get; }

        public long? CurrentTimeAsRecovered { get; }

        public IDictionary<int, long> CurrentTimeStageAsRecovered { get; }

        public void Destroy()
        {
            ((RuntimeExtensionServicesSPI) RuntimeExtensionServices).Destroy();
        }
    }
} // end of namespace