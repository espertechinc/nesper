///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading.threadlocal;

namespace com.espertech.esper.common.@internal.statement.dispatch
{
    /// <summary>
    /// Implements dispatch service using a thread-local linked list of Dispatchable instances.
    /// </summary>
    public class DispatchService
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IThreadLocal<ArrayDeque<Dispatchable>> dispatchStateThreadLocal =
            new SystemThreadLocal<ArrayDeque<Dispatchable>>(() => new ArrayDeque<Dispatchable>());

        public IThreadLocal<ArrayDeque<Dispatchable>> DispatchStateThreadLocal => dispatchStateThreadLocal;

        public void Dispatch()
        {
            DispatchFromQueue(dispatchStateThreadLocal.Value);
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

        private static void DispatchFromQueue(Deque<Dispatchable> dispatchQueue)
        {
            if (dispatchQueue != null) {
                using (new Tracer(Log, "DispatchFromQueue")) {
                    while (true) {
                        var next = dispatchQueue.Poll();
                        if (next == null) {
                            break;
                        }

                        next.Execute();
                    }
                }
            }
        }
        
        public void RemoveAll(UpdateDispatchView updateDispatchView) {
            var dispatchables = dispatchStateThreadLocal.GetOrCreate();
            dispatchables.RemoveWhere(
                dispatchable => dispatchable.View == updateDispatchView,
                dispatchable => dispatchable.Cancelled());
        }
    }
} // end of namespace