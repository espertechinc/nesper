///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;

using com.espertech.esper.common.client.dataflow.util;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.epl.dataflow.util;
using com.espertech.esper.common.@internal.settings;

namespace com.espertech.esper.common.@internal.epl.dataflow.realize
{
    public abstract class EPDataFlowEmitter1Stream1TargetBase : EPDataFlowEmitter,
        SubmitHandler
    {
        internal readonly EPDataFlowEmitterExceptionHandler exceptionHandler;
        internal readonly MethodInfo fastMethod;
        internal readonly object targetObject;

        private readonly int operatorNum;
        private readonly SignalHandler signalHandler;
        private readonly DataFlowSignalManager signalManager;

        public EPDataFlowEmitter1Stream1TargetBase(
            int operatorNum,
            DataFlowSignalManager signalManager,
            SignalHandler signalHandler,
            EPDataFlowEmitterExceptionHandler exceptionHandler,
            ObjectBindingPair target,
            ImportService importService)
        {
            this.operatorNum = operatorNum;
            this.signalManager = signalManager;
            this.signalHandler = signalHandler;
            this.exceptionHandler = exceptionHandler;

            fastMethod = target.Binding.ConsumingBindingDesc.Method;
            targetObject = target.Target;
        }

        public void Submit(object @object)
        {
            SubmitInternal(@object);
        }

        public void SubmitSignal(EPDataFlowSignal signal)
        {
            signalManager.ProcessSignal(operatorNum, signal);
            signalHandler.HandleSignal(signal);
        }

        public void SubmitPort(
            int portNumber,
            object @object)
        {
            if (portNumber == 0) {
                Submit(@object);
            }
        }

        public abstract void SubmitInternal(object @object);

        public void HandleSignal(EPDataFlowSignal signal)
        {
            signalHandler.HandleSignal(signal);
        }

        public MethodInfo FastMethod => fastMethod;
    }
} // end of namespace