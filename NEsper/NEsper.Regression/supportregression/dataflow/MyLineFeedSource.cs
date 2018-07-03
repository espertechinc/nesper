///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.client.dataflow;
using com.espertech.esper.dataflow.annotations;
using com.espertech.esper.dataflow.interfaces;

namespace com.espertech.esper.supportregression.dataflow
{
    [DataFlowOpProvideSignal]
    [OutputType(Name = "line", TypeName = "String")]
    public class MyLineFeedSource : DataFlowSourceOperator
    {
#pragma warning disable CS0649
        [DataFlowContext] private readonly EPDataFlowEmitter _dataFlowEmitter;
#pragma warning restore CS0649

        private readonly IEnumerator<String> _lines;

        public MyLineFeedSource(IEnumerator<String> lines)
        {
            _lines = lines;
        }

        public DataFlowOpInitializeResult Initialize(DataFlowOpInitializateContext context)
        {
            return null;
        }

        public void Open(DataFlowOpOpenContext openContext)
        {
        }

        public void Next()
        {
            if (_lines.MoveNext())
            {
                _dataFlowEmitter.Submit(new object[] { _lines.Current });
            }
            else
            {
                _dataFlowEmitter.SubmitSignal(new EPDataFlowSignalFinalMarkerImpl());
            }
        }

        public void Close(DataFlowOpCloseContext openContext)
        {
        }
    }
}
