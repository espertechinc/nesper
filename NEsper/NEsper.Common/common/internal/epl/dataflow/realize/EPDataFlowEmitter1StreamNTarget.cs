///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.dataflow.util;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.epl.dataflow.util;

namespace com.espertech.esper.common.@internal.epl.dataflow.realize
{
    public class EPDataFlowEmitter1StreamNTarget : EPDataFlowEmitter
    {
        private readonly int operatorNum;
        private readonly DataFlowSignalManager signalManager;
        private readonly SubmitHandler[] targets;

        public EPDataFlowEmitter1StreamNTarget(
            int operatorNum, DataFlowSignalManager signalManager, SubmitHandler[] targets)
        {
            this.operatorNum = operatorNum;
            this.signalManager = signalManager;
            this.targets = targets;
        }

        public void Submit(object @object)
        {
            foreach (var handler in targets) {
                handler.SubmitInternal(@object);
            }
        }

        public void SubmitSignal(EPDataFlowSignal signal)
        {
            signalManager.ProcessSignal(operatorNum, signal);
            foreach (var handler in targets) {
                handler.HandleSignal(signal);
            }
        }

        public void SubmitPort(int portNumber, object @object)
        {
        }
    }
} // end of namespace