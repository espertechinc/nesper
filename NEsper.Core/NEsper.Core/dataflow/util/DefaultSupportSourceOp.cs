///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;
using com.espertech.esper.client.dataflow;
using com.espertech.esper.compat;
using com.espertech.esper.compat.threading;
using com.espertech.esper.dataflow.annotations;
using com.espertech.esper.dataflow.interfaces;

namespace com.espertech.esper.dataflow.util
{
    [DataFlowOpProvideSignal]
    public class DefaultSupportSourceOp : DataFlowSourceOperator
    {
        public Object[] Instructions;

        public DefaultSupportSourceOp()
        {
            Instructions = new Object[0];
        }

        public DefaultSupportSourceOp(Object[] instructions)
        {
            Instructions = instructions;
        }

        [DataFlowContext]
#pragma warning disable 649
        private EPDataFlowEmitter _graphContext;
#pragma warning restore 649

        private int _currentCount = -1;

        public int GetCurrentCount()
        {
            return _currentCount;
        }

        public void Next()
        {
            _currentCount++;
            if (Instructions.Length <= _currentCount)
            {
                _graphContext.SubmitSignal(new DataFlowSignalFinalMarker());
                return;
            }

            Object next = Instructions[_currentCount];
            if (next is CountDownLatch)
            {
                CountDownLatch latch = (CountDownLatch)next;
                latch.Await();
            }
            else if (next.IsInt() || next.IsLong())
            {
                Thread.Sleep(next.AsInt());
            }
            else if (next is Exception)
            {
                var ex = (Exception)next;
                throw new Exception("Support-graph-source generated exception: " + ex.Message, ex);
            }
            else
            {
                _graphContext.Submit(next);
            }
        }

        public DataFlowOpInitializeResult Initialize(DataFlowOpInitializateContext context)
        {
            return null;
        }

        public void Open(DataFlowOpOpenContext openContext)
        {
        }

        public void Close(DataFlowOpCloseContext openContext)
        {
        }

        private class DataFlowSignalFinalMarker : EPDataFlowSignalFinalMarker
        {
        }
    }
}