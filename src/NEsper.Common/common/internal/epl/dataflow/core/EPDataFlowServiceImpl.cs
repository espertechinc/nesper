///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.container;

namespace com.espertech.esper.common.@internal.epl.dataflow.core
{
    public class EPDataFlowServiceImpl : EPDataFlowService
    {
        public const string OP_PACKAGE_NAME = "com.espertech.esper.runtime.@internal.dataflow.op";

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IDictionary<string, DataflowDeployment> _deployments =
            new Dictionary<string, DataflowDeployment>();

        private readonly IDictionary<string, EPDataFlowInstance> _instances =
            new Dictionary<string, EPDataFlowInstance>();

        private readonly DataFlowConfigurationStateService _configurationState =
            new DataFlowConfigurationStateServiceImpl();

        private readonly IContainer _container;
        private int _agentInstanceNumCurrent;

        /// <summary>
        /// Instantiate the service.
        /// </summary>
        /// <param name="container"></param>
        public EPDataFlowServiceImpl(IContainer container)
        {
            _container = container;
        }

        public EPDataFlowDescriptor GetDataFlow(
            string deploymentId,
            string dataflowName)
        {
            lock (this) {
                var entry = GetEntryMayNull(deploymentId, dataflowName);
                return entry == null
                    ? null
                    : new EPDataFlowDescriptor(entry.DataflowName, entry.StatementContext.StatementName);
            }
        }

        public DeploymentIdNamePair[] DataFlows {
            get {
                lock (this) {
                    IList<DeploymentIdNamePair> ids = new List<DeploymentIdNamePair>();
                    foreach (var deployment in _deployments) {
                        foreach (var entry in deployment.Value.Dataflows) {
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
                var entry = GetEntryMayNull(deploymentId, dataFlowName);
                if (entry == null) {
                    throw new EPDataFlowInstantiationException(
                        "Data flow by name '" +
                        dataFlowName +
                        "' for deployment id '" +
                        deploymentId +
                        "' has not been defined");
                }

                try {
                    _agentInstanceNumCurrent++;
                    return DataflowInstantiator.Instantiate(_container, _agentInstanceNumCurrent, entry, options);
                }
                catch (Exception ex) {
                    var message = "Failed to instantiate data flow '" + dataFlowName + "': " + ex.Message;
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
                if (_instances.ContainsKey(instanceName)) {
                    throw new EPDataFlowAlreadyExistsException(
                        "Data flow instance name '" + instanceName + "' already saved");
                }

                _instances.Put(instanceName, instance);
            }
        }

        public string[] SavedInstances {
            get {
                lock (this) {
                    var instanceids = _instances.Keys;
                    return instanceids.ToArray();
                }
            }
        }

        public EPDataFlowInstance GetSavedInstance(string instanceName)
        {
            lock (this) {
                return _instances.Get(instanceName);
            }
        }

        public bool RemoveSavedInstance(string instanceName)
        {
            lock (this) {
                return _instances.Delete(instanceName) != null;
            }
        }

        public void AddDataflow(
            string deploymentId,
            DataflowDesc dataflow)
        {
            lock (this) {
                var deployment = _deployments.Get(deploymentId);
                if (deployment == null) {
                    deployment = new DataflowDeployment();
                    _deployments.Put(deploymentId, deployment);
                }

                deployment.Add(dataflow.DataflowName, dataflow);
            }
        }

        public void RemoveDataflow(
            string deploymentId,
            DataflowDesc dataflow)
        {
            lock (this) {
                var deployment = _deployments.Get(deploymentId);
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
                var entry = GetEntryMayNull(deploymentId, dataFlowName);
                if (entry == null) {
                    var message = "Failed to locate data flow '" + dataFlowName + "'";
                    throw new EPDataFlowNotFoundException(message);
                }

                if (_configurationState.Exists(dataflowConfigName)) {
                    var message = "Data flow saved configuration by name '" +
                                  dataflowConfigName +
                                  "' already exists";
                    throw new EPDataFlowAlreadyExistsException(message);
                }

                _configurationState.Add(
                    new EPDataFlowSavedConfiguration(dataflowConfigName, deploymentId, dataFlowName, options));
            }
        }

        public string[] SavedConfigurations {
            get {
                lock (this) {
                    return _configurationState.SavedConfigNames;
                }
            }
        }

        public EPDataFlowSavedConfiguration GetSavedConfiguration(string configurationName)
        {
            lock (this) {
                return _configurationState.GetSavedConfig(configurationName);
            }
        }

        public EPDataFlowInstance InstantiateSavedConfiguration(string configurationName)
        {
            lock (this) {
                var savedConfiguration = _configurationState.GetSavedConfig(configurationName);
                if (savedConfiguration == null) {
                    throw new EPDataFlowInstantiationException(
                        "Dataflow saved configuration '" + configurationName + "' could not be found");
                }

                var options = savedConfiguration.Options;
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
                return _configurationState.RemovePrototype(configurationName) != null;
            }
        }

        private DataflowDesc GetEntryMayNull(
            string deploymentId,
            string dataFlowName)
        {
            var deployment = _deployments.Get(deploymentId);
            return deployment?.GetDataflow(dataFlowName);
        }
    }
} // end of namespace