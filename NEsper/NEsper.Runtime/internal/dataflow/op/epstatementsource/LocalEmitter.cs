///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.dataflow.util;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.runtime.@internal.dataflow.op.epstatementsource
{
    public class LocalEmitter : EPDataFlowEmitter
    {
        private readonly LinkedBlockingQueue<object> queue;

        public LocalEmitter(LinkedBlockingQueue<object> queue)
        {
            this.queue = queue;
        }

        public void Submit(object @object)
        {
            queue.Push(@object);
        }

        public void SubmitSignal(EPDataFlowSignal signal)
        {
            queue.Push(signal);
        }

        public void SubmitPort(int portNumber, object @object)
        {
            queue.Push(@object);
        }
    }
} // end of namespace