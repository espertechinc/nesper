///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client.context;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.context.aifactory.createwindow;
using com.espertech.esper.common.@internal.context.airegistry;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.context.controller.hash
{
    public class ContextControllerHashFactory : ContextControllerFactoryBase
    {
        protected internal ContextControllerDetailHash hashSpec;

        public ContextControllerDetailHash HashSpec {
            get => hashSpec;
            set => hashSpec = value;
        }

        public override ContextController Create(ContextManagerRealization contextManagerRealization)
        {
            return new ContextControllerHashImpl(this, contextManagerRealization);
        }

        public override FilterValueSetParam[][] PopulateFilterAddendum(
            FilterSpecActivatable filterSpec, bool forStatement, int nestingLevel, object partitionKey,
            ContextControllerStatementDesc optionalStatementDesc, AgentInstanceContext agentInstanceContextStatement)
        {
            // determine whether create-named-window
            var isCreateWindow = optionalStatementDesc != null &&
                                 optionalStatementDesc.Lightweight.StatementContext.StatementInformationals
                                     .StatementType == StatementType.CREATE_WINDOW;
            ContextControllerDetailHashItem foundPartition = null;
            var hashCode = partitionKey.AsInt();

            if (!isCreateWindow) {
                foundPartition = FindHashItemSpec(hashSpec, filterSpec);
            }
            else {
                var factory = (StatementAgentInstanceFactoryCreateNW) optionalStatementDesc.Lightweight.StatementContext
                    .StatementAIFactoryProvider.Factory;
                var declaredAsName = factory.AsEventTypeName;
                foreach (var partitionItem in hashSpec.Items) {
                    if (partitionItem.FilterSpecActivatable.FilterForEventType.Name.Equals(declaredAsName)) {
                        foundPartition = partitionItem;
                        break;
                    }
                }
            }

            if (foundPartition == null) {
                return null;
            }

            FilterValueSetParam filter = new FilterValueSetParamImpl(
                foundPartition.Lookupable, FilterOperator.EQUAL, hashCode);

            var addendum = new FilterValueSetParam[1][];
            addendum[0] = new[] {filter};

            var partitionFilters = foundPartition.FilterSpecActivatable.GetValueSet(
                null, null, agentInstanceContextStatement, agentInstanceContextStatement.StatementContextFilterEvalEnv);
            if (partitionFilters != null) {
                addendum = FilterAddendumUtil.AddAddendum(partitionFilters, filter);
            }

            return addendum;
        }

        public override void PopulateContextProperties(IDictionary<string, object> props, object allPartitionKey)
        {
            // nothing to populate
        }

        public override StatementAIResourceRegistry AllocateAgentInstanceResourceRegistry(
            AIRegistryRequirements registryRequirements)
        {
            if (hashSpec.Granularity <= 65536) {
                return AIRegistryUtil.AllocateRegistries(registryRequirements, AIRegistryFactoryMultiPerm.INSTANCE);
            }

            return AIRegistryUtil.AllocateRegistries(registryRequirements, AIRegistryFactoryMap.INSTANCE);
        }

        public override ContextPartitionIdentifier GetContextPartitionIdentifier(object partitionKey)
        {
            return new ContextPartitionIdentifierHash(partitionKey.AsInt());
        }

        private static ContextControllerDetailHashItem FindHashItemSpec(
            ContextControllerDetailHash hashSpec, FilterSpecActivatable filterSpec)
        {
            ContextControllerDetailHashItem foundPartition = null;
            foreach (var partitionItem in hashSpec.Items) {
                var typeOrSubtype = EventTypeUtility.IsTypeOrSubTypeOf(
                    filterSpec.FilterForEventType, partitionItem.FilterSpecActivatable.FilterForEventType);
                if (typeOrSubtype) {
                    foundPartition = partitionItem;
                }
            }

            return foundPartition;
        }
    }
} // end of namespace