///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.dataflow.util;

namespace com.espertech.esper.dataflow.core
{
    public class EPDataFlowEmitter1Stream1TargetUnwind
        : EPDataFlowEmitter1Stream1TargetBase
    {
        public EPDataFlowEmitter1Stream1TargetUnwind(int operatorNum, DataFlowSignalManager signalManager, SignalHandler signalHandler, EPDataFlowEmitterExceptionHandler exceptionHandler, ObjectBindingPair target)
            : base(operatorNum, signalManager, signalHandler, exceptionHandler, target)
        {
        }

        public override void SubmitInternal(Object @object)
        {
            var parameters = (Object[]) @object;
            try
            {
                ExceptionHandler.HandleAudit(TargetObject, parameters);
                FastMethod.Invoke(TargetObject, parameters);
            }
            catch (Exception e)
            {
                ExceptionHandler.HandleException(TargetObject, FastMethod, e, parameters);
            }
        }
    }
}
