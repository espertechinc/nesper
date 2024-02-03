///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.runtime.client
{
    /// <summary>
    /// Listener for events in respect to deployment and undeployment.
    /// </summary>
    public interface DeploymentStateListener
    {
        /// <summary>
        /// Called when a deployment completed
        /// </summary>
        /// <param name="event">deployment information</param>
        void OnDeployment(DeploymentStateEventDeployed @event);

        /// <summary>
        /// Called when an undeployment completed
        /// </summary>
        /// <param name="event">undeployment information</param>
        void OnUndeployment(DeploymentStateEventUndeployed @event);
    }

    public class ProxyDeploymentStateListener : DeploymentStateListener
    {
        public Action<DeploymentStateEventDeployed> ProcOnDeployment { get; set; }
        public void OnDeployment(DeploymentStateEventDeployed @event) => ProcOnDeployment?.Invoke(@event);

        public Action<DeploymentStateEventUndeployed> ProcOnUndeployment { get; set; }
        public void OnUndeployment(DeploymentStateEventUndeployed @event) => ProcOnUndeployment?.Invoke(@event);
    }
} // end of namespace