///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.@join.exec.outer;

namespace com.espertech.esper.common.@internal.epl.join.rep
{
    /// <summary>
    /// This class supplies position information for <seealso cref="LookupInstructionExec" />
    /// to use for iterating over events for lookup.
    /// </summary>
    public class Cursor
    {
        /// <summary>Ctor. </summary>
        /// <param name="theEvent">is the current event</param>
        /// <param name="stream">is the current stream</param>
        /// <param name="node">is the node containing the set of events to which the event belongs to</param>
        public Cursor(
            EventBean theEvent,
            int stream,
            Node node)
        {
            Event = theEvent;
            Stream = stream;
            Node = node;
        }

        /// <summary>Supplies current event. </summary>
        /// <value>event</value>
        public EventBean Event { get; private set; }

        /// <summary>Returns current stream the event belongs to. </summary>
        /// <value>stream number for event</value>
        public int Stream { get; private set; }

        /// <summary>Returns current result node the event belong to. </summary>
        /// <value>result node of event</value>
        public Node Node { get; private set; }
    }
}