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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.dataflow.annotations;
using com.espertech.esper.dataflow.interfaces;

namespace com.espertech.esper.supportregression.dataflow
{
    public class MyObjectArrayGraphSource : DataFlowSourceOperator {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        private readonly IEnumerator<object[]> _enumerator;

#pragma warning disable CS0649
        [DataFlowContext] private EPDataFlowEmitter _graphContext;
#pragma warning restore CS0649

        public MyObjectArrayGraphSource(IEnumerator<object[]> enumerator) {
            this._enumerator = enumerator;
        }
    
        public void Next() {
            if (_enumerator.MoveNext()) {
                object[] next = _enumerator.Current;
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("submitting row " + next.Render());
                }
                _graphContext.Submit(next);
            }
            else {
                if (Log.IsDebugEnabled) {
                    Log.Debug("submitting punctuation");
                }
                _graphContext.SubmitSignal(new EPDataFlowSignalFinalMarkerImpl());
            }
        }
    
        public DataFlowOpInitializeResult Initialize(DataFlowOpInitializateContext context) {
            return null;
        }
    
        public void Open(DataFlowOpOpenContext openContext) {
        }
    
        public void Close(DataFlowOpCloseContext openContext) {
        }
    }
}
