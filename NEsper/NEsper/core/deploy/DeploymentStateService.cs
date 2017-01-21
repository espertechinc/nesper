///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client.deploy;


namespace com.espertech.esper.core.deploy
{
    /// <summary>Interface for a service maintaining deployment state. </summary>
    public interface DeploymentStateService : IDisposable
    {
        /// <summary>Allocates a new deployment id. </summary>
        /// <value>deployment id</value>
        string NextDeploymentId { get; }

        /// <summary>Returns a list of deployment ids of deployments. </summary>
        /// <value>deployment ids</value>
        string[] Deployments { get; }

        /// <summary>Returns the deployment informaton for a given deployment id. </summary>
        /// <param name="deploymentId">id</param>
        /// <returns>deployment information</returns>
        DeploymentInformation GetDeployment(String deploymentId);

        /// <summary>Returns deployment information for all deployments. </summary>
        /// <value>array of deployment info</value>
        DeploymentInformation[] AllDeployments { get; }

        /// <summary>Add or Update the deployment information using the contained deployment id as a key. </summary>
        /// <param name="descriptor">to store</param>
        void AddUpdateDeployment(DeploymentInformation descriptor);
    
        /// <summary> </summary>
        /// <param name="deploymentId"></param>
        void Remove(String deploymentId);
    }
}
