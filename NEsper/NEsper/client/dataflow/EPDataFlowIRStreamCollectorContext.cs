///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.dataflow.interfaces;

namespace com.espertech.esper.client.dataflow
{
    /// <summary>
    /// Context for use with <seealso cref="EPDataFlowIRStreamCollector" />. &lt;p&gt; Do not retain a handle of this object as its contents are subject to change. &lt;/p&gt;
    /// </summary>
    public class EPDataFlowIRStreamCollectorContext
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="emitter">data flow emitter</param>
        /// <param name="submitEventBean">indicator whether the EventBean or the underlying event object must be submmitted</param>
        /// <param name="newEvents">insert stream events</param>
        /// <param name="oldEvents">remove stream events</param>
        /// <param name="statement">statement posting events</param>
        /// <param name="epServiceProvider">engine instances</param>
        public EPDataFlowIRStreamCollectorContext(EPDataFlowEmitter emitter, bool submitEventBean, EventBean[] newEvents, EventBean[] oldEvents, EPStatement statement, EPServiceProvider epServiceProvider)
        {
            Emitter = emitter;
            IsSubmitEventBean = submitEventBean;
            NewEvents = newEvents;
            OldEvents = oldEvents;
            Statement = statement;
            ServiceProvider = epServiceProvider;
        }

        /// <summary>Returns the emitter. </summary>
        /// <value>emitter</value>
        public EPDataFlowEmitter Emitter { get; private set; }

        /// <summary>Returns insert stream. </summary>
        /// <value>events</value>
        public EventBean[] NewEvents { get; set; }

        /// <summary>Returns remove stream. </summary>
        /// <value>events</value>
        public EventBean[] OldEvents { get; set; }

        /// <summary>Returns the statement. </summary>
        /// <value>statement</value>
        public EPStatement Statement { get; internal set; }

        /// <summary>Returns the engine instance. </summary>
        /// <value>engine instance</value>
        public EPServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// Returns indicator whether to submit wrapped events (EventBean) or underlying events
        /// </summary>
        /// <value>wrapped event indicator</value>
        public bool IsSubmitEventBean { get; private set; }
    }
}
