///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.historical.indexingstrategy;
using com.espertech.esper.common.@internal.epl.historical.lookupstrategy;
using com.espertech.esper.common.@internal.epl.@join.queryplan;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.join.@base
{
    public class JoinSetComposerPrototypeHistorical2StreamForge : JoinSetComposerPrototypeForge
    {
        private readonly HistoricalIndexLookupStrategyForge historicalIndexLookupStrategy;
        private readonly bool isAllHistoricalNoSubordinate;
        private readonly ExprNode outerJoinEqualsEval;
        private readonly bool[] outerJoinPerStream;

        private readonly int polledNum;
        private readonly PollResultIndexingStrategyForge pollResultIndexingStrategy;
        private readonly int streamNum;

        public JoinSetComposerPrototypeHistorical2StreamForge(
            EventType[] streamTypes, 
            ExprNode postJoinEvaluator, 
            bool outerJoins, 
            int polledNum, 
            int streamNum,
            ExprNode outerJoinEqualsEval, 
            HistoricalIndexLookupStrategyForge historicalIndexLookupStrategy,
            PollResultIndexingStrategyForge pollResultIndexingStrategy, 
            bool isAllHistoricalNoSubordinate,
            bool[] outerJoinPerStream)
            : base(streamTypes, postJoinEvaluator, outerJoins)
        {
            
            this.polledNum = polledNum;
            this.streamNum = streamNum;
            this.outerJoinEqualsEval = outerJoinEqualsEval;
            this.historicalIndexLookupStrategy = historicalIndexLookupStrategy;
            this.pollResultIndexingStrategy = pollResultIndexingStrategy;
            this.isAllHistoricalNoSubordinate = isAllHistoricalNoSubordinate;
            this.outerJoinPerStream = outerJoinPerStream;
        }

        public override QueryPlanForge OptionalQueryPlan => null;

        protected override Type Implementation()
        {
            return typeof(JoinSetComposerPrototypeHistorical2Stream);
        }

        protected override void PopulateInline(
            CodegenExpression impl, CodegenMethod method, SAIFFInitializeSymbol symbols, CodegenClassScope classScope)
        {
            method.Block
                .ExprDotMethod(Ref("impl"), "setPolledNum", Constant(polledNum))
                .ExprDotMethod(Ref("impl"), "setStreamNum", Constant(streamNum))
                .ExprDotMethod(
                    Ref("impl"), "setOuterJoinEqualsEval",
                    outerJoinEqualsEval == null
                        ? ConstantNull()
                        : ExprNodeUtilityCodegen.CodegenEvaluator(
                            outerJoinEqualsEval.Forge, method, GetType(), classScope))
                .ExprDotMethod(
                    Ref("impl"), "setLookupStrategy", historicalIndexLookupStrategy.Make(method, symbols, classScope))
                .ExprDotMethod(
                    Ref("impl"), "setIndexingStrategy", pollResultIndexingStrategy.Make(method, symbols, classScope))
                .ExprDotMethod(Ref("impl"), "setAllHistoricalNoSubordinate", Constant(isAllHistoricalNoSubordinate))
                .ExprDotMethod(Ref("impl"), "setOuterJoinPerStream", Constant(outerJoinPerStream));
        }
    }
} // end of namespace