///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.client
{
	/// <summary>
    /// Interface to iterate over events.
    /// <para>
    /// A concurrency-unsafe iterator over events representing statement results (pull API).
    /// </para>
    /// <para>
    /// The iterator is useful for applications that are single-threaded, or that coordinate the iterating thread
    /// with event processing threads that use the sendEvent method using application code locks or synchronization.
    /// </para>
    /// <para>
    /// The iterator returned by this method does not make any guarantees towards correctness of
    /// results and fail-behavior, if your application processes events into the engine instance
    /// using the sendEvent method by multiple threads.
    /// </para>
    /// <para>
    /// Use the safeIterator method for concurrency-safe iteration. Note the safe iterator requires
    /// applications to explicitly close the safe iterator when done iterating.
    /// </para>
    /// </summary>
    /// <returns>event iterator</returns>

    public interface EPIterable : IEnumerable<EventBean>
    {
        /// <summary> Returns the type of events the iterable returns.</summary>
        /// <returns> event type of events the iterator returns
        /// </returns>

        EventType EventType { get; }

        /// <summary>
        /// Returns a concurrency-safe iterator that iterates over events representing statement results (pull API)
        /// in the face of concurrent event processing by further threads.
        /// <para>
        /// In comparison to the regular iterator, the safe iterator guarantees correct results even
        /// as events are being processed by other threads. The cost is that the iterator holds
        /// one or more locks that must be released. Any locks are acquired at the time this method
        /// is called.
        /// </para>
        /// <para>
        /// This method is a blocking method. It may block until statement processing locks are released
        /// such that the safe iterator can acquire any required locks.
        /// </para>
        /// <para>
        /// An application MUST explicitly close the safe iterator instance using the close method, to release locks held by the
        /// iterator. The call to the close method should be done in a finally block to make sure
        /// the iterator gets closed.
        /// </para>
        /// <para>
        /// Multiple safe iterators may be not be used at the same time by different application threads.
        /// A single application thread may hold and use multiple safe iterators however this is discouraged.
        /// </para>
        /// </summary>
        /// <returns>
        /// safe iterator;
        /// </returns>
        IEnumerator<EventBean> GetSafeEnumerator();
    }
}
