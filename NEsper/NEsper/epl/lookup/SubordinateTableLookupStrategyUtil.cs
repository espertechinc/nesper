///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.join.plan;

namespace com.espertech.esper.epl.lookup
{
    public class SubordinateTableLookupStrategyUtil
    {
        public static SubordTableLookupStrategyFactory GetLookupStrategy(
            EventType[] outerStreamTypesZeroIndexed,
            IList<SubordPropHashKey> hashKeys,
            CoercionDesc hashKeyCoercionTypes,
            IList<SubordPropRangeKey> rangeKeys,
            CoercionDesc rangeKeyCoercionTypes,
            IList<ExprNode> inKeywordSingleIdxKeys,
            ExprNode inKeywordMultiIdxKey,
            bool isNWOnTrigger)
        {
            var isStrictKeys = SubordPropUtil.IsStrictKeyJoin(hashKeys);
            string[] hashStrictKeys = null;
            int[] hashStrictKeyStreams = null;
            if (isStrictKeys)
            {
                hashStrictKeyStreams = SubordPropUtil.GetKeyStreamNums(hashKeys);
                hashStrictKeys = SubordPropUtil.GetKeyProperties(hashKeys);
            }

            int numStreamsTotal = outerStreamTypesZeroIndexed.Length + 1;
            SubordTableLookupStrategyFactory lookupStrategy;
            if (inKeywordSingleIdxKeys != null)
            {
                lookupStrategy = new SubordInKeywordSingleTableLookupStrategyFactory(
                    isNWOnTrigger, numStreamsTotal, inKeywordSingleIdxKeys);
            }
            else if (inKeywordMultiIdxKey != null)
            {
                lookupStrategy = new SubordInKeywordMultiTableLookupStrategyFactory(
                    isNWOnTrigger, numStreamsTotal, inKeywordMultiIdxKey);
            }
            else if (hashKeys.IsEmpty() && rangeKeys.IsEmpty())
            {
                lookupStrategy = new SubordFullTableScanLookupStrategyFactory();
            }
            else if (hashKeys.Count > 0 && rangeKeys.IsEmpty())
            {
                if (hashKeys.Count == 1)
                {
                    if (!hashKeyCoercionTypes.IsCoerce)
                    {
                        if (isStrictKeys)
                        {
                            lookupStrategy = new SubordIndexedTableLookupStrategySinglePropFactory(
                                isNWOnTrigger, outerStreamTypesZeroIndexed, hashStrictKeyStreams[0], hashStrictKeys[0]);
                        }
                        else
                        {
                            lookupStrategy = new SubordIndexedTableLookupStrategySingleExprFactory(
                                isNWOnTrigger, numStreamsTotal, hashKeys[0]);
                        }
                    }
                    else
                    {
                        lookupStrategy = new SubordIndexedTableLookupStrategySingleCoercingFactory(
                            isNWOnTrigger, numStreamsTotal, hashKeys[0], hashKeyCoercionTypes.CoercionTypes[0]);
                    }
                }
                else
                {
                    if (!hashKeyCoercionTypes.IsCoerce)
                    {
                        if (isStrictKeys)
                        {
                            lookupStrategy = new SubordIndexedTableLookupStrategyPropFactory(
                                isNWOnTrigger, outerStreamTypesZeroIndexed, hashStrictKeyStreams, hashStrictKeys);
                        }
                        else
                        {
                            lookupStrategy = new SubordIndexedTableLookupStrategyExprFactory(
                                isNWOnTrigger, numStreamsTotal, hashKeys);
                        }
                    }
                    else
                    {
                        lookupStrategy = new SubordIndexedTableLookupStrategyCoercingFactory(
                            isNWOnTrigger, numStreamsTotal, hashKeys, hashKeyCoercionTypes.CoercionTypes);
                    }
                }
            }
            else if (hashKeys.Count == 0 && rangeKeys.Count == 1)
            {
                lookupStrategy = new SubordSortedTableLookupStrategyFactory(isNWOnTrigger, numStreamsTotal, rangeKeys[0]);
            }
            else
            {
                lookupStrategy = new SubordCompositeTableLookupStrategyFactory(
                    isNWOnTrigger, numStreamsTotal, hashKeys, hashKeyCoercionTypes.CoercionTypes,
                    rangeKeys, rangeKeyCoercionTypes.CoercionTypes);
            }
            return lookupStrategy;
        }
    }
}
