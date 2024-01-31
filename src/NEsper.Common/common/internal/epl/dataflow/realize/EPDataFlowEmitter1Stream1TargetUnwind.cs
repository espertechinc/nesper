///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;

using com.espertech.esper.common.@internal.epl.dataflow.util;
using com.espertech.esper.common.@internal.settings;

namespace com.espertech.esper.common.@internal.epl.dataflow.realize
{
    public class EPDataFlowEmitter1Stream1TargetUnwind : EPDataFlowEmitter1Stream1TargetBase
    {
        public EPDataFlowEmitter1Stream1TargetUnwind(
            int operatorNum,
            DataFlowSignalManager signalManager,
            SignalHandler signalHandler,
            EPDataFlowEmitterExceptionHandler exceptionHandler,
            ObjectBindingPair target,
            ImportService importService)
            : base(
                operatorNum,
                signalManager,
                signalHandler,
                exceptionHandler,
                target,
                importService)
        {
        }

        public override void SubmitInternal(object @object)
        {
            var parameters = (object[])@object;
            try {
                exceptionHandler.HandleAudit(targetObject, parameters);
                fastMethod.Invoke(targetObject, parameters);
            }
            catch (TargetException e) {
                exceptionHandler.HandleException(targetObject, fastMethod, e, parameters);
            }
        }
    }
} // end of namespace