///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.client.dataflow;

namespace com.espertech.esper.dataflow.util
{
    public interface DataFlowSignalListener
    {
        void ProcessSignal(EPDataFlowSignal signal);
    }

    public class ProxyDataFlowSignalListener : DataFlowSignalListener
    {
        public Action<EPDataFlowSignal> ProcSignal { get; set; }

        public ProxyDataFlowSignalListener()
        {
        }

        public ProxyDataFlowSignalListener(Action<EPDataFlowSignal> procSignal)
        {
            ProcSignal = procSignal;
        }

        public void ProcessSignal(EPDataFlowSignal signal)
        {
            ProcSignal.Invoke(signal);
        }
    }
}
