///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.collections;
using com.espertech.esper.runtime.client;

namespace com.espertech.esper.runtime.@internal.dataflow.op.epstatementsource
{
    public class EmitterUpdateListener : UpdateListener
    {
        private readonly IBlockingQueue<object> queue;
        private readonly bool submitEventBean;

        public EmitterUpdateListener(IBlockingQueue<object> queue, bool submitEventBean)
        {
            this.queue = queue;
            this.submitEventBean = submitEventBean;
        }

        public void Update(
            object sender,
            UpdateEventArgs eventArgs)
        {
            if (eventArgs.NewEvents != null)
            {
                foreach (var newEvent in eventArgs.NewEvents)
                {
                    if (submitEventBean)
                    {
                        queue.Push(newEvent);
                    }
                    else
                    {
                        queue.Push(newEvent.Underlying);
                    }
                }
            }
        }
    }
} // end of namespace