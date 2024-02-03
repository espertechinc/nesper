///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
        ///     Returns an iterator of deployment state listeners (read-only)
        /// </summary>
        /// <value>listeners</value>
        IEnumerator<DeploymentStateListener> DeploymentStateListeners { get; }

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
        ///     <para>
        ///         Does not un-deploy staged deployments.
        ///     </para>
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
        ///     <para>
        ///         A statement is uniquely identified by the deployment id that deployed the statement
        ///         and by the statement name.
        ///     </para>
        /// </summary>
        /// <param name="deploymentId">deployment id of the statement</param>
        /// <param name="statementName">statement name</param>
        /// <returns>statement or null if the statement could not be found</returns>
        EPStatement GetStatement(
            string deploymentId,
            string statementName);

        /// <summary>
        ///     Returns the deployment.
        ///     <para>
        ///         A deployment is uniquely identified by its deployment id.
        ///     </para>
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
        ///     Removes all deployment state listener
        /// </summary>
        void RemoveAllDeploymentStateListeners();
        
        /// <summary>
        /// Roll-out multiple deployments. See {@link #rollout(Collection, RolloutOptions)}.
        /// </summary>
        /// <param name="items">compiled units and deployment options</param>
        /// <returns>deployment</returns>
        /// <exception cref="EPDeployException">when any of the deployments failed</exception>
        EPDeploymentRollout Rollout(ICollection<EPDeploymentRolloutCompiled> items);

        /// <summary>
        /// Roll-out multiple deployments.
        /// <p>
        ///     Deploys each compiled module, either deploying all compilation units or deploying none of the compilation units.
        /// </p>
        /// <p>
        ///     Does not reorder compilation units and expects compilation units to be ordered according to module dependencies (if any).
        /// </p>
        /// <p>
        ///     The step-by-step is as allows:
        /// </p>
        /// <ol>
        ///     <li>For each compilation unit, determine the deployment id or use the deployment id when provided in the deployment options; Check that all deployment ids do not already exist</li>
        ///     <li>For each compilation unit, load compilation unit via classloader and validate basic class-related information such as manifest information and version</li>
        ///     <li>For each compilation unit, check deployment preconditions and resolve deployment dependencies on EPL objects</li>
        ///     <li>For each compilation unit, initialize statement-internal objects</li>
        ///     <li>For each compilation unit, perform internal deployment of each statement of each module</li>
        /// </ol>
        /// <p>
        ///     In case any of the above steps fail the runtime completely rolls back all changes.
        /// </p>
        /// </summary>
        /// <param name="items">compiled units and deployment options</param>
        /// <param name="options">rollout options</param>
        /// <returns>deployment</returns>
        /// <exception cref="EPDeployException">when any of the deployments failed</exception>
        EPDeploymentRollout Rollout(ICollection<EPDeploymentRolloutCompiled> items, RolloutOptions options);

        /// <summary>
        /// Obtain information about other deployments that are depending on the given deployment,
        /// i.e. EPL objects that this deployment provides to other deployments.
        /// This method acquires the runtime-wide event processing read lock for the duration.
        /// Does not return dependencies on predefined EPL objects such as configured event types or variables.
        /// Does not return deployment-internal dependencies i.e. dependencies on EPL objects that are defined by the same deployment.
        /// </summary>
        /// 
        /// <param name="deploymentId">deployment id</param>
        /// <returns>dependencies or null if the deployment is not found</returns>
        /// <exception cref="EPException">when the required lock cannot be obtained</exception>
        EPDeploymentDependencyProvided GetDeploymentDependenciesProvided(string deploymentId);

        /// <summary>
        /// Obtain information about the dependencies that the given deployment has on other deployments,
        /// i.e. EPL objects that this deployment consumes from other deployments.
        /// This method acquires the runtime-wide event processing read lock for the duration.
        /// Does not return dependencies on predefined EPL objects such as configured event types or variables.
        /// Does not return deployment-internal dependencies i.e. dependencies on EPL objects that are defined by the same deployment.
        /// </summary>
        /// 
        /// <param name="deploymentId">deployment id</param>
        /// <returns>dependencies or null if the deployment is not found</returns>
        /// <exception cref="EPException">when the required lock cannot be obtained</exception>
        EPDeploymentDependencyConsumed GetDeploymentDependenciesConsumed(string deploymentId);
    }
} // end of namespace