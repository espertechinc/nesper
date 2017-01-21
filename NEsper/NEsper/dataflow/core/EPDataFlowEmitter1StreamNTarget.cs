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
    public class EPDataFlowEmitter1StreamNTarget : EPDataFlowEmitter
    {
        private readonly int _operatorNum;
        private readonly DataFlowSignalManager _signalManager;
        private readonly SubmitHandler[] _targets;

        public EPDataFlowEmitter1StreamNTarget(int operatorNum, DataFlowSignalManager signalManager, SubmitHandler[] targets)
        {
            _operatorNum = operatorNum;
            _signalManager = signalManager;
            _targets = targets;
        }

        public void Submit(Object @object)
        {
            foreach (SubmitHandler handler in _targets)
            {
                handler.SubmitInternal(@object);
            }
        }

        public void SubmitSignal(EPDataFlowSignal signal)
        {
            _signalManager.ProcessSignal(_operatorNum, signal);
            foreach (SubmitHandler handler in _targets)
            {
                handler.HandleSignal(signal);
            }
        }

        public void SubmitPort(int portNumber, Object @object)
        {
        }
    }
}
