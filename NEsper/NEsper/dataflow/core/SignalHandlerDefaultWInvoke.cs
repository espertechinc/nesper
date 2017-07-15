///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.epl.core;

using XLR8.CGLib;

using com.espertech.esper.client.dataflow;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.dataflow.core
{
    public class SignalHandlerDefaultWInvoke : SignalHandlerDefault
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected readonly Object Target;
        protected readonly FastMethod FastMethod;

        public SignalHandlerDefaultWInvoke(Object target, MethodInfo method, EngineImportService engineImportService)
        {
            Target = target;

            FastClass fastClass = FastClass.Create(target.GetType());
            FastMethod = fastClass.GetMethod(method);
        }

        public override void HandleSignal(EPDataFlowSignal signal)
        {
            try
            {
                HandleSignalInternal(signal);
            }
            catch (TargetInvocationException ex)
            {
                Log.Error("Failed to invoke signal handler: " + ex.Message, ex);
            }
        }

        protected virtual void HandleSignalInternal(EPDataFlowSignal signal)
        {
            FastMethod.Invoke(
                Target, new Object[]
                {
                    signal
                });
        }
    }
}
