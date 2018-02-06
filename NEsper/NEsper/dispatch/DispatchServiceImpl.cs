///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.compat;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.util;

namespace com.espertech.esper.dispatch
{
    /// <summary>
    /// Implements dispatch service using a thread-local linked list of Dispatchable instances.
    /// </summary>
    public class DispatchServiceImpl : DispatchService
    {
        private readonly IThreadLocal<Queue<Dispatchable>> _threadDispatchQueue;

        /// <summary>
        /// Initializes a new instance of the <see cref="DispatchServiceImpl"/> class.
        /// </summary>
        public DispatchServiceImpl(IThreadLocalManager threadLocalManager)
        {
            _threadDispatchQueue = threadLocalManager.Create(
                () => new Queue<Dispatchable>());
        }

        /// <summary>
        /// Dispatches events in the queue.
        /// </summary>

        public void Dispatch()
        {
            DispatchFromQueue(_threadDispatchQueue.Value);
        }

        public void DispatchFromQueue(Queue<Dispatchable> dispatchQueue)
        {
            if (dispatchQueue == null)
            {
                return;
            }

            if (IsDebugEnabled)
            {
                Log.Debug(".DispatchFromQueue Dispatch queue is " + dispatchQueue.Count + " elements");
            }

            try
            {
                int count = dispatchQueue.Count;
                while (--count >= 0)
                {
                    dispatchQueue.Dequeue().Execute();
                }
            }
            catch (InvalidOperationException)
            {
            }
        }

        /// <summary>
        /// Add an item to be dispatched.  The item is added to
        /// the external dispatch queue.
        /// </summary>
        /// <param name="dispatchable">to execute later</param>
        public void AddExternal(Dispatchable dispatchable)
        {
            _threadDispatchQueue
                .GetOrCreate()
                .Enqueue(dispatchable);
        }

        private static readonly bool IsDebugEnabled;

        static DispatchServiceImpl()
        {
            IsDebugEnabled =
                ExecutionPathDebugLog.IsEnabled &&
                ExecutionPathDebugLog.IsTimerDebugEnabled &&
                Log.IsDebugEnabled;
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
