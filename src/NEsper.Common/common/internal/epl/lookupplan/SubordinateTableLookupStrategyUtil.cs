///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.join.queryplan;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.lookupplan
{
    public class SubordinateTableLookupStrategyUtil
    {
        public static SubordTableLookupStrategyFactoryForge GetLookupStrategy(
            IList<EventType> outerStreamTypesZeroIndexed,
            IList<SubordPropHashKeyForge> hashKeys,
            CoercionDesc hashKeyCoercionTypes,
            MultiKeyClassRef hashMultikeyClasses,
            IList<SubordPropRangeKeyForge> rangeKeys,
            CoercionDesc rangeKeyCoercionTypes,
            IList<ExprNode> inKeywordSingleIdxKeys,
            ExprNode inKeywordMultiIdxKey,
            bool isNWOnTrigger)
        {
            var isStrictKeys = SubordPropUtil.IsStrictKeyJoin(hashKeys);
            string[] hashStrictKeys = null;
            int[] hashStrictKeyStreams = null;
            if (isStrictKeys) {
                hashStrictKeyStreams = SubordPropUtil.GetKeyStreamNums(hashKeys);
                hashStrictKeys = SubordPropUtil.GetKeyProperties(hashKeys);
            }

            var numStreamsTotal = outerStreamTypesZeroIndexed.Count + 1;
            SubordTableLookupStrategyFactoryForge lookupStrategy;
            if (inKeywordSingleIdxKeys != null) {
                lookupStrategy = new SubordInKeywordSingleTableLookupStrategyFactoryForge(
                    isNWOnTrigger,
                    numStreamsTotal,
                    inKeywordSingleIdxKeys);
            }
            else if (inKeywordMultiIdxKey != null) {
                lookupStrategy = new SubordInKeywordMultiTableLookupStrategyFactoryForge(
                    isNWOnTrigger,
                    numStreamsTotal,
                    inKeywordMultiIdxKey);
            }
            else if (hashKeys.IsEmpty() && rangeKeys.IsEmpty()) {
                lookupStrategy = new SubordFullTableScanLookupStrategyFactoryForge();
            }
            else if (hashKeys.Count > 0 && rangeKeys.IsEmpty()) {
                lookupStrategy = new SubordHashedTableLookupStrategyFactoryForge(
                    isNWOnTrigger,
                    numStreamsTotal,
                    hashKeys,
                    hashKeyCoercionTypes,
                    isStrictKeys,
                    hashStrictKeys,
                    hashStrictKeyStreams,
                    outerStreamTypesZeroIndexed,
                    hashMultikeyClasses);
            }
            else if (hashKeys.Count == 0 && rangeKeys.Count == 1) {
                lookupStrategy = new SubordSortedTableLookupStrategyFactoryForge(
                    isNWOnTrigger,
                    numStreamsTotal,
                    rangeKeys[0],
                    rangeKeyCoercionTypes);
            }
            else {
                lookupStrategy = new SubordCompositeTableLookupStrategyFactoryForge(
                    isNWOnTrigger,
                    numStreamsTotal,
                    hashKeys,
                    hashKeyCoercionTypes.CoercionTypes,
                    hashMultikeyClasses,
                    rangeKeys,
                    rangeKeyCoercionTypes.CoercionTypes);
            }

            return lookupStrategy;
        }
    }
} // end of namespace