///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.client.dataflow.util;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.runtime.@internal.dataflow.op.eventbussink
{
    public class EventBusSinkFactory : DataFlowOperatorFactory
    {
        public EventBeanAdapterFactory[] AdapterFactories { get; private set; }

        public IDictionary<string, object> Collector { get; set; }

        public EventType[] EventTypes { get; set; }

        public void InitializeFactory(DataFlowOpFactoryInitializeContext context)
        {
            AdapterFactories = new EventBeanAdapterFactory[EventTypes.Length];
            for (var i = 0; i < EventTypes.Length; i++) {
                if (EventTypes[i] != null) {
                    AdapterFactories[i] = EventTypeUtility.GetAdapterFactoryForType(
                        EventTypes[i], context.StatementContext.EventBeanTypedEventFactory,
                        context.StatementContext.EventTypeAvroHandler);
                }
            }
        }

        public DataFlowOperator Operator(DataFlowOpInitializeContext context)
        {
            var collectorInstance = DataFlowParameterResolution.ResolveOptionalInstance<EPDataFlowEventCollector>(
                "collector", Collector, context);
            return new EventBusSinkOp(this, context.AgentInstanceContext, collectorInstance);
        }
    }
} // end of namespace