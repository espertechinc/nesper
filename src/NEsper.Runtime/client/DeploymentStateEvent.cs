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
    ///     Deployment base event.
    /// </summary>
    public abstract class DeploymentStateEvent
    {
        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="runtimeURI">runtime URI</param>
        /// <param name="deploymentId">deployment id</param>
        /// <param name="moduleName">module name</param>
        /// <param name="statements">statements</param>
        /// <param name="rolloutItemNumber">rollout item number when using rollout</param>
        public DeploymentStateEvent(
            string runtimeURI,
            string deploymentId,
            string moduleName,
            EPStatement[] statements,
            int rolloutItemNumber)
        {
            RuntimeURI = runtimeURI;
            DeploymentId = deploymentId;
            ModuleName = moduleName;
            Statements = statements;
            RolloutItemNumber = rolloutItemNumber;
        }

        /// <summary>
        ///     Returns the runtime uri
        /// </summary>
        /// <value>runtime uri</value>
        public string RuntimeURI { get; }

        /// <summary>
        ///     Returns the deployment id
        /// </summary>
        /// <value>deployment id</value>
        public string DeploymentId { get; }

        /// <summary>
        ///     Returns the module name, when provided, or null if none provided
        /// </summary>
        /// <value>module name</value>
        public string ModuleName { get; }

        /// <summary>
        ///     Returns the statements for the deployment or undeployment
        /// </summary>
        /// <value>statements</value>
        public EPStatement[] Statements { get; }
        
        /// <summary>
        /// Returns the rollout item number, or -1 when not using rollout
        /// </summary>
        public int RolloutItemNumber { get; }
    }
} // end of namespace