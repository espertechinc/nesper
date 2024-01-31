///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.dataflow.interfaces;

namespace com.espertech.esper.common.client.dataflow.core
{
    /// <summary>
    ///     Context for use with <seealso cref="EPDataFlowIRStreamCollector" />.
    ///     <para>
    ///         Do not retain a handle of this object as its contents are subject to change.
    ///     </para>
    /// </summary>
    public class EPDataFlowIRStreamCollectorContext
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="emitter">data flow emitter</param>
        /// <param name="submitEventBean">indicator whether the EventBean or the underlying event object must be submmitted</param>
        /// <param name="newEvents">insert stream events</param>
        /// <param name="oldEvents">remove stream events</param>
        /// <param name="statement">statement posting events</param>
        /// <param name="runtime">runtime instance</param>
        public EPDataFlowIRStreamCollectorContext(
            EPDataFlowEmitter emitter,
            bool submitEventBean,
            EventBean[] newEvents,
            EventBean[] oldEvents,
            object statement,
            object runtime)
        {
            Emitter = emitter;
            IsSubmitEventBean = submitEventBean;
            NewEvents = newEvents;
            OldEvents = oldEvents;
            Statement = statement;
            Runtime = runtime;
        }

        /// <summary>
        ///     Returns the emitter.
        /// </summary>
        /// <returns>emitter</returns>
        public EPDataFlowEmitter Emitter { get; }

        /// <summary>
        ///     Returns insert stream.
        /// </summary>
        /// <returns>events</returns>
        public EventBean[] NewEvents { get; }

        /// <summary>
        ///     Returns remove stream.
        /// </summary>
        /// <returns>events</returns>
        public EventBean[] OldEvents { get; }

        /// <summary>
        ///     Returns the statement and can safely be cast to EPStatement when needed (typed object to not require a dependency on
        ///     runtime)
        /// </summary>
        /// <returns>statement</returns>
        public object Statement { get; }

        /// <summary>
        ///     Returns the runtime instance and can safely be cast to EPRuntime when needed (typed object to not require a
        ///     dependency on runtime)
        /// </summary>
        /// <returns>runtime instance</returns>
        public object Runtime { get; }

        /// <summary>
        ///     Returns indicator whether to submit wrapped events (EventBean) or underlying events
        /// </summary>
        /// <returns>wrapped event indicator</returns>
        public bool IsSubmitEventBean { get; }
    }
} // end of namespace