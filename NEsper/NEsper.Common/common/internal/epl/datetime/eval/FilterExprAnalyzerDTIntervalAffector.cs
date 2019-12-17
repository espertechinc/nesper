///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.collection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.join.analyze;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.datetime.eval
{
    public class FilterExprAnalyzerDTIntervalAffector : FilterExprAnalyzerAffector
    {
        private readonly DateTimeMethodEnum currentMethod;
        private readonly string parameterEndProp;
        private readonly string parameterStartProp;
        private readonly int parameterStreamNum;
        private readonly string targetEndProp;
        private readonly string targetStartProp;
        private readonly int targetStreamNum;
        private readonly EventType[] typesPerStream;

        public FilterExprAnalyzerDTIntervalAffector(
            DateTimeMethodEnum currentMethod,
            EventType[] typesPerStream,
            int targetStreamNum,
            string targetStartProp,
            string targetEndProp,
            int parameterStreamNum,
            string parameterStartProp,
            string parameterEndProp)
        {
            this.currentMethod = currentMethod;
            this.typesPerStream = typesPerStream;
            this.targetStreamNum = targetStreamNum;
            this.targetStartProp = targetStartProp;
            this.targetEndProp = targetEndProp;
            this.parameterStreamNum = parameterStreamNum;
            this.parameterStartProp = parameterStartProp;
            this.parameterEndProp = parameterEndProp;
        }

        public ExprNode[] IndexExpressions => null;

        public IList<Pair<ExprNode, int[]>> KeyExpressions => null;

        public AdvancedIndexConfigContextPartition OptionalIndexSpec => null;

        public string OptionalIndexName => null;

        public void Apply(QueryGraphForge filterQueryGraph)
        {
            if (targetStreamNum == parameterStreamNum) {
                return;
            }

            var targetStartExpr = ExprNodeUtilityMake.MakeExprIdentNode(
                typesPerStream,
                targetStreamNum,
                targetStartProp);
            var targetEndExpr = ExprNodeUtilityMake.MakeExprIdentNode(typesPerStream, targetStreamNum, targetEndProp);
            var parameterStartExpr = ExprNodeUtilityMake.MakeExprIdentNode(
                typesPerStream,
                parameterStreamNum,
                parameterStartProp);
            var parameterEndExpr = ExprNodeUtilityMake.MakeExprIdentNode(
                typesPerStream,
                parameterStreamNum,
                parameterEndProp);

            if (targetStartExpr.Forge.EvaluationType != parameterStartExpr.Forge.EvaluationType) {
                return;
            }

            if (currentMethod == DateTimeMethodEnum.BEFORE) {
                // a.end < b.start
                filterQueryGraph.AddRelationalOpStrict(
                    targetStreamNum,
                    targetEndExpr,
                    parameterStreamNum,
                    parameterStartExpr,
                    RelationalOpEnum.LT);
            }
            else if (currentMethod == DateTimeMethodEnum.AFTER) {
                // a.start > b.end
                filterQueryGraph.AddRelationalOpStrict(
                    targetStreamNum,
                    targetStartExpr,
                    parameterStreamNum,
                    parameterEndExpr,
                    RelationalOpEnum.GT);
            }
            else if (currentMethod == DateTimeMethodEnum.COINCIDES) {
                // a.startTimestamp = b.startTimestamp and a.endTimestamp = b.endTimestamp
                filterQueryGraph.AddStrictEquals(
                    targetStreamNum,
                    targetStartProp,
                    targetStartExpr,
                    parameterStreamNum,
                    parameterStartProp,
                    parameterStartExpr);

                var noDuration = parameterEndProp.Equals(parameterStartProp) && targetEndProp.Equals(targetStartProp);
                if (!noDuration) {
                    var leftEndExpr = ExprNodeUtilityMake.MakeExprIdentNode(
                        typesPerStream,
                        targetStreamNum,
                        targetEndProp);
                    var rightEndExpr = ExprNodeUtilityMake.MakeExprIdentNode(
                        typesPerStream,
                        parameterStreamNum,
                        parameterEndProp);
                    filterQueryGraph.AddStrictEquals(
                        targetStreamNum,
                        targetEndProp,
                        leftEndExpr,
                        parameterStreamNum,
                        parameterEndProp,
                        rightEndExpr);
                }
            }
            else if (currentMethod == DateTimeMethodEnum.DURING || currentMethod == DateTimeMethodEnum.INCLUDES) {
                // DURING:   b.startTimestamp < a.startTimestamp <= a.endTimestamp < b.endTimestamp
                // INCLUDES: a.startTimestamp < b.startTimestamp <= b.endTimestamp < a.endTimestamp
                var relop = currentMethod == DateTimeMethodEnum.DURING ? RelationalOpEnum.LT : RelationalOpEnum.GT;
                filterQueryGraph.AddRelationalOpStrict(
                    parameterStreamNum,
                    parameterStartExpr,
                    targetStreamNum,
                    targetStartExpr,
                    relop);

                filterQueryGraph.AddRelationalOpStrict(
                    targetStreamNum,
                    targetEndExpr,
                    parameterStreamNum,
                    parameterEndExpr,
                    relop);
            }
            else if (currentMethod == DateTimeMethodEnum.FINISHES || currentMethod == DateTimeMethodEnum.FINISHEDBY) {
                // FINISHES:   b.startTimestamp < a.startTimestamp and a.endTimestamp = b.endTimestamp
                // FINISHEDBY: a.startTimestamp < b.startTimestamp and a.endTimestamp = b.endTimestamp
                var relop = currentMethod == DateTimeMethodEnum.FINISHES ? RelationalOpEnum.LT : RelationalOpEnum.GT;
                filterQueryGraph.AddRelationalOpStrict(
                    parameterStreamNum,
                    parameterStartExpr,
                    targetStreamNum,
                    targetStartExpr,
                    relop);

                filterQueryGraph.AddStrictEquals(
                    targetStreamNum,
                    targetEndProp,
                    targetEndExpr,
                    parameterStreamNum,
                    parameterEndProp,
                    parameterEndExpr);
            }
            else if (currentMethod == DateTimeMethodEnum.MEETS) {
                // a.endTimestamp = b.startTimestamp
                filterQueryGraph.AddStrictEquals(
                    targetStreamNum,
                    targetEndProp,
                    targetEndExpr,
                    parameterStreamNum,
                    parameterStartProp,
                    parameterStartExpr);
            }
            else if (currentMethod == DateTimeMethodEnum.METBY) {
                // a.startTimestamp = b.endTimestamp
                filterQueryGraph.AddStrictEquals(
                    targetStreamNum,
                    targetStartProp,
                    targetStartExpr,
                    parameterStreamNum,
                    parameterEndProp,
                    parameterEndExpr);
            }
            else if (currentMethod == DateTimeMethodEnum.OVERLAPS || currentMethod == DateTimeMethodEnum.OVERLAPPEDBY) {
                // OVERLAPS:     a.startTimestamp < b.startTimestamp < a.endTimestamp < b.endTimestamp
                // OVERLAPPEDBY: b.startTimestamp < a.startTimestamp < b.endTimestamp < a.endTimestamp
                var relop = currentMethod == DateTimeMethodEnum.OVERLAPS ? RelationalOpEnum.LT : RelationalOpEnum.GT;
                filterQueryGraph.AddRelationalOpStrict(
                    targetStreamNum,
                    targetStartExpr,
                    parameterStreamNum,
                    parameterStartExpr,
                    relop);

                filterQueryGraph.AddRelationalOpStrict(
                    targetStreamNum,
                    targetEndExpr,
                    parameterStreamNum,
                    parameterEndExpr,
                    relop);

                if (currentMethod == DateTimeMethodEnum.OVERLAPS) {
                    filterQueryGraph.AddRelationalOpStrict(
                        parameterStreamNum,
                        parameterStartExpr,
                        targetStreamNum,
                        targetEndExpr,
                        RelationalOpEnum.LT);
                }
                else {
                    filterQueryGraph.AddRelationalOpStrict(
                        targetStreamNum,
                        targetStartExpr,
                        parameterStreamNum,
                        parameterEndExpr,
                        RelationalOpEnum.LT);
                }
            }
            else if (currentMethod == DateTimeMethodEnum.STARTS || currentMethod == DateTimeMethodEnum.STARTEDBY) {
                // STARTS:       a.startTimestamp = b.startTimestamp and a.endTimestamp < b.endTimestamp
                // STARTEDBY:    a.startTimestamp = b.startTimestamp and b.endTimestamp < a.endTimestamp
                filterQueryGraph.AddStrictEquals(
                    targetStreamNum,
                    targetStartProp,
                    targetStartExpr,
                    parameterStreamNum,
                    parameterStartProp,
                    parameterStartExpr);

                var relop = currentMethod == DateTimeMethodEnum.STARTS ? RelationalOpEnum.LT : RelationalOpEnum.GT;
                filterQueryGraph.AddRelationalOpStrict(
                    targetStreamNum,
                    targetEndExpr,
                    parameterStreamNum,
                    parameterEndExpr,
                    relop);
            }
        }
    }
} // end of namespace