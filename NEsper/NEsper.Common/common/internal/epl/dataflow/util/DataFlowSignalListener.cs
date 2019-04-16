///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client.dataflow.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.dataflow.util
{
    public interface DataFlowSignalListener
    {
        void ProcessSignal(EPDataFlowSignal signal);
    }

    public class ProxyDataFlowSignalListener : DataFlowSignalListener
    {
        public Action<EPDataFlowSignal> ProcProcessSignal { get; set; }

        public ProxyDataFlowSignalListener()
        {
        }

        public ProxyDataFlowSignalListener(Action<EPDataFlowSignal> procProcessSignal)
        {
            ProcProcessSignal = procProcessSignal;
        }

        public void ProcessSignal(EPDataFlowSignal signal)
        {
            ProcProcessSignal?.Invoke(signal);
        }
    }
} // end of namespace