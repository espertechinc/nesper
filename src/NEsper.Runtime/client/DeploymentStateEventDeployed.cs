///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.runtime.client
{
    /// <summary>
    /// Deployment event indicating a deployment completed
    /// </summary>
    public class DeploymentStateEventDeployed : DeploymentStateEvent
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="runtimeURI">runtime uri</param>
        /// <param name="deploymentId">deployment id</param>
        /// <param name="moduleName">module name</param>
        /// <param name="statements">statements</param>
        /// <param name="rolloutItemNumber">rollout item number when using rollout</param>
        public DeploymentStateEventDeployed(
            string runtimeURI,
            string deploymentId,
            string moduleName,
            EPStatement[] statements,
            int rolloutItemNumber)
            : base(runtimeURI, deploymentId, moduleName, statements, rolloutItemNumber)
        {
        }
    }
} // end of namespace