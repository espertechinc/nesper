///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.@internal.context.aifactory.createdataflow;
using com.espertech.esper.common.@internal.epl.dataflow.realize;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.dataflow.core
{
    public class EPDataFlowServiceImpl : EPDataFlowService
    {
        public const string OP_PACKAGE_NAME = "com.espertech.esper.runtime.internal.dataflow.op";

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IDictionary<string, DataflowDeployment> deployments = new Dictionary<string, DataflowDeployment>();
        private readonly IDictionary<string, EPDataFlowInstance> instances = new Dictionary<string, EPDataFlowInstance>();
        private readonly DataFlowConfigurationStateService configurationState = new DataFlowConfigurationStateServiceImpl();

        private int agentInstanceNumCurrent;

        public EPDataFlowDescriptor GetDataFlow(
            string deploymentId,
            string dataflowName)
        {
            lock (this) {
                DataflowDesc entry = GetEntryMayNull(deploymentId, dataflowName);
                return entry == null
                    ? null
                    : new EPDataFlowDescriptor(entry.DataflowName, entry.StatementContext.StatementName);
            }
        }

        public DeploymentIdNamePair[] DataFlows {
            get {
                lock (this) {
                    IList<DeploymentIdNamePair> ids = new List<DeploymentIdNamePair>();
                    foreach (KeyValuePair<string, DataflowDeployment> deployment in deployments) {
                        foreach (KeyValuePair<string, DataflowDesc> entry in deployment.Value.Dataflows) {
                            ids.Add(new DeploymentIdNamePair(deployment.Key, entry.Key));
                        }
                    }

                    return ids.ToArray();
                }
            }
        }

        public EPDataFlowInstance Instantiate(
            string deploymentId,
            string dataflowName)
        {
            lock (this) {
                return Instantiate(deploymentId, dataflowName, new EPDataFlowInstantiationOptions());
            }
        }

        public EPDataFlowInstance Instantiate(
            string deploymentId,
            string dataFlowName,
            EPDataFlowInstantiationOptions options)
        {
            lock (this) {
                DataflowDesc entry = GetEntryMayNull(deploymentId, dataFlowName);
                if (entry == null) {
                    throw new EPDataFlowInstantiationException(
                        "Data flow by name '" + dataFlowName + "' for deployment id '" + deploymentId +
                        "' has not been defined");
                }

                try {
                    agentInstanceNumCurrent++;
                    return DataflowInstantiator.Instantiate(agentInstanceNumCurrent, entry, options);
                }
                catch (Exception ex) {
                    string message = "Failed to instantiate data flow '" + dataFlowName + "': " + ex.Message;
                    Log.Debug(message, ex);
                    throw new EPDataFlowInstantiationException(message, ex);
                }
            }
        }

        public void SaveInstance(
            string instanceName,
            EPDataFlowInstance instance)
        {
            lock (this) {
                if (instances.ContainsKey(instanceName)) {
                    throw new EPDataFlowAlreadyExistsException(
                        "Data flow instance name '" + instanceName + "' already saved");
                }

                instances.Put(instanceName, instance);
            }
        }

        public string[] SavedInstances {
            get {
                lock (this) {
                    var instanceids = instances.Keys;
                    return instanceids.ToArray();
                }
            }
        }

        public EPDataFlowInstance GetSavedInstance(string instanceName)
        {
            lock (this) {
                return instances.Get(instanceName);
            }
        }

        public bool RemoveSavedInstance(string instanceName)
        {
            lock (this) {
                return instances.Delete(instanceName) != null;
            }
        }

        public void AddDataflow(
            string deploymentId,
            DataflowDesc dataflow)
        {
            lock (this) {
                DataflowDeployment deployment = deployments.Get(deploymentId);
                if (deployment == null) {
                    deployment = new DataflowDeployment();
                    deployments.Put(deploymentId, deployment);
                }

                deployment.Add(dataflow.DataflowName, dataflow);
            }
        }

        public void RemoveDataflow(
            string deploymentId,
            DataflowDesc dataflow)
        {
            lock (this) {
                DataflowDeployment deployment = deployments.Get(deploymentId);
                if (deployment == null) {
                    return;
                }

                deployment.Remove(dataflow.DataflowName);
            }
        }

        public void SaveConfiguration(
            string dataflowConfigName,
            string deploymentId,
            string dataFlowName,
            EPDataFlowInstantiationOptions options)
        {
            lock (this) {
                DataflowDesc entry = GetEntryMayNull(deploymentId, dataFlowName);
                if (entry == null) {
                    string message = "Failed to locate data flow '" + dataFlowName + "'";
                    throw new EPDataFlowNotFoundException(message);
                }

                if (configurationState.Exists(dataflowConfigName)) {
                    string message = "Data flow saved configuration by name '" + dataflowConfigName + "' already exists";
                    throw new EPDataFlowAlreadyExistsException(message);
                }

                configurationState.Add(
                    new EPDataFlowSavedConfiguration(dataflowConfigName, deploymentId, dataFlowName, options));
            }
        }

        public string[] SavedConfigurations {
            get {
                lock (this) {
                    return configurationState.SavedConfigNames;
                }
            }
        }

        public EPDataFlowSavedConfiguration GetSavedConfiguration(string configurationName)
        {
            lock (this) {
                return configurationState.GetSavedConfig(configurationName);
            }
        }

        public EPDataFlowInstance InstantiateSavedConfiguration(string configurationName)
        {
            lock (this) {
                EPDataFlowSavedConfiguration savedConfiguration = configurationState.GetSavedConfig(configurationName);
                if (savedConfiguration == null) {
                    throw new EPDataFlowInstantiationException(
                        "Dataflow saved configuration '" + configurationName + "' could not be found");
                }

                EPDataFlowInstantiationOptions options = savedConfiguration.Options;
                if (options == null) {
                    options = new EPDataFlowInstantiationOptions();
                    options.WithDataFlowInstanceId(configurationName);
                }

                return Instantiate(savedConfiguration.DeploymentId, savedConfiguration.DataflowName, options);
            }
        }

        public bool RemoveSavedConfiguration(string configurationName)
        {
            lock (this) {
                return configurationState.RemovePrototype(configurationName) != null;
            }
        }

        private DataflowDesc GetEntryMayNull(
            string deploymentId,
            string dataFlowName)
        {
            DataflowDeployment deployment = deployments.Get(deploymentId);
            return deployment == null ? null : deployment.GetDataflow(dataFlowName);
        }
    }
} // end of namespace