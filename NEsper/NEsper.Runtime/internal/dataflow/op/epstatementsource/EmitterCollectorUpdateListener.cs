///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.runtime.client;

namespace com.espertech.esper.runtime.@internal.dataflow.op.epstatementsource
{
    public class EmitterCollectorUpdateListener : UpdateListener
    {
        private readonly EPDataFlowIRStreamCollector collector;
        private readonly LocalEmitter emitterForCollector;
        private readonly bool submitEventBean;

        public EmitterCollectorUpdateListener(EPDataFlowIRStreamCollector collector, LocalEmitter emitterForCollector, bool submitEventBean)
        {
            this.collector = collector;
            this.emitterForCollector = emitterForCollector;
            this.submitEventBean = submitEventBean;
        }

        public void Update(
            EventBean[] newEvents,
            EventBean[] oldEvents)
        {
            throw new System.NotImplementedException();
        }

        public void Update(EventBean[] newEvents, EventBean[] oldEvents, EPStatement statement, EPRuntime runtime)
        {
            EPDataFlowIRStreamCollectorContext holder = new EPDataFlowIRStreamCollectorContext(emitterForCollector, submitEventBean, newEvents, oldEvents, statement, runtime);
            collector.Collect(holder);
        }
    }
} // end of namespace