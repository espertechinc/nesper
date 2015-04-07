///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.client.dataflow;


namespace com.espertech.esper.dataflow.core
{
    public class SignalHandlerDefaultWInvokeStream : SignalHandlerDefaultWInvoke
    {

        private readonly int _streamNum;

        public SignalHandlerDefaultWInvokeStream(Object target, MethodInfo method, int streamNum)
            : base(target, method)
        {
            _streamNum = streamNum;
        }

        protected override void HandleSignalInternal(EPDataFlowSignal signal)
        {
            FastMethod.Invoke(Target, new Object[] { _streamNum, signal });
        }
    }
}
