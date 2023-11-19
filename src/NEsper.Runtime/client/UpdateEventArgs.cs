///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client;

namespace com.espertech.esper.runtime.client
{
    public class UpdateEventArgs : EventArgs
    {
        public UpdateEventArgs(
            EPRuntime runtime,
            EPStatement statement)
        {
            Runtime = runtime;
            Statement = statement;
        }

        public UpdateEventArgs(
            EPRuntime runtime,
            EPStatement statement,
            EventBean[] newEvents,
            EventBean[] oldEvents)
        {
            Runtime = runtime;
            Statement = statement;
            NewEvents = newEvents;
            OldEvents = oldEvents;
        }

        /// <summary>
        ///     Gets the runtime.
        /// </summary>
        /// <value>
        ///     The runtime.
        /// </value>
        public EPRuntime Runtime { get; }

        /// <summary>
        ///     Gets the statement.
        /// </summary>
        /// <value>
        ///     The statement.
        /// </value>
        public EPStatement Statement { get; }

        /// <summary>
        ///     Gets the new events. This will be null or empty if the Update is for old events only.
        /// </summary>
        /// <value>
        ///     The new events.
        /// </value>
        public EventBean[] NewEvents { get; }

        /// <summary>
        ///     Gets the old events.  This will be null or empty if the Update is for new events only.
        /// </summary>
        /// <value>
        ///     The old events.
        /// </value>
        public EventBean[] OldEvents { get; }
    }
}