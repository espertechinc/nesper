///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.adapter
{
    /// <summary>
    /// Subscriptions are associated with an output adapter and dictate which events are
    /// sent to a given adapter.
    /// </summary>
    public interface Subscription
    {
        /// <summary>Returns the subscription name. </summary>
        /// <returns>subscription name</returns>
        string SubscriptionName { get; set; }

        /// <summary>Returns the type name of the event type we are looking for. </summary>
        /// <returns>event type name</returns>
        string EventTypeName { get; }

        /// <summary>Returns the output adapter this subscription is associated with. </summary>
        /// <returns>output adapter</returns>
        OutputAdapter Adapter { get; }

        /// <summary>Sets the output adapter this subscription is associated with. </summary>
        /// <param name="adapter">to set</param>
        void RegisterAdapter(OutputAdapter adapter);
    }
}
