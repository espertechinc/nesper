///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.common.client.util
{
    /// <summary>
    ///     A concurrency-safe iterator that iterates over events representing statement results (pull API)
    ///     in the face of concurrent event processing by further threads.
    ///     <para>
    ///         In comparison to the regular iterator, the safe iterator guarantees correct results even
    ///         as events are being processed by other threads. The cost is that the iterator holds
    ///         one or more locks that must be released via the close method. Any locks are acquired
    ///         at the time an instance is created.
    ///     </para>
    ///     <para>
    ///         NOTE: An application MUST explicitly close the safe iterator instance using the close method, to release locks
    ///         held by the
    ///         iterator. The call to the close method should be done in a finally block to make sure
    ///         the iterator gets closed.
    ///     </para>
    ///     <para>
    ///         Multiple safe iterators may be not be used at the same time by different application threads.
    ///         A single application thread may hold and use multiple safe iterators however this is discouraged.
    ///     </para>
    /// </summary>
    public interface SafeEnumerator<E> : IEnumerator<E>
    {
        /// <summary>
        ///     Close the safe enumerator, releasing locks. This is a required call and should
        ///     preferably occur in a finally block.
        /// </summary>
        void Close();
    }
} // end of namespace