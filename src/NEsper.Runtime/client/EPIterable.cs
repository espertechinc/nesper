///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.context;
using com.espertech.esper.common.client.util;

namespace com.espertech.esper.runtime.client
{
    /// <summary>
    ///     Interface to iterate over events.
    /// </summary>
    public interface EPIterable : IEnumerable<EventBean>
    {
#if DEPRECATED
        /// <summary>
        ///     Returns a concurrency-unsafe enumerator over events representing statement results (pull API).
        ///     <para>
        ///         The enumerator is useful for applications that are single-threaded, or that coordinate the iterating thread
        ///         with event processing threads that use the sendEvent method using application code locks or synchronization.
        ///     </para>
        ///     <para>
        ///         The enumerator returned by this method does not make any guarantees towards correctness of
        ///         results and fail-behavior, if your application processes events into the runtime instance
        ///         using the sendEvent method by multiple threads.
        ///     </para>
        ///     <para>
        ///         Use the SafeEnumerator method for concurrency-safe iteration. Note the safe enumerator requires
        ///         applications to explicitly close the safe enumerator when done iterating.
        ///     </para>
        /// </summary>
        /// <returns>event enumerator</returns>
        IEnumerator<EventBean> GetEnumerator();
#endif

        /// <summary>
        ///     Returns a concurrency-safe enumerator that iterates over events representing statement results (pull API)
        ///     in the face of concurrent event processing by further threads.
        ///     <para>
        ///         In comparison to the regular enumerator, the safe enumerator guarantees correct results even
        ///         as events are being processed by other threads. The cost is that the enumerator holds
        ///         one or more locks that must be released via the close method. Any locks are acquired
        ///         at the time this method is called.
        ///     </para>
        ///     <para>
        ///         This method is a blocking method. It may block until statement processing locks are released
        ///         such that the safe enumerator can acquire any required locks.
        ///     </para>
        ///     <para>
        ///         An application MUST explicitly close the safe enumerator instance using the close method, to release locks held
        ///         by the
        ///         enumerator. The call to the close method should be done in a finally block to make sure
        ///         the enumerator gets closed.
        ///     </para>
        ///     <para>
        ///         Multiple safe enumerators may be not be used at the same time by different application threads.
        ///         A single application thread may hold and use multiple safe enumerators however this is discouraged.
        ///     </para>
        /// </summary>
        /// <returns>safe enumerator; NOTE: Must use the close method to close the safe enumerator, preferably in a finally block</returns>
        SafeEnumerator<EventBean> GetSafeEnumerator();

        /// <summary>
        ///     For use with statements that have a context declared and that may therefore have multiple context partitions,
        ///     allows to iterate over context partitions selectively.
        /// </summary>
        /// <param name="selector">selects context partitions to consider</param>
        /// <returns>enumerator</returns>
        IEnumerator<EventBean> GetEnumerator(ContextPartitionSelector selector);

        /// <summary>
        ///     For use with statements that have a context declared and that may therefore have multiple context partitions,
        ///     allows to safe-iterate over context partitions selectively.
        /// </summary>
        /// <param name="selector">selects context partitions to consider</param>
        /// <returns>safe enumerator</returns>
        SafeEnumerator<EventBean> GetSafeEnumerator(ContextPartitionSelector selector);
    }
} // end of namespace