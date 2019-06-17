///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.runtime.client
{
    /// <summary>
    ///     Interface to add and remove update listeners.
    /// </summary>
    public interface EPListenable
    {
        /// <summary>
        ///     Returns an iterator of update listeners.
        ///     <para>The returned iterator does not allow the remove operation.</para>
        /// </summary>
        /// <returns>iterator of update listeners</returns>
        IEnumerator<UpdateListener> UpdateListeners { get; }

        /// <summary>
        ///     Add a listener that observes events.
        /// </summary>
        /// <param name="listener">to add</param>
        /// <throws>IllegalStateException when attempting to add a listener to a destroyed statement</throws>
        void AddListener(UpdateListener listener);

        /// <summary>
        ///     Remove a listener that observes events.
        /// </summary>
        /// <param name="listener">to remove</param>
        void RemoveListener(UpdateListener listener);

        /// <summary>
        ///     Remove all listeners.
        /// </summary>
        void RemoveAllListeners();

        /// <summary>
        ///     Add an update listener replaying current statement results to the listener.
        ///     <para>
        ///         The listener receives current statement results as the first call to the update method
        ///         of the listener, passing in the newEvents parameter the current statement results as an array of zero or more
        ///         events.
        ///         Subsequent calls to the update method of the listener are statement results.
        ///     </para>
        ///     <para>
        ///         Current statement results are the events returned by the iterator or safeIterator methods.
        ///     </para>
        ///     <para>
        ///         Delivery of current statement results in the first call is performed by the same thread invoking this method,
        ///         while subsequent calls to the listener may deliver statement results by the same or other threads.
        ///     </para>
        ///     <para>
        ///         Note: this is a blocking call, delivery is atomic: Events occurring during iteration and
        ///         delivery to the listener are guaranteed to be delivered in a separate call and not lost.
        ///         The listener implementation should minimize long-running or blocking operations.
        ///     </para>
        ///     <para>
        ///         Delivery is only atomic relative to the current statement. If the same listener instance is
        ///         registered with other statements it may receive other statement results simultaneously.
        ///     </para>
        ///     <para>
        ///         If a statement is not started an therefore does not have current results, the listener
        ///         receives a single invocation with a null value in newEvents.
        ///     </para>
        /// </summary>
        /// <param name="listener">to add</param>
        void AddListenerWithReplay(UpdateListener listener);
    }
} // end of namespace