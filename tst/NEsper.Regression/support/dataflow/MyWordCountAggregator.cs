///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;

using com.espertech.esper.common.client.dataflow.annotations;
using com.espertech.esper.common.client.dataflow.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.compat.logging;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.regressionlib.support.dataflow
{
    [OutputTypes]
    [OutputType(Name = "stats", Type = typeof(MyWordCountStats))]
    public class MyWordCountAggregator : DataFlowOperatorForge,
        DataFlowOperatorFactory,
        DataFlowOperator
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly MyWordCountStats aggregate = new MyWordCountStats();

        [DataFlowContext] private EPDataFlowEmitter graphContext;

        public void InitializeFactory(DataFlowOpFactoryInitializeContext context)
        {
        }

        public DataFlowOperator Operator(DataFlowOpInitializeContext context)
        {
            return new MyWordCountAggregator();
        }

        public DataFlowOpForgeInitializeResult InitializeForge(DataFlowOpForgeInitializeContext context)
        {
            return null;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            return NewInstance(typeof(MyWordCountAggregator));
        }

        public void OnInput(
            int lines,
            int words,
            int chars)
        {
            aggregate.Add(lines, words, chars);
            log.Debug("Aggregated: " + aggregate);
        }

        public void OnSignal(EPDataFlowSignal signal)
        {
            log.Debug("Received punctuation, submitting totals: " + aggregate);
            graphContext.Submit(aggregate);
        }
    }
} // end of namespace