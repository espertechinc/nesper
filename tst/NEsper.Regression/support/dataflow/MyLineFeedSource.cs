///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.dataflow.annotations;
using com.espertech.esper.common.client.dataflow.util;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;

namespace com.espertech.esper.regressionlib.support.dataflow
{
    public class MyLineFeedSource : DataFlowSourceOperator
    {
        private readonly IEnumerator<string> lines;

        [DataFlowContext] private EPDataFlowEmitter dataFlowEmitter;

        public MyLineFeedSource(IEnumerator<string> lines)
        {
            this.lines = lines;
        }

        public void Open(DataFlowOpOpenContext openContext)
        {
        }

        public void Next()
        {
            if (lines.MoveNext()) {
                dataFlowEmitter.Submit(new object[] {lines.Current});
            }
            else {
                dataFlowEmitter.SubmitSignal(new EPDataFlowSignalFinalMarkerImpl());
            }
        }

        public void Close(DataFlowOpCloseContext openContext)
        {
        }
    }
} // end of namespace