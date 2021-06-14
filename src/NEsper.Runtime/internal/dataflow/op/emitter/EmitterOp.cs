///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.dataflow.annotations;
using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.client.dataflow.util;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;

namespace com.espertech.esper.runtime.@internal.dataflow.op.emitter
{
    public class EmitterOp : EPDataFlowEmitter,
        DataFlowOperator,
        EPDataFlowEmitterOperator
    {
#pragma warning disable 649
        [DataFlowContext] private EPDataFlowEmitter dataFlowEmitter;
#pragma warning restore 649

        public EmitterOp(string name)
        {
            this.Name = name;
        }

        public void Submit(object @object)
        {
            dataFlowEmitter.Submit(@object);
        }

        public void SubmitSignal(EPDataFlowSignal signal)
        {
            dataFlowEmitter.SubmitSignal(signal);
        }

        public void SubmitPort(
            int portNumber,
            object @object)
        {
            dataFlowEmitter.SubmitPort(portNumber, @object);
        }

        public string Name { get; }
    }
} // end of namespace