///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client.dataflow;
using com.espertech.esper.dataflow.annotations;
using com.espertech.esper.dataflow.interfaces;

namespace com.espertech.esper.dataflow.ops
{
    [DataFlowOperator]
    [DataFlowOpProvideSignal]
    public class Emitter : EPDataFlowEmitter
    {
        [DataFlowOpParameterAttribute] private String name;
        [DataFlowContextAttribute] private EPDataFlowEmitter dataFlowEmitter;

        public Emitter()
        {
            name = null;
            dataFlowEmitter = null;
        }

        public void Submit(Object @object)
        {
            dataFlowEmitter.Submit(@object);
        }

        public void SubmitSignal(EPDataFlowSignal signal)
        {
            dataFlowEmitter.SubmitSignal(signal);
        }

        public void SubmitPort(int portNumber, Object @object)
        {
            dataFlowEmitter.SubmitPort(portNumber, @object);
        }

        public string Name
        {
            get { return name; }
        }
    }
}
