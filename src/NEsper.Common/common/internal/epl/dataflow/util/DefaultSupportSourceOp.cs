///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.dataflow.annotations;
using com.espertech.esper.common.client.dataflow.util;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.compat;
using com.espertech.esper.compat.threading;

namespace com.espertech.esper.common.@internal.epl.dataflow.util
{
    public class DefaultSupportSourceOp : DataFlowSourceOperator
    {
#pragma warning disable 649
        [DataFlowContext] private EPDataFlowEmitter graphContext;
#pragma warning restore 649

        public object[] instructions;

        public DefaultSupportSourceOp()
        {
            instructions = Array.Empty<object>();
        }

        public DefaultSupportSourceOp(object[] instructions)
        {
            this.instructions = instructions;
        }

        public int CurrentCount { get; private set; } = -1;

        public void Next()
        {
            CurrentCount++;
            if (instructions.Length <= CurrentCount) {
                graphContext.SubmitSignal(new EPDataFlowSignalFinalMarkerImpl());
                return;
            }

            var next = instructions[CurrentCount];
            if (next is CountDownLatch latch) {
                latch.Await();
            }
            else if (next.IsInt64() || next.IsInt32()) {
                Thread.Sleep(next.AsInt32());
            }
            else if (next is EPRuntimeException cause) {
                throw new EPRuntimeException("Support-graph-source generated exception: " + cause.Message, cause);
            }
            else {
                graphContext.Submit(next);
            }
        }

        public void Open(DataFlowOpOpenContext openContext)
        {
        }

        public void Close(DataFlowOpCloseContext openContext)
        {
        }
    }
} // end of namespace