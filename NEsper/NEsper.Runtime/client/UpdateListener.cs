///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;

namespace com.espertech.esper.runtime.client
{
    /// <summary>
    ///     Defines an interface to notify of new and old events.
    ///     <para>
    ///         Also see <see cref="StatementAwareUpdateListener" /> for Update listeners that require
    ///         the statement and service provider instance to be passed to the listener in addition
    ///         to events.
    ///     </para>
    /// </summary>
    public interface UpdateListener
    {
        /// <summary>
        ///     Notify that new events are available or old events are removed.
        ///     <para>
        ///         If the call to Update contains new (inserted) events, then the first argument will be a non-empty list and
        ///         the second will be empty. Similarly, if the call is a notification of deleted events, then the first argument
        ///         will be empty and the second will be non-empty.
        ///     </para>
        ///     <para>
        ///         Either the newEvents or oldEvents will be non-null. This method won't be called with both arguments being null,
        ///         (unless using output rate limiting or force-output options),
        ///         but either one could be null. The same is true for zero-length arrays.
        ///     </para>
        ///     <para>
        ///         Either newEvents or oldEvents will be non-empty. This method won't be called with both arguments being null
        ///         (unless using output rate limiting or force-output options),
        ///         but either one could be null. The same is true for zero-length arrays.
        ///         Either newEvents or oldEvents will be non-empty.If both are non-empty, then the update is a modification
        ///     </para>
        /// </summary>
        
        void Update(
            object sender,
            UpdateEventArgs eventArgs);

#if DEPRECATED
        void Update(
            EventBean[] newEvents,
            EventBean[] oldEvents,
            EPStatement statement,
            EPRuntime runtime);
#endif
    }
}