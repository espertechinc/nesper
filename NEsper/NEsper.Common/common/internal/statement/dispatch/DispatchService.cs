///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;
using com.espertech.esper.compat.threading.threadlocal;

namespace com.espertech.esper.common.@internal.statement.dispatch
{
    /// <summary>
    /// Implements dispatch service using a thread-local linked list of Dispatchable instances.
    /// </summary>
    public class DispatchService
    {
        private readonly IThreadLocal<ArrayDeque<Dispatchable>> dispatchStateThreadLocal =
            new SlimThreadLocal<ArrayDeque<Dispatchable>>(() => new ArrayDeque<Dispatchable>());

        public IThreadLocal<ArrayDeque<Dispatchable>> DispatchStateThreadLocal => dispatchStateThreadLocal;

        public void Dispatch()
        {
            DispatchFromQueue(dispatchStateThreadLocal.GetOrCreate());
        }

        public void AddExternal(Dispatchable dispatchable)
        {
            ArrayDeque<Dispatchable> dispatchQueue = dispatchStateThreadLocal.GetOrCreate();
            AddToQueue(dispatchable, dispatchQueue);
        }

        private static void AddToQueue(
            Dispatchable dispatchable,
            ICollection<Dispatchable> dispatchQueue)
        {
            dispatchQueue.Add(dispatchable);
        }

        private static void DispatchFromQueue(ArrayDeque<Dispatchable> dispatchQueue)
        {
            while (true) {
                var next = dispatchQueue.Poll();
                if (next != null) {
                    next.Execute();
                }
                else {
                    break;
                }
            }
        }
    }
} // end of namespace