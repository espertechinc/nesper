///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client.module;

namespace com.espertech.esper.runtime.client
{
    /// <summary>
    ///     Represents a deployment.
    /// </summary>
    public class EPDeployment
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="deploymentId">deployment id</param>
        /// <param name="moduleName">module name or null if none provided</param>
        /// <param name="moduleProperties">module properties</param>
        /// <param name="statements">statements</param>
        /// <param name="deploymentIdDependencies">array of deployment ids that this deployment depends</param>
        /// <param name="lastUpdateDate">last update date</param>
        public EPDeployment(
            string deploymentId,
            string moduleName,
            IDictionary<ModuleProperty, object> moduleProperties,
            EPStatement[] statements,
            string[] deploymentIdDependencies,
            DateTime lastUpdateDate)
        {
            DeploymentId = deploymentId;
            ModuleName = moduleName;
            ModuleProperties = moduleProperties;
            Statements = statements;
            DeploymentIdDependencies = deploymentIdDependencies;
            LastUpdateDate = lastUpdateDate;
        }

        /// <summary>
        ///     Returns the statements
        /// </summary>
        /// <value>statements</value>
        public EPStatement[] Statements { get; }

        /// <summary>
        ///     Returns the module name or null if none provided
        /// </summary>
        /// <value>module name</value>
        public string ModuleName { get; }

        /// <summary>
        ///     Returns module properties
        /// </summary>
        /// <value>module properties</value>
        public IDictionary<ModuleProperty, object> ModuleProperties { get; }

        /// <summary>
        ///     Returns the last update date
        /// </summary>
        /// <value>last update date</value>
        public DateTime LastUpdateDate { get; }

        /// <summary>
        ///     Returns the deployment ids of the deployments that this deployment depends on
        /// </summary>
        /// <value>deployment id array of dependencies</value>
        public string[] DeploymentIdDependencies { get; }

        /// <summary>
        ///     Returns the deployment id
        /// </summary>
        /// <value>deployment id</value>
        public string DeploymentId { get; }
    }
} // end of namespace