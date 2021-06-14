///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.dataflow.core
{
    /// <summary>
    /// Collector for use with the EventBusSourceOp operator.
    /// </summary>
    public interface EPDataFlowEventBeanCollector
    {
        /// <summary>
        /// Collect: use the context to transform an event bean to a data flow event.
        /// </summary>
        /// <param name="context">contains event bean, emitter and related information</param>
        void Collect(EPDataFlowEventBeanCollectorContext context);
    }
}