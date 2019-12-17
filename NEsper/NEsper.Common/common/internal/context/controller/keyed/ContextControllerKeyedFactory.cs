///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.context;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.airegistry;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.controller.keyed
{
    public class ContextControllerKeyedFactory : ContextControllerFactoryBase
    {
        protected internal ContextControllerDetailKeyed keyedSpec;

        public ContextControllerDetailKeyed KeyedSpec {
            get => keyedSpec;
            set => keyedSpec = value;
        }

        public override ContextController Create(ContextManagerRealization contextManagerRealization)
        {
            return new ContextControllerKeyedImpl(this, contextManagerRealization);
        }

        public override FilterValueSetParam[][] PopulateFilterAddendum(
            FilterSpecActivatable filterSpec,
            bool forStatement,
            int nestingLevel,
            object partitionKey,
            ContextControllerStatementDesc optionalStatementDesc,
            AgentInstanceContext agentInstanceContextStatement)
        {
            if (!forStatement) {
                var found = false;
                var filters = keyedSpec.FilterSpecActivatables;
                foreach (var def in filters) {
                    if (EventTypeUtility.IsTypeOrSubTypeOf(filterSpec.FilterForEventType, def.FilterForEventType)) {
                        found = true;
                        break;
                    }
                }

                if (!found) {
                    return null;
                }
            }

            var includePartitionKey =
                forStatement || nestingLevel != FactoryEnv.NestingLevel; //factoryContext.NestingLevel;
            var getterKey = GetGetterKey(partitionKey);
            return ContextControllerKeyedUtil.GetAddendumFilters(
                getterKey,
                filterSpec,
                keyedSpec,
                includePartitionKey,
                optionalStatementDesc,
                agentInstanceContextStatement);
        }

        public override void PopulateContextProperties(
            IDictionary<string, object> props,
            object partitionKey)
        {
            if (!keyedSpec.HasAsName) {
                PopulateContextPropertiesAddKeyInfo(props, partitionKey);
                return;
            }

            var info = (ContextControllerKeyedPartitionKeyWInit) partitionKey;
            PopulateContextPropertiesAddKeyInfo(props, info.GetterKey);
            if (info.OptionalInitAsName != null) {
                props.Put(info.OptionalInitAsName, info.OptionalInitBean);
            }
        }

        public override StatementAIResourceRegistry AllocateAgentInstanceResourceRegistry(
            AIRegistryRequirements registryRequirements)
        {
            if (keyedSpec.OptionalTermination != null) {
                return AIRegistryUtil.AllocateRegistries(registryRequirements, AIRegistryFactoryMap.INSTANCE);
            }

            return AIRegistryUtil.AllocateRegistries(registryRequirements, AIRegistryFactoryMultiPerm.INSTANCE);
        }

        public override ContextPartitionIdentifier GetContextPartitionIdentifier(object partitionKey)
        {
            var getterKey = GetGetterKey(partitionKey);
            if (getterKey is object[]) {
                return new ContextPartitionIdentifierPartitioned((object[]) getterKey);
            }

            return new ContextPartitionIdentifierPartitioned(new[] {getterKey});
        }

        public object GetGetterKey(object partitionKey)
        {
            if (keyedSpec.HasAsName) {
                var info = (ContextControllerKeyedPartitionKeyWInit) partitionKey;
                return info.GetterKey;
            }

            return partitionKey;
        }

        private void PopulateContextPropertiesAddKeyInfo(
            IDictionary<string, object> props,
            object getterKey)
        {
            if (getterKey is HashableMultiKey) {
                var values = ((HashableMultiKey) getterKey).Keys;
                for (var i = 0; i < values.Length; i++) {
                    var propertyName = ContextPropertyEventType.PROP_CTX_KEY_PREFIX + (i + 1);
                    props.Put(propertyName, values[i]);
                }
            }
            else {
                props.Put(ContextPropertyEventType.PROP_CTX_KEY_PREFIX_SINGLE, getterKey);
            }
        }
    }
} // end of namespace