///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.dataflow.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.dataflow.util
{
    public class DataFlowSignalManager
    {
        private IDictionary<int, IList<DataFlowSignalListener>> listenersPerOp =
            new Dictionary<int, IList<DataFlowSignalListener>>();

        public void ProcessSignal(
            int operatorNum,
            EPDataFlowSignal signal)
        {
            var listeners = listenersPerOp.Get(operatorNum);
            if (listeners == null || listeners.IsEmpty()) {
                return;
            }

            foreach (var listener in listeners) {
                listener.ProcessSignal(signal);
            }
        }

        public void AddSignalListener(
            int producerOpNum,
            DataFlowSignalListener listener)
        {
            var listeners = listenersPerOp.Get(producerOpNum);
            if (listeners == null) {
                listeners = new List<DataFlowSignalListener>();
                listenersPerOp.Put(producerOpNum, listeners);
            }

            listeners.Add(listener);
        }
    }
} // end of namespace