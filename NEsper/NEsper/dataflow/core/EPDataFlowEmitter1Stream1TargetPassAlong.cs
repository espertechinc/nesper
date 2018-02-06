///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.compat;
using com.espertech.esper.dataflow.util;
using com.espertech.esper.epl.core;

namespace com.espertech.esper.dataflow.core
{
    public class EPDataFlowEmitter1Stream1TargetPassAlong
        : EPDataFlowEmitter1Stream1TargetBase
    {
        public EPDataFlowEmitter1Stream1TargetPassAlong(
            int operatorNum,
            DataFlowSignalManager signalManager,
            SignalHandler signalHandler,
            EPDataFlowEmitterExceptionHandler exceptionHandler,
            ObjectBindingPair target,
            EngineImportService engineImportService)
            : base(operatorNum, signalManager, signalHandler, exceptionHandler, target, engineImportService)
        {
        }

        public override void SubmitInternal(Object @object)
        {
            var parameters = new Object[] { @object };
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
