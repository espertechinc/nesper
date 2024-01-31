///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.context;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.context.mgr
{
    public class ContextStateEventUtil
    {
        public static void DispatchContext<T>(
            CopyOnWriteList<ContextStateListener> listeners,
            Supplier<T> supplier,
            BiConsumer<ContextStateListener, T> consumer)
        {
            if (listeners.IsEmpty()) {
                return;
            }

            var @event = supplier.Invoke();
            foreach (var listener in listeners) {
                consumer.Invoke(listener, @event);
            }
        }

        public static void DispatchPartition<T>(
            CopyOnWriteList<ContextPartitionStateListener> listeners,
            Supplier<T> supplier,
            BiConsumer<ContextPartitionStateListener, T> consumer)
        {
            if (listeners == null || listeners.IsEmpty()) {
                return;
            }

            var @event = supplier.Invoke();
            foreach (var listener in listeners) {
                consumer.Invoke(listener, @event);
            }
        }
    }
} // end of namespace