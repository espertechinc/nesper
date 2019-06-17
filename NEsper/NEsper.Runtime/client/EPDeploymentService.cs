///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;

namespace com.espertech.esper.runtime.client
{
    /// <summary>
    ///     Service for deploying and undeploying modules and obtaining information about current deployments and statements.
    /// </summary>
    public interface EPDeploymentService
    {
        /// <summary>
        ///     Returns the deployment ids of all deployments.
        /// </summary>
        /// <value>deployment ids</value>
        string[] Deployments { get; }

        /// <summary>
        ///     Deploy a compiled module and with the default options.
        /// </summary>
        /// <param name="compiled">byte code</param>
        /// <returns>deployment</returns>
        /// <throws>EPDeployException when the deployment failed</throws>
        EPDeployment Deploy(EPCompiled compiled);

        /// <summary>
        ///     Deploy a compiled module and with the provided options.
        /// </summary>
        /// <param name="compiled">byte code</param>
        /// <param name="options">deployment options</param>
        /// <returns>deployment</returns>
        /// <throws>EPDeployException when the deployment failed</throws>
        EPDeployment Deploy(
            EPCompiled compiled,
            DeploymentOptions options);

        /// <summary>
        ///     Undeploy a deployment and with the default options.
        /// </summary>
        /// <param name="deploymentId">of the deployment to undeploy</param>
        /// <throws>
        ///     EPUndeployException when the deployment does not exist or the undeployment failed and the deployment remains
        ///     deployed
        /// </throws>
        void Undeploy(string deploymentId);

        /// <summary>
        ///     Undeploy a deployment and with the provided options
        /// </summary>
        /// <param name="deploymentId">of the deployment to undeploy</param>
        /// <param name="options">undeployment options</param>
        /// <throws>
        ///     EPUndeployException when the deployment does not exist or the undeployment failed and the deployment remains
        ///     deployed
        /// </throws>
        void Undeploy(
            string deploymentId,
            UndeploymentOptions options);

        /// <summary>
        ///     Undeploy all deployments and with the default options.
        /// </summary>
        /// <throws>EPUndeployException when the undeployment failed, of the deployments may remain deployed</throws>
        void UndeployAll();

        /// <summary>
        ///     Undeploy all deployments and with the provided options.
        /// </summary>
        /// <param name="options">undeployment options or null if none provided</param>
        /// <throws>EPUndeployException when the undeployment failed, of the deployments may remain deployed</throws>
        void UndeployAll(UndeploymentOptions options);

        /// <summary>
        ///     Returns the statement of a given deployment.
        ///     <para />
        ///     A statement is uniquely identified by the deployment id that deployed the statement
        ///     and by the statement name.
        /// </summary>
        /// <param name="deploymentId">deployment id of the statement</param>
        /// <param name="statementName">statement name</param>
        /// <returns>statement or null if the statement could not be found</returns>
        EPStatement GetStatement(
            string deploymentId,
            string statementName);

        /// <summary>
        ///     Returns the deployment.
        ///     <para />
        ///     A deployment is uniquely identified by its deployment id.
        /// </summary>
        /// <param name="deploymentId">the deployment id of the deployment</param>
        /// <returns>deployment or null if the deployment could not be found</returns>
        EPDeployment GetDeployment(string deploymentId);

        /// <summary>
        ///     Add a deployment state listener
        /// </summary>
        /// <param name="listener">to add</param>
        void AddDeploymentStateListener(DeploymentStateListener listener);

        /// <summary>
        ///     Remove a deployment state listener
        /// </summary>
        /// <param name="listener">to remove</param>
        void RemoveDeploymentStateListener(DeploymentStateListener listener);

        /// <summary>
        ///     Returns an iterator of deployment state listeners (read-only)
        /// </summary>
        /// <value>listeners</value>
        IEnumerator<DeploymentStateListener> DeploymentStateListeners { get; }

        /// <summary>
        ///     Removes all deployment state listener
        /// </summary>
        void RemoveAllDeploymentStateListeners();
    }
} // end of namespace