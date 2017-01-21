///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client.dataflow;
using com.espertech.esper.dataflow.runnables;

namespace com.espertech.esper.dataflow.util
{
    public class PunctuationEventListenerImpl : DataFlowSignalListener
    {
        private readonly OperatorMetadataDescriptor myOperator;

        private BaseRunnable _runnable;

        public PunctuationEventListenerImpl(OperatorMetadataDescriptor myOperator)
        {
            this.myOperator = myOperator;
        }

        public void SetRunnable(BaseRunnable runnable)
        {
            _runnable = runnable;
        }

        public void ProcessSignal(EPDataFlowSignal signal)
        {
            if (signal is EPDataFlowSignalFinalMarker)
            {
                if (_runnable != null)
                {
                    _runnable.Shutdown();
                }
            }
        }
    }
}