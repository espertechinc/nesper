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
using com.espertech.esper.epl.core;

using XLR8.CGLib;

namespace com.espertech.esper.dataflow.core
{
    public abstract class EPDataFlowEmitter1Stream1TargetBase : EPDataFlowEmitter, SubmitHandler
    {
        protected readonly int OperatorNum;
        protected readonly DataFlowSignalManager SignalManager;
        protected readonly SignalHandler SignalHandler;
        protected readonly EPDataFlowEmitterExceptionHandler ExceptionHandler;

        private readonly FastMethod _fastMethod;
        protected object TargetObject;

        protected EPDataFlowEmitter1Stream1TargetBase(
            int operatorNum,
            DataFlowSignalManager signalManager,
            SignalHandler signalHandler,
            EPDataFlowEmitterExceptionHandler exceptionHandler,
            ObjectBindingPair target,
            EngineImportService engineImportService)
        {
            OperatorNum = operatorNum;
            SignalManager = signalManager;
            SignalHandler = signalHandler;
            ExceptionHandler = exceptionHandler;

            var fastClass = FastClass.Create(target.Target.GetType());
            _fastMethod = fastClass.GetMethod(target.Binding.ConsumingBindingDesc.Method);
            TargetObject = target.Target;

        }

        public abstract void SubmitInternal(Object @object);

        public void Submit(Object @object)
        {
            SubmitInternal(@object);
        }

        public void SubmitSignal(EPDataFlowSignal signal)
        {
            SignalManager.ProcessSignal(OperatorNum, signal);
            SignalHandler.HandleSignal(signal);
        }

        public void HandleSignal(EPDataFlowSignal signal)
        {
            SignalHandler.HandleSignal(signal);
        }

        public void SubmitPort(int portNumber, Object @object)
        {
            if (portNumber == 0)
            {
                Submit(@object);
            }
        }

        public FastMethod FastMethod
        {
            get { return _fastMethod; }
        }
    }
}
