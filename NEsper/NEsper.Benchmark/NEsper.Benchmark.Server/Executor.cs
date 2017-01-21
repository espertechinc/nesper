///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System.Threading;

namespace NEsper.Benchmark.Server
{
    /// <summary>
    /// Performs a work item on behalf of the caller.  However, the
    /// semantics of how the executor works are hidden from the caller.
    /// </summary>

    public interface Executor
    {
        /// <summary>
        /// Gets the thread count.
        /// </summary>
        /// <value>The thread count.</value>
        int ThreadCount { get; }
        /// <summary>
        /// Gets the queue depth.
        /// </summary>
        /// <value>The queue depth.</value>
        int QueueDepth { get; }
        /// <summary>
        /// Executes the specified work item.
        /// </summary>
        /// <param name="workItem">The work item.</param>
        void Execute(WaitCallback workItem);
    }
}
