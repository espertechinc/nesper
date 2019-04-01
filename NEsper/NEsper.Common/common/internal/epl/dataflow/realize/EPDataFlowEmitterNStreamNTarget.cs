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
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.dataflow.realize
{
    public class EPDataFlowEmitterNStreamNTarget : EPDataFlowEmitter
    {
        private readonly SubmitHandler[][] handlersPerStream;

        private readonly int operatorNum;
        private readonly DataFlowSignalManager signalManager;

        public EPDataFlowEmitterNStreamNTarget(
            int operatorNum, DataFlowSignalManager signalManager, SubmitHandler[][] handlersPerStream)
        {
            this.operatorNum = operatorNum;
            this.signalManager = signalManager;
            this.handlersPerStream = handlersPerStream;
        }

        public void Submit(object @object)
        {
            throw new UnsupportedOperationException("Submit to a specific port is excepted");
        }

        public void SubmitSignal(EPDataFlowSignal signal)
        {
            signalManager.ProcessSignal(operatorNum, signal);
            foreach (var handlerArr in handlersPerStream) {
                foreach (var handler in handlerArr) {
                    handler.HandleSignal(signal);
                }
            }
        }

        public void SubmitPort(int portNumber, object @object)
        {
            var targets = handlersPerStream[portNumber];
            foreach (var handler in targets) {
                handler.SubmitInternal(@object);
            }
        }
    }
} // end of namespace