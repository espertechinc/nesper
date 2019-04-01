///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client
{
    /// <summary>
    /// Receives notification from an engine that an event that has been sent into the engine or that
    /// has been generated via insert-into has not been matched to any statement, considering all started statement's
    /// event stream filter criteria (not considering where-clause and having-clauses).
    /// </summary>
    /// <see cref="EPRuntime"/>
	public delegate void UnmatchedListener(EventBean theEvent);

    public class UnmatchedEventArgs : EventArgs
    {
        public EventBean Event { get; private set; }

        public UnmatchedEventArgs(EventBean theEvent)
        {
            Event = theEvent;
        }
    }
} // End of namespace
