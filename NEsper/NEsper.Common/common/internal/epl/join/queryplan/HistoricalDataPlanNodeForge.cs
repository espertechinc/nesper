///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.historical.indexingstrategy;
using com.espertech.esper.common.@internal.epl.historical.lookupstrategy;
using com.espertech.esper.common.@internal.epl.@join.queryplanbuild;
using com.espertech.esper.common.@internal.util;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.join.queryplan
{
    /// <summary>
    ///     Query plan for performing a historical data lookup.
    ///     <para />
    ///     Translates into a particular execution for use in regular and outer joins.
    /// </summary>
    public class HistoricalDataPlanNodeForge : QueryPlanNodeForge
    {
        private readonly ExprForge outerJoinExprEval;
        private HistoricalIndexLookupStrategyForge historicalIndexLookupStrategy;
        private PollResultIndexingStrategyForge pollResultIndexingStrategy;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="streamNum">the historical stream num</param>
        /// <param name="rootStreamNum">the stream number of the query plan providing incoming events</param>
        /// <param name="lookupStreamNum">the stream that provides polling/lookup events</param>
        /// <param name="numStreams">number of streams in join</param>
        /// <param name="outerJoinExprEval">outer join expression node or null if none defined</param>
        public HistoricalDataPlanNodeForge(
            int streamNum,
            int rootStreamNum,
            int lookupStreamNum,
            int numStreams,
            ExprForge outerJoinExprEval)
        {
            StreamNum = streamNum;
            RootStreamNum = rootStreamNum;
            LookupStreamNum = lookupStreamNum;
            NumStreams = numStreams;
            this.outerJoinExprEval = outerJoinExprEval;
        }

        public PollResultIndexingStrategyForge PollResultIndexingStrategy {
            set => pollResultIndexingStrategy = value;
        }

        public HistoricalIndexLookupStrategyForge HistoricalIndexLookupStrategy {
            set => historicalIndexLookupStrategy = value;
        }

        public int StreamNum { get; }

        public int RootStreamNum { get; }

        public int LookupStreamNum { get; }

        public int NumStreams { get; }

        public override void AddIndexes(HashSet<TableLookupIndexReqKey> usedIndexes)
        {
            // none to add
        }

        public override CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(HistoricalDataPlanNode), GetType(), classScope);
            method.Block
                .DeclareVar(typeof(HistoricalDataPlanNode), "node", NewInstance(typeof(HistoricalDataPlanNode)))
                .ExprDotMethod(Ref("node"), "setStreamNum", Constant(StreamNum))
                .ExprDotMethod(Ref("node"), "setNumStreams", Constant(NumStreams))
                .ExprDotMethod(
                    Ref("node"), "setIndexingStrategy", pollResultIndexingStrategy.Make(method, symbols, classScope))
                .ExprDotMethod(
                    Ref("node"), "setLookupStrategy", historicalIndexLookupStrategy.Make(method, symbols, classScope))
                .ExprDotMethod(Ref("node"), "setRootStreamNum", Constant(RootStreamNum))
                .ExprDotMethod(
                    Ref("node"), "setOuterJoinExprEval",
                    outerJoinExprEval == null
                        ? ConstantNull()
                        : ExprNodeUtilityCodegen.CodegenEvaluator(outerJoinExprEval, method, GetType(), classScope))
                .MethodReturn(Ref("node"));
            return LocalMethod(method);
        }

        protected override void Print(IndentWriter writer)
        {
            writer.IncrIndent();
            writer.WriteLine("HistoricalDataPlanNode streamNum=" + StreamNum);
        }

        public override void Accept(QueryPlanNodeForgeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
} // end of namespace