///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.@join.querygraph;

namespace com.espertech.esper.common.@internal.epl.join.analyze
{
    public class RangeFilterAnalyzer
    {
        public static void Apply(
            ExprNode target,
            ExprNode start,
            ExprNode end,
            bool includeStart,
            bool includeEnd,
            bool isNot,
            QueryGraphForge queryGraph)
        {
            var rangeOp = QueryGraphRangeEnum.GetRangeOp(includeStart, includeEnd, isNot);

            if (target is ExprIdentNode &&
                start is ExprIdentNode &&
                end is ExprIdentNode) {
                var identNodeValue = (ExprIdentNode) target;
                var identNodeStart = (ExprIdentNode) start;
                var identNodeEnd = (ExprIdentNode) end;

                var keyStreamStart = identNodeStart.StreamId;
                var keyStreamEnd = identNodeEnd.StreamId;
                var valueStream = identNodeValue.StreamId;
                queryGraph.AddRangeStrict(
                    keyStreamStart, identNodeStart, keyStreamEnd,
                    identNodeEnd, valueStream,
                    identNodeValue, rangeOp);
                return;
            }

            // handle constant-compare or transformation case
            if (target is ExprIdentNode) {
                var identNode = (ExprIdentNode) target;
                var indexedStream = identNode.StreamId;

                var eligibilityStart = EligibilityUtil.VerifyInputStream(start, indexedStream);
                if (!eligibilityStart.Eligibility.IsEligible()) {
                    return;
                }

                var eligibilityEnd = EligibilityUtil.VerifyInputStream(end, indexedStream);
                if (!eligibilityEnd.Eligibility.IsEligible()) {
                    return;
                }

                queryGraph.AddRangeExpr(
                    indexedStream, identNode, start, eligibilityStart.StreamNum, end, eligibilityEnd.StreamNum,
                    rangeOp);
            }
        }
    }
} // end of namespace