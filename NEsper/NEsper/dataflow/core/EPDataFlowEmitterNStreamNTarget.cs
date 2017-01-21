///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client.dataflow;
using com.espertech.esper.compat;
using com.espertech.esper.dataflow.interfaces;
using com.espertech.esper.dataflow.util;

namespace com.espertech.esper.dataflow.core
{
    public class EPDataFlowEmitterNStreamNTarget : EPDataFlowEmitter
    {
        private readonly int _operatorNum;
        private readonly DataFlowSignalManager _signalManager;
        private readonly SubmitHandler[][] _handlersPerStream;

        public EPDataFlowEmitterNStreamNTarget(int operatorNum, DataFlowSignalManager signalManager, SubmitHandler[][] handlersPerStream)
        {
            _operatorNum = operatorNum;
            _signalManager = signalManager;
            _handlersPerStream = handlersPerStream;
        }

        public void Submit(Object @object)
        {
            throw new UnsupportedOperationException("Submit to a specific port is excepted");
        }

        public void SubmitSignal(EPDataFlowSignal signal)
        {
            _signalManager.ProcessSignal(_operatorNum, signal);
            foreach (SubmitHandler[] handlerArr in _handlersPerStream)
            {
                foreach (SubmitHandler handler in handlerArr)
                {
                    handler.HandleSignal(signal);
                }
            }
        }

        public void SubmitPort(int portNumber, Object @object)
        {
            SubmitHandler[] targets = _handlersPerStream[portNumber];
            foreach (SubmitHandler handler in targets)
            {
                handler.SubmitInternal(@object);
            }
        }
    }
}
