///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client.dataflow;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.dataflow.util
{
    public class DataFlowSignalManager
    {
        private readonly IDictionary<int, IList<DataFlowSignalListener>> _listenersPerOp =
            new Dictionary<int, IList<DataFlowSignalListener>>();

        public void ProcessSignal(int operatorNum, EPDataFlowSignal signal)
        {
            IList<DataFlowSignalListener> listeners = _listenersPerOp.Get(operatorNum);
            if (listeners == null || listeners.IsEmpty())
            {
                return;
            }
            foreach (DataFlowSignalListener listener in listeners)
            {
                listener.ProcessSignal(signal);
            }
        }

        public void AddSignalListener(int producerOpNum, DataFlowSignalListener listener)
        {
            IList<DataFlowSignalListener> listeners = _listenersPerOp.Get(producerOpNum);
            if (listeners == null)
            {
                listeners = new List<DataFlowSignalListener>();
                _listenersPerOp.Put(producerOpNum, listeners);
            }
            listeners.Add(listener);
        }
    }
}