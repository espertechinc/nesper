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
    public class EPDataFlowEmitterNoTarget : EPDataFlowEmitter
    {
        internal readonly DataFlowSignalManager dataFlowSignalManager;

        internal readonly int operatorNum;

        public EPDataFlowEmitterNoTarget(int operatorNum, DataFlowSignalManager dataFlowSignalManager)
        {
            this.operatorNum = operatorNum;
            this.dataFlowSignalManager = dataFlowSignalManager;
        }

        public void Submit(object @object)
        {
        }

        public void SubmitSignal(EPDataFlowSignal signal)
        {
            dataFlowSignalManager.ProcessSignal(operatorNum, signal);
        }

        public void SubmitPort(int portNumber, object @object)
        {
        }
    }
} // end of namespace