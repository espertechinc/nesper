///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.client.dataflow.util;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.dataflow.realize
{
    public class SignalHandlerDefaultWInvokeStream : SignalHandlerDefaultWInvoke
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SignalHandlerDefaultWInvokeStream));

        private readonly int streamNum;

        public SignalHandlerDefaultWInvokeStream(
            object target,
            MethodInfo method,
            int streamNum)
            : base(target, method)
        {
            this.streamNum = streamNum;
        }

        internal override void HandleSignalInternal(EPDataFlowSignal signal)
        {
            try {
                method.Invoke(target, new object[] { streamNum, signal });
            }
            catch (MemberAccessException e) {
                log.Error("Failed to invoke signal handler: " + e.Message, e);
            }
        }
    }
} // end of namespace