///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.client.dataflow;
using com.espertech.esper.epl.core;

namespace com.espertech.esper.dataflow.core
{
    public class SignalHandlerDefaultWInvokeStream : SignalHandlerDefaultWInvoke
    {
        private readonly int _streamNum;

        public SignalHandlerDefaultWInvokeStream(Object target, MethodInfo method, EngineImportService engineImportService, int streamNum)
            : base(target, method, engineImportService)
        {
            _streamNum = streamNum;
        }

        protected override void HandleSignalInternal(EPDataFlowSignal signal)
        {
            Method.Invoke(Target, new Object[] { _streamNum, signal });
        }
    }
}
