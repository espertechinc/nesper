///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.dataflow.core
{
    /// <summary>
    ///     A data flow configuration is just a configuration name, a data flow name
    ///     and an instantiation options object.
    /// </summary>
    [Serializable]
    public class EPDataFlowSavedConfiguration
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="savedConfigurationName">name of saved configuration</param>
        /// <param name="deploymentId">deployment id</param>
        /// <param name="dataflowName">data flow name</param>
        /// <param name="options">options object</param>
        public EPDataFlowSavedConfiguration(
            string savedConfigurationName,
            string deploymentId,
            string dataflowName,
            EPDataFlowInstantiationOptions options)
        {
            SavedConfigurationName = savedConfigurationName;
            DeploymentId = deploymentId;
            DataflowName = dataflowName;
            Options = options;
        }

        /// <summary>
        ///     Configuation name.
        /// </summary>
        /// <returns>name</returns>
        public string SavedConfigurationName { get; }

        /// <summary>
        ///     Data flow name.
        /// </summary>
        /// <returns>data flow name</returns>
        public string DataflowName { get; }

        /// <summary>
        ///     Data flow instantiation options.
        /// </summary>
        /// <returns>options</returns>
        public EPDataFlowInstantiationOptions Options { get; }

        /// <summary>
        ///     Returns the deployment id
        /// </summary>
        /// <returns>deployment id</returns>
        public string DeploymentId { get; }
    }
} // end of namespace