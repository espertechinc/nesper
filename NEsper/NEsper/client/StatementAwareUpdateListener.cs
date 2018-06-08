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
    /// Defines an interface to notify of new and old events.
    /// Also see <see cref="UpdateListener"/> for Update listeners that only
    /// require event delivery.
    /// </summary>
    [Obsolete]
    public interface StatementAwareUpdateListener
    {
        /// <summary>
        /// Notify that new events are available or old events are removed.
        /// <para>
        /// If the call to Update contains new (inserted) events, then the first argument will be a non-empty list and
        /// the second will be empty. Similarly, if the call is a notification of deleted events, then the first argument
        /// will be empty and the second will be non-empty.
        /// </para>
        /// <para>
        /// Either the newEvents or oldEvents will be non-null. This method won't be called with both arguments being null,
        /// (unless using output rate limiting or force-output options),
        /// but either one could be null. The same is true for zero-length arrays.
        /// Either newEvents or oldEvents will be non-empty. If both are non-empty, then the Update is a modification
        /// notification.
        /// </para>
        /// </summary>
        /// <param name="newEvents">
        /// is any new events. This will be null or empty if the Update is for old events only.
        /// </param>
        /// <param name="oldEvents">
        /// is any old events. This will be null or empty if the Update is for new events only.
        /// </param>
        /// <param name="statement">is the statement producing the result</param>
        /// <param name="svcProvider">
        /// is the engine instance that provided the administrative API that created the statement which produces the result
        /// </param>
        void Update(EventBean[] newEvents, EventBean[] oldEvents, EPStatement statement, EPServiceProvider svcProvider);
    }
}
