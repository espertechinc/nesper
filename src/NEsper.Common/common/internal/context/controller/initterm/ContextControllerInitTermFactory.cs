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
using com.espertech.esper.common.@internal.context.controller.condition;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.controller.initterm
{
    public class ContextControllerInitTermFactory : ContextControllerFactoryBase
    {
        protected internal ContextControllerDetailInitiatedTerminated initTermSpec;

        public ContextControllerDetailInitiatedTerminated InitTermSpec {
            get => initTermSpec;
            set => initTermSpec = value;
        }

        public override ContextController Create(ContextManagerRealization contextManagerRealization)
        {
            if (initTermSpec.IsOverlapping) {
                return new ContextControllerInitTermOverlap(this, contextManagerRealization);
            }

            return new ContextControllerInitTermNonOverlap(this, contextManagerRealization);
        }

        public override FilterValueSetParam[][] PopulateFilterAddendum(
            FilterSpecActivatable filterSpec,
            bool forStatement,
            int nestingLevel,
            object partitionKey,
            ContextControllerStatementDesc optionalStatementDesc,
            IDictionary<int, ContextControllerStatementDesc> statements,
            AgentInstanceContext agentInstanceContextStatement)
        {
            // none
            return null;
        }

        public override void PopulateContextProperties(
            IDictionary<string, object> props,
            object partitionKey)
        {
            var key = (ContextControllerInitTermPartitionKey) partitionKey;
            props.Put(ContextPropertyEventType.PROP_CTX_STARTTIME, key.StartTime);
            props.Put(ContextPropertyEventType.PROP_CTX_ENDTIME, key.ExpectedEndTime);

            var filter = initTermSpec.StartCondition as ContextConditionDescriptorFilter;
            if (filter?.OptionalFilterAsName != null) {
                props.Put(filter.OptionalFilterAsName, key.TriggeringEvent);
            }

            if (initTermSpec.StartCondition is ContextConditionDescriptorPattern) {
                var pattern = key.TriggeringPattern;
                if (pattern != null) {
                    props.PutAll(pattern);
                }
            }
        }

        public override StatementAIResourceRegistry AllocateAgentInstanceResourceRegistry(
            AIRegistryRequirements registryRequirements)
        {
            if (initTermSpec.IsOverlapping) {
                return AIRegistryUtil.AllocateRegistries(registryRequirements, AIRegistryFactoryMap.INSTANCE);
            }

            return AIRegistryUtil.AllocateRegistries(registryRequirements, AIRegistryFactorySingle.INSTANCE);
        }

        public override ContextPartitionIdentifier GetContextPartitionIdentifier(object partitionKey)
        {
            var key = (ContextControllerInitTermPartitionKey) partitionKey;
            var ident = new ContextPartitionIdentifierInitiatedTerminated();
            ident.StartTime = key.StartTime;
            ident.EndTime = key.ExpectedEndTime;

            var filter = initTermSpec.StartCondition as ContextConditionDescriptorFilter;
            if (filter?.OptionalFilterAsName != null) {
                ident.Properties = Collections.SingletonDataMap(filter.OptionalFilterAsName, key.TriggeringEvent);
            }

            return ident;
        }
    }
} // end of namespace