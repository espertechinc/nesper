///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.join.querygraph;

namespace com.espertech.esper.common.@internal.epl.join.exec.sorted
{
    public class SortedAccessStrategyFactory
    {
        public static SortedAccessStrategy Make(
            bool isNWOnTrigger,
            int lookupStream,
            int numStreams,
            QueryGraphValueEntryRange rangeKeyPair)
        {
            if (rangeKeyPair.Type.IsRange()) {
                var rangeIn = (QueryGraphValueEntryRangeIn)rangeKeyPair;
                var startExpr = rangeIn.ExprStart;
                var endExpr = rangeIn.ExprEnd;
                var includeStart = rangeKeyPair.Type.IsIncludeStart();

                var includeEnd = rangeKeyPair.Type.IsIncludeEnd();
                if (!rangeKeyPair.Type.IsRangeInverted()) {
                    return new SortedAccessStrategyRange(
                        isNWOnTrigger,
                        lookupStream,
                        numStreams,
                        startExpr,
                        includeStart,
                        endExpr,
                        includeEnd,
                        rangeIn.IsAllowRangeReversal);
                }
                else {
                    return new SortedAccessStrategyRangeInverted(
                        isNWOnTrigger,
                        lookupStream,
                        numStreams,
                        startExpr,
                        includeStart,
                        endExpr,
                        includeEnd);
                }
            }
            else {
                var relOp = (QueryGraphValueEntryRangeRelOp)rangeKeyPair;
                var keyExpr = relOp.Expression;
                if (rangeKeyPair.Type == QueryGraphRangeEnum.GREATER_OR_EQUAL) {
                    return new SortedAccessStrategyGE(isNWOnTrigger, lookupStream, numStreams, keyExpr);
                }
                else if (rangeKeyPair.Type == QueryGraphRangeEnum.GREATER) {
                    return new SortedAccessStrategyGT(isNWOnTrigger, lookupStream, numStreams, keyExpr);
                }
                else if (rangeKeyPair.Type == QueryGraphRangeEnum.LESS_OR_EQUAL) {
                    return new SortedAccessStrategyLE(isNWOnTrigger, lookupStream, numStreams, keyExpr);
                }
                else if (rangeKeyPair.Type == QueryGraphRangeEnum.LESS) {
                    return new SortedAccessStrategyLT(isNWOnTrigger, lookupStream, numStreams, keyExpr);
                }
                else {
                    throw new ArgumentException("Comparison operator " + rangeKeyPair.Type + " not supported");
                }
            }
        }
    }
} // end of namespace