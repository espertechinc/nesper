///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.lookup;

namespace com.espertech.esper.epl.join.exec.sorted
{
    public class SortedAccessStrategyFactory
    {
        public static SortedAccessStrategy Make(bool isNWOnTrigger, int lookupStream, int numStreams, QueryGraphValueEntryRange rangeKeyPair, Type coercionType)
        {
            return Make(isNWOnTrigger, lookupStream, numStreams, new SubordPropRangeKey(rangeKeyPair, coercionType));
        }
    
        public static SortedAccessStrategy Make(bool isNWOnTrigger, int lookupStream, int numStreams, SubordPropRangeKey streamRangeKey)
        {
            var rangeKeyPair = streamRangeKey.RangeInfo;
    
            if (rangeKeyPair.RangeType.IsRange()) {
                var rangeIn = (QueryGraphValueEntryRangeIn)rangeKeyPair;
                var startExpr = rangeIn.ExprStart.ExprEvaluator;
                var endExpr = rangeIn.ExprEnd.ExprEvaluator;
                var includeStart = rangeKeyPair.RangeType.IsIncludeStart();

                var includeEnd = rangeKeyPair.RangeType.IsIncludeEnd();
                if (!rangeKeyPair.RangeType.IsRangeInverted()) {
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
                return new SortedAccessStrategyRangeInverted(
                    isNWOnTrigger, 
                    lookupStream, 
                    numStreams, 
                    startExpr, 
                    includeStart, 
                    endExpr, 
                    includeEnd);
            }
            var relOp = (QueryGraphValueEntryRangeRelOp) rangeKeyPair;
            var keyExpr = relOp.Expression.ExprEvaluator;
            if (rangeKeyPair.RangeType == QueryGraphRangeEnum.GREATER_OR_EQUAL) {
                return new SortedAccessStrategyGE(isNWOnTrigger, lookupStream, numStreams, keyExpr);
            }
            if (rangeKeyPair.RangeType == QueryGraphRangeEnum.GREATER)
            {
                return new SortedAccessStrategyGT(isNWOnTrigger, lookupStream, numStreams, keyExpr);
            }
            if (rangeKeyPair.RangeType == QueryGraphRangeEnum.LESS_OR_EQUAL)
            {
                return new SortedAccessStrategyLE(isNWOnTrigger, lookupStream, numStreams, keyExpr);
            }
            if (rangeKeyPair.RangeType == QueryGraphRangeEnum.LESS)
            {
                return new SortedAccessStrategyLT(isNWOnTrigger, lookupStream, numStreams, keyExpr);
            }
            throw new ArgumentException("Comparison operator " + rangeKeyPair.RangeType + " not supported");
        }
    }
}
