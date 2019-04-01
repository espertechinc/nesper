///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.dataflow.interfaces;

namespace com.espertech.esper.common.client.dataflow.core
{
    /// <summary>
    /// For use with <seealso cref="EPDataFlowEventBeanCollector" /> provides collection context. &lt;p&gt; Do not retain handles to this instance as its contents may change. &lt;/p&gt;
    /// </summary>
    public class EPDataFlowEventBeanCollectorContext
    {
        /// <summary>Ctor. </summary>
        /// <param name="emitter">to emit into the data flow</param>
        /// <param name="submitEventBean">indicator whether to submit EventBean or underlying events</param>
        /// <param name="theEvent">to process</param>
        public EPDataFlowEventBeanCollectorContext(EPDataFlowEmitter emitter, bool submitEventBean, EventBean theEvent)
        {
            Emitter = emitter;
            IsSubmitEventBean = submitEventBean;
            Event = theEvent;
        }

        /// <summary>Returns the event to process. </summary>
        /// <value>event</value>
        public EventBean Event { get; set; }

        /// <summary>Returns the emitter. </summary>
        /// <value>emitter</value>
        public EPDataFlowEmitter Emitter { get; private set; }

        /// <summary>Returns true to submit EventBean instances, false to submit underlying event. </summary>
        /// <value>indicator whether wrapper required or not</value>
        public bool IsSubmitEventBean { get; private set; }
    }
}
