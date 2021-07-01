///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.aifactory.@select;
using com.espertech.esper.common.@internal.context.airegistry;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.runtime.@internal.dataflow.op.select
{
    public class SelectFactory : DataFlowOperatorFactory
    {
        public EventBeanAdapterFactory[] AdapterFactories { get; private set; }

        public StatementAIResourceRegistry ResourceRegistry { get; private set; }

        public EventType[] EventTypes { get; set; }

        public StatementAIFactoryProvider FactoryProvider { get; set; }

        public StatementAgentInstanceFactorySelect FactorySelect { get; private set; }

        public bool IsSubmitEventBean { get; set; }

        public bool IsIterate { get; set; }

        public int[] OriginatingStreamToViewableStream { get; set; }

        public void InitializeFactory(DataFlowOpFactoryInitializeContext context)
        {
            var ha = context.StatementContext.ViewFactoryService != ViewFactoryServiceImpl.INSTANCE;
            if (ha) {
                throw new EPException("The select-operator is not supported in the HA environment");
            }

            AdapterFactories = new EventBeanAdapterFactory[EventTypes.Length];
            for (var i = 0; i < EventTypes.Length; i++) {
                if (EventTypes[i] != null) {
                    AdapterFactories[i] = EventTypeUtility.GetAdapterFactoryForType(
                        EventTypes[i], context.StatementContext.EventBeanTypedEventFactory,
                        context.StatementContext.EventTypeAvroHandler);
                }
            }

            FactorySelect = (StatementAgentInstanceFactorySelect) FactoryProvider.Factory;
            var registryRequirements = FactorySelect.RegistryRequirements;
            ResourceRegistry = AIRegistryUtil.AllocateRegistries(registryRequirements, AIRegistryFactoryMap.INSTANCE);
            FactoryProvider.Assign(new StatementAIFactoryAssignmentContext(ResourceRegistry));
        }

        public DataFlowOperator Operator(DataFlowOpInitializeContext context)
        {
            return new SelectOp(this, context.AgentInstanceContext);
        }
    }
} // end of namespace