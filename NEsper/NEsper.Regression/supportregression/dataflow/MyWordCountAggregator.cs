///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client.dataflow;
using com.espertech.esper.compat.logging;
using com.espertech.esper.dataflow.annotations;
using com.espertech.esper.dataflow.interfaces;

namespace com.espertech.esper.supportregression.dataflow
{
    [DataFlowOperator]
    [OutputType(Name = "stats", Type = typeof(MyWordCountStats))]
    public class MyWordCountAggregator
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

#pragma warning disable CS0649
        [DataFlowContext] private EPDataFlowEmitter graphContext;
#pragma warning restore CS0649

        private readonly MyWordCountStats _aggregate = new MyWordCountStats();

        public void OnInput(int lines, int words, int chars)
        {
            _aggregate.Add(lines, words, chars);
            Log.Debug("Aggregated: " + _aggregate);
        }

        public void OnSignal(EPDataFlowSignal signal)
        {
            Log.Debug("Received punctuation, submitting totals: " + _aggregate);
            graphContext.Submit(_aggregate);
        }
    }
}