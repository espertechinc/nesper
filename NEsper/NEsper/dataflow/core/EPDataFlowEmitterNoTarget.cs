///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client.dataflow;
using com.espertech.esper.dataflow.interfaces;
using com.espertech.esper.dataflow.util;

namespace com.espertech.esper.dataflow.core
{
    public class EPDataFlowEmitterNoTarget : EPDataFlowEmitter
    {
        protected readonly int OperatorNum;
        protected readonly DataFlowSignalManager DataFlowSignalManager;

        public EPDataFlowEmitterNoTarget(int operatorNum, DataFlowSignalManager dataFlowSignalManager)
        {
            OperatorNum = operatorNum;
            DataFlowSignalManager = dataFlowSignalManager;
        }

        public void Submit(Object @object)
        {
        }

        public void SubmitSignal(EPDataFlowSignal signal)
        {
            DataFlowSignalManager.ProcessSignal(OperatorNum, signal);
        }

        public void SubmitPort(int portNumber, Object @object)
        {
        }
    }
}
