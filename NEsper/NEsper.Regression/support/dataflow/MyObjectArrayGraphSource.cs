///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client.dataflow.annotations;
using com.espertech.esper.common.client.dataflow.util;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.regressionlib.support.dataflow
{
    public class MyObjectArrayGraphSource : DataFlowSourceOperator
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEnumerator<object[]> enumerator;

        [DataFlowContext] private EPDataFlowEmitter graphContext;

        public MyObjectArrayGraphSource(IEnumerator<object[]> enumerator)
        {
            this.enumerator = enumerator;
        }

        public void Next()
        {
            if (enumerator.MoveNext()) {
                var next = enumerator.Current;
                if (log.IsDebugEnabled) {
                    log.Debug("submitting row " + next.RenderAny());
                }

                graphContext.Submit(next);
            }
            else {
                if (log.IsDebugEnabled) {
                    log.Debug("submitting punctuation");
                }

                graphContext.SubmitSignal(new EPDataFlowSignalFinalMarkerImpl());
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