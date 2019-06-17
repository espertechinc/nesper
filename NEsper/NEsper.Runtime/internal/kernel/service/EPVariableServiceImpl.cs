///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.context;
using com.espertech.esper.common.client.variable;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.context.util.StatementCPCacheService;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
    public class EPVariableServiceImpl : EPVariableServiceSPI
    {
        private readonly EPServicesContext services;

        public EPVariableServiceImpl(EPServicesContext services)
        {
            this.services = services;
        }

        public IDictionary<DeploymentIdNamePair, Type> VariableTypeAll {
            get {
                var variables = services.VariableManagementService.VariableReadersNonCP;
                IDictionary<DeploymentIdNamePair, Type> values = new Dictionary<DeploymentIdNamePair, Type>();
                foreach (var entry in variables) {
                    var type = entry.Value.MetaData.Type;
                    values.Put(entry.Key, type);
                }

                return values;
            }
        }

        public Type GetVariableType(
            string deploymentId,
            string variableName)
        {
            var metaData = services.VariableManagementService.GetVariableMetaData(deploymentId, variableName);
            if (metaData == null) {
                return null;
            }

            return metaData.MetaData.Type;
        }

        public object GetVariableValue(
            string deploymentId,
            string variableName)
        {
            services.VariableManagementService.SetLocalVersion();
            var metaData = services.VariableManagementService.GetVariableMetaData(deploymentId, variableName);
            if (metaData == null) {
                throw new VariableNotFoundException("Variable by name '" + variableName + "' has not been declared");
            }

            if (metaData.MetaData.OptionalContextName != null) {
                throw new VariableNotFoundException(
                    "Variable by name '" + variableName + "' has been declared for context '" + metaData.MetaData.OptionalContextName +
                    "' and cannot be read without context partition selector");
            }

            var reader = services.VariableManagementService.GetReader(deploymentId, variableName, DEFAULT_AGENT_INSTANCE_ID);
            var value = reader.Value;
            if (value == null || reader.MetaData.EventType == null) {
                return value;
            }

            return ((EventBean) value).Underlying;
        }

        public IDictionary<DeploymentIdNamePair, IList<ContextPartitionVariableState>> GetVariableValue(
            ISet<DeploymentIdNamePair> variableNames,
            ContextPartitionSelector contextPartitionSelector)
        {
            services.VariableManagementService.SetLocalVersion();
            string contextPartitionName = null;
            string contextDeploymentId = null;
            var variables = new Variable[variableNames.Count];
            var count = 0;
            foreach (var namePair in variableNames) {
                var variable = services.VariableManagementService.GetVariableMetaData(namePair.DeploymentId, namePair.Name);
                if (variable == null) {
                    throw new VariableNotFoundException("Variable by name '" + namePair.Name + "' has not been declared");
                }

                if (variable.MetaData.OptionalContextName == null) {
                    throw new VariableNotFoundException("Variable by name '" + namePair.Name + "' is a global variable and not context-partitioned");
                }

                if (contextPartitionName == null) {
                    contextPartitionName = variable.MetaData.OptionalContextName;
                    contextDeploymentId = variable.OptionalContextDeploymentId;
                }
                else {
                    if (!contextPartitionName.Equals(variable.MetaData.OptionalContextName) ||
                        !contextDeploymentId.Equals(variable.OptionalContextDeploymentId)) {
                        throw new VariableNotFoundException(
                            "Variable by name '" + namePair.Name + "' is a declared for context '" + variable.MetaData.OptionalContextName +
                            "' however the expected context is '" + contextPartitionName + "'");
                    }
                }

                variables[count++] = variable;
            }

            var contextManager = services.ContextManagementService.GetContextManager(contextDeploymentId, contextPartitionName);
            if (contextManager == null) {
                throw new VariableNotFoundException("Context by name '" + contextPartitionName + "' cannot be found");
            }

            var contextPartitions = contextManager.GetContextPartitions(contextPartitionSelector).Identifiers;
            if (contextPartitions.IsEmpty()) {
                return new EmptyDictionary<DeploymentIdNamePair, IList<ContextPartitionVariableState>>();
            }

            IDictionary<DeploymentIdNamePair, IList<ContextPartitionVariableState>> statesMap =
                new Dictionary<DeploymentIdNamePair, IList<ContextPartitionVariableState>>();
            count = 0;
            foreach (var pair in variableNames) {
                IList<ContextPartitionVariableState> states = new List<ContextPartitionVariableState>();
                var variable = variables[count++];
                statesMap.Put(pair, states);
                foreach (var entry in contextPartitions) {
                    var reader = services.VariableManagementService.GetReader(variable.DeploymentId, variable.MetaData.VariableName, entry.Key);
                    var value = reader.Value;
                    if (value != null && reader.MetaData.EventType != null) {
                        value = ((EventBean) value).Underlying;
                    }

                    states.Add(new ContextPartitionVariableState(entry.Key, entry.Value, value));
                }

                count++;
            }

            return statesMap;
        }

        public IDictionary<DeploymentIdNamePair, object> GetVariableValue(ISet<DeploymentIdNamePair> variableNames)
        {
            services.VariableManagementService.SetLocalVersion();
            IDictionary<DeploymentIdNamePair, object> values = new Dictionary<DeploymentIdNamePair, object>();
            foreach (var pair in variableNames) {
                var metaData = services.VariableManagementService.GetVariableMetaData(pair.DeploymentId, pair.Name);
                CheckVariable(pair.DeploymentId, pair.Name, metaData, false, false);
                var reader = services.VariableManagementService.GetReader(pair.DeploymentId, pair.Name, DEFAULT_AGENT_INSTANCE_ID);
                var value = reader.Value;
                if (value != null && reader.MetaData.EventType != null) {
                    value = ((EventBean) value).Underlying;
                }

                values.Put(pair, value);
            }

            return values;
        }

        public IDictionary<DeploymentIdNamePair, object> GetVariableValueAll()
        {
            services.VariableManagementService.SetLocalVersion();
            var variables = services.VariableManagementService.VariableReadersNonCP;
            IDictionary<DeploymentIdNamePair, object> values = new Dictionary<DeploymentIdNamePair, object>();
            foreach (KeyValuePair<DeploymentIdNamePair, VariableReader> entry in variables) {
                var value = entry.Value.Value;
                values.Put(entry.Key, value);
            }

            return values;
        }

        public void SetVariableValue(
            string deploymentId,
            string variableName,
            object variableValue)
        {
            var metaData = services.VariableManagementService.GetVariableMetaData(deploymentId, variableName);
            CheckVariable(deploymentId, variableName, metaData, true, false);

            using (services.VariableManagementService.ReadWriteLock.WriteLock.Acquire()) {
                services.VariableManagementService.CheckAndWrite(deploymentId, variableName, DEFAULT_AGENT_INSTANCE_ID, variableValue);
                services.VariableManagementService.Commit();
            }
        }

        public void SetVariableValue(IDictionary<DeploymentIdNamePair, object> variableValues)
        {
            SetVariableValueInternal(variableValues, DEFAULT_AGENT_INSTANCE_ID, false);
        }

        public void SetVariableValue(
            IDictionary<DeploymentIdNamePair, object> variableValues,
            int agentInstanceId)
        {
            SetVariableValueInternal(variableValues, agentInstanceId, true);
        }

        private void CheckVariable(
            string deploymentId,
            string variableName,
            Variable variable,
            bool settable,
            bool requireContextPartitioned)
        {
            if (variable == null) {
                if (deploymentId == null) {
                    throw new VariableNotFoundException("Variable by name '" + variableName + "' has not been declared");
                }

                throw new VariableNotFoundException(
                    "Variable by name '" + variableName + "' and deployment id '" + deploymentId + "' has not been declared");
            }

            var optionalContextName = variable.MetaData.OptionalContextName;
            if (!requireContextPartitioned) {
                if (optionalContextName != null) {
                    throw new VariableNotFoundException(
                        "Variable by name '" + variableName + "' has been declared for context '" + optionalContextName +
                        "' and cannot be set without context partition selectors");
                }
            }
            else {
                if (optionalContextName == null) {
                    throw new VariableNotFoundException("Variable by name '" + variableName + "' is a global variable and not context-partitioned");
                }
            }

            if (settable && variable.MetaData.IsConstant) {
                throw new VariableConstantValueException(
                    "Variable by name '" + variableName + "' is declared as constant and may not be assigned a new value");
            }
        }

        private void SetVariableValueInternal(
            IDictionary<DeploymentIdNamePair, object> variableValues,
            int agentInstanceId,
            bool requireContextPartitioned)
        {
            // verify
            foreach (KeyValuePair<DeploymentIdNamePair, object> entry in variableValues) {
                var deploymentId = entry.Key.DeploymentId;
                var variableName = entry.Key.Name;
                var metaData = services.VariableManagementService.GetVariableMetaData(deploymentId, variableName);
                CheckVariable(deploymentId, variableName, metaData, true, requireContextPartitioned);
            }

            // set values
            using (services.VariableManagementService.ReadWriteLock.WriteLock.Acquire()) {
                foreach (KeyValuePair<DeploymentIdNamePair, object> entry in variableValues) {
                    var deploymentId = entry.Key.DeploymentId;
                    var variableName = entry.Key.Name;
                    try {
                        services.VariableManagementService.CheckAndWrite(deploymentId, variableName, agentInstanceId, entry.Value);
                    }
                    catch (Exception ) {
                        services.VariableManagementService.Rollback();
                        throw;
                    }
                }

                services.VariableManagementService.Commit();
            }
        }
    }
} // end of namespace