///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.client.dataflow.core
{
    /// <summary>
    /// Data flow runtime for instantiating data flows.
    /// </summary>
    public interface EPDataFlowService
    {
        /// <summary>
        /// Returns a descriptor for the given data flow, or null if the data flow has not been declared.
        /// </summary>
        /// <param name="deploymentId">deployment id of dataflow (deployment id of create-dataflow statement)</param>
        /// <param name="dataflowName">data flow name</param>
        /// <returns>data flow descriptor</returns>
        EPDataFlowDescriptor GetDataFlow(
            string deploymentId,
            string dataflowName);

        /// <summary>
        /// Returns the names of all declared data flows.
        /// </summary>
        /// <value>data flow names</value>
        DeploymentIdNamePair[] DataFlows { get; }

        /// <summary>
        /// Instantiate a data flow.
        /// </summary>
        /// <param name="deploymentId">deployment id of dataflow (deployment id of create-dataflow statement)</param>
        /// <param name="dataflowName">name of data flow to instantiate</param>
        /// <returns>data flow instance</returns>
        /// <throws>EPDataFlowInstantiationException when the instantiation failed</throws>
        EPDataFlowInstance Instantiate(
            string deploymentId,
            string dataflowName);

        /// <summary>
        /// Instantiate a data flow, with options.
        /// </summary>
        /// <param name="deploymentId">deployment id of dataflow (deployment id of create-dataflow statement)</param>
        /// <param name="dataFlowName">name of data flow to instantiate</param>
        /// <param name="options">populate options to control parameterization, instantiation etc.</param>
        /// <returns>data flow instance</returns>
        /// <throws>EPDataFlowInstantiationException when the instantiation failed</throws>
        EPDataFlowInstance Instantiate(
            string deploymentId,
            string dataFlowName,
            EPDataFlowInstantiationOptions options);

        /// <summary>
        /// Save an existing instance with the runtime, for later retrieval.
        /// </summary>
        /// <param name="instanceName">name to use to save, must be unique among currently saved instances</param>
        /// <param name="instance">saved</param>
        /// <throws>EPDataFlowAlreadyExistsException if an instance by this name already exists</throws>
        void SaveInstance(
            string instanceName,
            EPDataFlowInstance instance);

        /// <summary>
        /// Returns the instance names of a saved data flow instances.
        /// </summary>
        /// <value>data flow instance names</value>
        string[] SavedInstances { get; }

        /// <summary>
        /// Returns a specific saved data flow instance, or null if it has not been found
        /// </summary>
        /// <param name="instanceName">to look for</param>
        /// <returns>instance</returns>
        EPDataFlowInstance GetSavedInstance(string instanceName);

        /// <summary>
        /// Remove an instance previously saved.
        /// </summary>
        /// <param name="instanceName">to be removed</param>
        /// <returns>indicator whether found or not</returns>
        bool RemoveSavedInstance(string instanceName);

        /// <summary>
        /// Save an existing data flow configuration (data flow name and its options) for later retrieval.
        /// </summary>
        /// <param name="dataflowConfigName">configuration name to save, must be unique</param>
        /// <param name="deploymentId">deployment id</param>
        /// <param name="dataFlowName">data flow name</param>
        /// <param name="options">options object</param>
        /// <throws>EPDataFlowAlreadyExistsException if the configuration name is already used</throws>
        /// <throws>EPDataFlowNotFoundException      if the data flow by this name does not exist</throws>
        void SaveConfiguration(
            string dataflowConfigName,
            string deploymentId,
            string dataFlowName,
            EPDataFlowInstantiationOptions options);

        /// <summary>
        /// Returns the names of a saved data flow configurations.
        /// </summary>
        /// <value>data flow configuration names</value>
        string[] SavedConfigurations { get; }

        /// <summary>
        /// Returns a saved dataflow configuration or null if it is not found.
        /// </summary>
        /// <param name="configurationName">name to find</param>
        /// <returns>data flow configuration</returns>
        EPDataFlowSavedConfiguration GetSavedConfiguration(string configurationName);

        /// <summary>
        /// Instantiate a data flow from a saved configuration.
        /// </summary>
        /// <param name="configurationName">configuration name</param>
        /// <returns>instance</returns>
        /// <throws>EPDataFlowInstantiationException if the configuration name could not be found</throws>
        EPDataFlowInstance InstantiateSavedConfiguration(string configurationName);

        /// <summary>
        /// Remove a previously saved data flow configuration.
        /// </summary>
        /// <param name="configurationName">to remove</param>
        /// <returns>indicator whether found and removed</returns>
        bool RemoveSavedConfiguration(string configurationName);
    }
} // end of namespace