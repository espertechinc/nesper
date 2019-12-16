///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.context;
using com.espertech.esper.common.@internal.context.airegistry;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.context.util.ContextPropertyEventType;

namespace com.espertech.esper.common.@internal.context.controller.category
{
    public class ContextControllerCategoryFactory : ContextControllerFactoryBase
    {
        public ContextControllerDetailCategory CategorySpec { get; set; }

        public override ContextController Create(ContextManagerRealization contextManagerRealization)
        {
            return new ContextControllerCategoryImpl(contextManagerRealization, this);
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
                if (!EventTypeUtility.IsTypeOrSubTypeOf(
                    filterSpec.FilterForEventType,
                    CategorySpec.FilterSpecActivatable.FilterForEventType)) {
                    return null;
                }
            }

            int categoryNum = partitionKey.AsInt32();
            ContextControllerDetailCategoryItem item = CategorySpec.Items[categoryNum];
            return FilterSpecActivatable.EvaluateValueSet(
                item.CompiledFilterParam,
                null,
                agentInstanceContextStatement);
        }

        public override void PopulateContextProperties(
            IDictionary<string, object> props,
            object allPartitionKey)
        {
            ContextControllerDetailCategoryItem item = CategorySpec.Items[allPartitionKey.AsInt32()];
            props.Put(PROP_CTX_LABEL, item.Name);
        }

        public override StatementAIResourceRegistry AllocateAgentInstanceResourceRegistry(
            AIRegistryRequirements registryRequirements)
        {
            return AIRegistryUtil.AllocateRegistries(registryRequirements, AIRegistryFactoryMultiPerm.INSTANCE);
        }

        public override ContextPartitionIdentifier GetContextPartitionIdentifier(object partitionKey)
        {
            int categoryNum = partitionKey.AsInt32();
            ContextControllerDetailCategoryItem item = CategorySpec.Items[categoryNum];
            return new ContextPartitionIdentifierCategory(item.Name);
        }
    }
} // end of namespace