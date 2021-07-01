///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.client.dataflow.util;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.filterspec;

namespace com.espertech.esper.runtime.@internal.dataflow.op.eventbussource
{
    public class EventBusSourceFactory : DataFlowOperatorFactory
    {
        public bool IsSubmitEventBean { get; set; }

        public IDictionary<string, object> Collector { get; set; }

        public FilterSpecActivatable FilterSpecActivatable { get; set; }

        public void InitializeFactory(DataFlowOpFactoryInitializeContext context)
        {
        }

        public DataFlowOperator Operator(DataFlowOpInitializeContext context)
        {
            var collectorInstance = DataFlowParameterResolution.ResolveOptionalInstance<EPDataFlowEventBeanCollector>(
                "collector", Collector, context);
            return new EventBusSourceOp(this, context.AgentInstanceContext, collectorInstance);
        }
    }
} // end of namespace