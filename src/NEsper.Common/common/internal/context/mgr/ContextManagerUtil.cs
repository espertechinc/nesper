///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.mgr
{
    public class ContextManagerUtil
    {
        public static IList<AgentInstance> GetAgentInstances(
            ContextControllerStatementDesc statement,
            ICollection<int> agentInstanceIds)
        {
            var statementContext = statement.Lightweight.StatementContext;
            IList<AgentInstance> instances = new List<AgentInstance>();
            foreach (var id in agentInstanceIds) {
                var agentInstance = GetAgentInstance(statementContext, id);
                instances.Add(agentInstance);
            }

            return instances;
        }

        public static AgentInstance GetAgentInstance(
            StatementContext statementContext,
            int agentInstanceId)
        {
            var holder =
                statementContext.StatementCPCacheService.MakeOrGetEntryCanNull(agentInstanceId, statementContext);
            return new AgentInstance(holder.AgentInstanceStopCallback, holder.AgentInstanceContext, holder.FinalView);
        }

        public static IList<AgentInstance> GetAgentInstancesFiltered(
            ContextControllerStatementDesc statement,
            ICollection<int> agentInstanceIds,
            Func<AgentInstance, bool> filter)
        {
            var statementContext = statement.Lightweight.StatementContext;
            IList<AgentInstance> instances = new List<AgentInstance>();
            foreach (var id in agentInstanceIds) {
                var agentInstance = GetAgentInstance(statementContext, id);
                if (filter.Invoke(agentInstance)) {
                    instances.Add(agentInstance);
                }
            }

            return instances;
        }

        public static IDictionary<FilterSpecActivatable, FilterValueSetParam[][]> ComputeAddendumForStatement(
            ContextControllerStatementDesc statementDesc,
            IDictionary<int, ContextControllerStatementDesc> statements,
            ContextControllerFactory[] controllerFactories,
            object[] allPartitionKeys,
            AgentInstanceContext agentInstanceContextCreate)
        {
            var filters =
                statementDesc.Lightweight.StatementContext.FilterSpecActivatables;
            IDictionary<FilterSpecActivatable, FilterValueSetParam[][]> map =
                new IdentityDictionary<FilterSpecActivatable, FilterValueSetParam[][]>();
            foreach (var filter in filters) {
                var addendum = ComputeAddendum(
                    allPartitionKeys,
                    filter.Value,
                    true,
                    statementDesc,
                    controllerFactories,
                    statements,
                    agentInstanceContextCreate);
                if (addendum != null && addendum.Length > 0) {
                    map.Put(filter.Value, addendum);
                }
            }

            return map;
        }

        public static FilterValueSetParam[][] ComputeAddendumNonStmt(
            object[] partitionKeys,
            FilterSpecActivatable filterCallback,
            ContextManagerRealization realization)
        {
            return ComputeAddendum(
                partitionKeys,
                filterCallback,
                false,
                null,
                realization.ContextManager.ContextDefinition.ControllerFactories,
                realization.ContextManager.Statements,
                realization.AgentInstanceContextCreate);
        }

        private static FilterValueSetParam[][] ComputeAddendum(
            object[] parentPartitionKeys,
            FilterSpecActivatable filterCallback,
            bool forStatement,
            ContextControllerStatementDesc optionalStatementDesc,
            ContextControllerFactory[] controllerFactories,
            IDictionary<int, ContextControllerStatementDesc> statements,
            AgentInstanceContext agentInstanceContextCreate)
        {
            var result = Array.Empty<FilterValueSetParam[]>();
            for (var i = 0; i < parentPartitionKeys.Length; i++) {
                var addendumForController = controllerFactories[i]
                    .PopulateFilterAddendum(
                        filterCallback,
                        forStatement,
                        i + 1,
                        parentPartitionKeys[i],
                        optionalStatementDesc,
                        statements,
                        agentInstanceContextCreate);
                result = FilterAddendumUtil.MultiplyAddendum(result, addendumForController);
            }

            return result;
        }

        public static MappedEventBean BuildContextProperties(
            int agentInstanceId,
            object[] allPartitionKeys,
            ContextDefinition contextDefinition,
            StatementContext statementContextCreate)
        {
            var props = BuildContextPropertiesMap(agentInstanceId, allPartitionKeys, contextDefinition);
            return statementContextCreate.EventBeanTypedEventFactory.AdapterForTypedMap(
                props,
                contextDefinition.EventTypeContextProperties);
        }

        private static IDictionary<string, object> BuildContextPropertiesMap(
            int agentInstanceId,
            object[] allPartitionKeys,
            ContextDefinition contextDefinition)
        {
            IDictionary<string, object> props = new Dictionary<string, object>();
            props.Put(ContextPropertyEventType.PROP_CTX_NAME, contextDefinition.ContextName);
            props.Put(ContextPropertyEventType.PROP_CTX_ID, agentInstanceId);

            var controllerFactories = contextDefinition.ControllerFactories;

            if (controllerFactories.Length == 1) {
                controllerFactories[0].PopulateContextProperties(props, allPartitionKeys[0]);
                return props;
            }

            for (var level = 0; level < controllerFactories.Length; level++) {
                var nestedContextName = controllerFactories[level].FactoryEnv.ContextName;
                IDictionary<string, object> nestedProps = new Dictionary<string, object>();
                nestedProps.Put(ContextPropertyEventType.PROP_CTX_NAME, nestedContextName);
                if (level == controllerFactories.Length - 1) {
                    nestedProps.Put(ContextPropertyEventType.PROP_CTX_ID, agentInstanceId);
                }

                controllerFactories[level].PopulateContextProperties(nestedProps, allPartitionKeys[level]);
                props.Put(nestedContextName, nestedProps);
            }

            return props;
        }
    }
} // end of namespace