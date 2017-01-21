///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;

namespace com.espertech.esper.view
{
    /// <summary>
    /// Interface that marks an event collection. Every event in the event collection must be 
    /// of the same event type, as defined by the EventType property.
    /// </summary>
    public interface EventCollection : IEnumerable<EventBean>
    {
        /// <summary> Provides metadata information about the type of object the event collection contains.</summary>
        /// <returns> metadata for the objects in the collection
        /// </returns>

        EventType EventType { get; }
    }
}
