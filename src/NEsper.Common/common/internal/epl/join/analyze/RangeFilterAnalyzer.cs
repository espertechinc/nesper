///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.join.querygraph;

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
            var rangeOp = QueryGraphRangeEnumExtensions.GetRangeOp(includeStart, includeEnd, isNot);

            if (target is ExprIdentNode identNodeValue &&
                start is ExprIdentNode identNodeStart &&
                end is ExprIdentNode identNodeEnd) {
                var keyStreamStart = identNodeStart.StreamId;
                var keyStreamEnd = identNodeEnd.StreamId;
                var valueStream = identNodeValue.StreamId;
                queryGraph.AddRangeStrict(
                    keyStreamStart,
                    identNodeStart,
                    keyStreamEnd,
                    identNodeEnd,
                    valueStream,
                    identNodeValue,
                    rangeOp);
                return;
            }

            // handle constant-compare or transformation case
            if (target is ExprIdentNode identNode) {
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
                    indexedStream,
                    identNode,
                    start,
                    eligibilityStart.StreamNum,
                    end,
                    eligibilityEnd.StreamNum,
                    rangeOp);
            }
        }
    }
} // end of namespace