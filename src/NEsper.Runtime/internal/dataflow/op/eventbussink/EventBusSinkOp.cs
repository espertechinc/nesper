///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;

namespace com.espertech.esper.runtime.@internal.dataflow.op.eventbussink
{
    public class EventBusSinkOp : DataFlowOperatorLifecycle
    {
        private readonly EventBusSinkFactory factory;
        private readonly AgentInstanceContext agentInstanceContext;
        private readonly EPDataFlowEventCollector collector;

        public EventBusSinkOp(EventBusSinkFactory factory, AgentInstanceContext agentInstanceContext, EPDataFlowEventCollector collector)
        {
            this.factory = factory;
            this.agentInstanceContext = agentInstanceContext;
            this.collector = collector;
        }

        public void OnInput(int port, object data)
        {
            if (collector != null)
            {
                EPDataFlowEventCollectorContext holder = new EPDataFlowEventCollectorContext(agentInstanceContext.EPRuntimeSendEvent, data);
                collector.Collect(holder);
            }
            else
            {
                if (data is EventBean)
                {
                    agentInstanceContext.EPRuntimeEventProcessWrapped.ProcessWrappedEvent((EventBean) data);
                }
                else
                {
                    EventBean @event = factory.AdapterFactories[port].MakeAdapter(data);
                    agentInstanceContext.EPRuntimeEventProcessWrapped.ProcessWrappedEvent(@event);
                }
            }
        }

        public void Open(DataFlowOpOpenContext openContext)
        {
            // no action
        }

        public void Close(DataFlowOpCloseContext openContext)
        {
            // no action
        }
    }
} // end of namespace